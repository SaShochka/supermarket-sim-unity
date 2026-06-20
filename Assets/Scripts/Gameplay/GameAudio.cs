using UnityEngine;

namespace SupermarketSim.Gameplay
{
    public class GameAudio : MonoBehaviour
    {
        public static GameAudio Instance { get; private set; }

        private AudioSource musicSource;
        private AudioSource sfxSource;
        private AudioClip pickupClip;
        private AudioClip placeClip;
        private AudioClip buyClip;
        private AudioClip scanClip;
        private AudioClip errorClip;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureAudio()
        {
            if (Instance != null) return;
            var obj = new GameObject("GameAudio");
            Object.DontDestroyOnLoad(obj);
            obj.AddComponent<GameAudio>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.34f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.volume = 0.78f;

            pickupClip = Tone("pickup", 660f, 0.08f, 0.35f);
            placeClip = Tone("place", 440f, 0.09f, 0.35f);
            buyClip = Chime("buy", 523f, 659f, 0.16f);
            scanClip = Chime("scan", 880f, 1320f, 0.12f);
            errorClip = Tone("error", 180f, 0.18f, 0.3f);

            musicSource.clip = MakeMusicLoop();
            musicSource.Play();
        }

        public static void PlayPickup() => Instance?.Play(Instance.pickupClip);
        public static void PlayPlace() => Instance?.Play(Instance.placeClip);
        public static void PlayBuy() => Instance?.Play(Instance.buyClip);
        public static void PlayScan() => Instance?.Play(Instance.scanClip);
        public static void PlayError() => Instance?.Play(Instance.errorClip);

        private void Play(AudioClip clip)
        {
            if (clip != null)
                sfxSource.PlayOneShot(clip);
        }

        private static AudioClip Tone(string clipName, float frequency, float duration, float volume)
        {
            int sampleRate = AudioSettings.outputSampleRate;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - (i / (float)samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
            }

            var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip Chime(string clipName, float first, float second, float duration)
        {
            int sampleRate = AudioSettings.outputSampleRate;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - (i / (float)samples);
                float f = i < samples * 0.5f ? first : second;
                data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * envelope * 0.35f;
            }

            var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeMusicLoop()
        {
            int sampleRate = AudioSettings.outputSampleRate;
            float duration = 24f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            float[][] bassSections =
            {
                new[] { 110f, 130.81f, 146.83f, 164.81f, 146.83f, 130.81f, 196f, 164.81f },
                new[] { 98f, 123.47f, 146.83f, 196f, 174.61f, 146.83f, 123.47f, 146.83f },
                new[] { 130.81f, 164.81f, 196f, 220f, 196f, 164.81f, 146.83f, 164.81f },
                new[] { 110f, 146.83f, 164.81f, 220f, 196f, 164.81f, 130.81f, 146.83f }
            };
            float[][] arpSections =
            {
                new[] { 440f, 523.25f, 659.25f, 783.99f, 659.25f, 523.25f, 587.33f, 739.99f },
                new[] { 392f, 493.88f, 587.33f, 783.99f, 698.46f, 587.33f, 493.88f, 587.33f },
                new[] { 523.25f, 659.25f, 783.99f, 880f, 783.99f, 659.25f, 587.33f, 659.25f },
                new[] { 440f, 587.33f, 659.25f, 880f, 783.99f, 659.25f, 523.25f, 587.33f }
            };
            float beat = duration / 64f;
            float sectionLength = duration / 4f;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                int section = Mathf.Clamp(Mathf.FloorToInt(t / sectionLength), 0, 3);
                int step = Mathf.FloorToInt(t / beat);
                float local = (t - step * beat) / beat;
                int sectionStep = step - section * 16;
                int bassIndex = Mathf.Abs(sectionStep / 2) % bassSections[section].Length;
                int arpIndex = Mathf.Abs(sectionStep + section) % arpSections[section].Length;

                float pulse = 1f - Mathf.SmoothStep(0.15f, 1f, local);
                float attack = Mathf.SmoothStep(0f, 1f, Mathf.Min(local * 10f, 1f));
                float env = pulse * attack;
                float sectionEnergy = 0.85f + section * 0.06f;

                float bassNote = bassSections[section][bassIndex];
                float arpNote = arpSections[section][arpIndex];
                float bass = Mathf.Sin(2f * Mathf.PI * bassNote * t) * 0.17f * env * sectionEnergy;
                bass += Mathf.Sin(2f * Mathf.PI * bassNote * 2f * t) * 0.04f * env;

                float arpEnv = (1f - Mathf.SmoothStep(0.25f, 1f, local)) * attack;
                float arp = Mathf.Sin(2f * Mathf.PI * arpNote * t) * 0.08f * arpEnv * sectionEnergy;
                if (section >= 2)
                    arp += Mathf.Sin(2f * Mathf.PI * arpNote * 1.5f * t) * 0.025f * arpEnv;

                float kick = (step % 4 == 0) ? Mathf.Sin(2f * Mathf.PI * 70f * t) * (1f - Mathf.SmoothStep(0f, 0.35f, local)) * 0.18f : 0f;
                float hat = (step % 2 == 1 || section >= 2) ? (PseudoNoise(i) * 2f - 1f) * (1f - Mathf.SmoothStep(0f, 0.18f, local)) * 0.03f : 0f;
                float transition = Mathf.Sin(Mathf.PI * Mathf.Clamp01((t % sectionLength) / sectionLength));
                float pad = Mathf.Sin(2f * Mathf.PI * bassNote * 0.5f * t) * 0.035f * transition;

                data[i] = Mathf.Clamp(bass + arp + kick + hat + pad, -0.9f, 0.9f);
            }

            var clip = AudioClip.Create("supermarket_music_loop", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float PseudoNoise(int sample)
        {
            uint x = (uint)sample;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            return (x & 1023) / 1023f;
        }
    }
}
