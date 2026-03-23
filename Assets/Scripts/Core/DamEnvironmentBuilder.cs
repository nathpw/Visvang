using UnityEngine;

namespace Visvang.Core
{
    /// <summary>
    /// Programmatically creates the dam fishing environment.
    /// Builds water, terrain, skybox, lighting, camera, and basic scenery.
    /// </summary>
    public static class DamEnvironmentBuilder
    {
        public static void Build()
        {
            CreateCamera();
            CreateLighting();
            CreateWater();
            CreateTerrain();
            CreateSkybox();
            CreateScenery();

            Debug.Log("[Visvang] Dam environment built");
        }

        private static void CreateCamera()
        {
            // Remove ALL existing cameras (default scene camera, etc.)
            var existingCams = Object.FindObjectsOfType<Camera>();
            foreach (var oldCam in existingCams)
                Object.Destroy(oldCam.gameObject);

            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.4f, 0.55f, 0.7f); // SA sky blue
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 500f;

            // Position: sitting on the bank, looking out at the water
            camGo.transform.position = new Vector3(0f, 2.5f, -5f);
            camGo.transform.rotation = Quaternion.Euler(15f, 0f, 0f);

            camGo.AddComponent<AudioListener>();
        }

        private static void CreateLighting()
        {
            // Remove default scene light if exists
            var existingLights = Object.FindObjectsOfType<Light>();
            foreach (var light in existingLights)
            {
                if (light.type == LightType.Directional)
                    Object.Destroy(light.gameObject);
            }

            // Directional light (sun)
            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.85f); // Warm SA sun
            sun.intensity = 1.2f;
            sunGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            sun.shadows = LightShadows.Soft;

            // Ambient light
            RenderSettings.ambientLight = new Color(0.3f, 0.35f, 0.4f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientIntensity = 0.8f;

            // Fog for atmosphere
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.5f, 0.6f, 0.7f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 80f;
            RenderSettings.fogEndDistance = 200f;
        }

        private static void CreateWater()
        {
            // Water plane
            var waterGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterGo.name = "WaterSurface";
            waterGo.transform.position = new Vector3(0f, 0f, 50f);
            waterGo.transform.localScale = new Vector3(30f, 1f, 20f);

            var renderer = waterGo.GetComponent<Renderer>();
            var waterMat = new Material(Shader.Find("Standard"));
            waterMat.color = new Color(0.15f, 0.35f, 0.45f, 0.85f);
            waterMat.SetFloat("_Metallic", 0.3f);
            waterMat.SetFloat("_Glossiness", 0.8f);

            // Make it semi-transparent
            waterMat.SetFloat("_Mode", 3); // Transparent mode
            waterMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            waterMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            waterMat.SetInt("_ZWrite", 0);
            waterMat.DisableKeyword("_ALPHATEST_ON");
            waterMat.EnableKeyword("_ALPHABLEND_ON");
            waterMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            waterMat.renderQueue = 3000;

            renderer.material = waterMat;

            // Underwater dark plane (for depth illusion)
            var deepGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            deepGo.name = "WaterDepth";
            deepGo.transform.position = new Vector3(0f, -3f, 50f);
            deepGo.transform.localScale = new Vector3(30f, 1f, 20f);

            var deepRenderer = deepGo.GetComponent<Renderer>();
            var deepMat = new Material(Shader.Find("Standard"));
            deepMat.color = new Color(0.05f, 0.12f, 0.15f);
            deepRenderer.material = deepMat;

            // Remove colliders (not needed)
            Object.Destroy(waterGo.GetComponent<Collider>());
            Object.Destroy(deepGo.GetComponent<Collider>());

            // Add gentle wave animation
            waterGo.AddComponent<WaterWave>();
        }

