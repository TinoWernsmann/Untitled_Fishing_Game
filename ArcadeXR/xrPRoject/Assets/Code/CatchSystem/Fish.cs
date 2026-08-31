using Core.UI;
using Game.Fishing;
using Game.FishingRod;
using Game.FishingRod.Minigame;
using Game.FishingRod.transfer;
using MixedRealityArcade.CrossoverNetcode;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using util;
using util.Quat;
using Animation.Fish;





namespace Core.Catching
{
    public enum FishBehaviorType
    {
        Calm,
        Curious,
        Shy,
        Aggressive
    }

    [RequireComponent(typeof(FishAnimator))]
    public class Fish : CatchBase
    {
        [Header("Movement")]
        [Tooltip("Base swim speed the fish uses for normal movement.")]
        [SerializeField] public float baseSwimSpeed = 0.05f;//1.5f;

        [SerializeField] private float swimDepth = 0.1f;
        [SerializeField] private bool randomizeBehaviorOnStart = true;
        [SerializeField] private FishBehaviorType behaviorType = FishBehaviorType.Calm;

        public GameObject flag;


        private float deltaTimeMultiply = 1.0f;
        private bool catchAttemptFailed = false;
        private bool swimUpOnStartFinished = false;

        private float timeToDestroy = 2.0f;

        private bool debugCubeEnabled = false;
        private FishAnimator animator;

        private FishingBuoy buoyReference = null;
        
        public void SetBuoyReference(FishingBuoy buoy)
        {
            buoyReference = buoy;
        }




        [Header("Reel In Params")]

        /// value between 0 and 1 since when a catch is valid
        [SerializeField] private float catchScalarTo1 = 0.9f;
        [SerializeField] public float timeAtBuoy = 1.0f; //time at buoy allowed


        [Header("Water Surface")]
        [SerializeField] private float flatWaterThreshold = 0.05f;

        private Bounds waterBounds;
        private bool hasWaterBounds;
        private Vector3 targetPosition;
       

        private float minY;
        private float maxY;
        private float waterY;
        private bool useFlatWater;

        [Header("Water Surface")]
        [Tooltip("Offset above the water plane to avoid clipping with the surface.")]
        [SerializeField] private float surfaceYOffset = 0.1f;

        private bool debugBlockTick = false;


        public void SetWaterBounds(Collider bounds)
        {
            if (bounds == null)
            {
                return;
            }

            SetWaterBounds(bounds.bounds);
        }

        public void SetWaterBounds(Bounds bounds)
        {
            waterBounds = bounds;
            hasWaterBounds = true;

            useFlatWater = bounds.size.y <= Mathf.Max(0.01f, flatWaterThreshold);
            if (useFlatWater)
            {
                waterY = bounds.center.y;
                minY = waterY;
                maxY = waterY;
            }
            else
            {
                minY = bounds.min.y;
                maxY = bounds.max.y;

                if (minY + swimDepth > maxY - swimDepth)
                {
                    useFlatWater = true;
                    waterY = bounds.center.y;
                    minY = waterY;
                    maxY = waterY;
                }
            }

            targetPosition = GetRandomPointInsideBounds();
        }

        private void Start()
        {
            animator = GetComponent<FishAnimator>();

            //Debug.Log("Fisch spawned: " + _catchData.name);
            if (randomizeBehaviorOnStart)
            {
                behaviorType = GetRandomBehavior();
            }

            targetPosition = GetRandomPointInsideBounds();
            deltaTimeMultiply = 1.0f;

            FindAndSetupMovementPattern();

            EnableFlagDebug(false);
        }


        private MovementPatternWithEvents pattern = null;
        private RotationPattern rotationPattern = null;
        private VerticalLerp verticalLerp = null;

        private void FindAndSetupMovementPattern()
        {
            if (pattern == null)
            {
                pattern = new MovementPatternWithEvents(gameObject);
                SetupMovementEvents(pattern);

                //set time to stay at buoy
                pattern.SetTimeDelayAfterMaxTime(timeAtBuoy);
        
            }

            if (rotationPattern == null)
            {
                rotationPattern = new RotationPattern(this.gameObject);
            }

            if (verticalLerp == null)
            {
                verticalLerp = new VerticalLerp();
            }


        }

