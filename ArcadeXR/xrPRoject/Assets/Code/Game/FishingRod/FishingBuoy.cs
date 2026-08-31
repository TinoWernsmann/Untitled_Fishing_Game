using UnityEngine;
using Core.Game;
using Core.Game.Entity;
using System.Collections.Generic;
using Core.Catching;
using Game.FishingRod.Minigame;
using Game.FishingRod.transfer;
using Game.Player;


namespace Game.FishingRod //use this to have include paths like in cpp!, includes all classes!
{

    public class FishingBuoy : EntityFloatingBase
    {

        [SerializeField] private GameObject splashPrefab = null;
        [SerializeField] private float splashDestroyAfterSeconds = 5f;
        private bool splashPlayed = false;


        //--- USE WATER HEIGHT FROM RENDER OR RAYCAST --- 
        private bool useRaycasterHeight = false;
        
        //--- USE WATER HEIGHT FROM RENDER OR RAYCAST --- 

        private GameObject parentRod = null;

        public GameObject lureSwitcher = null;


        //current attach fish
        private CatchBase attachedCatchable = null;
        private bool isThrown = false;
        private bool groundedFlag = false;
        private CatchBase targetFish = null;



        private bool bMiniGameMarkedFinished = false;
        private bool hasPlayedCatchSuccessSound = false;
        private bool hasPlayedBiteSound = false;

        private RadiusMinigame minigame = null;
        

        private PlayerCircleTarget targetDecal = null;

        public int DepthLevel()
        {
            if (FishingRodParameters.Instance != null)
            {
                return FishingRodParameters.Instance.depthLevel;
            }
            return 1; //to be changed
        }

        




        
        public void DispatchPlayerCircleDecal(PlayerCircleTarget decal)
        {
            targetDecal = decal;
        }

        


        public void setParentRodReference(GameObject other)
        {
            parentRod = other;
        }

        public void SetThrown(bool thrown)
        {
            if(isThrown != thrown && thrown)
            {
                TryShowLure();
            }


            isThrown = thrown;
            if (!isThrown)
            {
                ClearTargetFish();
                
            }
            groundedFlag = false;
            splashPlayed = false;
            
        }

        private void PlaySplashOnce()
        {
            if (splashPlayed || splashPrefab == null)
                return;

            splashPlayed = true;

            GameObject splash = Instantiate(
                splashPrefab,
                GetPosition(),
                Quaternion.identity
            );
            //Debug.Log("SPLASH");

            Destroy(splash, splashDestroyAfterSeconds);
        }

        private void PlayBiteSound()
        {
            if (gameObject == null || hasPlayedBiteSound)
            {
                return;
            }

            hasPlayedBiteSound = true;
            Debug.Log("FishingBuoy: FishBite_Placeholder triggered");
            Manager.Audio.AudioManager.GetOrCreateInstance().PlayFishBiteSfx();
            Manager.Audio.AudioManager.GetOrCreateInstance().LogAudioStatus();
        }

        public bool IsThrown()
        {
            return isThrown;
        }

        public bool HasTargetFish()
        {
            return targetFish != null;
        }

        public CatchBase GetTargetFish()
        {
            return targetFish;
        }

        public void SetTargetFish(CatchBase fish)
        {
            targetFish = fish;
        }

        public void ClearTargetFish()
        {
            targetFish = null;
        }

        public void TryReplaceTargetFish(CatchBase item)
        {
            if (item != null)
            {
                if (targetFish != null)
                {
                    if (Distance(item) < Distance(targetFish))
                    {
                        ReplaceTargetFish(item);
                    }
                }
                else
                {
                    ReplaceTargetFish(item);
                }
            }
        }

        public void ReplaceTargetFish(CatchBase item)
        {
            if (item != null)
            {
                SetTargetFish(item);
            }
        }


        private float Distance(CatchBase item)
        {
            return Distance(item, false);
        }

