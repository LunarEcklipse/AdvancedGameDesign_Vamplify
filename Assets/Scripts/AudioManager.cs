using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public enum MusicState
{
    NOT_PLAYING, // Both audio sources are not playing music.
    PRIMARY_PLAYING, // The primary audio source is playing music.
    SECONDARY_PLAYING, // The secondary audio source is playing music.
    FADING_TO_PRIMARY, // The secondary audio source is fading out and the primary audio source is fading in.
    FADING_TO_SECONDARY, // The primary audio source is fading out and the secondary audio source is fading in.
    FADING_TO_NOT_PLAYING, // Both audio sources are fading out.
    FADING_PRIMARY_FROM_NOT_PLAYING, // Primary is fading in after nothing was playing.
    FADING_SECONDARY_FROM_NOT_PLAYING // Secondary is fading in after nothing was playing.
}

public class AudioManager : MonoBehaviour
{
    private MusicState musicState = MusicState.NOT_PLAYING;
    private AudioSource musicManagerPrimary; // We switch between these two audio sources to fade between music.
    private AudioSource musicManagerSecondary; // See above
    private AudioClip currentPlayingMusic;
    private AudioClip nextPlayingMusic;
    [Range(0, 1)] public float musicVolume = 0.5f;
    [Range(0, 1)] public float sfxVolume = 0.5f;
    public float fadeSpeed = 1.0f;

    private Coroutine currentFadingPrimaryCoroutine;
    private Coroutine currentFadingSecondaryCoroutine;

    public static AudioManager Instance;

    [Header("Damage SFX Clips")]
    public AudioClip physicalDamageSFX;
    public AudioClip bloodDamageSFX;
    public AudioClip fireDamageSFX;
    public AudioClip iceDamageSFX;
    public AudioClip lightningDamageSFX;
    public AudioClip poisonDamageSFX;
    public AudioClip radiantDamageSFX;
    public AudioClip voidDamageSFX;
    public AudioClip healSFX;
    public AudioClip missSFX;

    [Header("Music Clips")]
    public AudioClip dungeonTheme1;
    public AudioClip dungeonTheme2;
    public AudioClip combatTheme;
    public AudioClip bossTheme;

    private bool isPlayingTheme1 = false;

