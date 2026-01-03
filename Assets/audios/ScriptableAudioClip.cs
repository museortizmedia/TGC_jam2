using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace MuseOrtizLibrary
{
    [CreateAssetMenu(fileName = "New ScriptableAudioClip", menuName = "MuseOrtizLibrary/New ScriptableAudioClip")]
    public class ScriptableAudioClip : ScriptableObject
    {
        [System.Serializable]
        public struct AudioClipSettings
        {
            public string AudioName;                // Unique name for the audio source
            public AudioClip[] AudioClips;         // List of audio clips to randomly play
            public float Volume;                   // Audio volume
            public float Pitch;                    // Audio pitch
            public bool Loop;                      // Should the audio loop?
            public float StartDelay;               // Delay before the audio starts playing
            public AudioMixerGroup OutputChannel; // Mixer group for audio output
        }

        public AudioClipSettings AudioParameters  = new AudioClipSettings(){};

        private AudioSource _audioSource;
        public AudioSource AudioSource { get => _audioSource; }
        private GameObject _sourceGameObject;

        /// <summary>
        /// Plays the audio with the configured parameters.
        /// </summary>
        public void PlayAudio()
        {
            if (AudioParameters.AudioClips == null || AudioParameters.AudioClips.Length == 0)
            {
                Debug.LogWarning($"Audio '{AudioParameters.AudioName}' has no clips assigned.");
                return;
            }

            EnsureAudioSourceExists();

            // Configure the AudioSource
            _audioSource.clip = AudioParameters.AudioClips[Random.Range(0, AudioParameters.AudioClips.Length)];
            _audioSource.volume = AudioParameters.Volume;
            _audioSource.pitch = AudioParameters.Pitch;
            _audioSource.loop = AudioParameters.Loop;

            // Play with a delay if specified
            _audioSource.PlayDelayed(AudioParameters.StartDelay);
        }

        /// <summary>
        /// Plays the audio with the configured parameters.
        /// </summary>
        public void PlayAudio(int index)
        {
            if (AudioParameters.AudioClips == null || AudioParameters.AudioClips.Length == 0)
            {
                Debug.LogWarning($"Audio '{AudioParameters.AudioName}' has no clips assigned.");
                return;
            }

            EnsureAudioSourceExists();

            // Configure the AudioSource
            _audioSource.clip = AudioParameters.AudioClips[index];
            _audioSource.volume = AudioParameters.Volume;
            _audioSource.pitch = AudioParameters.Pitch;
            _audioSource.loop = AudioParameters.Loop;

            // Play with a delay if specified
            _audioSource.PlayDelayed(AudioParameters.StartDelay);
        }

        /// <summary>
        /// Stops the currently playing audio.
        /// </summary>
        public void StopAudio()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }

        /// <summary>
        /// Changes the volume of the currently playing audio.
        /// </summary>
        /// <param name="value">New volume value (0.0 to 1.0).</param>
        public void ChangeVolume(float value)
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.volume = Mathf.Clamp01(value);
            }
        }

        /// <summary>
        /// Ensures that the audio source exists and is configured properly.
        /// </summary>
        private void EnsureAudioSourceExists()
        {
            if (_sourceGameObject == null)
            {
                GameObject existingGameObject = GameObject.Find($"Audio {AudioParameters.AudioName}");
                if (existingGameObject == null)
                {
                    _sourceGameObject = new GameObject($"Audio {AudioParameters.AudioName}");
                    _audioSource = _sourceGameObject.AddComponent<AudioSource>();
                    _audioSource.outputAudioMixerGroup = AudioParameters.OutputChannel;

                    // Ensure the GameObject persists in the scene
                    DontDestroyOnLoad(_sourceGameObject);
                }
                else
                {
                    _sourceGameObject = existingGameObject;
                    _audioSource = _sourceGameObject.GetComponent<AudioSource>();
                }
            }
        }
    }
}