        private float Distance(CatchBase item, bool distance2D)
        {
            if (item != null)
            {
                Vector3 a = item.GetPosition();
                Vector3 b = GetPosition();

                if (distance2D)
                {
                    a.y = 0.0f;
                    b.y = 0.0f;
                }


                return Vector3.Distance(a,b);
            }
            return float.MaxValue;
        }



        public Transform tipOfBoy = null;


        private bool waitForGroundHit = false;

        public Vector3 GetPosition()
        {
            if (tipOfBoy == null)
            {
                return transform.position;
            }
            return tipOfBoy.position;
        }

        

        public void SetPosition(Vector3 pos)
        {
            gameObject.transform.position = pos;
        }

        public float Distance(GameObject other)
        {
            return Distance(other.transform);
        }

        public float Distance(Transform other)
        {
            return Distance(other.position);
        }

        public float Distance(Vector3 other)
        {
            return Vector3.Distance(GetPosition(), other);
        }


        protected override void Start()
        {
            base.Start();
            FindRigidBody();
            caster = new Raycaster();
        }


        protected override void Update()
        {
            base.Update();
            stopMovementIfGroundHit();
            UpdatePositionBasedOnWaterHeight();
            LerpFaceDownWards();
            UpdateMiniGame();
        }










        void stopMovementIfGroundHit()
        {




            //if (waitForGroundHit)
            if (waitForGroundHit)
            {
                //Debug.Log("Test Ground Buoy " + gameObject.transform.position);
                if (SnapToGround(0.2f))
                {
                    PlaySplashOnce();
                    groundedFlag = true;
                    waitForGroundHit = false;
                    //Debug.Log("Test Ground Buoy HIT! HIT! HIT!" + gameObject.transform.position);
                    RemoveVelocity();

                }

            }
            else
            {
                RemoveVelocity();
            }

            if (groundedFlag)
            {
                //Debug.DrawLine(GetPosition(), GetPosition() + Vector3.up, Color.red, 20.0f);
            }
        }
        
        private void UpdatePositionBasedOnWaterHeight()
        {
            //there for we are grounded
            if (!waitForGroundHit && groundedFlag)
            {
                //using water shader height
                if (!useRaycasterHeight)
                {
                    //ApplyLatestWaterLocation();
                    ApplyLatestWaterLocationVerticalOnly(true);
                }
            }
        }






        public bool IsGroundedFlag()
        {
            return groundedFlag;
        }

        public bool IsGrounded()
        {
            float sizeRay = .1f;
            return IsGrounded(sizeRay);
        }

        public bool IsGrounded(float sizeRay)
        {


            // TODO - CHECK FROM LATEST HEIGHT


            if (!useRaycasterHeight)
            {
                return InRangeVertical(sizeRay, latestWaterGroundPosition);
            }


            return caster.bPerformRaycastHit(
                gameObject.transform.position + new Vector3(0, sizeRay * 2.0f, 0),
                Vector3.down, //down
                sizeRay * 3.0f,
                GetAllColliders()
            );
        }

        private bool SnapToGround(float range)
        {
            //use raycaster


            //use shader height
            if (useRaycasterHeight)
            {
                Vector3 hitpos;
                float sizeRay = 100;
                Vector3 ownPos = gameObject.transform.position;
                if (caster.bPerformRaycastHit(
                    ownPos + Vector3.up * sizeRay * 0.5f,
                    Vector3.down,
                    sizeRay,
                    GetAllColliders(),
                    out hitpos))
                {
                    latestWaterGroundPosition = hitpos;
                    if(InRangeVertical(range, hitpos))
                    {
                        SetPosition(hitpos);
                        return true;
                    }
                }
                return false;
            }
            else
            {
                if(InRangeVertical(range, latestWaterGroundPosition))
                {
                    SetPosition(latestWaterGroundPosition);
                    return true;
                }
            }
            return false;
        }

        




        public override Collider[] GetAllColliders()
        {
            List<Collider> result = new List<Collider>();

            result.AddRange(base.GetAllColliders());

            EntityBase casted = parentRod.GetComponent<EntityBase>();
            if (casted != null)
            {
                result.AddRange(casted.GetAllColliders());
            }

            Collider parentCol = parentRod.GetComponent<Collider>();
            if (parentCol != null)
                result.Add(parentCol);

            return result.ToArray();
        }

