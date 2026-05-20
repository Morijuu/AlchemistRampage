using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource footstepSource;

    [Header("Volume")]
    [Range(0f,1f)]
    public float musicVolume = 0.4f;

    [Range(0f,1f)]
    public float sfxVolume = 0.8f;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip winMusic;
    public AudioClip loseMusic;

    [Header("Player")]
    public AudioClip shootSound;
    public AudioClip playerHitSound;
    public AudioClip playerDeathSound;
    public AudioClip footstepSound;
    public AudioClip healSound;

    [Header("Bullets")]
    public AudioClip pickupBulletSound;
    public AudioClip mergeBulletSound;

    [Header("Zombie")]
    public AudioClip zombieIdle1;
    public AudioClip zombieIdle2;
    public AudioClip zombieIdle3;
    public AudioClip zombieHitSound;
    public AudioClip zombieDeathSound;

    [Header("UI")]
    public AudioClip buttonSound;
    public AudioClip uiClickSound;

    [Header("Objects")]
    public AudioClip crateBreakSound;

    private bool clickedLastFrame = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        musicSource.playOnAwake = false;
        musicSource.loop = true;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        footstepSource.playOnAwake = false;
        footstepSource.loop = true;
    }

    private void Update()
    {
        musicSource.volume = musicVolume;

        sfxSource.volume = sfxVolume;

        footstepSource.volume = sfxVolume;

        bool click = Input.GetMouseButton(0);

        if (click && !clickedLastFrame)
        {
            string scene =
                SceneManager
                .GetActiveScene()
                .name;

            if (scene == "InicioScene")
            {
                PlayMenuClick();
            }
            else
            {
                bool uiOpen =
                    UIManager.Instance != null &&
                    UIManager.Instance.AnyMenuOpen();

                if (uiOpen)
                {
                    PlayUIClick();
                }
            }
        }

        clickedLastFrame = click;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        StopFootsteps();

        if (
            scene.name ==
            "InicioScene"
        )
        {
            PlayMenuMusic();
        }
        else
        {
            PlayGameMusic();
        }
    }

    void PlaySFX(
        AudioClip clip
    )
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(
            clip
        );
    }

    public void PlayMenuMusic()
    {
        if (
            menuMusic == null
        )
            return;

        if (
            musicSource.clip ==
            menuMusic &&
            musicSource.isPlaying
        )
            return;

        musicSource.Stop();

        musicSource.clip =
            menuMusic;

        musicSource.loop =
            true;

        musicSource.Play();
    }

    public void PlayGameMusic()
    {
        if (
            gameMusic == null
        )
            return;

        if (
            musicSource.clip ==
            gameMusic &&
            musicSource.isPlaying
        )
            return;

        musicSource.Stop();

        musicSource.clip =
            gameMusic;

        musicSource.loop =
            true;

        musicSource.Play();
    }

    public void PlayWin()
    {
        StopFootsteps();

        musicSource.Stop();

        musicSource.clip =
            winMusic;

        musicSource.loop =
            false;

        musicSource.Play();
    }

    public void PlayLose()
    {
        StopFootsteps();

        musicSource.Stop();

        musicSource.clip =
            loseMusic;

        musicSource.loop =
            false;

        musicSource.Play();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void PlayShoot()
    {
        PlaySFX(
            shootSound
        );
    }

    public void PlayPlayerHit()
    {
        PlaySFX(
            playerHitSound
        );
    }

    public void PlayPlayerDeath()
    {
        PlaySFX(
            playerDeathSound
        );
    }

    public void PlayHeal()
    {
        PlaySFX(
            healSound
        );
    }

    public void PlayPickupBullet()
    {
        PlaySFX(
            pickupBulletSound
        );
    }

    public void PlayMergeBullet()
    {
        PlaySFX(
            mergeBulletSound
        );
    }

    public void PlayZombieHit()
    {
        PlaySFX(
            zombieHitSound
        );
    }

    public void PlayZombieDeath()
    {
        PlaySFX(
            zombieDeathSound
        );
    }

    public void PlayCrateBreak()
    {
        PlaySFX(
            crateBreakSound
        );
    }

    public void PlayMenuClick()
    {
        PlaySFX(
            buttonSound
        );
    }

    public void PlayUIClick()
    {
        PlaySFX(
            uiClickSound
        );
    }

    public void PlayZombieIdle()
    {
        AudioClip[] clips =
        {
            zombieIdle1,
            zombieIdle2,
            zombieIdle3
        };

        AudioClip clip =
            clips[
                Random.Range(
                    0,
                    clips.Length
                )
            ];

        PlaySFX(
            clip
        );
    }

    public void StartFootsteps()
    {
        if (
            footstepSound == null
        )
            return;

        if (
            footstepSource.isPlaying
        )
            return;

        footstepSource.clip =
            footstepSound;

        footstepSource.Play();
    }

    public void StopFootsteps()
    {
        footstepSource.Stop();
    }
}