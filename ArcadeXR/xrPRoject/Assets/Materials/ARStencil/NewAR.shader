Shader "Custom/NewAR"
{
    SubShader
    {
        // Rendert direkt vor den normalen 3D-Objekten (Queue = Geometry-1)
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" "RenderPipeline"="UniversalPipeline" }
        
        // Zeichnet keine Farben in den Frame, der Hintergrund (Passthrough) bleibt unangetastet
        ColorMask 0 
        
        // Schreibt aber in den Depth-Buffer, wodurch alles dahinter unsichtbar wird
        ZWrite On 

        Pass
        {
            // Dieser Pass bleibt leer, er dient nur als Tiefen-Blocker
        }
    }
}