        public void WaitForGroundHit()
        {
            waitForGroundHit = true;
        }

        private void LerpFaceDownWards()
        {
            float deltaTime = Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                5.0f * deltaTime);
        }
        public bool HasCatchableAttached()
        {
            return attachedCatchable != null;
        }

        public bool IsCatchableTargeted(CatchBase item)
        {
            if (targetFish != null)
            {
                return item == targetFish;
            }
            return false;
        }


        public CatchBase GetAttachedCatchable()
        {
            return attachedCatchable;
        }

        public void DetachCurrentCatchable()
        {
            ClearTargetFish();
            if (HasCatchableAttached())
            {
                attachedCatchable.SetColliderEnabled(true);
                attachedCatchable.DetachFromParent();
                attachedCatchable.MarkAttachedToBuoy(false);
                attachedCatchable = null;
            }
        }

        public void AttachCatchable(CatchBase item)
        {
            //Debug.Log("FISH ATTACHED");
            ClearTargetFish();
            DetachCurrentCatchable();
            HideLure();
            if (item != null)
            {
                attachedCatchable = item;
                attachedCatchable.SetColliderEnabled(false);
                attachedCatchable.MarkAttachedToBuoy(true);
                AttachComponentLocalSpace(gameObject.transform, item.gameObject);
            }
        }

        /// <summary>
        /// trys to catch the current fish by distance and 
        /// in time scalar of pattern
        /// </summary>
        public void TargetToCatched()
        {
            if (!HasCatchableAttached() && TargetFishInRange())
            {
                AttachCatchable(targetFish);
                targetFish = null;
                

                if (attachedCatchable.IsCaught())
                {
                    return;
                }



                //since we have the fish attached now:
                //we launch the minigame
                //and tell the fish to move accordingly
                PlayBiteSound();
                ResetMiniGame();
                attachedCatchable.NotifyMiniGameStarted();
                Manager.Haptics.HapticManager.Instance?.PlayBiteHaptics();
                Manager.Haptics.HapticManager.Instance?.StartFishFightHaptics();

            }
        }

        private void ReattachCatched()
        {
            CatchBase copyRef = attachedCatchable;
            DetachCurrentCatchable();
            AttachCatchable(copyRef);

        }
        
        public bool IsAttached(CatchBase other)
        {
            return attachedCatchable != null && attachedCatchable == other;
        }



        private bool TargetFishInRange()
        {
            //both flags must be executed
            bool flagA = TargetFishInRangeByDistance();
            bool flagB = TargetFishInRangeByScalar();
            bool result = flagA && flagB;

            //if the attempt failed, the fish must swim away in any case
            if (!result && targetFish != null)
            {
                targetFish.NotifyCatchAttemptFailed();
            }


            return result;
        }

        private bool TargetFishInRangeByDistance()
        {
            if (targetFish != null)
            {
                //horizontal only!
                return Distance(targetFish, true) < targetFish.MaxDistanceCatch();
            }
            return false;
        }

        private bool TargetFishInRangeByScalar()
        {
            if (targetFish != null)
            {
                Fish casted = (Fish)targetFish;
                if (casted)
                {
                    return casted.CanBeReeledInByPatternScalar();
                }
            }
            return false;
        }



        public void RemoveReference(CatchBase baseItem)
        {
            if (targetFish == baseItem)
            {
                targetFish = null;
            }
            if (baseItem == attachedCatchable)
            {
                DetachCurrentCatchable();
            }
        }


