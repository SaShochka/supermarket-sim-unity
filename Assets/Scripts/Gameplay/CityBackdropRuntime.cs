using UnityEngine;

namespace SupermarketSim.Gameplay
{
    public static class CityBackdropRuntime
    {
        private const string RootName = "CityBackdrop";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBackdrop()
        {
            var existing = GameObject.Find(RootName);
            if (existing != null)
                Object.Destroy(existing);

            var textures = new[]
            {
                Resources.Load<Texture2D>("city_skyline_silhouette"),
                Resources.Load<Texture2D>("city_skyline_silhouette_2"),
                Resources.Load<Texture2D>("city_skyline_silhouette_3")
            };
            if (textures[0] == null)
                textures[0] = GenerateSkylineTexture(0);
            if (textures[1] == null)
                textures[1] = GenerateSkylineTexture(1);
            if (textures[2] == null)
                textures[2] = GenerateSkylineTexture(2);

            var root = new GameObject(RootName).transform;

            CreatePanel(root, "CityBackdrop_Front", new Vector3(0f, 12f, -84f), Quaternion.identity, new Vector3(150f, 30f, 1f), CreateMaterial(textures[0]));
            CreatePanel(root, "CityBackdrop_Left", new Vector3(-78f, 10f, -48f), Quaternion.Euler(0f, 90f, 0f), new Vector3(90f, 24f, 1f), CreateMaterial(textures[1] != null ? textures[1] : textures[0]));
            CreatePanel(root, "CityBackdrop_Right", new Vector3(78f, 10f, -48f), Quaternion.Euler(0f, -90f, 0f), new Vector3(90f, 24f, 1f), CreateMaterial(textures[2] != null ? textures[2] : textures[0]));
        }

        private static void CreatePanel(Transform root, string name, Vector3 position, Quaternion rotation, Vector3 scale, Material mat)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(root, false);
            quad.transform.position = position;
            quad.transform.rotation = rotation;
            quad.transform.localScale = scale;

            var collider = quad.GetComponent<Collider>();
            if (collider != null)
                Object.Destroy(collider);

            var renderer = quad.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = mat;
        }

        private static Material CreateMaterial(Texture2D texture)
        {
            var shader = Shader.Find("HDRP/Unlit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Unlit/Transparent");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");

            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", texture);
            if (mat.HasProperty("_UnlitColorMap")) mat.SetTexture("_UnlitColorMap", texture);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
            if (mat.HasProperty("_SurfaceType")) mat.SetFloat("_SurfaceType", 1f);
            if (mat.HasProperty("_BlendMode")) mat.SetFloat("_BlendMode", 0f);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return mat;
        }

        private static Texture2D GenerateSkylineTexture(int variant)
        {
            const int width = 1024;
            const int height = 432;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "GeneratedCitySkyline_" + variant;

            var clear = new Color32(0, 0, 0, 0);
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = clear;

            var buildings = GetBuildings(variant);
            var dark = new Color32(18, 24, 28, 240);
            var window = new Color32(38, 44, 46, 180);

            foreach (var b in buildings)
            {
                FillRect(pixels, width, height, b.x, b.y, b.width, b.height, dark);
                if (b.width < 90)
                    FillRect(pixels, width, height, b.x + b.width / 2 - 3, Mathf.Max(30, b.y - 42), 6, 42, dark);

                for (int x = b.x + 14; x < b.x + b.width - 10; x += 24)
                {
                    for (int y = b.y + 24; y < b.y + b.height - 24; y += 36)
                    {
                        if ((x + y + variant) % 3 == 0)
                            FillRect(pixels, width, height, x, y, 5, 8, window);
                    }
                }
            }

            FillRect(pixels, width, height, 0, 420, width, 12, dark);
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static RectInt[] GetBuildings(int variant)
        {
            if (variant == 1)
            {
                return new[]
                {
                    new RectInt(0, 235, 95, 185), new RectInt(110, 150, 70, 270), new RectInt(195, 250, 90, 170),
                    new RectInt(300, 205, 60, 215), new RectInt(380, 115, 85, 305), new RectInt(485, 245, 85, 175),
                    new RectInt(590, 160, 60, 260), new RectInt(670, 220, 100, 200), new RectInt(790, 180, 85, 240),
                    new RectInt(890, 260, 134, 160)
                };
            }

            if (variant == 2)
            {
                return new[]
                {
                    new RectInt(0, 280, 130, 140), new RectInt(145, 210, 80, 210), new RectInt(240, 245, 75, 175),
                    new RectInt(335, 160, 55, 260), new RectInt(410, 225, 115, 195), new RectInt(545, 130, 75, 290),
                    new RectInt(640, 255, 80, 165), new RectInt(740, 190, 90, 230), new RectInt(850, 235, 90, 185),
                    new RectInt(960, 170, 64, 250)
                };
            }

            return new[]
            {
                new RectInt(0, 260, 120, 160), new RectInt(130, 220, 80, 200), new RectInt(225, 170, 75, 250),
                new RectInt(310, 245, 85, 175), new RectInt(410, 135, 60, 285), new RectInt(485, 230, 80, 190),
                new RectInt(580, 185, 100, 235), new RectInt(700, 255, 105, 165), new RectInt(825, 205, 85, 215),
                new RectInt(930, 150, 80, 270)
            };
        }

        private static void FillRect(Color32[] pixels, int width, int height, int x, int y, int w, int h, Color32 color)
        {
            int xMin = Mathf.Clamp(x, 0, width);
            int xMax = Mathf.Clamp(x + w, 0, width);
            int yMin = Mathf.Clamp(y, 0, height);
            int yMax = Mathf.Clamp(y + h, 0, height);

            for (int yy = yMin; yy < yMax; yy++)
            {
                int row = yy * width;
                for (int xx = xMin; xx < xMax; xx++)
                    pixels[row + xx] = color;
            }
        }
    }
}
