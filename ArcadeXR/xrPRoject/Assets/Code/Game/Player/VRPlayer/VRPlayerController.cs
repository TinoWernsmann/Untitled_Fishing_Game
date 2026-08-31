using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.XR;
using Manager.Haptics;

public class VRPlayerController : PlayerControllerBaseShared
{

    // --- hand references
    public GameObject rightHandReference;

    public Transform headTransform;
    public Transform rodAttachedPosition;

    [Header("XRI Input Actions")]
    public InputActionReference rightHandMoveInput;
    public InputActionReference leftHandMoveInput;

    public float reelPower = 3f;

    public float moveSpeed = 1.5f;
    // degrees per second for smooth yaw rotation using the right stick
    public float rotationSpeed = 90f;
    public bool debugHandLogging = false;

    [Header("Audio")]
    public bool playBackgroundLoopOnStart = true;

    [Range(0f, 1f)]
    public float backgroundLoopVolume = 0.35f;

    [Header("Arcade Casting")]
    public float maxPullDistance = 0.6f;

    [Range(0f, 1f)]
    public float handDirectionWeight = 0.6f;

    [Range(0f, 1f)]
    public float headDirectionWeight = 0.4f;

    public float minimumCastPower = 0f;
    public float maximumCastPower = 14f;

    private Vector3 grabStartPosition;
    private float maxPullBackDistance;
    private float peakHandSpeed;

    CharacterController characterController;
    bool isHoldingRod;
    bool hasThrownDuringHold;
    Vector3 previousRightHandPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        SetupPlayer();
        StartBackgroundLoop();
        CacheHandSamples();