        // ----  event setup here ! ---- > derive and add in subclass
        protected virtual void SetupMovementEvents(MovementPatternWithEvents patternIn)
        {
            if (patternIn != null)
            {
                MovementEvent event1 = new MovementEvent(0.5f, "event 1");
                MovementEvent event2 = new MovementEvent(0.6f, "event 2");
                MovementEvent event3 = new MovementEvent(0.7f, "event 3");
                patternIn.AddEvent(event1);
                patternIn.AddEvent(event2);
                patternIn.AddEvent(event3);

            }
        }











        private void Update()
        {
            if (!isAttachedToBuoy)
            {
                FindBait();
                TryReplaceClosestFishOnBait();
            }
            MoveFish(); //is called!
            
            //ok
            //Debug.Log("FISH_UPDATE");
            
            
            
            
        }

        // --- NEW REEL CONDITION ---
        public bool CanBeReeledInByPatternScalar()
        {
            if (HasMovementPattern())
            {
                float t = pattern.GetScalarT();
                if (t >= catchScalarTo1)
                {
                    //Debug.Log("Catch By scalar valid: " + t + " of 1.0f");
                    return true;
                }
                else
                {
                    //be scared and move away
                    SwimAwayAndPrepareForDestroy();
                }
            }


            return false;
        }

        public override void NotifyCatchAttemptFailed()
        {
            if (IsCaught())
            {
                return;
            }
            SwimAwayAndPrepareForDestroy();
        }

        //needed for mini game behaiviour
        public override void NotifyMiniGameStarted()
        {
            state = EFishState.EMinigame;
            if(animator != null)
            {
                animator.SetFishCatching();
            }
            
            Debug.Log("FISH MINIGAME STARTED");
        }

        public override void NotifyMiniGameFinished(bool sucessFull)
        {
            if (sucessFull)
            {
                state = EFishState.ECaught;
                if (animator != null)
                {
                    animator.SetFishCatched();
                }
                return;
            }
            else
            {
                if (animator != null)
                {
                    animator.SetFishSwimmingFromBeingCatching();
                }
                SwimAwayAndPrepareForDestroy();
            }
        }





        protected override void OnObjectCaught()
        {
            base.OnObjectCaught();
            //Debug.Log("Fisch gefangen!");
            CollectedFishUI.Instance?.Add(1);
        }

        private void FindBait()
        {
            if (currentBait == null)
            {
                foreach (FishingBuoy buoy in Object.FindObjectsByType<FishingBuoy>())
                {
                    if (buoy == null || !buoy.IsThrown())
                    {
                        continue;
                    }
                    currentBait = buoy;
                    return;
                }
            }

        }

        private void SwimAwayAndPrepareForDestroy()
        {
            if (catchAttemptFailed)
            {
                return;
            }
            state = EFishState.ECatchAttemptFailed;

            deltaTimeMultiply = 2.0f;
            catchAttemptFailed = true;
            pattern.SetEventsEnabled(false); //disable ankündigungs event
            pattern.DisableTimeDelayAfterMaxTime();

       

            rotationPattern.ResetTarget();
            pattern.ResetWithRotationUpdate(transform.rotation);

            StartCoroutine(DestroyRoutine(timeToDestroy));
        }





        private void TryReplaceClosestFishOnBait()
        {
            if (currentBait)
            {
                currentBait.TryReplaceTargetFish(this);
            }
        }

        public bool IsCaught()
        {
            return state == EFishState.ECaught;
        }


        private void MoveFish()
        {

            //ApproachBait()
            if (!debugBlockTick)
            {
                if(state == EFishState.ECaught)
                {
                    return;
                }

                if (state == EFishState.EDefault || state == EFishState.ECatchAttemptFailed)
                {
                    //TickMoveRotation(); //disable on vr: bricked!
                    TickMoveOnPattern();

                    /// ----- PROBLEM ------
                    //bricks movement of fish RN! Do not touch!
                    // --> since env added: bounds are corruptued somehow
                    //TickMoveAwayFromBounds();
                    /// ----- PROBLEM ------
                }
                if (state == EFishState.EMinigame)
                {
                    TickMoveOnMiniGame();
                }

                /*TickMoveRotation();
                TickMoveOnPattern();
                TickMoveAwayFromBounds();*/
            }

            if (!catchAttemptFailed)
            {
                ClampPositionInsideBounds();
            }


        }

        private void LookAt(Vector3 target, System.String name)
        {
            rotationPattern.UpdateTarget(target);
        }

        private void TickMoveAwayFromBounds()
        {
            if (rotationPattern.HasTarget())
            {
                return;
            }
            if (!IsInsideBounds(transform.position + transform.forward))
            {
                Quaternion q = Quaternion.Euler(0, 170, 0);
                LookAt(transform.position + q * transform.forward * 2.0f, "");
            }
        }








