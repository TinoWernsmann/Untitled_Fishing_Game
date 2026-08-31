

using UnityEngine;
using Core.Game.Entity;
using Game.FishingRod;
using Core.Catching;
using System;
using System.Collections.Generic;
using System.Linq;
using Game.FishingRod.transfer;
using Game.Player;



namespace Game.Fishing //use this to have include paths like in cpp!, includes all classes!
{

    public class FishingRod : EntityBase
    {
        float reelInDistanceFlag = 0.3f; //10 cm

        //80cm hat sich als wert bewährt.
        float snapBuoyDistance = 0.8f;

        bool bAutoresetBuoyOnMinigameFinish = true;

        float sizeTangentScalarForBSpline = 0.3f; //should be a value between max 0 and 0.5 - 1.0f

        public GameObject buoyPrefab;

        private GameObject buoy = null;

        //needed for boy reference point
        public Transform tip = null;

        public GameObject fishingLinePrefab = null;

        //to seperate lines for to top and to buoy bezier
        private FishingLine lineToBuoy = null;
        private FishingLine lineToTip = null;


        private bool isThrown = false;

        [Header("Audio")]
        public AudioClip castThrowClip;
        public bool playCastSound = true;

        [Range(0f, 1f)]
        public float castSoundVolume = 0.6f;

        private AudioSource castAudioSource;

        private List<Transform> schnurHalterList;
        public String schnurHalterTag = "SchnurHalter";
        public float orthogonalPushBetweenPoints = 0.05f;

        private FBuoyFlags flagPackage;
        

        protected override void Start()
        {
            initBuoyOnStart();
            initLineOnStart();

            schnurHalterList = findTransformsWithGameObjectTag(schnurHalterTag);
            Debug.Log("Halter: " + schnurHalterList.Count);
        }

        

        public void DispatchPlayerCircleDecal(PlayerCircleTarget decal)
        { 
            FishingBuoy buoyPtr = GetBuoy();
            if (buoyPtr)
            {
                buoyPtr.DispatchPlayerCircleDecal(decal);
            }
        }




        void initBuoyOnStart()
        {
            buoy = null;
            if (buoyPrefab != null)
            {
                buoy = Instantiate(buoyPrefab);
                FishingBuoy script = GetBuoy();
                if (script)
                {
                    script.setParentRodReference(this.gameObject);
                }
            }
            CreateBuoyFlags();
        }
        
        private void CreateBuoyFlags()
        {
            if (flagPackage == null)
            {
                flagPackage = new FBuoyFlags();
            }
            if (buoy)
            {
                flagPackage.Update(GetBuoy());
            }
        }


        void initLineOnStart()
        {
            if (fishingLinePrefab != null)
            {
                GameObject instance = Instantiate(fishingLinePrefab);
                if (instance)
                {
                    lineToBuoy = instance.GetComponent<FishingLine>();
                }

                instance = Instantiate(fishingLinePrefab);
                if (instance)
                {
                    lineToTip = instance.GetComponent<FishingLine>();
                    lineToTip.Enable(true);
                }
            }
        }
        //throws the buoy with a power factor
        public void CastBuoy(float power, Vector3 dir)
        {
            if (isThrown)
            {
                return;
            }
            isThrown = true;
            lineToBuoy.Enable(true);
            PlayCastSound();
            /*if (!IsReeledIn())
            {
                //cant cast buoy if not reeled in.
                Debug.Log("Buoy Apply Impulse IS REELED IN");
                return;
            }*/

            FishingBuoy buoyscript = GetBuoy();
            if (buoyscript)
            {
                //Debug.Log("Buoy Apply Impulse " + dir + " " + power);
                buoyscript.ApplyImpulse(dir, power);
                buoyscript.WaitForGroundHit();
                buoyscript.SetThrown(true);
                Manager.Haptics.HapticManager.Instance?.PlayCastHaptics();
            }
        }

        //reels the buoy with a power factor
        public void ReelBuoy(float power)
        {
            if(Mathf.Abs(power) <= 0.00001f)
            {
                return;
            }

            //Debug.Log("Try Reel in");
            if (!isThrown)
            {
                return;
            }
            if (IsReeledIn())
            {
                isThrown = false;
                return;
            }

            //ground cast, move towards player
            //ALWAYS GROUNDED ON WATER (?)
            if (!IsReeledIn())
            {

                FishingBuoy buoyscript = GetBuoy();
                if (buoyscript)
                {
                    buoyscript.TargetToCatched();
                    
                    buoyscript.RemoveVelocity();
                    Vector3 dirScaled = DirectionToBuoy();
                    power = SlowDownPowerIfBuoyIsClose(dirScaled, power);
                    ResetBuoyIfCloseEnough(dirScaled);
                    ResetBuoyOnMinigameMarkedFinished();

                    Vector3 dragDirection = DragDirection(dirScaled);
                    ApplyDragDirection(dragDirection, power);


                    //buoyscript.ApplyImpulse(dragDirection, power);
                    //Debug.Log(" Reel in");

                    //Debug.Log("DISTANCE BUOY " + DirectionToBuoy().magnitude);

                    OnBuoyIsReeledIn();
                }
                //drag towards xy if not below tip

                //drag towards tip if pependicular 
            }
        }

