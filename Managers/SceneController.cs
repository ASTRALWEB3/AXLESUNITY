using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private RawImage screenshotImage;
    [SerializeField] private Slider progressBar;

    [Header("Settings")]
    [SerializeField] private float slideDuration = 0.75f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float canvasWidth;
    private RectTransform screenshotRect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (loadingCanvas != null) loadingCanvas.SetActive(false);
        if (screenshotImage != null)
        {
            screenshotRect = screenshotImage.GetComponent<RectTransform>();
        }

        // Cache screen width
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.rootCanvas != null)
        {
            RectTransform canvasRect = canvas.rootCanvas.GetComponent<RectTransform>();
            canvasWidth = canvasRect.rect.width;
        }
        else
        {
            canvasWidth = Screen.width;
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneRoutineIndex(sceneIndex));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // 1. CAPTURE SCREENSHOT
        yield return StartCoroutine(CaptureScreen());

        // 2. SHOW CANVAS (Now holding the screenshot)
        if (loadingCanvas != null) loadingCanvas.SetActive(true);
        if (progressBar != null) progressBar.value = 0;

        // 3. LOAD SCENE
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (progressBar != null) progressBar.value = progress;

            if (operation.progress >= 0.9f)
            {
                // Wait a tiny bit to ensure the new scene renders at least one frame
                yield return new WaitForSeconds(0.1f);
                operation.allowSceneActivation = true;
            }
            yield return null;
        }

        // Wait for the new scene to fully initialize
        yield return new WaitForSeconds(0.2f);

        // 4. SLIDE OUT THE SCREENSHOT (Left)
        // This reveals the new scene underneath
        yield return StartCoroutine(SlideScreenshot(Vector2.zero, new Vector2(-canvasWidth, 0)));

        // 5. CLEANUP
        if (loadingCanvas != null) loadingCanvas.SetActive(false);
        // Reset position for next time
        if (screenshotRect != null) screenshotRect.anchoredPosition = Vector2.zero;
    }

    // Duplicate logic for Index loading
    private IEnumerator LoadSceneRoutineIndex(int sceneIndex)
    {
        yield return StartCoroutine(CaptureScreen());

        if (loadingCanvas != null) loadingCanvas.SetActive(true);
        if (progressBar != null) progressBar.value = 0;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (progressBar != null) progressBar.value = progress;

            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.1f);
                operation.allowSceneActivation = true;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(SlideScreenshot(Vector2.zero, new Vector2(-canvasWidth, 0)));

        if (loadingCanvas != null) loadingCanvas.SetActive(false);
        if (screenshotRect != null) screenshotRect.anchoredPosition = Vector2.zero;
    }

    private IEnumerator CaptureScreen()
    {
        // Wait for end of frame so UI is drawn
        yield return new WaitForEndOfFrame();

        // Create texture
        Texture2D screenTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        // Read screen pixels
        screenTex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenTex.Apply();

        // Apply to RawImage
        if (screenshotImage != null)
        {
            screenshotImage.texture = screenTex;
            screenshotImage.color = Color.white; // Ensure it's visible

            // Reset position to center
            if (screenshotRect != null) screenshotRect.anchoredPosition = Vector2.zero;
        }
    }

    private IEnumerator SlideScreenshot(Vector2 startPos, Vector2 endPos)
    {
        if (screenshotRect == null) yield break;

        float time = 0;
        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float t = time / slideDuration;
            float curveValue = slideCurve.Evaluate(t);

            screenshotRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            yield return null;
        }

        screenshotRect.anchoredPosition = endPos;
    }
}
