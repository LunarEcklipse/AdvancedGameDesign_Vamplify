using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

public class LoadPlaySceneFromTitle : MonoBehaviour
{
    private int numPresses = 1;
    public static LoadPlaySceneFromTitle Instance;
    public Scene playScene;

    private AsyncOperation sceneLoader;

    private InputSystem_Actions inputActions;
    private InputAction startGame;
    public UnityEvent sceneReady = new();

    public IEnumerator SceneLoadingCoroutine()
    {
        yield return new WaitUntil(() => sceneLoader != null);
        yield return new WaitUntil(() => sceneLoader.isDone);
        sceneReady.Invoke();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        inputActions = new InputSystem_Actions();
        startGame = inputActions.TitleScreen.StartGame;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        startGame.performed += StartGame;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        startGame.performed -= StartGame;
    }

    private void OnDestroy()
    {
        sceneReady.RemoveAllListeners();
    }
    void Start()
    {
        StartCoroutine(SceneLoadingCoroutine());
    }

    private void StartGame(InputAction.CallbackContext ctx)
    {
        if (IntroVideoController.Instance != null)
        {
            if (IntroVideoController.Instance.isPlaying)
            {
                return;
            }
            return;
        }
        if (numPresses <= 0) // This is a shitty hack to make sure the first keypress doesn't cause the video to run off and take the title screen with it.
        {
            SceneManager.LoadScene("PlayScene");
        }
        else
        {
            numPresses--;
        }        
    }
}