    private IEnumerator FadeInPrimary(AudioClip newMusic, bool shouldLoop, MusicState stateWhileFadingIn, MusicState stateWhenDone)
    {
        currentPlayingMusic = newMusic;
        musicState = stateWhileFadingIn;
        // Stop the primary audio source if it's playing.
        if (musicManagerPrimary.isPlaying)
        {
            musicManagerPrimary.Stop();
        }
        // Set the new music to the primary audio source.
        musicManagerPrimary.clip = newMusic;
        musicManagerPrimary.loop = shouldLoop;
        // Set the volume to 0.
        musicManagerPrimary.volume = 0;
        // Play the music.
        musicManagerPrimary.Play();
        // While the volume is less than 1, increase the volume by the fade speed.
        while (musicManagerPrimary.volume < musicVolume)
        {
            musicManagerPrimary.volume += fadeSpeed * Time.deltaTime;
            yield return null;
        }
        musicState = stateWhenDone;
    }
    private IEnumerator FadeInSecondary(AudioClip newMusic, bool shouldLoop, MusicState stateWhileFadingIn, MusicState stateWhenDone)
    {
        currentPlayingMusic = newMusic;
        musicState = stateWhileFadingIn;
        // Stop the secondary audio source if it's playing.
        if (musicManagerSecondary.isPlaying)
        {
            musicManagerSecondary.Stop();
        }
        // Set the new music to the secondary audio source.
        musicManagerSecondary.clip = newMusic;
        musicManagerSecondary.loop = shouldLoop;
        // Set the volume to 0.
        musicManagerSecondary.volume = 0;
        // Play the music.
        musicManagerSecondary.Play();
        // While the volume is less than 1, increase the volume by the fade speed.
        while (musicManagerSecondary.volume < musicVolume)
        {
            musicManagerSecondary.volume += fadeSpeed * Time.deltaTime;
            yield return null;
        }
        musicState = stateWhenDone;
    }
    private IEnumerator FadeOutPrimary() // Use this version of the function only when something else is assigning the music state.
    {
        while (musicManagerPrimary.volume > 0)
        {
            musicManagerPrimary.volume -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        musicManagerPrimary.Stop();
    }
    private IEnumerator FadeOutPrimary(MusicState stateWhileFadingOut, MusicState stateWhenDone)
    {
        musicState = stateWhileFadingOut;
        while (musicManagerPrimary.volume > 0)
        {
            musicManagerPrimary.volume -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        musicManagerPrimary.Stop();
        musicState = stateWhenDone;
        if (stateWhenDone == MusicState.NOT_PLAYING)
        {
            currentPlayingMusic = null;
        }
    }
    private IEnumerator FadeOutSecondary() // Use this version of the function only when something else is assigning the music state.
    {
        while (musicManagerSecondary.volume > 0)
        {
            musicManagerSecondary.volume -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        musicManagerSecondary.Stop();
    }
    private IEnumerator FadeOutSecondary(MusicState stateWhileFadingOut, MusicState stateWhenDone)
    {
        musicState = stateWhileFadingOut;
        while (musicManagerSecondary.volume > 0)
        {
            musicManagerSecondary.volume -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        musicManagerSecondary.Stop();
        musicState = stateWhenDone;
        if (stateWhenDone == MusicState.NOT_PLAYING)
        {
            currentPlayingMusic = null;
        }
    }
    private void StopAllMusic(bool shouldFade)
    {
        if (shouldFade)
        {
            if (currentFadingPrimaryCoroutine != null) // Safety check
            {
                StopCoroutine(currentFadingPrimaryCoroutine);
            }
            if (currentFadingSecondaryCoroutine != null)
            {
                StopCoroutine(currentFadingSecondaryCoroutine);
            }
            if (musicManagerPrimary.isPlaying)
            {
                currentFadingPrimaryCoroutine = StartCoroutine(FadeOutPrimary(MusicState.FADING_TO_NOT_PLAYING, MusicState.NOT_PLAYING));
                if (musicManagerSecondary.isPlaying)
                {
                    currentFadingSecondaryCoroutine = StartCoroutine(FadeOutSecondary());
                }
            }
            else if (musicManagerSecondary.isPlaying)
            {
                currentFadingSecondaryCoroutine = StartCoroutine(FadeOutSecondary(MusicState.FADING_TO_NOT_PLAYING, MusicState.NOT_PLAYING));
            }
        }
        else
        {
            if (musicManagerPrimary.isPlaying)
            {
                musicManagerPrimary.Stop();
            }
            if (musicManagerSecondary.isPlaying)
            {
                musicManagerSecondary.Stop();
            }
        }
        musicState = MusicState.NOT_PLAYING;
    }

    public void PlayNewMusic(AudioClip newMusic, bool shouldLoop, bool shouldFade) // Play this music.
    {
        if (shouldFade)
        {
            switch (musicState)
            {
                case MusicState.NOT_PLAYING: // Fade the music in to primary and play from there.
                    if (currentFadingPrimaryCoroutine != null) // Safety check
                    {
                        StopCoroutine(currentFadingPrimaryCoroutine);
                    }
                    if (currentFadingSecondaryCoroutine != null)
                    {
                        StopCoroutine(currentFadingSecondaryCoroutine);
                    }
                    currentFadingPrimaryCoroutine = StartCoroutine(FadeInPrimary(newMusic, shouldLoop, MusicState.FADING_PRIMARY_FROM_NOT_PLAYING, MusicState.PRIMARY_PLAYING));
                    break;
                case MusicState.PRIMARY_PLAYING: // Fade the music out of primary and fade into secondary.
                    if (currentFadingPrimaryCoroutine != null) // Safety check
                    {
                        StopCoroutine(currentFadingPrimaryCoroutine);
                    }
                    if (currentFadingSecondaryCoroutine != null)
                    {
                        StopCoroutine(currentFadingSecondaryCoroutine);
                    }
                    currentFadingPrimaryCoroutine = StartCoroutine(FadeOutPrimary());
                    currentFadingSecondaryCoroutine = StartCoroutine(FadeInSecondary(newMusic, shouldLoop, MusicState.FADING_TO_SECONDARY, MusicState.SECONDARY_PLAYING));
                    break;
                case MusicState.SECONDARY_PLAYING: // Fade the music out of secondary and fade into primary.
                    if (currentFadingPrimaryCoroutine != null) // Safety check
                    {
                        StopCoroutine(currentFadingPrimaryCoroutine);
                    }
                    if (currentFadingSecondaryCoroutine != null)
                    {
                        StopCoroutine(currentFadingSecondaryCoroutine);
                    }
                    currentFadingSecondaryCoroutine = StartCoroutine(FadeOutSecondary());
                    currentFadingPrimaryCoroutine = StartCoroutine(FadeInPrimary(newMusic, shouldLoop, MusicState.FADING_TO_PRIMARY, MusicState.PRIMARY_PLAYING));
                    break;
                case MusicState.FADING_TO_PRIMARY: // Stop the secondary track entirely, start fading out primary from where it currently is, set new music into secondary, and fade that in instead.
                    if (currentFadingPrimaryCoroutine != null) // Safety check
                    {
                        StopCoroutine(currentFadingPrimaryCoroutine);
                    }
                    if (currentFadingSecondaryCoroutine != null)
                    {
                        StopCoroutine(currentFadingSecondaryCoroutine);
                    }
                    musicManagerSecondary.Stop();
                    currentFadingPrimaryCoroutine = StartCoroutine(FadeOutPrimary());
                    currentFadingSecondaryCoroutine = StartCoroutine(FadeInSecondary(newMusic, shouldLoop, MusicState.FADING_TO_SECONDARY, MusicState.SECONDARY_PLAYING));
                    break;
                case MusicState.FADING_TO_SECONDARY: // Stop the primary track entirely, start fading out the secondary from where it currently is, set new music into primary, and fade that in instead.
                    if (currentFadingPrimaryCoroutine != null) // Safety check
                    {
                        StopCoroutine(currentFadingPrimaryCoroutine);
                    }
                    if (currentFadingSecondaryCoroutine != null)
                    {
                        StopCoroutine(currentFadingSecondaryCoroutine);
                    }
                    musicManagerPrimary.Stop();
                    currentFadingSecondaryCoroutine = StartCoroutine(FadeOutSecondary());
                    currentFadingPrimaryCoroutine = StartCoroutine(FadeInPrimary(newMusic, shouldLoop, MusicState.FADING_TO_PRIMARY, MusicState.PRIMARY_PLAYING));
                    break;
                case MusicState.FADING_TO_NOT_PLAYING: // Stop both fade outs and then fade in the new music to primary.
                    if (currentFadingPrimaryCoroutine != null) // Get these out of here
                    {
                        StopCoroutine(currentFadingPrimaryCoroutine);
                    }
                    if (currentFadingSecondaryCoroutine != null)
                    {
                        StopCoroutine(currentFadingSecondaryCoroutine);
                    }
                    musicManagerPrimary.Stop();
                    musicManagerSecondary.Stop();
                    currentFadingPrimaryCoroutine = StartCoroutine(FadeInPrimary(newMusic, shouldLoop, MusicState.FADING_PRIMARY_FROM_NOT_PLAYING, MusicState.PRIMARY_PLAYING));
                    break;
                case MusicState.FADING_PRIMARY_FROM_NOT_PLAYING: // Fade out primary from where it is. Fade in a secondary.
                    if (currentFadingPrimaryCoroutine != null) // Safety check
                    {
                        StopCoroutine(currentFadingPrimaryCoroutine);
                    }
                    if (currentFadingSecondaryCoroutine != null)
                    {
                        StopCoroutine(currentFadingSecondaryCoroutine);
                    }
                    currentFadingPrimaryCoroutine = StartCoroutine(FadeOutPrimary());
                    currentFadingSecondaryCoroutine = StartCoroutine(FadeInSecondary(newMusic, shouldLoop, MusicState.FADING_TO_SECONDARY, MusicState.SECONDARY_PLAYING));
                    break;
                case MusicState.FADING_SECONDARY_FROM_NOT_PLAYING: // Fade out secondary from where it is. Fade in a primary.
                    if (currentFadingPrimaryCoroutine != null) // Safety check
                    {
                        StopCoroutine(currentFadingPrimaryCoroutine);
                    }
                    if (currentFadingSecondaryCoroutine != null)
                    {
                        StopCoroutine(currentFadingSecondaryCoroutine);
                    }
                    currentFadingSecondaryCoroutine = StartCoroutine(FadeOutSecondary());
                    currentFadingPrimaryCoroutine = StartCoroutine(FadeInPrimary(newMusic, shouldLoop, MusicState.FADING_TO_PRIMARY, MusicState.PRIMARY_PLAYING));
                    break;
                default: // If somehow we get here.
                    Debug.LogError("Music state is in an invalid state. Stopping all music and starting new music in primary to hopefully reset the state.");
                    StopAllMusic(false);
                    currentFadingPrimaryCoroutine = StartCoroutine(FadeInPrimary(newMusic, shouldLoop, MusicState.FADING_PRIMARY_FROM_NOT_PLAYING, MusicState.PRIMARY_PLAYING));
                    break;
            }
        }
        else
        {
            // Stop all music and play the new music.
            StopAllMusic(false);
            musicManagerPrimary.clip = newMusic;
            musicManagerPrimary.loop = shouldLoop;
            musicManagerPrimary.volume = musicVolume;
            musicManagerPrimary.Play();
        }
    }
    public void PlayNewMusic(AudioClip newMusic, bool shouldLoop) // Play this music. A shortcut for the above function that assumes we want to fade in.
    {
        PlayNewMusic(newMusic, shouldLoop, true);
    }
    public void PlayNewMusic(AudioClip newMusic) // Play this music. A shortcut for the above function that assumes we want to fade in and loop.
    {
        PlayNewMusic(newMusic, true, true);
    }
    public bool IsMusicPlaying()
    {
        return musicState != MusicState.NOT_PLAYING;
    }
    public AudioClip GetPlayingMusic()
    {
        return currentPlayingMusic;
    }
    public void ChangeMusic(AudioClip newMusic, bool shouldLoop, bool shouldFade)
    {
        PlayNewMusic(newMusic, shouldLoop, shouldFade);
    }

    /* *** SPECIFIC MUSIC PLAYERS *** */
    public void PlayDungeonTheme1()
    {
        PlayNewMusic(dungeonTheme1, true);
    }
    public void PlayDungeonTheme2()
    {
        PlayNewMusic(dungeonTheme2, true);
    }
    public void PlayCombatTheme()
    {
        PlayNewMusic(combatTheme, true);
    }
    public void PlayBossTheme()
    {
        PlayNewMusic(bossTheme, true);
    }

    /* *** SFX PLAYERS *** */
    private AudioClip GetDamageSFX(DamageType damageType, bool miss = false, bool heal = false)
    {
        if (miss) return missSFX;
        if (heal) return healSFX;
        return damageType switch
        { 
            DamageType.Physical => physicalDamageSFX, 
            DamageType.Blood => bloodDamageSFX,
            DamageType.Fire => fireDamageSFX,
            DamageType.Ice => iceDamageSFX,
            DamageType.Lightning => lightningDamageSFX,
            DamageType.Poison => poisonDamageSFX,
            DamageType.Radiant => radiantDamageSFX,
            DamageType.Void => voidDamageSFX,
            _ => missSFX
        };
    }

    public void PlayHitSoundAtLocation(Vector3 location, DamageType damageType, bool miss, bool heal)
    {

        AudioSource.PlayClipAtPoint(GetDamageSFX(damageType, miss, heal), location, sfxVolume);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        musicManagerPrimary = gameObject.AddComponent<AudioSource>();
        musicManagerSecondary = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        CombatManager.Instance.GenerateNewRoom.AddListener(OnRoomChange);
        CombatManager.Instance.CombatStart.AddListener(OnCombatStart);
        CombatManager.Instance.CombatEnded.AddListener(OnCombatEnd);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void OnRoomChange()
    {
        if (isPlayingTheme1)
        {
            isPlayingTheme1 = false;
            PlayDungeonTheme2();
        }
        else
        {
            isPlayingTheme1 = true;
            PlayDungeonTheme1();
        }
    }

    private void OnCombatStart()
    {
        PlayCombatTheme();
    }

    private void OnCombatEnd()
    {
        if (isPlayingTheme1)
        {
            PlayDungeonTheme1();
        }
        else
        {
            PlayDungeonTheme2();
        }
    }
}
