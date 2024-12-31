using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
public class IntroVideoController : MonoBehaviour
{
    public static IntroVideoController Instance;
    public bool isPlaying;
    public VideoPlayer videoPlayer;
    public AudioSource musicPlayer;
    public AudioClip introMusic;
    private InputSystem_Actions inputActions;
    private InputAction skipVideo;


    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        skipVideo = inputActions.Player.EndTurn;
    }
    private void OnEnable()
    {
        inputActions.Enable();
        skipVideo.performed += SkipVideo;
    }
    
    private void OnDisable()
    {
        inputActions.Disable();
        skipVideo.performed -= SkipVideo;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        videoPlayer.Play();
        musicPlayer.clip = introMusic;
        musicPlayer.Play();
        isPlaying = true;

    }

    private void SkipVideo(InputAction.CallbackContext context)
    {
        if (isPlaying)
        {
            videoPlayer.Stop();
            musicPlayer.Stop();
            isPlaying = false;
            Destroy(videoPlayer.gameObject);
            Destroy(gameObject);
        }

    }
}