        private void EnableFlagDebug(bool b)
        {
            if (flag != null)
            {
                flag.SetActive(false);
                if (debugCubeEnabled)
                {
                    flag.SetActive(b);
                }
            }
        }





        private bool BaitIsFree()
        {
            return
            currentBait != null &&
            !currentBait.HasCatchableAttached() &&
            currentBait.IsThrown();
        }


        private bool CanApproachBait()
        {
            return
            !catchAttemptFailed &&
            BaitIsFree() &&
            currentBait.IsCatchableTargeted(this) &&
            currentBait.Distance(gameObject) > pattern.SizeDirectionOfPattern();
        }

        private Vector3 BaitPosition()
        {
            if (currentBait)
            {
                return currentBait.GetPosition();
            }
            return new Vector3();
        }








        private void TickMoveRotation()
        {
            if (isAttachedToBuoy)
            {
                return;
            }

            if (rotationPattern != null)
            {
                if (rotationPattern.HasTarget())
                {
                    EnableFlagDebug(true);

                    TickPattern(rotationPattern);
                    //once the rotation has ended, we want to
                    //switch back to the original pattern
                    if (rotationPattern.ReachedEnd())
                    {
                        //debug
                        EnableFlagDebug(false);
                        lockAfterRotation = true;
                        locks = 10;
                        //debug

                        //since we are switching back to the movement pattern
                        //we need to update its start location and rotation values
                        pattern.ResetWithRotationUpdate(transform.rotation);


                    }
                }
            }
        }

        bool lockAfterRotation = false;
        int locks = 10;




        private void TickMoveOnPattern()
        {
            //if we hava a rotation pattern target / busy, dont move along main pattern
            if (rotationPattern.HasTarget())
            {
                //Debug.Log("Rotation Busy: cannot move");
                return;
            }

            //--- WAIT DEBUG ---
            if (lockAfterRotation && locks > 0)
            {
                locks--;
                if (locks <= 0)
                {
                    lockAfterRotation = false;
                }
                return;
            }




            if (HasMovementPattern()) //!CanApproachBait()
            {
                TickPattern(pattern);
                //Debug.Log("FISH_MOVE_PATTERN!");


                //if the pattern is finished we are allowed to look at the buoy
                if (pattern.ReachedEnd())
                {
                    reachedEndCount++;

                    if (CanApproachBait())
                    {
                        LookAt(BaitPosition(), "bait");
                    }

                }
                ReactToCatchAttemptFailed();
            }
        }

        private int reachedEndCount = 0;
        private void ReactToCatchAttemptFailed()
        {
            //if the pattern is finished we are allowed to look at the buoy
            if (reachedEndCount >= 1){
                //wenn das muster einmal ausgeführt wurde, und der fisch nicht gefangen
                //ist - verliert man dann punkte?

                //bool IsAttached(CatchBase other)
                if(buoyReference != null)
                {
                    if (!buoyReference.IsAttached(this))
                    {
                        SwimAwayAndPrepareForDestroy();
                    }
                }
            }
        }




        //tick pattern and copy location and rotation at ticked spline point
        private void TickPattern(MovementPattern anyPattern)
        {
            targetPosition = anyPattern.Tick(Time.deltaTime * deltaTimeMultiply);
            targetPosition = ClampPosition(targetPosition);
            targetPosition = LerpDown(targetPosition);

            SetLocation(targetPosition);
            SetRotation(anyPattern.Rotation());
        }

        private Vector3 LerpDown(Vector3 target)
        {
            if (catchAttemptFailed)
            {
                target = verticalLerp.Tick(GetPosition(), target, Time.deltaTime, -1.0f);
                return target;
            }
            if (!swimUpOnStartFinished)
            {

            }

            return target;
        }


        //soll den fisch anhand des aktuellen mini games bewegen,
        //z.b. von der boye weg
        private void TickMoveOnMiniGame()
        {
            //deprecated! - is inside buoy -> radius minigame
        }



        private bool HasMovementPattern()
        {
            return pattern != null;
        }






