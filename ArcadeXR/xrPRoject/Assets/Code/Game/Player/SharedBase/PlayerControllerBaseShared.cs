using UnityEngine;

using UnityEngine.InputSystem;

using Player.FirstPerson;
using Core.Game;
using Core.Game.Entity;
using Game.Fishing;
using Game.Player;
using Gameworld.Pond;
using UnityEngine.Rendering;

public class PlayerControllerBaseShared : EntityBase
{
    protected GameObject rodBaseObject = null;
    protected FishingRod fishingRod = null;

    protected PlayerCircleTarget targetCircle = null;

    protected float volume = 0.0f;



    void Start()
    {

    }


    void Update()
    {

    }

    public FishingRod GetFishingRod()
    {
        return fishingRod;
    }

    //attach to right hand or body for now
    public void ReplaceFishingRod(GameObject rod)
    {
        //keep empty. Abstract.
        rodBaseObject = rod;
        if (rodBaseObject != null)
        {
            //attach transform
            //attach to body
            Transform customTransform = rodAttachToTransform();
            AttachComponentLocalSpace(customTransform, rod);


            //rod.transform.SetParent(customTransform);

            Debug.Log("TRANFORM ATTACH TO: ", customTransform);


            //get script
            FishingRod found = rodBaseObject.GetComponent<FishingRod>();
            if (found != null)
            {
                fishingRod = found;
            }


        }
    }

    public void RemoveRodReference()
    {
        rodBaseObject = null;
        fishingRod = null;
    }


    //returns the desired transform the rod should be attached to
    //override in subclass (player pc hard coded one, vr: hand transform.)
    public virtual Transform rodAttachToTransform()
    {
        Transform attachTo = this.gameObject.transform;
        return attachTo; //TO BE OVERRIDEN!
    }


    public void SetupPlayerCircleDecal(Pond pond)
    {
        // Komponente dynamisch erstellen und anhängen
        if (targetCircle == null)
        {
            targetCircle = gameObject.AddComponent<PlayerCircleTarget>();
            if (targetCircle != null)
            {
                targetCircle.SetupExternal(pond, FindCameraTransform());
            }
        }



    }


    public PlayerCircleTarget GetPlayerTargetCircle()
    {
        return targetCircle;
    }


    private Transform FindCameraTransform()
    {
        Camera camera = FindCamera();
        if (camera != null)
        {
            return camera.transform;
        }
        return null;
    }

    public Camera FindCamera()
    {
        return GetComponentInChildren<Camera>();
    }



    public void SetAudioVolume(float s)
    {
        s = Mathf.Clamp(s, 0.0f, 1.0f);
        volume = s;
        OnVolumeUpdate();

    }

    protected virtual void OnVolumeUpdate()
    {
        
    }


}