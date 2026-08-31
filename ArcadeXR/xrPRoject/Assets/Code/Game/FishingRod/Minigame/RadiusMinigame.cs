


using System;
using UnityEngine;
using Core.Catching;
using util.Quat;
using Core.Game;
using Game.Player;
using Unity.Mathematics;
using Game.FishingRod.transfer;
using Unity.Burst.Intrinsics;



namespace Game.FishingRod.Minigame //use this to have include paths like in cpp!, includes all classes!
{
    public class RadiusMinigame
    {
        private float radius2;
        private float radius;

        //initial time
        private float time;


        private BaseLerp timeLerp = null;

        private Vector3 radiusCenter;
        private Vector3 playerLocation;
        private Vector3 resistVector = new Vector3(0, 0, 0); //drag of playr rod



        /// <summary>
        /// TODO HIER: Minimum Drag Velocity kann variable sein / ausdauer vom Fisch
        /// - noch nicht drüber nachgedacht.
        /// </summary>
        private float minimumDragVelocity = 0.2f;


        //track minigame result
        private float timeSpentInRadius = 0.0f;


        private DebugRadius debugRadiusInstance = null;

        private bool isInRadiusState = true;


        public RadiusMinigame()
        {

        }

        public void Destroy()
        {
            if (debugRadiusInstance)
            {
                debugRadiusInstance.Destroy();
            }
        }

        

        public void Reset(
            FishingRodParameters rodParams,
            Vector3 radiusCenterIn,
            Vector3 playerLocationIn,
            GameObject radiusInstancePrefab = null,
            bool debugRadiusEnabled = false
        )
        {
            //Debug.Log("MINGAME Start Mini Game from Rod Params!  A");
            float _MiniGameRadius = 4.0f;
            float _MiniGameTime = 1.0f;
            if (rodParams != null)
            {
                _MiniGameRadius = rodParams.MiniGameRadius;
                _MiniGameTime = rodParams.MiniGameTime;
                //Debug.Log("MINGAME Start Mini Game from Rod Params!");
            }
            Reset(
                _MiniGameRadius,
                _MiniGameTime,
                radiusCenterIn,
                playerLocationIn,
                radiusInstancePrefab,
                debugRadiusEnabled //if no decal: show
            );

        }





        public void Reset(
            float radiusIn, float timeIn, Vector3 radiusCenterIn, Vector3 playerLocationIn,
            GameObject radiusInstancePrefab, bool debugRadiusEnabled
        )
        {
            UpdateRadius(radiusIn);
            UpdateTime(timeIn);
            radiusCenter = radiusCenterIn;
            playerLocation = playerLocationIn;
            UpdateRadiusInstance(radiusInstancePrefab, debugRadiusEnabled);
            
        }

        private void ShowRadius(bool flag)
        {
            //to be changed to shader

            if(debugRadiusInstance != null)
            {
                debugRadiusInstance.gameObject.SetActive(flag); 
            }
        }



        private void UpdateRadiusInstance(GameObject radiusInstancePrefab, bool debugRadiusEnabled)
        {
            if (!radiusInstancePrefab)
            {
                debugRadiusInstance = null;
                return;
            }
            if (!debugRadiusEnabled)
            {
                if (debugRadiusInstance)
                {
                    debugRadiusInstance.Destroy();
                    debugRadiusInstance = null;
                }
                return;
            }

            
            if (debugRadiusInstance == null)
            {
                debugRadiusInstance = DebugRadius.MakeInstance(radiusInstancePrefab);
            }
            if(debugRadiusInstance != null)
            {
                debugRadiusInstance.SetLocation(radiusCenter);
                
                //Debug.Log("RADIUS SET SCALE " + radius);
                debugRadiusInstance.SetScale(radius, 0.1f, radius);
                ShowRadius(true);
            }
        }


        public void UpdateRadius(float r)
        {
            radius2 = r * r;
            radius = math.abs(r);
            //Debug.Log("RADIUS SET SCALE " + r + " r2 " + r);
        }

        public void UpdateTime(float t)
        {
            time = Math.Abs(t);

            InitLerpIfNeeded();
            timeLerp.Reset();
            timeLerp.timeMax = time;
        }

        public void Reset()
        {
            ShowRadius(false);
            if (timeLerp != null)
            {
                timeLerp.Reset();
            }
            
            
        }

        



        private void InitLerpIfNeeded()
        {
            if (timeLerp == null)
            {
                timeLerp = new BaseLerp();
                UpdateTime(time);
            }
        }



