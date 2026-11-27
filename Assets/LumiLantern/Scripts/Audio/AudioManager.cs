using UnityEngine;

namespace LumiLantern
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource sfxSource;

        private AudioClip clipPlace;
        private AudioClip clipRotate;
        private AudioClip clipRemove;
        private AudioClip clipLantern;
        private AudioClip clipWin;
        private AudioClip clipClick;
        private AudioClip clipGate;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                GenerateAudioClips();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void GenerateAudioClips()
        {
            clipPlace = CreateTone(523.25f, 0.12f, 0.3f, 1.2f); // High C chime
            clipRotate = CreateTone(659.25f, 0.08f, 0.25f, 1.5f); // E chime
            clipRemove = CreateTone(392.00f, 0.10f, 0.25f, 0.8f); // G chime
            clipLantern = CreateChord(new float[] { 523.25f, 659.25f, 783.99f }, 0.45f, 0.35f); // C Major
            clipWin = CreateFanfare();
            clipClick = CreateTone(800f, 0.04f, 0.15f, 2.0f);
            clipGate = CreateTone(220f, 0.18f, 0.4f, 0.5f); // Low clunk
        }

        public void PlayPlace() => Play(clipPlace, 0.8f);
        public void PlayRotate() => Play(clipRotate, 0.8f);
        public void PlayRemove() => Play(clipRemove, 0.7f);
        public void PlayLantern() => Play(clipLantern, 0.9f);
        public void PlayWin() => Play(clipWin, 1.0f);
        public void PlayClick() => Play(clipClick, 0.6f);
        public void PlayGate() => Play(clipGate, 0.85f);

        private void Play(AudioClip clip, float volume = 1f)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        private AudioClip CreateTone(float freq, float duration, float volume, float decay)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * decay * 10f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * volume;
            }

            AudioClip clip = AudioClip.Create("Tone_" + freq, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateChord(float[] freqs, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 5f);
                float val = 0;
                foreach (float f in freqs)
                {
                    val += Mathf.Sin(2f * Mathf.PI * f * t);
                }
                samples[i] = (val / freqs.Length) * env * volume;
            }

            AudioClip clip = AudioClip.Create("Chord", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateFanfare()
        {
            int sampleRate = 44100;
            float totalDuration = 1.2f;
            int sampleCount = Mathf.CeilToInt(sampleRate * totalDuration);
            float[] samples = new float[sampleCount];

            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f }; // C5, E5, G5, C6
            float noteDuration = 0.22f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                int noteIndex = Mathf.Clamp((int)(t / noteDuration), 0, notes.Length - 1);
                float noteT = t - (noteIndex * noteDuration);
                float freq = notes[noteIndex];
                float env = Mathf.Exp(-noteT * 3.5f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * noteT) * env * 0.4f;
            }

            AudioClip clip = AudioClip.Create("Fanfare", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
