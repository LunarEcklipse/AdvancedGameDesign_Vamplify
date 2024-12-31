using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public enum FadePanelState
{
    FadingToClear,
    FadingToBlack,
    Black,
    Clear
}

[RequireComponent(typeof(Image))]
public class FadeToBlackPanel : MonoBehaviour
{
    public static FadeToBlackPanel Instance;
    private Image panel;
    public FadePanelState state = FadePanelState.Clear;
    public float fadeSpeed = 1.0f;

    private Coroutine fadeCoroutine;

    public UnityEvent fadeToBlackStart;
    public UnityEvent fadeToBlackEnd;
    public UnityEvent fadeToBlackInstant;
    public UnityEvent fadeToClearStart;
    public UnityEvent fadeToClearEnd;
    public UnityEvent fadeToClearInstant;
    public UnityEvent<FadePanelState> fadeStateChange;

    /* *** FADING COROUTINE *** */
    public IEnumerator FadeToBlackCoroutine(float time)
    {
        fadeToBlackStart.Invoke();
        state = FadePanelState.FadingToBlack;
        fadeStateChange.Invoke(state);

        float elapsedTime = 0f;
        Color color = panel.color;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / time);
            panel.color = color;
            yield return null;
        }

        color.a = 1.0f;
        panel.color = color;
        state = FadePanelState.Black;
        fadeToBlackEnd.Invoke();
        fadeStateChange.Invoke(state);
    }

    public IEnumerator FadeToClearCoroutine(float time)
    {
        fadeToClearStart.Invoke();
        state = FadePanelState.FadingToClear;
        fadeStateChange.Invoke(state);

        float elapsedTime = 0f;
        Color color = panel.color;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1.0f - Mathf.Clamp01(elapsedTime / time);
            panel.color = color;
            yield return null;
        }

        color.a = 0.0f;
        panel.color = color;
        state = FadePanelState.Clear;
        fadeToClearEnd.Invoke();
        fadeStateChange.Invoke(state);
    }

    /* *** FADE STARTERS *** */
    public void FadeToBlackInstant()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        state = FadePanelState.Black;
        panel.color = new(panel.color.r, panel.color.g, panel.color.b, 1.0f);
        fadeToBlackInstant.Invoke();
        fadeToBlackEnd.Invoke();
        fadeStateChange.Invoke(FadePanelState.Black);
    }
    public void FadeToClearInstant()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        state = FadePanelState.Clear;
        panel.color = new(panel.color.r, panel.color.g, panel.color.b, 0.0f);
        fadeToClearInstant.Invoke();
        fadeToClearEnd.Invoke();
        fadeStateChange.Invoke(FadePanelState.Clear);
    }
    public void FadeToBlack(float transitionSpeed)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToBlackCoroutine(transitionSpeed));
    }
    public void FadeToClear(float transitionSpeed)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToClearCoroutine(transitionSpeed));
    }
    public void FadeToBlack() { FadeToBlack(fadeSpeed); }
    public void FadeToClear() { FadeToClear(fadeSpeed); }

    /* *** MONOBEHAVIOUR FUNCTIONS *** */
    private void Awake()
    {
        if (Instance == null) // Singleton pattern
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        fadeToBlackStart = new();
        fadeToBlackEnd = new();
        fadeToBlackInstant = new();
        fadeToClearStart = new();
        fadeToClearEnd = new();
        fadeToClearInstant = new();
        fadeStateChange = new();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!TryGetComponent<Image>(out panel))
        {
            Debug.LogError("FadeToBlackPanel script requires an Image component on the same GameObject, but one was not found.");
        }
        panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, 1.0f);
    }
    private void OnDestroy() // Clean up event listeners to prevent leaks
    {
        fadeToBlackEnd.RemoveAllListeners();
        fadeToBlackStart.RemoveAllListeners();
        fadeToBlackInstant.RemoveAllListeners();
        fadeToClearEnd.RemoveAllListeners();
        fadeToClearStart.RemoveAllListeners();
        fadeToClearInstant.RemoveAllListeners();
        fadeStateChange.RemoveAllListeners();
        Instance = null;
    }
}