        public GameObject debugRadiusPrafab = null;
        private void ResetMiniGame()
        {
            if (parentRod != null)
            {
                

                if (minigame == null){
                    

                    Vector3 postionBuoy = GetPosition();
                    postionBuoy = WaterInteractionManager.Instance.GetSurfacePosition(postionBuoy);



                    minigame = new RadiusMinigame();
                    //minigame = new RadiusMinigame();
                    minigame.Reset(
                        //MiniGameRadius,
                        //MiniGameTime,
                        FishingRodParameters.Instance,
                        //GetPosition(),
                        postionBuoy,
                        parentRod.gameObject.transform.position,
                        debugRadiusPrafab,
                        !HasPlayerDecal() //if no decal: show
                    );
                    bMiniGameMarkedFinished = false;
                    hasPlayedCatchSuccessSound = false;
                    hasPlayedBiteSound = false;
                }
               

                
                
            }

        }



        public void UpdateMiniGame()
        {
            //CreateMiniGameIfNeeded();
            UpdateDecalBasedOnMiniGame();
            if (minigame != null && attachedCatchable != null)
            {

                //Debug.Log("Tick Buoy Game");


                bool isFinished = minigame.TickGame(attachedCatchable.gameObject);
                UpdateGameObjectMovementOnMinigame();


                if (isFinished)
                {
                    //sobald das spiel fertig ist, wird es beendet.
                    FinishMiniGame();

                    minigame.Destroy();
                    minigame = null;
                    UpdateDecalVisibility();
                    bMiniGameMarkedFinished = true;
                }
            }
        }

        public bool ReadMiniGameMarkedFinishedOnce()
        {
            bool copy = bMiniGameMarkedFinished;
            bMiniGameMarkedFinished = false;
            return copy;
        }

        private void UpdateDecalBasedOnMiniGame()
        {
            UpdateDecalVisibility();

            //Debug.Log("Buoy::UPDATE CIRCLE DECAL EXTERNAL - TRY");
            if (targetDecal)
            {
                //external position update klappt noch nicht!
                //wieso unklar!

                bool targetPosOverride = minigame != null;
                targetDecal.EnableManualTargetPosition(targetPosOverride);
                //Debug.Log("RadiusMiniGame::UPDATE CIRCLE DECAL EXTERNAL A");

                if (minigame != null)
                {
                    //Debug.Log("RadiusMiniGame::UPDATE CIRCLE DECAL EXTERNAL B");
                    minigame.UpdateCircleDecal(targetDecal);
                }
            }
            
        }
        
        private void UpdateDecalVisibility()
        {
            SetDecalVisible(minigame != null);
        }

        private bool HasPlayerDecal()
        {
            return targetDecal != null;
        }
        
        private void SetDecalVisible(bool flag)
        {
            if (targetDecal){
                targetDecal.SetCircleVisible(flag);
                if (!flag)
                {
                    //Debug.Log("BUOY HIDE DECAL!");
                }
            }
        }


        //updates the movement of the fish and buoy based on the minigame data
        private void UpdateGameObjectMovementOnMinigame()
        {
            if (minigame != null)
            {
                minigame.UpdateGameObjectMovement(attachedCatchable);

                //update buoy location - based on fish location
                //since its moving away from radius
                Vector3 pos = attachedCatchable.GetPosition();

                //klappt aber ist das richtig?
                SetLocation(pos);
                SetRotation(Quaternion.identity);
            }

        }