        private static void CreateTerrain()
        {
            // Bank (ground player sits on)
            var bankGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bankGo.name = "Bank";
            bankGo.transform.position = new Vector3(0f, -0.5f, -3f);
            bankGo.transform.localScale = new Vector3(40f, 1f, 10f);

            var bankRenderer = bankGo.GetComponent<Renderer>();
            var bankMat = new Material(Shader.Find("Standard"));
            bankMat.color = new Color(0.35f, 0.28f, 0.18f); // Brown earth
            bankRenderer.material = bankMat;

            // Slope into water
            var slopeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slopeGo.name = "BankSlope";
            slopeGo.transform.position = new Vector3(0f, -0.3f, 2f);
            slopeGo.transform.localScale = new Vector3(40f, 0.5f, 6f);
            slopeGo.transform.rotation = Quaternion.Euler(15f, 0f, 0f);

            var slopeRenderer = slopeGo.GetComponent<Renderer>();
            var slopeMat = new Material(Shader.Find("Standard"));
            slopeMat.color = new Color(0.3f, 0.25f, 0.15f);
            slopeRenderer.material = slopeMat;

            // Far bank
            var farBankGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            farBankGo.name = "FarBank";
            farBankGo.transform.position = new Vector3(0f, 1f, 120f);
            farBankGo.transform.localScale = new Vector3(60f, 4f, 20f);

            var farBankRenderer = farBankGo.GetComponent<Renderer>();
            var farBankMat = new Material(Shader.Find("Standard"));
            farBankMat.color = new Color(0.25f, 0.35f, 0.2f); // Green far bank
            farBankRenderer.material = farBankMat;
        }

        private static void CreateSkybox()
        {
            // Use solid color sky (the camera already has blue bg)
            // Add some clouds using simple stretched cubes at distance
            for (int i = 0; i < 5; i++)
            {
                var cloudGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cloudGo.name = $"Cloud_{i}";
                cloudGo.transform.position = new Vector3(
                    Random.Range(-80f, 80f),
                    Random.Range(30f, 50f),
                    Random.Range(40f, 120f)
                );
                cloudGo.transform.localScale = new Vector3(
                    Random.Range(15f, 35f),
                    Random.Range(2f, 5f),
                    Random.Range(8f, 18f)
                );

                var cloudRenderer = cloudGo.GetComponent<Renderer>();
                var cloudMat = new Material(Shader.Find("Standard"));
                cloudMat.color = new Color(0.9f, 0.92f, 0.95f, 0.7f);
                cloudMat.SetFloat("_Mode", 3);
                cloudMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                cloudMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                cloudMat.SetInt("_ZWrite", 0);
                cloudMat.EnableKeyword("_ALPHABLEND_ON");
                cloudMat.renderQueue = 3000;
                cloudRenderer.material = cloudMat;
                cloudRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                Object.Destroy(cloudGo.GetComponent<Collider>());
            }
        }

        private static void CreateScenery()
        {
            // Rod stand (two sticks)
            CreateRodStand(new Vector3(-1f, 0f, -1f));

            // Pap bucket (small cube)
            var bucketGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bucketGo.name = "PapBucket";
            bucketGo.transform.position = new Vector3(1.5f, 0.2f, -2f);
            bucketGo.transform.localScale = new Vector3(0.3f, 0.25f, 0.3f);
            var bucketMat = new Material(Shader.Find("Standard"));
            bucketMat.color = new Color(0.7f, 0.7f, 0.75f);
            bucketGo.GetComponent<Renderer>().material = bucketMat;

            // Chair
            var chairGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chairGo.name = "FishingChair";
            chairGo.transform.position = new Vector3(0f, 0.3f, -3.5f);
            chairGo.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
            var chairMat = new Material(Shader.Find("Standard"));
            chairMat.color = new Color(0.2f, 0.35f, 0.15f); // Green camping chair
            chairGo.GetComponent<Renderer>().material = chairMat;

            // Tackle box
            var tackleGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tackleGo.name = "TackleBox";
            tackleGo.transform.position = new Vector3(-2f, 0.15f, -3f);
            tackleGo.transform.localScale = new Vector3(0.5f, 0.3f, 0.35f);
            var tackleMat = new Material(Shader.Find("Standard"));
            tackleMat.color = new Color(0.3f, 0.3f, 0.35f);
            tackleGo.GetComponent<Renderer>().material = tackleMat;

            // Bushes along the bank
            for (int i = 0; i < 6; i++)
            {
                var bushGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bushGo.name = $"Bush_{i}";
                bushGo.transform.position = new Vector3(
                    -18f + i * 7f + Random.Range(-1f, 1f),
                    0.5f,
                    -6f + Random.Range(-1f, 0f)
                );
                bushGo.transform.localScale = new Vector3(
                    Random.Range(1.5f, 3f),
                    Random.Range(1f, 2f),
                    Random.Range(1.5f, 3f)
                );
                var bushMat = new Material(Shader.Find("Standard"));
                bushMat.color = new Color(
                    Random.Range(0.15f, 0.25f),
                    Random.Range(0.3f, 0.5f),
                    Random.Range(0.1f, 0.2f)
                );
                bushGo.GetComponent<Renderer>().material = bushMat;
                Object.Destroy(bushGo.GetComponent<Collider>());
            }

            // Trees on far bank
            for (int i = 0; i < 8; i++)
            {
                CreateSimpleTree(new Vector3(
                    -30f + i * 8f + Random.Range(-2f, 2f),
                    0f,
                    115f + Random.Range(-3f, 3f)
                ));
            }
        }

