using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SupermarketSim.Architecture
{
    public class MainMenuUI : MonoBehaviour
    {
        private const string BackgroundResourceName = "menu_supermarket_background";
        private const string GameSceneName = "Game";
        private const string MenuSceneName = "Menu";
        private RectTransform titleRect;
        private Vector2 titleBasePosition;
        private RectTransform playButtonRect;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureMenuExists()
        {
            if (SceneManager.GetActiveScene().name != MenuSceneName)
                return;

            if (Object.FindAnyObjectByType<MainMenuUI>() != null)
                return;

            new GameObject("MainMenuRuntime").AddComponent<MainMenuUI>();
        }

        private void Awake()
        {
            if (Object.FindObjectsByType<MainMenuUI>(FindObjectsInactive.Exclude).Length > 1)
            {
                Destroy(this);
                return;
            }

            EnsureEventSystem();
            BuildMenu();
            StartMenuMusic();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (titleRect == null)
                return;

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 2.2f) * 0.035f;
            float bob = Mathf.Sin(Time.unscaledTime * 1.35f) * 8f;
            titleRect.localScale = Vector3.one * pulse;
            titleRect.anchoredPosition = titleBasePosition + new Vector2(0f, bob);

            if (playButtonRect != null)
            {
                float buttonPulse = 1f + Mathf.Sin(Time.unscaledTime * 3.1f) * 0.018f;
                playButtonRect.localScale = new Vector3(buttonPulse, buttonPulse, 1f);
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
                return;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

        private void BuildMenu()
        {
            var canvasObject = new GameObject("MainMenuCanvas");
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            CreateBackground(canvasObject.transform);
            CreateDarkOverlay(canvasObject.transform);
            CreateTitle(canvasObject.transform);
            CreatePlayButton(canvasObject.transform);
        }

        private void StartMenuMusic()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = MakeMenuLoop();
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0.28f;
            source.spatialBlend = 0f;
            source.Play();
        }

        private static void CreateBackground(Transform parent)
        {
            var imageObject = new GameObject("SupermarketBackground");
            imageObject.transform.SetParent(parent, false);

            var image = imageObject.AddComponent<Image>();
            var texture = Resources.Load<Texture2D>(BackgroundResourceName);
            if (texture != null)
            {
                image.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image.preserveAspect = false;
            }
            image.color = Color.white;

            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreateDarkOverlay(Transform parent)
        {
            var overlayObject = new GameObject("DarkOverlay");
            overlayObject.transform.SetParent(parent, false);

            var image = overlayObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.42f);

            var rect = overlayObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void CreateTitle(Transform parent)
        {
            var titleObject = new GameObject("Title");
            titleObject.transform.SetParent(parent, false);

            var text = titleObject.AddComponent<Text>();
            text.text = "Симулятор мерчендайзера";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 72;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.96f, 0.78f);

            var shadow = titleObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(4f, -4f);

            var rect = titleObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1300f, 150f);
            rect.anchoredPosition = new Vector2(0f, 185f);
            titleRect = rect;
            titleBasePosition = rect.anchoredPosition;
        }

        private void CreatePlayButton(Transform parent)
        {
            var buttonObject = new GameObject("PlayButton");
            buttonObject.transform.SetParent(parent, false);

            var glowObject = new GameObject("OuterGlow");
            glowObject.transform.SetParent(buttonObject.transform, false);
            var glow = glowObject.AddComponent<Image>();
            glow.color = new Color(1f, 0.62f, 0.08f, 0.28f);
            glow.raycastTarget = false;
            var glowRect = glowObject.GetComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-26f, -18f);
            glowRect.offsetMax = new Vector2(26f, 18f);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.95f, 0.42f, 0.05f, 0.98f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = new Color(0.95f, 0.42f, 0.05f, 0.98f),
                highlightedColor = new Color(1f, 0.62f, 0.08f, 1f),
                pressedColor = new Color(0.72f, 0.24f, 0.02f, 1f),
                selectedColor = new Color(1f, 0.52f, 0.06f, 1f),
                disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f),
                colorMultiplier = 1f,
                fadeDuration = 0.12f
            };
            button.onClick.AddListener(() => SceneManager.LoadScene(GameSceneName));

            var outline = buttonObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.08f, 0f, 0.9f);
            outline.effectDistance = new Vector2(5f, -5f);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(470f, 116f);
            rect.anchoredPosition = new Vector2(0f, -55f);
            playButtonRect = rect;

            CreateButtonLayer(buttonObject.transform, "BottomShade", new Color(0.28f, 0.09f, 0.02f, 0.38f), new Vector2(12f, 12f), new Vector2(-12f, -58f));
            CreateButtonLayer(buttonObject.transform, "InnerFrame", new Color(1f, 0.9f, 0.45f, 0.55f), new Vector2(12f, 12f), new Vector2(-12f, -12f));
            CreateButtonLayer(buttonObject.transform, "InnerFill", new Color(1f, 0.62f, 0.09f, 0.34f), new Vector2(20f, 20f), new Vector2(-20f, -20f));
            CreateButtonLayer(buttonObject.transform, "LeftAccent", new Color(1f, 0.95f, 0.55f, 0.8f), new Vector2(26f, 18f), new Vector2(-400f, -18f));
            CreateButtonLayer(buttonObject.transform, "RightAccent", new Color(1f, 0.95f, 0.55f, 0.8f), new Vector2(400f, 18f), new Vector2(-26f, -18f));

            var highlightObject = new GameObject("TopHighlight");
            highlightObject.transform.SetParent(buttonObject.transform, false);
            var highlight = highlightObject.AddComponent<Image>();
            highlight.color = new Color(1f, 1f, 1f, 0.26f);
            highlight.raycastTarget = false;
            var highlightRect = highlightObject.GetComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(0f, 0.55f);
            highlightRect.anchorMax = Vector2.one;
            highlightRect.offsetMin = new Vector2(18f, 0f);
            highlightRect.offsetMax = new Vector2(-18f, -14f);

            var textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform, false);

            var text = textObject.AddComponent<Text>();
            text.text = "Играть";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 50;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            var textShadow = textObject.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0.2f, 0.08f, 0f, 0.9f);
            textShadow.effectDistance = new Vector2(3f, -3f);

            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private static void CreateButtonLayer(Transform parent, string name, Color color, Vector2 offsetMin, Vector2 offsetMax)
        {
            var layerObject = new GameObject(name);
            layerObject.transform.SetParent(parent, false);
            var image = layerObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            var rect = layerObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static AudioClip MakeMenuLoop()
        {
            int sampleRate = AudioSettings.outputSampleRate;
            float duration = 16f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            float[] notes = { 196f, 246.94f, 293.66f, 329.63f, 293.66f, 246.94f, 220f, 261.63f };
            float beat = 0.5f;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                int step = Mathf.FloorToInt(t / beat);
                float local = (t - step * beat) / beat;
                float envelope = (1f - Mathf.SmoothStep(0.2f, 1f, local)) * Mathf.SmoothStep(0f, 1f, local * 8f);
                float note = notes[step % notes.Length];
                float bass = Mathf.Sin(2f * Mathf.PI * note * 0.5f * t) * 0.08f;
                float melody = Mathf.Sin(2f * Mathf.PI * note * t) * 0.07f * envelope;
                float shimmer = Mathf.Sin(2f * Mathf.PI * note * 2f * t) * 0.025f * envelope;
                float pad = Mathf.Sin(2f * Mathf.PI * 98f * t) * 0.035f;
                data[i] = Mathf.Clamp(bass + melody + shimmer + pad, -0.8f, 0.8f);
            }

            var clip = AudioClip.Create("MainMenuMusic", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