        private float SlowDownPowerIfBuoyIsClose(Vector3 vecToBuoy, float power)
        {
            float distance = vecToBuoy.magnitude;
            if (distance < 1.0f)
            {
                power = 1.0f; //0.2f
            }
            return power;
        }

        //macht das rinziehen etwas einfacher
        private void ResetBuoyIfCloseEnough(Vector3 vecToBuoy)
        {
            FishingBuoy buoyscript = GetBuoy();
            if (buoyscript)
            {
                if (buoyscript.SnapAllowed())
                {
                    vecToBuoy.y = 0.0f;
                    float distance = vecToBuoy.magnitude;
                    if (distance < snapBuoyDistance)
                    {
                        //ResetBuoyPosition();
                        SetBuoyReeledIn();
                    }
                }

            }
        }

        private void ResetBuoyOnMinigameMarkedFinished()
        {
            if (!bAutoresetBuoyOnMinigameFinish)
            {
                return;
            }
            FishingBuoy buoyscript = GetBuoy();
            if (buoyscript)
            {
                if (buoyscript.ReadMiniGameMarkedFinishedOnce())
                {
                    //ResetBuoyPosition();
                    SetBuoyReeledIn();
                } 
            }
        }




        private void ApplyDragDirection(Vector3 dragDirection, float power)
        {   
            FishingBuoy buoyscript = GetBuoy();
            if (buoyscript)
            {
                // optional: leichte Gravitätsverstärkung statt harte Nullung
                Vector3 gravityAssist = Vector3.down * 0.2f;
                buoyscript.ClampVerticalIfMiniGameIsRunning(gravityAssist, out gravityAssist);


                Vector3 finalDir = (dragDirection + gravityAssist).normalized;
                buoyscript.ApplyImpulse(finalDir, power);
            }
        }

        private Vector3 DragDirection(Vector3 dirScaled)
        {
            //if the mini game is running we only pull horizontally
            FishingBuoy buoy = GetBuoy();
            if (buoy)
                buoy.ClampVerticalIfMiniGameIsRunning(dirScaled, out dirScaled);




            Vector3 dragDirection = (dirScaled * -1.0f).normalized;

            /*float maxDist = 10.0f;
            float forceByDistance = distance / maxDist;
            power *= forceByDistance;*/

            float slow = 0.5f;
            if (Vector3.Dot(dirScaled.normalized, Vector3.down) > 0.7f) //45 grad
            {
                dragDirection.y = Math.Abs(dragDirection.y);
                dragDirection.z *= slow; //no horizontal drag
                dragDirection.x *= slow; //no horizontal drag
            }
            else
            {
                dragDirection.y = 0.0f; //no vertical drag
            }
            return dragDirection;
        }   
        

        public void SetColorDefault()
        {
            if (lineToBuoy)
            {
                lineToBuoy.ResetColor();
            }
            if (lineToTip)
            {
                lineToTip.ResetColor();
            }
        }

        public void SetColorCatchFailed()
        {
            if (lineToBuoy)
            {
                lineToBuoy.SetColorRed();
            }
            if (lineToTip)
            {
                lineToTip.SetColorRed();
            }
        }

        public void SetColorCatchSuccess()
        {
            if (lineToBuoy)
            {
                lineToBuoy.SetColorGreen();
            }
            if (lineToTip)
            {
                lineToTip.SetColorGreen();
            }
        }




        private void OnBuoyIsReeledIn()
        {

            SetColorDefault();

            FishingBuoy buoyscript = GetBuoy();
            if (buoyscript)
            {
                //if the minigame is still running, we cannot reel the buoy fully and
                //catch the fish!
                if (buoyscript.MiniGameRunning())
                {
                    return;
                }



                if (DirectionToBuoy().magnitude <= reelInDistanceFlag)
                {
                    /*Debug.Log(" Reel in FINISH");
                    if (buoyscript.HasCatchableAttached())
                    {
                        CatchBase attached = buoyscript.GetAttachedCatchable();
                        if (attached != null)
                        {
                            buoyscript.DetachCurrentCatchable();
                            attached.Catch();
                        }
                    }

                    isThrown = false;
                    buoyscript.SetThrown(false);
                    ResetBuoy();
                    ResetBuoy();
                    ResetBuoy();
                    lineToBuoy.Enable(false);*/
                    SetBuoyReeledIn();
                }
            }
        }
        
