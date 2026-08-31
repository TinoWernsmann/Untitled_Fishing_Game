using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Manager.Audio
{
    /// <summary>
    /// Zentrale Audio-Verwaltung für Musik und SFX.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup masterGroup;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;

        [Header("Mixer Parameters")]
        [SerializeField] private string masterVolumeParameter = "MasterVolume";
        [SerializeField] private string musicVolumeParameter = "MusicVolume";
        [SerializeField] private string sfxVolumeParameter = "SFXVolume";

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField, Range(1, 8)] private int sfxSourcePoolSize = 4;

        [Header("Startup Defaults")]
        [SerializeField] private string defaultMusicClipPath = "Audio/Fishjazz";

        private static class AudioPaths
        {
            public const string BackgroundMusic = "Audio/Fishjazz";
            public const string FishBite = "Audio/FishBite_Placeholder";
            public const string FishCaught = "Audio/FishCatched_Placeholder";
            public const string RodCut = "Audio/RodCut_Placeholder";
            public const string RodThrow = "Audio/RodThrow_Placeholder";
        }

        [Header("Startup Mixer Volumes")]
        [Tooltip("Linearer Startwert fuer die Master-Mixergruppe.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("masterVolume")]
        [SerializeField] private float startupMasterVolume = 1f;

        [Tooltip("Linearer Startwert fuer die Musik-Mixergruppe.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("musicVolume")]
        [SerializeField] private float startupMusicVolume = 1f;

        [Tooltip("Linearer Startwert fuer die SFX-Mixergruppe.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("sfxVolume")]
        [SerializeField] private float startupSfxVolume = 1f;

        private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            NormalizeMixerParameterNames();
            EnsureMusicSource();
            EnsureSfxSource();
        }

        private void Start()
        {
            ApplyStartupMixerVolumes();
        }

        private void NormalizeMixerParameterNames()
        {
            if (string.Equals(defaultMusicClipPath, "Audio/BackgroundMusic_Placeholder", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(defaultMusicClipPath))
            {
                defaultMusicClipPath = AudioPaths.BackgroundMusic;
            }

            if (string.Equals(masterVolumeParameter, "MasterVolume", StringComparison.OrdinalIgnoreCase) == false &&
                string.IsNullOrWhiteSpace(masterVolumeParameter))
            {
                masterVolumeParameter = "MasterVolume";
            }

            if (string.Equals(musicVolumeParameter, "MusicVolume", StringComparison.OrdinalIgnoreCase) == false &&
                string.IsNullOrWhiteSpace(musicVolumeParameter))
            {
                musicVolumeParameter = "MusicVolume";
            }

            if (string.Equals(sfxVolumeParameter, "SFXVolume", StringComparison.OrdinalIgnoreCase))
            {
                sfxVolumeParameter = "SFXVolume";
            }
            else if (string.Equals(sfxVolumeParameter, "SfxVolume", StringComparison.OrdinalIgnoreCase))
            {
                sfxVolumeParameter = "SFXVolume";
            }
            else if (string.IsNullOrWhiteSpace(sfxVolumeParameter))
            {
                sfxVolumeParameter = "SFXVolume";
            }
        }

        private void EnsureMusicSource()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }

            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            if (musicGroup != null)
            {
                musicSource.outputAudioMixerGroup = musicGroup;
            }
        }

        private void EnsureSfxSource()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            if (sfxGroup != null)
            {
                sfxSource.outputAudioMixerGroup = sfxGroup;
            }

            EnsureSfxSourcePool();
        }

        private void EnsureSfxSourcePool()
        {
            if (sfxSource == null)
            {
                return;
            }

            if (sfxSourcePoolSize <= 1)
            {
                sfxSourcePoolSize = 1;
            }

            if (gameObject.GetComponents<AudioSource>().Length >= sfxSourcePoolSize)
            {
                return;
            }

            int existing = gameObject.GetComponents<AudioSource>().Length;
            for (int i = existing; i < sfxSourcePoolSize; i++)
            {
                AudioSource extraSource = gameObject.AddComponent<AudioSource>();
                extraSource.playOnAwake = false;
                extraSource.loop = false;
                extraSource.spatialBlend = 0f;
                if (sfxGroup != null)
                {
                    extraSource.outputAudioMixerGroup = sfxGroup;
                }
            }
        }

        private AudioSource GetAvailableSfxSource()
        {
            EnsureSfxSource();

            AudioSource[] sources = gameObject.GetComponents<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null || sources[i] == musicSource)
                {
                    continue;
                }

                if (!sources[i].isPlaying)
                {
                    return sources[i];
                }
            }

            return sfxSource != null ? sfxSource : null;
        }

        private void ApplyStartupMixerVolumes()
        {
            SetMixerVolume(masterVolumeParameter, startupMasterVolume);
            SetMixerVolume(musicVolumeParameter, startupMusicVolume);
            SetMixerVolume(sfxVolumeParameter, startupSfxVolume);
        }

        public static AudioManager GetOrCreateInstance()
        {
            if (Instance == null)
            {
                var go = new GameObject("AudioManager");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<AudioManager>();
            }

            return Instance;
        }

        public void SetMasterVolume(float volume)
        {
            SetMixerVolume(masterVolumeParameter, startupMasterVolume * Mathf.Clamp01(volume));
        }

        public void SetMusicVolume(float volume)
        {
            SetMixerVolume(musicVolumeParameter, startupMusicVolume * Mathf.Clamp01(volume));

            if (musicSource != null)
            {
                musicSource.outputAudioMixerGroup = musicGroup;
            }
        }

        public void SetSfxVolume(float volume)
        {
            SetMixerVolume(sfxVolumeParameter, startupSfxVolume * Mathf.Clamp01(volume));
        }

        public void PlayFishBiteSfx(float volume = 1f)
        {
            PlayOneShotSfx(AudioPaths.FishBite, volume);
        }

        public void PlayFishCaughtSfx(float volume = 1f)
        {
            PlayOneShotSfx(AudioPaths.FishCaught, volume);
        }

        public void PlayRodCutSfx(float volume = 1f)
        {
            PlayOneShotSfx(AudioPaths.RodCut, volume);
        }

        public void PlayRodThrowSfx(float volume = 1f)
        {
            PlayOneShotSfx(AudioPaths.RodThrow, volume);
        }

        private void PlayOneShotSfx(string fallbackResourcePath, float volume = 1f)
        {
            if (string.IsNullOrWhiteSpace(fallbackResourcePath))
            {
                return;
            }

            AudioClip clip = GetCachedClip(fallbackResourcePath);
            if (clip == null)
            {
                return;
            }

            PlaySfxClip(clip, volume);
        }

        public void PlaySfxClipByPath(string path, float volume = 1f)
        {
            PlayOneShotSfx(path, volume);
        }

        private AudioClip GetCachedClip(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (clipCache.TryGetValue(path, out AudioClip cachedClip) && cachedClip != null)
            {
                return cachedClip;
            }

            AudioClip loadedClip = Resources.Load<AudioClip>(path);
            if (loadedClip != null)
            {
                clipCache[path] = loadedClip;
            }

            return loadedClip;
        }

        private AudioSource PlaySfxClip(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return null;
            }

            AudioSource source = GetAvailableSfxSource();
            if (source == null)
            {
                return null;
            }

            source.outputAudioMixerGroup = ResolveClipMixerGroup(clip, false);
            source.volume = Mathf.Clamp01(volume);
            source.PlayOneShot(clip);
            return source;
        }

        public void ConfigureMusicSource(AudioSource source)
        {
            ConfigureSource(source, musicGroup, true, false, true);
        }

        public void ConfigureSfxSource(AudioSource source, bool loop = false, bool spatial2D = true)
        {
            ConfigureSource(source, sfxGroup, loop, false, spatial2D);
        }

        private AudioMixerGroup ResolveClipMixerGroup(AudioClip clip, bool isMusicClip)
        {
            if (clip == null)
            {
                return isMusicClip ? musicGroup : sfxGroup;
            }

            return isMusicClip ? musicGroup : sfxGroup;
        }

        private void ConfigureSource(AudioSource source, AudioMixerGroup mixerGroup, bool loop, bool playOnAwake, bool spatial2D)
        {
            if (source == null)
            {
                return;
            }

            source.outputAudioMixerGroup = mixerGroup;
            source.loop = loop;
            source.playOnAwake = playOnAwake;
            source.spatialBlend = spatial2D ? 0f : 1f;
        }

        private void SetMixerVolume(string parameterName, float linearVolume)
        {
            if (audioMixer == null || string.IsNullOrWhiteSpace(parameterName))
            {
                return;
            }

            audioMixer.SetFloat(parameterName, LinearToDb(linearVolume));
        }

        private static float LinearToDb(float linearVolume)
        {
            if (linearVolume <= 0.0001f)
            {
                return -80f;
            }

            return Mathf.Log10(Mathf.Clamp(linearVolume, 0.0001f, 1f)) * 20f;
        }

        public AudioSource PlayMusic(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                clip = GetCachedClip(defaultMusicClipPath);
            }

            if (clip == null)
            {
                return null;
            }

            EnsureMusicSource();
            bool clipChanged = musicSource.clip != clip;
            musicSource.clip = clip;
            musicSource.volume = Mathf.Clamp01(volume);
            musicSource.loop = true;
            musicSource.outputAudioMixerGroup = ResolveClipMixerGroup(clip, true);

            if (!musicSource.isPlaying || clipChanged)
            {
                musicSource.Play();
            }

            return musicSource;
        }

        public AudioSource PlayMusicFromPath(string path, float volume = 1f)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            AudioClip clip = GetCachedClip(path);
            return PlayMusic(clip, volume);
        }

        public AudioSource PlaySoundOn(GameObject owner, AudioClip clip, float volume, bool loop = false, bool spatial2D = true, string fallbackResourcePath = null)
        {
            if (clip == null && !string.IsNullOrEmpty(fallbackResourcePath))
            {
                clip = GetCachedClip(fallbackResourcePath);
            }

            if (clip == null || owner == null)
            {
                return null;
            }

            if (owner == gameObject)
            {
                AudioSource source = GetAvailableSfxSource();
                if (source == null)
                {
                    return null;
                }

                ConfigureSfxSource(source, loop, spatial2D);
                source.outputAudioMixerGroup = ResolveClipMixerGroup(clip, false);
                source.volume = Mathf.Clamp01(volume);
                source.PlayOneShot(clip, source.volume);
                return source;
            }

            AudioSource ownedSource = owner.GetComponent<AudioSource>();
            if (ownedSource == null || ownedSource == musicSource)
            {
                ownedSource = owner.AddComponent<AudioSource>();
            }

            ConfigureSfxSource(ownedSource, loop, spatial2D);
            ownedSource.outputAudioMixerGroup = ResolveClipMixerGroup(clip, false);
            ownedSource.volume = Mathf.Clamp01(volume);
            ownedSource.PlayOneShot(clip, ownedSource.volume);

            return ownedSource;
        }

        private AudioSource EnsureManagerSfxSource()
        {
            EnsureSfxSource();
            return GetAvailableSfxSource();
        }

        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        public void StopAllSfx()
        {
            AudioSource[] sources = gameObject.GetComponents<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] != null && sources[i] != musicSource)
                {
                    sources[i].Stop();
                }
            }
        }

        public void StopAllAudio()
        {
            StopMusic();
            StopAllSfx();
        }

        public void SetMusicPitch(float pitch)
        {
            if (musicSource != null)
            {
                musicSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
            }
        }

        public void SetSfxPitch(float pitch)
        {
            AudioSource[] sources = gameObject.GetComponents<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] != null && sources[i] != musicSource)
                {
                    sources[i].pitch = Mathf.Clamp(pitch, 0.1f, 3f);
                }
            }
        }

        public void SetMusicMuted(bool muted)
        {
            if (musicSource != null)
            {
                musicSource.mute = muted;
            }
        }

        public void SetSfxMuted(bool muted)
        {
            AudioSource[] sources = gameObject.GetComponents<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] != null && sources[i] != musicSource)
                {
                    sources[i].mute = muted;
                }
            }
        }

        public bool IsMusicPlaying()
        {
            return musicSource != null && musicSource.isPlaying;
        }

        public bool IsAnySfxPlaying()
        {
            AudioSource[] sources = gameObject.GetComponents<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] != null && sources[i] != musicSource && sources[i].isPlaying)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetSfxSourceCount()
        {
            if (gameObject == null)
            {
                return 0;
            }

            return gameObject.GetComponents<AudioSource>().Length;
        }

        public void LogAudioStatus()
        {
            Debug.Log($"AudioManager status - MusicPlaying: {IsMusicPlaying()}, SfxPlaying: {IsAnySfxPlaying()}, SfxSources: {GetSfxSourceCount()}");
        }

        public void StopAudioSource(AudioSource source)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }

    }
}