        if (HapticManager.Instance == null)
        {
            new GameObject("HapticManager").AddComponent<HapticManager>();
        }
    }

    protected override void OnVolumeUpdate()
    {
        backgroundLoopVolume = volume;
        ApplyBackgroundVolume();
    }

    void StartBackgroundLoop()
    {
        if (!playBackgroundLoopOnStart)
        {
            return;
        }

        var audioManager = Manager.Audio.AudioManager.GetOrCreateInstance();
        audioManager.PlayMusic(null, 1f);
        audioManager.SetMusicVolume(backgroundLoopVolume);
    }

    void ApplyBackgroundVolume()
    {
        Manager.Audio.AudioManager.GetOrCreateInstance().SetMusicVolume(backgroundLoopVolume);
    }

    // Cache the controller rig parts and the head camera.
    void SetupPlayer()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }

        if (headTransform == null)
        {
            Camera camera = GetComponentInChildren<Camera>();
            if (camera != null)
            {
                headTransform = camera.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleRodReel();
        HandleRodGrabAndThrow();
        HandleMovement();

        if (debugHandLogging)
        {
            Debug.Log("VR right hand " + RightHandLocation().ToString());
        }
    }

    // Reel the buoy in when the right A button is pressed and the rod is out.
    void HandleRodReel()
    {
        if (fishingRod == null)
        {
            return;
        }

        Game.FishingRod.transfer.FBuoyFlags buoyState = fishingRod.GetCurrentBuoyState();
        if (buoyState == null || buoyState.RodIsReeledIn())
        {
            return;
        }

        if (IsAButtonPressed())
        {
            fishingRod.ReelBuoy(reelPower);
        }
    }

    // Handle grabbing the rod with the right grip button and casting on release.
    void HandleRodGrabAndThrow()
    {
        bool castButtonPressed = IsRightGripPressed();

        if (castButtonPressed && !isHoldingRod)
        {
            BeginGrabRod();
        }

        if (!castButtonPressed && isHoldingRod)
        {
            ReleaseRodAndCast();
            return;
        }

        if (!isHoldingRod)
        {
            return;
        }

        UpdateThrowCharge();
    }

    // Start tracking a grab on the rod.
    void BeginGrabRod()
    {
        if (rodBaseObject == null)
        {
            return;
        }

        isHoldingRod = true;
        hasThrownDuringHold = false;

        peakHandSpeed = 0f;
        maxPullBackDistance = 0f;

        grabStartPosition = GetRightHandPosition();

        Transform grabTransform = GetGrabTransform();

        if (grabTransform != null)
        {
            AttachComponentLocalSpace(grabTransform, rodBaseObject);
        }

        CacheHandSamples();
    }

    // Release the rod and throw it using the measured swing speed.
    void ReleaseRodAndCast()
    {
        if (!hasThrownDuringHold)
        {
            TryCastRod(peakHandSpeed);
        }

        isHoldingRod = false;
        hasThrownDuringHold = false;
        peakHandSpeed = 0f;

        ReturnRodToRestTransform();
    }

    // Measure how fast the right hand moves while the rod is held.
    void UpdateThrowCharge()
    {
        if (Time.deltaTime <= Mathf.Epsilon)
        {
            return;
        }

        Vector3 currentRightHandPosition = GetRightHandPosition();

        float pullDistance =
            Vector3.Distance(grabStartPosition, currentRightHandPosition);

        maxPullBackDistance =
            Mathf.Max(
                maxPullBackDistance,
                pullDistance);

        Vector3 velocity =
            (currentRightHandPosition - previousRightHandPosition) / Time.deltaTime;

        float handSpeed = velocity.magnitude;

        peakHandSpeed =
            Mathf.Max(peakHandSpeed, handSpeed);

        previousRightHandPosition = currentRightHandPosition;
    }

    void TryCastRod(float throwSpeed)
    {
        if (fishingRod == null)
        {
            return;
        }

        float throwPower = CalculateArcadeThrowPower();

        fishingRod.CastBuoy(throwPower, GetThrowDirection());

        hasThrownDuringHold = true;
    }

    float CalculateArcadeThrowPower()
    {
        float pullNormalized =
            Mathf.Clamp01(maxPullBackDistance / maxPullDistance);

        float speedNormalized =
            Mathf.Clamp01(peakHandSpeed / 2f);

        float combined =
            (pullNormalized * 0.7f) + (speedNormalized * 0.3f);

        float throwPower =
            Mathf.Lerp(minimumCastPower, maximumCastPower, combined);

        return throwPower;
    }

    Vector3 GetThrowDirection()
    {
        Vector3 headDir =
            headTransform != null
            ? headTransform.forward
            : transform.forward;

        Vector3 handDir =
            rightHandReference != null
            ? rightHandReference.transform.forward
            : transform.forward;

        Vector3 direction =
            (headDir * headDirectionWeight) + (handDir * handDirectionWeight);

        direction.Normalize();

        direction.y =
            Mathf.Clamp(direction.y, 0.1f, 0.8f);

        return direction.normalized;
    }

    // Read the right grip button, or fall back to keyboard testing.
    bool IsRightGripPressed()
    {
        UnityEngine.XR.InputDevice rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightHandDevice.isValid && rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool gripPressed))
        {
            return gripPressed;
        }

        return Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
    }

    // Read the A button, or fall back to keyboard testing.
    bool IsAButtonPressed()
    {
        if (Keyboard.current != null && Keyboard.current.aKey.isPressed)
        {
            return true;
        }

        UnityEngine.XR.InputDevice rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightHandDevice.isValid && rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryPressed))
        {
            return primaryPressed;
        }

        return false;
    }

    // Next 3 Methods might be useless, since only one Controller is used. Might be deleted after testing.
    // Choose the transform the rod should stay attached to while held.
    Transform GetGrabTransform()
    {
        if (rodAttachedPosition != null)
        {
            return rodAttachedPosition;
        }

        if (rightHandReference != null)
        {
            return rightHandReference.transform;
        }

        return transform;
    }

    // Choose the default resting transform for the rod.
    Transform GetRestTransform()
    {
        if (rodAttachedPosition != null)
        {
            return rodAttachedPosition;
        }

        return transform;
    }

    // Reattach the rod to the resting transform after a cast.
    void ReturnRodToRestTransform()
    {
        if (rodBaseObject == null)
        {
            return;
        }

        Transform restTransform = GetRestTransform();
        if (restTransform != null)
        {
            AttachComponentLocalSpace(restTransform, rodBaseObject);
        }
    }

    // Store the current hand position as the baseline for throw tracking.
    void CacheHandSamples()
    {
        previousRightHandPosition = GetRightHandPosition();
    }

    // Get the current world position of the right hand.
    Vector3 GetRightHandPosition()
    {
        return rightHandReference != null ? rightHandReference.transform.position : transform.position;
    }

    Vector2 ReadHandStick(InputActionReference actionReference, XRNode handNode)
    {
        if (actionReference != null && actionReference.action != null)
        {
            return actionReference.action.ReadValue<Vector2>();
        }

        UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(handNode);
        if (device.isValid && device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 stickValue))
        {
            return stickValue;
        }

        return Vector2.zero;
    }

    // Apply rotation from the right controller stick and movement from the left controller stick.
    void HandleMovement()
    {
        Vector2 rightStick = ReadHandStick(rightHandMoveInput, XRNode.RightHand);
        Vector2 leftStick = ReadHandStick(leftHandMoveInput, XRNode.LeftHand);

        // Apply yaw rotation from the right stick X axis.
        if (Mathf.Abs(rightStick.x) > 0.001f)
        {
            float yaw = rightStick.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, yaw, 0f);
        }

        if (leftStick == Vector2.zero)
        {
            return;
        }

        Transform referenceTransform = headTransform != null ? headTransform : transform;

        Vector3 forward = referenceTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = referenceTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 movement = (right * leftStick.x) + (forward * leftStick.y);
        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        if (characterController != null)
        {
            characterController.Move(movement * moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += movement * moveSpeed * Time.deltaTime;
        }
    }

    // Return the right hand position for debug output.
    Vector3 RightHandLocation()
    {
        Vector3 none = transform.position;
        if (rightHandReference)
        {
            none = rightHandReference.gameObject.transform.position;
        }
        return none;
    }

    // Tell the shared base which transform the rod should attach to.
    public override Transform rodAttachToTransform()
    {
        if (isHoldingRod)
        {
            return GetGrabTransform();
        }

        if (rodAttachedPosition != null)
        {
            return rodAttachedPosition;
        }
        return base.rodAttachToTransform();
    }

}