        //sobald das spiel fertig ist, wird es beendet.
        //das sorgt für ein fisch bleibt dran oder fisch haut ab
        //je nach dem ob man zeitlich gut abgeschnitten hat.
        private void FinishMiniGame()
        {
            Debug.Log("Tick Buoy Game FINISH!");
            Manager.Haptics.HapticManager.Instance?.StopFishFightHaptics();
            //fish pop / notify rod for fish catched / or released
            //based on time in radius spent

            float inRadiusSpentScalar = minigame.TimeSpentInRadiusFracFullTime();
            bool isCaughtByTime = inRadiusSpentScalar >= 0.9f;
            CatchData data = attachedCatchable.GetData();

            if (isCaughtByTime)
            {
                //catch allowed
                ReattachCatched();

                if (!hasPlayedCatchSuccessSound)
                {
                    hasPlayedCatchSuccessSound = true;
                    Debug.Log("FishingBuoy: FishCatched_Placeholder triggered");
                    Manager.Audio.AudioManager.GetOrCreateInstance().PlayFishCaughtSfx();
                    Manager.Audio.AudioManager.GetOrCreateInstance().LogAudioStatus();
                }

                if (data != null)
                {
                    data.SetScoreIncrease(minigame.ScoreIncreaseBasedOnTimeSpentInRadius(), true);
                }
                attachedCatchable.NotifyMiniGameFinished(isCaughtByTime);

                
                Game.Fishing.FishingRod rodRef = FindRod();
                if (rodRef){
                    rodRef.SetColorCatchSuccess();
                }


            }
            else
            {
                hasPlayedBiteSound = false;
                Debug.Log("FishingBuoy: RodCut_Placeholder triggered");
                Manager.Audio.AudioManager.GetOrCreateInstance().PlayRodCutSfx();
                Manager.Haptics.HapticManager.Instance?.PlayLineBreakHaptics();
                Manager.Audio.AudioManager.GetOrCreateInstance().LogAudioStatus();

                //remove -5*fish points if catch failed
                if (data != null)
                {
                    float decreaseFactor = -5.0f;
                    data.SetScoreIncrease(decreaseFactor, false);
                }
                attachedCatchable.NotifyParentForCatch(); //manual notify for catch

                attachedCatchable.NotifyMiniGameFinished(isCaughtByTime);
                DetachCurrentCatchable();

                Game.Fishing.FishingRod rodRef = FindRod();
                if (rodRef){
                    rodRef.SetColorCatchFailed();
                }
            }
        }

        
        private Game.Fishing.FishingRod FindRod()
        {
            if (parentRod)
            {
                Game.Fishing.FishingRod rodscript = parentRod.GetComponent<Game.Fishing.FishingRod>();
                return rodscript;
            }
            return null;
        }






        //modified apply impulse to 
        //update minigame for dragging against direction
        public override void ApplyImpulse(Vector3 forceDir, float v)
        {
            if (MiniGameRunning())
            {
                //might be deprecated
                float mass = 1.0f;
                if (rigidbodyPtr)
                {
                    mass = rigidbodyPtr.mass;
                }
                minigame.SetDragResist(forceDir, mass, v);
            }
            else
            {
                base.ApplyImpulse(forceDir, v);
            }



            //deprecated
            /*base.ApplyImpulse(forceDir, v);
            if (minigame != null)
            {
                //might be deprecated
                float mass = 1.0f;
                if (rigidbodyPtr)
                {
                    mass = rigidbodyPtr.mass;
                }
                minigame.SetDragResist(forceDir, mass, v);
            }*/
        }



        public bool SnapAllowed()
        {
            return minigame == null;
        }

        public bool MiniGameRunning()
        {
            return minigame != null;
        }

        public void ClampVerticalIfMiniGameIsRunning(Vector3 varIn, out Vector3 other)
        {
            if (MiniGameRunning())
            {
                varIn.y = 0.0f;
                other = varIn;
                return;
            }
            other = varIn;
        }



        /// <summary>
        /// might be blocked to allow a object to be below the water --> during mini game the fish drags the buoy.
        /// </summary>
        /// <returns></returns>
        protected override bool BlockApplyWaterHeight()
        {
            if (minigame != null)
            {
                return true;
            }
            return base.BlockApplyWaterHeight();
        }




        //on buoy throw: show lure
        private void TryShowLure()
        {
            if (!HasCatchableAttached())
            {
                ShowLure();
            }
        }

        public void ShowLure()
        {
            SetLureVisible(true);
        }

        private void HideLure()
        {
            SetLureVisible(false);
        }
        

        //once fish is attached: hide lure, even if the fish dissappears
        public void SetLureVisible(bool visible)
        {
            FishingBuoyLureSwitcher switcher = GetLureSwitcher();
            if(switcher != null)
            {
                switcher.ShowLure(visible);
            }
        }



        private FishingBuoyLureSwitcher GetLureSwitcher()
        {
            if (lureSwitcher != null)
            {
                return lureSwitcher.GetComponent<FishingBuoyLureSwitcher>();
            }
            return null;
        }


    }
}