using UnityEngine;
using System;
using Gameworld.Pond;
using UnityEngine;


public class DebugHelperLine : MonoBehaviour
{
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 2.0f)
    {
        Vector3 dir = end - start; //AB = B - A
        DrawRay(start, dir, color, duration); 
    }


    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration = 2.0f)
    {
        GameObject lineGO = new GameObject("InGameRay");
        LineRenderer line = lineGO.AddComponent<LineRenderer>();

        // Material für unlit transparente/farbige Linien
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.startWidth = 0.05f; // Liniendicke am Anfang
        line.endWidth = 0.05f;   // Liniendicke am Ende
        line.positionCount = 2;

        line.SetPosition(0, start);
        line.SetPosition(1, start + dir);

        // Nach Ablauf der Dauer automatisch zerstören
        Destroy(lineGO, duration);
    }

    public static void DrawPoint(Vector3 position, Color color, float size = 0.5f, float duration = 2.0f)
    {
        // Zeichnet ein Kruzifix/Crosshair im Spiel an der Position
        DrawRay(position - Vector3.right * (size * 0.5f), Vector3.right * size, color, duration);
        DrawRay(position - Vector3.forward * (size * 0.5f), Vector3.forward * size, color, duration);
        DrawRay(position - Vector3.up * (size * 0.5f), Vector3.up * size, color, duration);
    }
}