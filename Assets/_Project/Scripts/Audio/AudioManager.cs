using UnityEngine;
using System.Collections.Generic;
using ApexArena.Gameplay;

namespace ApexArena.Audio
{
    /// <summary>
    /// مدير الصوت - يدير الموسيقى والتأثيرات الصوتية
    /// Audio Manager - manages music and sound effects
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("Music Clips")]
        [SerializeField] private AudioClip exploreMusic;
        [SerializeField] private AudioClip tensionMusic;
        [SerializeField] private AudioClip combatMusic;
        [SerializeField] private AudioClip victoryStinger;
        [SerializeField] private AudioClip defeatStinger;

        [Header("SFX Clips")]
        [SerializeField] private List<AudioClip> weaponSounds = new List<AudioClip>();
        [SerializeField] private List<AudioClip> techSounds = new List<AudioClip>();
        [SerializeField] private List<AudioClip> uiSounds = new List<AudioClip>();

        [Header("Ambient")]
        [SerializeField] private AudioClip industrialAmbient;
        [SerializeField] private AudioClip militaryAmbient;
        [SerializeField] private AudioClip forestAmbient;
        [SerializeField] private AudioClip dangerAmbient;

        private MusicState currentMusicState = MusicState.Explore;
        private float musicTransitionSpeed = 2f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            PlayMusic(MusicState.Explore);
        }

        public void PlayMusic(MusicState state)
        {
            if (currentMusicState == state) return;

            currentMusicState = state;
            AudioClip clip = GetMusicClip(state);

            if (clip != null && musicSource != null)
            {
                musicSource.clip = clip;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        public void PlaySFX(string soundName, Vector3 position, float volume = 1f)
        {
            AudioClip clip = FindSFX(soundName);
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, volume);
            }
        }

        public void PlayUISound(string soundName)
        {
            AudioClip clip = FindUISound(soundName);
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void SetAmbient(ZoneType zoneType)
        {
            AudioClip clip = zoneType switch
            {
                ZoneType.Industrial => industrialAmbient,
                ZoneType.Military => militaryAmbient,
                ZoneType.Forest => forestAmbient,
                ZoneType.Danger => dangerAmbient,
                _ => null
            };

            if (clip != null && ambientSource != null)
            {
                ambientSource.clip = clip;
                ambientSource.loop = true;
                ambientSource.Play();
            }
        }

        public void PlayVictory()
        {
            if (victoryStinger != null)
            {
                musicSource.PlayOneShot(victoryStinger);
            }
        }

        public void PlayDefeat()
        {
            if (defeatStinger != null)
            {
                musicSource.PlayOneShot(defeatStinger);
            }
        }

        private AudioClip GetMusicClip(MusicState state)
        {
            return state switch
            {
                MusicState.Explore => exploreMusic,
                MusicState.Tension => tensionMusic,
                MusicState.Combat => combatMusic,
                _ => exploreMusic
            };
        }

        private AudioClip FindSFX(string name)
        {
            return weaponSounds.Find(c => c.name == name) ?? 
                   techSounds.Find(c => c.name == name);
        }

        private AudioClip FindUISound(string name)
        {
            return uiSounds.Find(c => c.name == name);
        }
    }

    public enum MusicState
    {
        Explore,
        Tension,
        Combat,
        Victory,
        Defeat
    }
}
