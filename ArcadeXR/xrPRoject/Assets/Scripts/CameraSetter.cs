using UnityEngine;

public class CameraSetter : MonoBehaviour
{
   
    public Canvas canvas;

    public void SetCamera(Camera camera)
    {
        if (canvas != null)
        {
            canvas.worldCamera = camera;
        }
    }


}