        private Vector3 GetRandomPointInsideBounds()
        {
            if (!hasWaterBounds)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-1.0f, 1.0f), 0f, Random.Range(-1.0f, 1.0f));
                return transform.position + randomOffset;
            }

            Bounds bounds = waterBounds;
            float randomY = useFlatWater ? waterY : Random.Range(bounds.min.y, bounds.max.y);

            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                randomY,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            return ValidateHeight(randomPoint);
        }

        private Vector3 ValidateHeight(Vector3 position)
        {
            if (!hasWaterBounds)
            {
                return position;
            }

            if (useFlatWater)
            {
                position.y = waterY + surfaceYOffset;
            }
            else
            {
                position.y = Mathf.Clamp(position.y, minY + swimDepth + surfaceYOffset, maxY - swimDepth + surfaceYOffset);
            }

            return position;
        }

        private void ClampPositionInsideBounds()
        {
            transform.position = ClampPosition(transform.position);
        }

        




        private bool IsInsideBounds(Vector3 pos)
        {

            return
            InRange(pos.x, waterBounds.min.x, waterBounds.max.x) &&
            InRange(pos.z, waterBounds.min.z, waterBounds.max.z);

        }

        private bool InRange(float target, float lower, float higher)
        {
            return target >= lower && target <= higher;
        }
        
        public override void SetLocation(Vector3 pos)
        {
            if (IsValid(pos))
            {
                pos = ClampPosition(pos);
                base.SetLocation(pos);
            }
        }


        public Vector3 ClampPosition(Vector3 position)
        {
            if (!hasWaterBounds)
            {
                return position;
            }

            Bounds bounds = waterBounds;
            position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
            position.z = Mathf.Clamp(position.z, bounds.min.z, bounds.max.z);

            if (useFlatWater)
            {
                position.y = waterY + surfaceYOffset;
            }
            else
            {
                position.y = Mathf.Clamp(position.y, minY + swimDepth + surfaceYOffset, maxY - swimDepth + surfaceYOffset);
            }

            return position;
        }

        private float GetBehaviorSpeedFactor()
        {
            return behaviorType switch
            {
                FishBehaviorType.Calm => 0.85f,
                FishBehaviorType.Curious => 1.0f,
                FishBehaviorType.Shy => 0.9f,
                FishBehaviorType.Aggressive => 1.4f,
                _ => 1.0f,
            };
        }

        private FishBehaviorType GetRandomBehavior()
        {
            return (FishBehaviorType)Random.Range(0, System.Enum.GetValues(typeof(FishBehaviorType)).Length);
        }






        // --- new ---
        public void ReverseBuildTransformForSingleMovementPattern(FBuoyFlags flags)
        {
            FishingBuoy buoy = flags.GetBuoy();
            currentBait = buoy;
            buoy.ReplaceTargetFish(this);
            ReverseBuildTransformForSingleMovementPattern(buoy.GetPosition());
            //debugBlockTick = true;
        }

        public void ReverseBuildTransformForSingleMovementPattern(Vector3 posPatternEnd)
        {
            FindAndSetupMovementPattern();
            //create a transform where the end point of movevment pattern matches the
            //targeted position

            if (pattern != null)
            {
                Vector3 localMovementPatternEnd = pattern.LocalEnd();
                //Debug.Log("Spawn Fish At - localEnd" + localMovementPatternEnd);
                //M = T * R 
                //M^-1 = R^-1 * T^-1
                Quaternion rotationAroundBuoy = RandomRotationYaw();//Quaternion.identity; //create random rotation
                Vector3 t1 = rotationAroundBuoy * -localMovementPatternEnd;

                Vector3 spawnOrigin = posPatternEnd + t1;
                spawnOrigin = UpdatePositionVerticalBelowSurface(spawnOrigin);

                SetRotation(rotationAroundBuoy);
                SetLocation(spawnOrigin);
                pattern.ResetWithRotationUpdate(transform.rotation);

                //DebugHelperLine.DrawLine(spawnOrigin, posPatternEnd, Color.red, 20.0f);



            }
            if (rotationPattern != null)
            {
                rotationPattern.ResetTarget();
            }
        }
        

        private Vector3 UpdatePositionVerticalBelowSurface(Vector3 posIn)
        {
            WaterInteractionManager instance = WaterInteractionManager.Instance;
            if (instance != null)
            {
                posIn = instance.GetBelowSurfacePositon(posIn);
            }
            return posIn;
        }


        private Quaternion RandomRotationYaw()
        {
            /*if (true)
            {
                return Quaternion.identity;
            }*/


            float deg = Random.Range(0f, 180f); // float inkl. 10.Math.

            Quaternion q = Quaternion.Euler(0, deg, 0);
            return q;
        }
    }

}