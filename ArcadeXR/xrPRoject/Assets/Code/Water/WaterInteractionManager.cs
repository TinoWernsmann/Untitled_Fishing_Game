using System;
using System.Xml.Linq;
using Core.Game;
using UnityEngine;

public class WaterInteractionManager : MonoBehaviour
{
    // Global access for all water objects
    public static WaterInteractionManager Instance;

    [Header("Water Reference")]
    // Transform used to get the water height
    public Transform waterPlane;

    [Header("Settings")]
    // Small offset above the water surface
    public float surfaceOffset = 0f;


    private MaterialHeightReader reader = null;



    public float WaterHeight
    {
        get
        {
            if (waterPlane == null)
                return 0f;

            // Return the water surface height
            return waterPlane.position.y;
        }
    }

    private void Awake()
    {
        // Store this manager instance
        Instance = this;
        SetupMaterialHeightReader();
    }


    public Vector3 GetBelowSurfacePositon(Vector3 posIn)
    {
        posIn.y = WaterHeight;
        return posIn;
    }


    public Vector3 GetSurfacePosition(Vector3 worldPosition)
    {
        // Keep X/Z, place Y on the water surface
        return new Vector3(
            worldPosition.x,
            WaterHeight + surfaceOffset,
            worldPosition.z
        );
    }

    public Renderer FindMeshRenderer()
    {
        Renderer renderer = GameObjectBase.TFindFirstComponentInChildrenRecursive<Renderer>(this.gameObject);
        return renderer;
    }


    private void SetupMaterialHeightReader()
    {
        if (reader == null)
        {
            //shader must be setup
            reader = gameObject.GetComponent<MaterialHeightReader>();
            Debug.Log("FoundMaterialHeightReader from Components!"); //yes
        }

        if (reader == null)
        {
            reader = gameObject.AddComponent<MaterialHeightReader>();
        }

        Renderer renderer = FindMeshRenderer();
        if (renderer != null && reader != null)
        {
            reader.InitRenderer(renderer);
        }
    }


    //WaterInteractionManager.Instance.ExtractSurfacePositonFromRender(...);
    public void ExtractSurfacePositonFromRender(MaterialHeightReaderTask posData)
    {
        if (reader != null && posData != null)
        {
            reader.GetExactMaterialHeight(posData);
        }
    }














}