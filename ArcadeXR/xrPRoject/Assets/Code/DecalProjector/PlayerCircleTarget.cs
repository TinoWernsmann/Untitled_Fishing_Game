using Gameworld.Pond;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Zeigt einen Zielkreis auf der Wasseroberfläche an.
    ///
    /// Der Kreis wird an der Stelle angezeigt, an der die Blickrichtung
    /// des Spielers eine horizontale Wasserfläche schneidet.
    /// </summary>
    public sealed class PlayerCircleTarget : MonoBehaviour
    {
        // Namen der Properties aus dem Water Shader Graph.
        private const string CirclePositionPropertyName = "_CircleTargetPosition";
        private const string CircleRadiusPropertyName = "_CircleTargetRadius";
        private const string CircleWidthPropertyName = "_CircleTargetWidth";
        private const string CircleColorPropertyName = "_CircleTargetColor";
        private const string CircleVisibilityPropertyName = "_ShowCircleTarget";

        // Property-IDs sind effizienter als wiederholte String-Zugriffe.
        private static readonly int CirclePositionPropertyId =
            Shader.PropertyToID(CirclePositionPropertyName);

        private static readonly int CircleRadiusPropertyId =
            Shader.PropertyToID(CircleRadiusPropertyName);

        private static readonly int CircleWidthPropertyId =
            Shader.PropertyToID(CircleWidthPropertyName);

        private static readonly int CircleColorPropertyId =
            Shader.PropertyToID(CircleColorPropertyName);

        private static readonly int CircleVisibilityPropertyId =
            Shader.PropertyToID(CircleVisibilityPropertyName);

        [Header("References")]

        [Tooltip(
            "Die Kamera oder der Transform, dessen Vorwärtsrichtung " +
            "als Blickrichtung verwendet wird."
        )]
        [SerializeField]
        private Transform lookOrigin;

        [Tooltip(
            "Der Mesh Renderer der Wasserfläche. " +
            "Über diesen Renderer werden die Shader-Werte gesetzt."
        )]
        [SerializeField]
        private Renderer waterRenderer;

        [Tooltip(
            "Der Transform der Wasserfläche. " +
            "Seine Y-Position bestimmt die Höhe der horizontalen Wasserfläche."
        )]
        [SerializeField]
        private Transform waterSurface;

        [Header("Circle Appearance")]

        [Tooltip("Radius des Zielkreises in Welt-Einheiten.")]
        [SerializeField, Min(0.01f)]
        private float circleRadius = 1.5f;

        [Tooltip("Breite des Kreisrings in Welt-Einheiten.")]
        [SerializeField, Min(0.001f)]
        private float circleWidth = 0.1f;

        [Tooltip("Farbe des Zielkreises.")]
        [SerializeField]
        private Color circleColor = Color.red;

        [Header("Target Detection")]

        [Tooltip(
            "Maximale Entfernung, in der ein gültiger Zielpunkt " +
            "auf der Wasserfläche liegen darf."
        )]
        [SerializeField, Min(1f)]
        private float maximumTargetDistance = 100f;

        [Header("Debug")]

        [Tooltip(
            "Zeichnet die Blickrichtung in der Scene View. " +
            "Grün bedeutet gültiges Ziel, Rot bedeutet ungültiges Ziel."
        )]
        [SerializeField]
        private bool drawDebugRay = true;

        [Tooltip(
            "Prüft beim Start, ob das Wasser-Material " +
            "alle benötigten Shader-Properties besitzt."
        )]
        [SerializeField]
        private bool validateShaderProperties = true;

        private MaterialPropertyBlock waterPropertyBlock;




        private bool MoveDecalWithCamera = true;
        private Vector3 targetPosition;
        private Vector3 targetPositionAssigned;

        public void UpdateTargetPosition(Vector3 targetPositionIn)
        {
            targetPositionAssigned = targetPositionIn;
        }

        public void UpdateTargetPosition(Vector3 targetPositionIn, bool enable)
        {
            UpdateTargetPosition(targetPositionIn);
            EnableManualTargetPosition(enable);
        }

        public void EnableManualTargetPosition(bool flag)
        {
            MoveDecalWithCamera = !flag;
        }

        public void UpdateCircleRadius(float radius)
        {
            circleRadius = math.abs(radius);
        }

        public void UpdateColor(Color colorIn)
        {
            circleColor = colorIn;
        }



        private void Awake()
        {
            Setup();
        }

        private void Setup()
        {
            waterPropertyBlock = new MaterialPropertyBlock();

            AssignMainCameraIfLookOriginIsMissing();
            ValidateRequiredShaderProperties();
            HideCircle();
        }



        private void Update()
        {
            UpdateCircleTarget();
        }

        private void OnDisable()
        {
            HideCircle();
        }

        /// <summary>
        /// Verwendet automatisch die Main Camera, wenn im Inspector
        /// kein Look Origin zugewiesen wurde.
        /// </summary>
        private void AssignMainCameraIfLookOriginIsMissing()
        {
            if (lookOrigin != null)
            {
                return;
            }

            if (Camera.main != null)
            {
                lookOrigin = Camera.main.transform;
            }
        }

        /// <summary>
        /// Berechnet den Schnittpunkt zwischen der Blickrichtung
        /// und der horizontalen Wasserfläche.
        /// </summary>
        private void UpdateCircleTarget()
        {
            if (!AreRequiredReferencesAssigned())
            {
                HideCircle();
                return;
            }

            Plane waterPlane = CreateHorizontalWaterPlane();
            Ray playerLookRay = CreateTargetRay();//CreatePlayerLookRay();


            bool rayIntersectsWater = waterPlane.Raycast(
                playerLookRay,
                out float distanceToWater
            );

            bool targetIsWithinAllowedDistance =
                distanceToWater > 0f &&
                distanceToWater <= maximumTargetDistance;

            bool targetIsValid =
                rayIntersectsWater &&
                targetIsWithinAllowedDistance;

            if (!targetIsValid)
            {
                HideCircle();
                DrawInvalidTargetRay(playerLookRay);
                return;
            }

            targetPosition =
                playerLookRay.GetPoint(distanceToWater);

            ShowCircleAt(targetPosition);
            DrawValidTargetRay(playerLookRay.origin, targetPosition);
        }

        /// erstellt einen strahl von der kamera aus
        /// oder vom target assigned nach unten hin
        private Ray CreateTargetRay()
        {
            if (MoveDecalWithCamera)
            {
                return CreatePlayerLookRay();
            }else{
                Vector3 targetCurrent = targetPositionAssigned;
                Vector3 waterPos = waterSurface.position;
                int offset = 10;
                targetCurrent.y = math.max(targetCurrent.y, waterPos.y + offset);
                return new Ray(
                    targetCurrent,
                    Vector3.down
                );
            }
        }




        /// <summary>
        /// Erstellt einen Strahl von der Kamera in Blickrichtung.
        /// </summary>
        private Ray CreatePlayerLookRay()
        {
            return new Ray(
                lookOrigin.position,
                lookOrigin.forward
            );
        }

        /// <summary>
        /// Erstellt eine horizontale Ebene auf Höhe der Wasserfläche.
        /// </summary>
        private Plane CreateHorizontalWaterPlane()
        {
            return new Plane(
                Vector3.up,
                waterSurface.position
            );
        }

        /// <summary>
        /// Übergibt Position und Darstellung des Kreises
        /// an das Material der Wasserfläche.
        /// </summary>
        private void ShowCircleAt(Vector3 worldPosition)
        {
            ReadCurrentWaterProperties();

            waterPropertyBlock.SetVector(
                CirclePositionPropertyId,
                worldPosition
            );

            waterPropertyBlock.SetFloat(
                CircleRadiusPropertyId,
                circleRadius
            );

            waterPropertyBlock.SetFloat(
                CircleWidthPropertyId,
                circleWidth
            );

            waterPropertyBlock.SetColor(
                CircleColorPropertyId,
                circleColor
            );

            waterPropertyBlock.SetFloat(
                CircleVisibilityPropertyId,
                1f
            );

            ApplyWaterProperties();
        }

        /// <summary>
        /// Blendet den Zielkreis im Water Shader aus.
        /// </summary>
        public void HideCircle()
        {
            /*if (waterRenderer == null || waterPropertyBlock == null)
            {
                return;
            }

            ReadCurrentWaterProperties();

            waterPropertyBlock.SetFloat(
                CircleVisibilityPropertyId,
                0f
            );

            ApplyWaterProperties();*/
            SetCircleVisible(false);
        }

        public void ShowCircle()
        {
            SetCircleVisible(true);
        }


        //true by default.
        private bool CurrentVisibility = false;

        public void SetCircleVisible(bool flag)
        {
            /*if (CurrentVisibility == flag){
                return;
            }
            CurrentVisibility = flag;*/


            float asfloat = flag ? 1.0f : 0.0f;
            if (waterRenderer == null || waterPropertyBlock == null)
            {
                return;
            }

            ReadCurrentWaterProperties();

            waterPropertyBlock.SetFloat(
                CircleVisibilityPropertyId,
                asfloat
            );

            ApplyWaterProperties();
        }





        /// <summary>
        /// Prüft, ob alle benötigten Unity-Referenzen vorhanden sind.
        /// </summary>
        private bool AreRequiredReferencesAssigned()
        {
            return lookOrigin != null &&
                   waterRenderer != null &&
                   waterSurface != null;
        }

        /// <summary>
        /// Liest vorhandene Werte aus dem MaterialPropertyBlock.
        /// Dadurch werden Werte anderer Systeme nicht überschrieben.
        /// </summary>
        private void ReadCurrentWaterProperties()
        {
            waterRenderer.GetPropertyBlock(waterPropertyBlock);
        }

        /// <summary>
        /// Schreibt die geänderten Shader-Werte zurück auf den Renderer.
        /// </summary>
        private void ApplyWaterProperties()
        {
            waterRenderer.SetPropertyBlock(waterPropertyBlock);
        }

        /// <summary>
        /// Prüft, ob das Wasser-Material alle benötigten
        /// Shader-Properties enthält.
        /// </summary>
        private void ValidateRequiredShaderProperties()
        {
            if (!validateShaderProperties || waterRenderer == null)
            {
                return;
            }

            Material waterMaterial = waterRenderer.sharedMaterial;

            if (waterMaterial == null)
            {
                Debug.LogError(
                    "PlayerCircleTarget: Der Water Renderer besitzt kein Material.",
                    this
                );

                return;
            }

            ValidateShaderProperty(
                waterMaterial,
                CirclePositionPropertyId,
                CirclePositionPropertyName
            );

            ValidateShaderProperty(
                waterMaterial,
                CircleRadiusPropertyId,
                CircleRadiusPropertyName
            );

            ValidateShaderProperty(
                waterMaterial,
                CircleWidthPropertyId,
                CircleWidthPropertyName
            );

            ValidateShaderProperty(
                waterMaterial,
                CircleColorPropertyId,
                CircleColorPropertyName
            );

            ValidateShaderProperty(
                waterMaterial,
                CircleVisibilityPropertyId,
                CircleVisibilityPropertyName
            );
        }

        /// <summary>
        /// Gibt einen verständlichen Fehler aus, wenn eine bestimmte
        /// Property im Wasser-Material fehlt.
        /// </summary>
        private void ValidateShaderProperty(
            Material material,
            int propertyId,
            string propertyName
        )
        {
            if (material.HasProperty(propertyId))
            {
                return;
            }

            Debug.LogError(
                $"PlayerCircleTarget: Das Wasser-Material besitzt die " +
                $"Shader-Property '{propertyName}' nicht. " +
                "Prüfe den Reference-Namen im Shader Graph.",
                this
            );
        }

        /// <summary>
        /// Zeichnet eine grüne Linie bis zum gültigen Zielpunkt.
        /// </summary>
        private void DrawValidTargetRay(
            Vector3 startPosition,
            Vector3 targetPosition
        )
        {
            if (!drawDebugRay)
            {
                return;
            }

            Debug.DrawLine(
                startPosition,
                targetPosition,
                Color.green
            );
        }

        /// <summary>
        /// Zeichnet eine rote Linie, wenn kein gültiger
        /// Zielpunkt auf der Wasserfläche gefunden wurde.
        /// </summary>
        private void DrawInvalidTargetRay(Ray playerLookRay)
        {
            if (!drawDebugRay)
            {
                return;
            }

            Debug.DrawRay(
                playerLookRay.origin,
                playerLookRay.direction * maximumTargetDistance,
                Color.red
            );
        }





        // ---- EXTERNAL SETUP ----
        private bool bExternalSetupFinished = false;
        public void SetupExternal(
            Renderer waterRendererIn,
            Transform waterSurfaceIn,
            Transform lookOriginIn
        )
        {
            if (bExternalSetupFinished)
            {
                return;
            }
            if(waterRendererIn && lookOriginIn && waterSurfaceIn)
            {
                waterRenderer = waterRendererIn;
                lookOrigin = lookOriginIn;
                waterSurface = waterSurfaceIn;
                Setup();
                bExternalSetupFinished = true;
                //Debug.Log("SETUP EXTERNAL DECAL FINISHED!");
            }
        }

        public void SetupExternal(
            Pond pondIn,
            Transform lookOriginIn
        )
        {
            if (bExternalSetupFinished)
            {
                return;
            }
            //find waterRenderer
            //and water surface transform from Pond   
            Renderer foundRenderer = pondIn.FindWaterRenderer();
            Transform waterTransform = pondIn.FindWaterRendererTransform();
            SetupExternal(foundRenderer, waterTransform, lookOriginIn);
        }
        
        public bool ExternalSetupFinished()
        {
            return bExternalSetupFinished;
        }














    }
}