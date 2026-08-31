using System;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// allows this class to read vertex data from a material
/// 
/// not that the RGB Channel Out of the material must 
/// pass the Vertex shader data into the channel
/// if "someMaterial.SetFloat("_IsHeightPass", 1f);"
/// and "someMaterial.SetFloat("_IsHeightPass", 0f);"
/// is possible, to switch the render pass to a vertex output
/// 
/// the camera always faces downward and is auto generated
/// 
/// --- usuage inside the material ---
/// -> create a value input of name "IsHeightPass"
/// -> create a branch which gets the output color (default) and vertex position,
/// to be pasted.
/// -> the branch then pushes the output value of the branch into the color RGBA output.
/// 
/// </summary>
public class MaterialHeightReader : MonoBehaviour
{
    private Renderer waterRenderer = null;
    private Camera sampleCamera = null;
    private RenderTexture heightRT = null;
    private Texture2D cpuTexture = null;

    public void InitRenderer(Renderer rendererIn)
    {
        if (waterRenderer == null)
        {
            waterRenderer = rendererIn;
            Init();
        }
    }

    private void Init()
    {
        if (heightRT != null) return;

        InitRenderTarget();
        InitCamera();
    }

    private void InitRenderTarget()
    {
        // 32-Bit Float Texture zur Speicherung der Welt-Y-Höhe
        //heightRT = new RenderTexture(1, 1, 24, RenderTextureFormat.RFloat)


        //heightRT = new RenderTexture(1, 1, 24, RenderTextureFormat.ARGBFloat)
        int sizeMin = 16;
        heightRT = new RenderTexture(sizeMin, sizeMin, 24, RenderTextureFormat.ARGBFloat)
        {
            filterMode = FilterMode.Point,
            autoGenerateMips = false,
            useMipMap = false,
            antiAliasing = 1
        };
        heightRT.Create();

        // Texture2D zum synchronen Auslesen des RFloat-Werts
        //cpuTexture = new Texture2D(1, 1, TextureFormat.RFloat, false);
        cpuTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false); //RGBA Komplett lesen
    }

    private void InitCamera()
    {
        GameObject camGO = new GameObject("HeightSampleCam");
        camGO.transform.SetParent(this.transform, false);

        sampleCamera = camGO.AddComponent<Camera>();
        sampleCamera.enabled = false; // Wird nur manuell gerendert

        // ClearColor auf -9999f setzen (Eindeutige Kennung für "Nichts getroffen")
        sampleCamera.clearFlags = CameraClearFlags.SolidColor;
        sampleCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);

        sampleCamera.orthographic = true; //false (?) true gut so hier?
        sampleCamera.orthographicSize = 0.1f;

        sampleCamera.nearClipPlane = 0.01f;
        sampleCamera.farClipPlane = 3000f;

        sampleCamera.targetTexture = heightRT;
        sampleCamera.allowMSAA = false;
        sampleCamera.allowHDR = true; // Wichtig für unbegrenzte Float-Höhenwerte!
    }






    private Vector3 MakeCameraPosition(MaterialHeightReaderTask task)
    {
        Vector3 camPos = task.pos + new Vector3(0.0f, 10f, 0.0f);
        return camPos;
    }

    private void SetCameraPositionAndTarget(MaterialHeightReaderTask task)
    {
        Vector3 camPos = MakeCameraPosition(task);
        sampleCamera.transform.position = camPos;

        Vector3 targetPoint = task.pos;
        sampleCamera.transform.LookAt(targetPoint, Vector3.up);
    }

    private void RenderCamera()
    {
        // Culling-Maske beschränken
        //------
        //TODO: EIGENEN CHANNEL ANLEGEN, SONST IST DIE BOYE IM WEG (?)
        //------
        sampleCamera.cullingMask = 1 << waterRenderer.gameObject.layer;

        // 1. Material auf Height-Pass umschalten
        Material waterMat = waterRenderer.sharedMaterial;
        if (waterMat != null)
        {
            waterMat.SetFloat("_IsHeightPass", 1f);
            Debug.Log($"HEIGHT PASS SET to 1.0, actual value: {waterMat.GetFloat("_IsHeightPass")}");
            sampleCamera.Render();
            waterMat.SetFloat("_IsHeightPass", 0f);
            Debug.Log($"HEIGHT PASS SET to 0.0, actual value: {waterMat.GetFloat("_IsHeightPass")}");
        }
        else
        {
            Debug.LogError("Water material is NULL!");
            sampleCamera.Render();
        }
    }

    private Vector3 ReadResultPositionFromRT(MaterialHeightReaderTask task)
    {

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = heightRT;

        // Pixel in die Texture2D kopieren, nur einer, size rect 1,1
        cpuTexture.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        cpuTexture.Apply();

        RenderTexture.active = previousActive;

        // ROHDATEN DIREKT ALS FLOAT-ARRAY LESEN - FLOAT KANN NICHT GECLAMPT WERDEN!!!
        // [0]=R, [1]=G, [2]=B, [3]=A
        // und water positon / vertex positon: [0]=X, [1]=Y, [2]=Z, [3]=none
        var rawFloatData = cpuTexture.GetPixelData<float>(0); //das ist 4 mal float.
        Vector3 localMeshPoint = new Vector3(rawFloatData[0], rawFloatData[1], rawFloatData[2]);

        Vector3 worldMeshPoint = waterRenderer.gameObject.transform.TransformPoint(localMeshPoint);

        //override xz, wollen nur höhe. Anders ist es auch verbuggt.
        worldMeshPoint.x = task.pos.x;
        worldMeshPoint.z = task.pos.z;


        return worldMeshPoint;
    }





    public void GetExactMaterialHeight(MaterialHeightReaderTask task)
    {
        if (waterRenderer == null || sampleCamera == null) return;

        SetCameraPositionAndTarget(task);
        RenderCamera();
        Vector3 worldMeshPoint = ReadResultPositionFromRT(task);


        //UNKLAR!
        if (task.bEnableDownOffset())
        {
            worldMeshPoint.y -= 0.26f; //WaterSurface local Y offset
        }


        Debug.Log("DATA READ SHADER " + worldMeshPoint);

#if UNITY_EDITOR
        //DebugHelperLine.DrawLine(camPos, resultPos, Color.red);
        //DebugHelperLine.DrawLine(resultPos, pos, Color.green);
        DebugHelperLine.DrawLine(worldMeshPoint, worldMeshPoint + Vector3.down * 0.1f, Color.blue, 0.1f);
#endif

        task.result = worldMeshPoint;


    }


    private void OnDestroy()
    {
        if (heightRT != null)
        {
            heightRT.Release();
            Destroy(heightRT);
        }
        if (cpuTexture != null)
        {
            Destroy(cpuTexture);
        }
        if (sampleCamera != null)
        {
            Destroy(sampleCamera.gameObject);
        }
    }
}