        private static void CreateRodStand(Vector3 position)
        {
            // Left stick
            var stick1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stick1.name = "RodStand_Left";
            stick1.transform.position = position + new Vector3(-0.15f, 0.5f, 0f);
            stick1.transform.localScale = new Vector3(0.03f, 0.5f, 0.03f);
            stick1.transform.rotation = Quaternion.Euler(0f, 0f, -5f);
            var mat1 = new Material(Shader.Find("Standard"));
            mat1.color = new Color(0.3f, 0.3f, 0.3f);
            stick1.GetComponent<Renderer>().material = mat1;

            // Right stick
            var stick2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stick2.name = "RodStand_Right";
            stick2.transform.position = position + new Vector3(0.15f, 0.5f, 0f);
            stick2.transform.localScale = new Vector3(0.03f, 0.5f, 0.03f);
            stick2.transform.rotation = Quaternion.Euler(0f, 0f, 5f);
            stick2.GetComponent<Renderer>().material = mat1;

            // Rod (long thin cylinder resting on stand)
            var rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rod.name = "FishingRod";
            rod.transform.position = position + new Vector3(0f, 0.9f, 2f);
            rod.transform.localScale = new Vector3(0.02f, 2.5f, 0.02f);
            rod.transform.rotation = Quaternion.Euler(0f, 0f, 90f) * Quaternion.Euler(10f, 0f, 0f);
            var rodMat = new Material(Shader.Find("Standard"));
            rodMat.color = new Color(0.2f, 0.2f, 0.25f);
            rod.GetComponent<Renderer>().material = rodMat;

            // Fishing line (thin stretched cube)
            var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "FishingLine";
            line.transform.position = position + new Vector3(0f, 0.5f, 15f);
            line.transform.localScale = new Vector3(0.005f, 0.005f, 25f);
            var lineMat = new Material(Shader.Find("Standard"));
            lineMat.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            line.GetComponent<Renderer>().material = lineMat;
            Object.Destroy(line.GetComponent<Collider>());
        }

        private static void CreateSimpleTree(Vector3 position)
        {
            // Trunk
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "TreeTrunk";
            trunk.transform.position = position + new Vector3(0f, 2f, 0f);
            trunk.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
            var trunkMat = new Material(Shader.Find("Standard"));
            trunkMat.color = new Color(0.3f, 0.2f, 0.1f);
            trunk.GetComponent<Renderer>().material = trunkMat;
            Object.Destroy(trunk.GetComponent<Collider>());

            // Canopy
            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "TreeCanopy";
            canopy.transform.position = position + new Vector3(0f, 5f, 0f);
            canopy.transform.localScale = new Vector3(
                Random.Range(3f, 5f),
                Random.Range(2.5f, 4f),
                Random.Range(3f, 5f)
            );
            var canopyMat = new Material(Shader.Find("Standard"));
            canopyMat.color = new Color(
                Random.Range(0.1f, 0.2f),
                Random.Range(0.3f, 0.5f),
                Random.Range(0.05f, 0.15f)
            );
            canopy.GetComponent<Renderer>().material = canopyMat;
            Object.Destroy(canopy.GetComponent<Collider>());
        }
    }

    /// <summary>
    /// Simple water surface animation.
    /// </summary>
    public class WaterWave : MonoBehaviour
    {
        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
        }

        private void Update()
        {
            float y = startPos.y + Mathf.Sin(Time.time * 0.5f) * 0.05f;
            transform.position = new Vector3(startPos.x, y, startPos.z);
        }
    }
}
