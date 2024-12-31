using UnityEngine;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GameLoadingText : MonoBehaviour
{
    TextMeshProUGUI text;

    private void Awake()
    {
        if (!TryGetComponent<TextMeshProUGUI>(out text))
        {
            Debug.LogError("No TextMeshProUGUI component found on GameLoadingText object.");
            Destroy(this);
        }
        text.text = "Game Loading...";
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        LoadPlaySceneFromTitle.Instance.sceneReady.AddListener(SetLoadingTextReady);
    }

    private void OnDestroy()
    {
        LoadPlaySceneFromTitle.Instance.sceneReady.RemoveListener(SetLoadingTextReady);
    }

    public void SetLoadingTextReady()
    {
        text.text = "Press [Enter] to Start Game";
    }
}