        //todo: wenn false, spiel abbrechen
        public bool TickGame(GameObject other)
        {
            
            InitLerpIfNeeded();
            if (other != null)
            {
                //event time: global
                //time spent in radius: bewertung

                //the time is always ticked.
                //if in range, the time spent in radius is accumulated too
                timeLerp.BaseTick(Time.deltaTime);
                if (IsInRange(other))
                {
                    //update timer
                    timeSpentInRadius += Time.deltaTime;
                    isInRadiusState = true;
                }
                else
                {
                    isInRadiusState = false;
                }



                if (timeLerp.ReachedFlagRead())
                {
                    ShowRadius(false);

                    return true;
                }
            }

            return false;
        }

        public float TimeSpentInRadiusFracFullTime()
        {
            return timeSpentInRadius / time;
        }

        public float ScoreIncreaseBasedOnTimeSpentInRadius()
        {
            float invertedScalar = 1.0f - TimeSpentInRadiusFracFullTime();
            return 1.0f + invertedScalar;
            
            
            /*//von 0 bis 1, näher an 0: besser
            float capped = Mathf.Clamp(invertedScalar, 0.1f, 1.0f);
            float scale = 1.0f / capped;
            //1 / 0.1 = 10 z.b.
            return scale;*/
        }


        private bool IsInRange(GameObject other)
        {
            if (other != null)
            {
                return IsInRange(other.transform.position);
            }
            return false;
        }

        private bool IsInRange(Vector3 other)
        {
            float dist2 = Distance2(other);
            return dist2 <= radius2;
        }

        private float Distance2(Vector3 other)
        {
            //distance = sqrt(sum a_i^2) |^2
            //distance^2 = sum a_i^2
            //distance^2 = a * a //dot Product
            Vector3 toTarget = other - radiusCenter;//AB = B - A
            toTarget = Make2D(toTarget);
            float dist2 = Vector3.Dot(toTarget, toTarget);
            return dist2;
        }

        private Vector3 Make2D(Vector3 other)
        {
            other.y = 0.0f;
            return other;
        }



        // ACHTUNG ---> MOVEMENT DIRECTION MUSS RELATIV ZUM PLAYER SEIN!
        // nicht vom zentrum weg!
        private Vector3 GetMovementLookDirection2D()
        {
            //playerLocation
            Vector3 dir = radiusCenter - playerLocation;//AB = B - A
            dir = Make2D(dir);

            return dir.normalized;
        }

        private float FishDistanceToPlayer(GameObject other)
        {
            if (other)
            {
                return (playerLocation - other.transform.position).magnitude;
            }
            return 0.0f;
        }

        private float FishDistanceFromCenter(GameObject other)
        {
            return DistanceToCenter(other.transform.position);
        }
        
        private float PlayerDistanceFromCenter()
        {
            return DistanceToCenter(playerLocation); 
        }

        private float DistanceToCenter(Vector3 other)
        {
            Vector3 dir = other - radiusCenter; //AB = B - A
            float dist = dir.magnitude;
            return dist;
        }


        private bool FishIsBetweenPlayerAndRadiusCenter(GameObject other)
        {
            Vector3 posFish = other.transform.position;
            Vector3 dirFishToCenter = Make2D(radiusCenter - posFish);
            Vector3 dirPlayerToCenter = Make2D(radiusCenter - playerLocation);//AB = B - A

            if(dirFishToCenter.magnitude < 0.5f)
            {
                return false;
            }


            //wenn der fisch grade zum zentrum schwimmt, ist der
            //richtungs vektor ähnlich oder gleich wie die vom player
            //zum zentrum.

            //die "ähnlichkeit" wird reduziert auf dot = 0.0f,
            //also 180 grad genauigkeit

            return Vector3.Dot(dirFishToCenter, dirPlayerToCenter) >= 0.0f;
        }





        private float FishDragRelatedToCenter(GameObject other, float minDrag)
        {
            if (other)
            {
                //wenn der fisch zwischen center ist und player: 
                //dahin auch hinsteuern mit voller kraft
                if (FishIsBetweenPlayerAndRadiusCenter(other))
                {
                    //Debug.Log("Scale drag between center and player");
                    return 1.0f;
                }

                //andern falls lassen wir nach,
                //wenn wir auf der ferneren seite den radius verlassen
                float distToCenter = FishDistanceFromCenter(other);
                float scalar = distToCenter / radius;

                //näher am zentrum: voller drag
                //ausserhalb des kreises: kein drag
                float inverted = 1.0f - scalar;
                inverted = Mathf.Clamp(inverted, minDrag, 1.0f);



                //Debug.Log("Scale drag " + inverted + " distance scalar raw " + scalar);
                return inverted;
            }

            return 0.0f;
        }

        private void UpdateResistVector(float dragOfFish, float scalar)
        {
            if (scalar >= 0.9f && resistVector.magnitude > 0.0f && dragOfFish > 0.0f)
            {
                resistVector = resistVector.normalized * dragOfFish;
            }
        }
        

