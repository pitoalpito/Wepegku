
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CircleTransition : MonoBehaviour
{
    public float transitionDuration = 0.6f;

    private Image circleImage;
    private RectTransform circleRect;
    private float fullScale;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Membuat Canvas
        GameObject canvasObject = new GameObject("TransitionCanvas");
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        // Membuat Image lingkaran
        GameObject circleObject = new GameObject("Circle");
        circleObject.transform.SetParent(canvasObject.transform, false);

        circleImage = circleObject.AddComponent<Image>();
        circleImage.color = Color.black;

        circleRect = circleImage.rectTransform;
        circleRect.anchorMin = new Vector2(0.5f, 0.5f);
        circleRect.anchorMax = new Vector2(0.5f, 0.5f);
        circleRect.anchoredPosition = Vector2.zero;
        circleRect.sizeDelta = new Vector2(100, 100);

        // Membuat sprite lingkaran secara procedural
        int size = 128;
        Texture2D texture = new Texture2D(size, size);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    new Vector2(size / 2f, size / 2f)
                );

                float alpha = Mathf.Clamp01(size / 2f - distance + 1f);

                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }

        texture.Apply();

        Sprite circleSprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f)
        );

        circleImage.sprite = circleSprite;

        // Menghitung ukuran untuk menutupi seluruh layar
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        float diagonal = Mathf.Sqrt(
            canvasRect.rect.width * canvasRect.rect.width +
            canvasRect.rect.height * canvasRect.rect.height
        );

        fullScale = (diagonal * 2f) / 100f;

        circleRect.localScale = Vector3.one * fullScale;
    }

    private IEnumerator Start()
    {
        // Saat game dimulai, lingkaran mengecil
        yield return null;
        yield return AnimateCircle(fullScale, 0.001f);
    }

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        // Lingkaran membesar
        yield return AnimateCircle(0.001f, fullScale);

        // Pindah scene setelah layar tertutup
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            yield return null;
        }

        // Lingkaran mengecil di scene baru
        yield return AnimateCircle(fullScale, 0.001f);
    }

    private IEnumerator AnimateCircle(float start, float end)
    {
        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / transitionDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            float scale = Mathf.Lerp(start, end, t);

            circleRect.localScale = Vector3.one * scale;

            yield return null;
        }

        circleRect.localScale = Vector3.one * end;
    }
}
