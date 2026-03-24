using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Visvang.UI
{
    /// <summary>
    /// Full-screen splash screen showing the Visvang artwork on game launch.
    /// Fades in, holds, then fades out to reveal the main menu.
    /// Loads from Resources/Sprites/splash_screen.png.
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        private Canvas splashCanvas;
        private CanvasGroup canvasGroup;
        private Image splashImage;
        private bool isComplete;

        public bool IsComplete => isComplete;
        public event System.Action OnSplashComplete;

        public void Show(float fadeInTime = 0.5f, float holdTime = 2.5f, float fadeOutTime = 0.8f)
        {
            // Load the splash image from Resources
            Sprite splashSprite = LoadSplashSprite();
            if (splashSprite == null)
            {
                Debug.LogWarning("[SplashScreen] No splash_screen.png found in Resources/Sprites/. Skipping.");
                CompleteSplash();
                return;
            }

            CreateSplashUI(splashSprite);
            StartCoroutine(SplashSequence(fadeInTime, holdTime, fadeOutTime));
        }

        private Sprite LoadSplashSprite()
        {
            // Try loading as Sprite first
            Sprite sprite = Resources.Load<Sprite>("Sprites/splash_screen");
            if (sprite != null) return sprite;

            // Try loading as Texture2D and converting
            Texture2D tex = Resources.Load<Texture2D>("Sprites/splash_screen");
            if (tex != null)
            {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            return null;
        }

        private void CreateSplashUI(Sprite splashSprite)
        {
            // Create a separate canvas that renders on top of everything
            var canvasGo = new GameObject("SplashCanvas");
            splashCanvas = canvasGo.AddComponent<Canvas>();
            splashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            splashCanvas.sortingOrder = 999; // On top of everything

            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            canvasGo.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;

            canvasGroup = canvasGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // Black background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = Color.black;

            // Splash image — fill screen while maintaining aspect ratio
            var imgGo = new GameObject("SplashImage");
            imgGo.transform.SetParent(canvasGo.transform, false);
            var imgRect = imgGo.AddComponent<RectTransform>();
            imgRect.anchorMin = Vector2.zero;
            imgRect.anchorMax = Vector2.one;
            imgRect.offsetMin = Vector2.zero;
            imgRect.offsetMax = Vector2.zero;

            splashImage = imgGo.AddComponent<Image>();
            splashImage.sprite = splashSprite;
            splashImage.preserveAspect = true;
            splashImage.color = Color.white;

            // "Tap to start" text at the bottom (appears after hold)
            var tapGo = new GameObject("TapText");
            tapGo.transform.SetParent(canvasGo.transform, false);
            var tapRect = tapGo.AddComponent<RectTransform>();
            tapRect.anchorMin = new Vector2(0.5f, 0.05f);
            tapRect.anchorMax = new Vector2(0.5f, 0.05f);
            tapRect.sizeDelta = new Vector2(600, 50);

            var tapText = tapGo.AddComponent<Text>();
            tapText.text = "Tap to start fishing!";
            tapText.fontSize = 28;
            tapText.color = new Color(1f, 1f, 1f, 0.8f);
            tapText.alignment = TextAnchor.MiddleCenter;
            tapText.fontStyle = FontStyle.Italic;
            tapText.font = TextHelper.GetFont();
        }

        private IEnumerator SplashSequence(float fadeIn, float hold, float fadeOut)
        {
            // Fade in
            float t = 0f;
            while (t < fadeIn)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeIn);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // Hold — but also allow tap to skip
            float holdTimer = 0f;
            while (holdTimer < hold)
            {
                holdTimer += Time.unscaledDeltaTime;

                // Tap to skip (after at least 1 second)
                if (holdTimer > 1f && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
                    break;

                yield return null;
            }

            // Fade out
            t = 0f;
            while (t < fadeOut)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeOut);
                yield return null;
            }

            CompleteSplash();
        }

        private void CompleteSplash()
        {
            isComplete = true;

            if (splashCanvas != null)
                Destroy(splashCanvas.gameObject);

            OnSplashComplete?.Invoke();
        }
    }
}