        private void SetBuoyReeledIn()
        {
            FishingBuoy buoyscript = GetBuoy();
            if (buoyscript)
            {
                Debug.Log(" Reel in FINISH");
                if (buoyscript.HasCatchableAttached())
                {
                    CatchBase attached = buoyscript.GetAttachedCatchable();
                    if (attached != null)
                    {
                        buoyscript.DetachCurrentCatchable();
                        attached.Catch();
                    }
                }

                isThrown = false;
                buoyscript.SetThrown(false);
                //buoyscript.ShowLure(); //show lure again on reeled in.
                ResetBuoy();
                ResetBuoy();
                ResetBuoy();
                lineToBuoy.Enable(false);
            }
        }



        /// <summary>
        /// returns if the line is fully reeled in
        /// </summary>
        /// <returns></returns>
        bool IsReeledIn()
        {
            FishingBuoy found = GetBuoy();
            if (found)
            {
                return GetBuoy().Distance(this.gameObject) < reelInDistanceFlag;
            }
            return true;
        }

        //buoy script reference
        private FishingBuoy GetBuoy()
        {
            if (buoy != null)
            {
                FishingBuoy buoyScript = buoy.GetComponent<FishingBuoy>();
                return buoyScript;
            }
            return null;
        }


        /// <summary>
        /// returns the current Buoy Flags.
        /// </summary>
        /// <returns></returns>
        public FBuoyFlags GetCurrentBuoyState()
        {
            if(flagPackage == null){
                CreateBuoyFlags();
            }

            flagPackage.Update(GetBuoy());
            flagPackage.SetReeledIn(IsReeledIn());
            return flagPackage;
        }

        private Vector3 DirectionToBuoy()
        {
            FishingBuoy buoyscript = GetBuoy();
            if (buoyscript)
            {
                Vector3 buoyPos = buoyscript.GetPosition();
                Vector3 start = StartPositionOfLine();

                //AB = B - A
                return buoyPos - start;

            }
            return new Vector3();
        }

        private Vector3 StartPositionOfLine()
        {
            if (tip != null)
            {
                return tip.position;
            }
            return transform.position;
        }

        private Vector3 EndPositionOfLine()
        {
            FishingBuoy found = GetBuoy();
            if (found)
            {
                return found.GetPosition();
            }
            return StartPositionOfLine(); //fallback.
        }
        void Update()
        {
            if (!isThrown)
            {
                ResetBuoy();
            }

            if (playCastSound && castAudioSource != null && castAudioSource.isPlaying)
            {
                FishingBuoy buoyscript = GetBuoy();
                if (buoyscript != null && buoyscript.IsGroundedFlag())
                {
                    StopCastSound();
                }
            }

            UpdateLines();
            
        }

        void ResetBuoy()
        {
            FishingBuoy found = GetBuoy();
            if (found)
            {
                ResetBuoyPosition();
                found.RemoveVelocity();
                found.SetThrown(false);
            }
        }

        void ResetBuoyPosition()
        {
            FishingBuoy found = GetBuoy();
            if (found)
            {
                found.SetPosition(StartPositionOfLine());
            }
        }




        private void PlayCastSound()
        {
            if (!playCastSound)
            {
                return;
            }

            castAudioSource = Manager.Audio.AudioManager.GetOrCreateInstance().PlaySoundOn(
                gameObject,
                castThrowClip,
                castSoundVolume,
                false,
                true,
                null
            );

            if (castAudioSource == null)
            {
                Manager.Audio.AudioManager.GetOrCreateInstance().PlayRodThrowSfx(castSoundVolume);
            }
        }

        private void StopCastSound()
        {
            if (castAudioSource != null)
            {
                castAudioSource.Stop();
            }
        }

        private void UpdateLines()
        {
            UpdateLineAtRod();
            UpdateLineToBuoy();
        }

        private void UpdateLineAtRod()
        {
            if (lineToTip)
            {

                lineToTip.RebuildWithOrthogonalExtraPoints(
                    schnurHalterList,
                    sizeTangentScalarForBSpline,
                    orthogonalPushBetweenPoints
                );
                //Debug.Log(" Line To Tip Update");
                
            }
        }

        private void UpdateLineToBuoy()
        {
            if (lineToBuoy != null)
            {
                FishingBuoy buoy = GetBuoy();
                bool ground = true;
                //buoy is in air, no middle point projection
                if (!buoy.IsGrounded())
                {
                    ground = false;
                }

                lineToBuoy.RebuildMiddleProjected(
                    StartPositionOfLine(),
                    EndPositionOfLine(),
                    sizeTangentScalarForBSpline,
                    ground
                );
            }
        }
        //halter finden
        public List<Transform> findTransformsWithGameObjectTag(String name)
        {
            List<Transform> outList = new List<Transform>();
            Transform[] array = GetComponentsInChildren<Transform>()
                .OrderBy(t => t.name)
                .ToArray();

            foreach (Transform t in array)
            {
                GameObject obj = t.gameObject;
                if (obj.name.Contains(name))
                {
                    outList.Add(t);
                }
            }

            foreach (Transform t in outList)
            {
                //Debug.Log("Transform: " + t.gameObject.name);
            }
            return outList;
        }
    }
}