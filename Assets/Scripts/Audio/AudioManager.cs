using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Created automatically at runtime. Loads clips from Resources/Audio/SFX and Resources/Audio/Music by name;
// a missing clip is skipped silently. Master volume stays on AudioListener.volume (menu slider).
public class AudioManager : MonoBehaviour
{
    private const float FadeSeconds = 1.5f;
    private const float DuckedFraction = 0.2f;
    private const float MusicVolume = 0.6f;

    public static AudioManager Instance { get; private set; }
    public static MusicState CurrentState => Instance != null ? Instance.current : MusicState.Silent;

    private AudioSource sfxSource;
    private readonly AudioSource[] musicSources = new AudioSource[2];
    private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    private readonly DangerTracker danger = new DangerTracker();

    private int activeMusic;
    private bool menuOpen;
    private MusicState current = MusicState.Silent;
    private float duckUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (Instance == null)
        {
            new GameObject("AudioManager").AddComponent<AudioManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = NewSource(false);
        for (int i = 0; i < musicSources.Length; i++)
        {
            musicSources[i] = NewSource(true);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private AudioSource NewSource(bool loop)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.volume = loop ? 0f : 1f;
        return source;
    }

    // A reloaded scene has new turrets and enemies, so old danger sources no longer exist.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        danger.Clear();
        menuOpen = false;
    }

    private void Update()
    {
        MusicState target = MusicStateResolver.Resolve(menuOpen, danger.Active);
        if (target != current)
        {
            SwitchMusic(target);
        }

        // Unscaled time keeps fades running while the menu sets timeScale to 0.
        float step = MusicVolume / FadeSeconds * Time.unscaledDeltaTime;
        float duck = Time.unscaledTime < duckUntil ? DuckedFraction : 1f;

        for (int i = 0; i < musicSources.Length; i++)
        {
            AudioSource source = musicSources[i];
            float goal = (i == activeMusic && source.clip != null) ? MusicVolume * duck : 0f;
            source.volume = Mathf.MoveTowards(source.volume, goal, step);

            if (i != activeMusic && source.volume <= 0f && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    private void SwitchMusic(MusicState state)
    {
        current = state;
        activeMusic = 1 - activeMusic;

        AudioSource next = musicSources[activeMusic];
        next.clip = Load("Audio/Music/" + state.ToString().ToLowerInvariant());
        if (next.clip != null)
        {
            next.volume = 0f;
            next.Play();
        }
    }

    private AudioClip Load(string path)
    {
        if (!cache.TryGetValue(path, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>(path);
            cache[path] = clip;
        }
        return clip;
    }

    public static void PlaySfx(string name, float volume = 1f)
    {
        if (Instance == null) return;
        AudioClip clip = Instance.Load("Audio/SFX/" + name);
        if (clip != null)
        {
            Instance.sfxSource.PlayOneShot(clip, volume);
        }
    }

    // Plays a short jingle and ducks the music underneath it.
    public static void PlayStinger(string name)
    {
        if (Instance == null) return;
        AudioClip clip = Instance.Load("Audio/SFX/" + name);
        if (clip != null)
        {
            Instance.sfxSource.PlayOneShot(clip);
            Instance.duckUntil = Time.unscaledTime + clip.length;
        }
    }

    public static void SetDanger(Object source, bool nearby)
    {
        if (Instance == null || source == null) return;
        Instance.danger.Set(source.GetInstanceID(), nearby);
    }

    public static void SetMenuOpen(bool open)
    {
        if (Instance == null) return;
        Instance.menuOpen = open;
    }
}