        //verhindern dass es keine bewegung gibt:
        //wenn keine bei raus kommt: einfach ein leichter drag von fisch.
        private Vector3 PreventNoMovement(
            Vector3 finalMovement,
            Vector3 drag
        )
        {
            if (finalMovement.magnitude <= minimumDragVelocity)
            {
                //todo: setzen auf ein minimum drag?
                finalMovement = drag * minimumDragVelocity;
            }
            return finalMovement;
        }



        private Vector3 GetMovementDirection_PullAndDragApplied(
            GameObject fish,
            float dragOfFish
        )
        {
            //dragOfFish = SafeDragScale(dragOfFish);

            //drag abhängig von der distanz um das einzufedern?
            float scaleDrag = FishDragRelatedToCenter(fish, dragOfFish * 0.5f);
            dragOfFish *= scaleDrag;

            //probieren die resist etwas einzudämmen
            UpdateResistVector(dragOfFish, scaleDrag);







            // ----- TODO HIER -----
            //-> angel routen attribut mit stärke des einholens
            //-> fisch attribut mit gegenwehr

            //-> wie läuft das mit der ausdauer

            //müssen dann folgende attribute umskallieren:
            // ----- TODO HIER -----

            //erstmal in einheits form damit das minigame
            //eben spielbar ist
            Vector3 rawDir = GetMovementLookDirection2D();
            Vector3 drag = rawDir * dragOfFish;
            Vector3 pull = resistVector;

            //try reset
            resistVector = new Vector3(0, 0, 0);




            Vector3 rawDragPull = drag + pull;

            //avoid ovement trough air!
            Vector3 finalMovement = Make2D(rawDragPull);

            //final fix to prevent no movement at all
            finalMovement = PreventNoMovement(finalMovement, drag);


            return finalMovement;

        }
        



        






        public void UpdateGameObjectMovement(GameObjectBase other)
        {
            
            Fish asFish = (Fish)other;
            if (asFish != null)
            {
                //Debug.Log("Minigame: CastFish Ok");
                UpdateGameObjectMovement(other, asFish.baseSwimSpeed);
                return;
            }
            else
            {
                //Debug.Log("Minigame: CastFish failed");
            }

            UpdateGameObjectMovement(other, 1.0f);
        }



        private void UpdateGameObjectMovement(GameObjectBase other, float scaledir)
        {
            //Debug.Log("TickMoveOnMiniGame");
            Vector3 dirMovement = GetMovementDirection_PullAndDragApplied(other.gameObject, scaledir);
            dirMovement *= Time.deltaTime;//gx: A + t (B-A)
            Vector3 updatePos = other.transform.position + dirMovement;//gx: A + t (B-A)

            updatePos = SetPositionBelowWaterSurface(updatePos);

            other.SetLocation(updatePos);


            Vector3 lookDir = GetMovementLookDirection2D();
            Quaternion quat = QuatHelper.MakeQuatFromDirection2D(lookDir);
            other.SetRotation(quat);


            ResetDragResist();
        }
        
        private Vector3 SetPositionBelowWaterSurface(Vector3 posIn)
        {
            WaterInteractionManager instance = WaterInteractionManager.Instance;
            if (instance != null)
            {
                posIn = instance.GetBelowSurfacePositon(posIn);
            }
            return posIn;
        }
        


        private void ResetDragResist()
        {
            resistVector.x = 0.0f;
            resistVector.y = 0.0f;
            resistVector.z = 0.0f;

        }


        //must be updated while resisting, values are RESETED
        //each computation
        public void SetDragResist(Vector3 forceDir, float mass, float vResist)
        {
            //F = m * a
            //x(t) = x0 + v0t + 0.5t^2
            //v(t) = v0 + at
            //a(t) = a

            //F / m = a
            //v = v0 + at
            //resist value from rod beeing reeled in
            Vector3 vmin = forceDir.normalized * vResist;
            Vector3 result = (forceDir / mass) * Time.deltaTime + vmin;
            SetDragResist(result);
        }

        public void SetDragResist(Vector3 velocity)
        {
            velocity.y = 0.0f;
            resistVector = velocity;
        }



        //update circle decal
        public void UpdateCircleDecal(PlayerCircleTarget decal)
        {
            if (decal != null)
            {
                decal.UpdateTargetPosition(radiusCenter, true);
                decal.UpdateCircleRadius(radius);
                //Debug.Log("RadiusMiniGame::UPDATE CIRCLE DECAL EXTERNAL radius: " + radius);

                //if in range change color
                decal.UpdateColor(DecalColor());
            }


        }
        
        //Decal color to be set based on minigame state
        private Color DecalColor()
        {
            Color inRange = new Color(0.0f,0.7f,0.2f,1);
            Color outOfRange = Color.red;
            return isInRadiusState ? inRange : outOfRange;
        }


        


    }

};