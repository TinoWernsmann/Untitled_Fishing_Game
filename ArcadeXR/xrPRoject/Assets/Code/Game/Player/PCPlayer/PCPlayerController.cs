using UnityEngine;
using Player.FirstPerson;
using Core.Game;

public class PCPlayerController : PlayerControllerBaseShared
{
    FirstPersonControllerComponent controller = null;
    Raycaster raycaster = null;

    public Transform rodAttachedPosition = null;

    [Header("Audio")]
    public bool playBackgroundLoopOnStart = true;

    [Range(0f, 1f)]
    public float backgroundLoopVolume = 0.35f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        SetupPlayer();
        StartBackgroundLoop();
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

    void SetupPlayer()
    {
        controller = new FirstPersonControllerComponent(gameObject); //gameObject <=> this as gameobject!
        raycaster = new Raycaster();
    }

    // Update is called once per frame
    void Update()
    {
        controller.Tick();
        HandleMouseData(Time.deltaTime);
    }

    void HandleMouseData(float deltatime)
    {
        if (controller.input != null)
        {
            InputData input = controller.input; //ref

            if (input.IsMouseLeftDown())
            {

                if (fishingRod){
                    float debugPower = 10.0f;
                    fishingRod.CastBuoy(debugPower, controller.LookDirHorizontalPlane());
                }

                //to be removed
                //performDebugraycast();
            }
            if (input.IsMouseRightDown())
            {
                if (fishingRod)
                {
                    float debugReelPower = 2.0f;
                    fishingRod.ReelBuoy(debugReelPower);
                }
            }


        }
    }
    

    private void performDebugraycast()
    {
        //debug raycasting
        GameObject hit = null;
        if (raycaster.PerformRaycast(
            controller.CameraPosition(),
            controller.LookDir(),
            300.0f,
            out hit //ref muss auch so deklariert werden
            )
        )
        {
            if (hit)
            {
                Debug.Log("PLAYER RAYCAST " + hit.name);

                IRaycastReceiver casted = hit.GetComponent<IRaycastReceiver>();

                if (casted != null)
                {
                    casted.ReceiveRaycast(controller.CameraPosition());
                }


            }
        }
    }

    
    //attach to fixed transform for pc player
    public override Transform rodAttachToTransform()
    {
        if (rodAttachedPosition != null)
        {
            return rodAttachedPosition;
        }
        return base.rodAttachToTransform();
    }




}