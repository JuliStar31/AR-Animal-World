using System.Collections.Generic;
using UnityEngine;

namespace ARAnimalWorld
{
    // Generates simple synthesized sounds so the game is playable before
    // real recordings are added under Resources/Audio. Replace them by
    // dropping lion_roar, elephant_trumpet, rabbit_squeak (etc.) there.
    public static class PlaceholderSounds
    {
        private const int SampleRate = 22050;
        private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();

        public static AudioClip ForAnimal(AnimalId id)
        {
            switch (id)
            {
                case AnimalId.Lion:
                    return GetOrCreate("placeholder_roar", 1.2f, Roar);
                case AnimalId.Elephant:
                    return GetOrCreate("placeholder_trumpet", 1.0f, Trumpet);
                default:
                    return GetOrCreate("placeholder_squeak", 0.5f, Squeak);
            }
        }

        public static AudioClip Correct()
        {
            return GetOrCreate("placeholder_correct", 0.6f, Chime);
        }

        public static AudioClip TryAgain()
        {
            return GetOrCreate("placeholder_try_again", 0.4f, SoftBoop);
        }

        public static AudioClip Tap()
        {
            return GetOrCreate("placeholder_tap", 0.08f, (t, d) => Mathf.Sin(2f * Mathf.PI * 880f * t) * (1f - t / d) * 0.4f);
        }

        private static AudioClip GetOrCreate(string name, float duration, System.Func<float, float, float> wave)
        {
            if (Cache.TryGetValue(name, out var cached) && cached != null)
            {
                return cached;
            }

            int count = Mathf.CeilToInt(SampleRate * duration);
            var samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                samples[i] = Mathf.Clamp(wave(i / (float)SampleRate, duration), -1f, 1f);
            }

            var clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            Cache[name] = clip;
            return clip;
        }

        private static float Envelope(float t, float duration, float attack)
        {
            float rise = Mathf.Clamp01(t / attack);
            float fall = Mathf.Clamp01((duration - t) / (duration * 0.5f));
            return rise * fall;
        }

        // Low growl: falling pitch plus a bit of noise.
        private static float Roar(float t, float d)
        {
            float freq = Mathf.Lerp(180f, 90f, t / d);
            float tone = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * freq * 2.01f * t);
            float noise = Random.Range(-1f, 1f) * 0.35f;
            return (tone * 0.45f + noise) * Envelope(t, d, 0.15f) * 0.8f;
        }

        // Brassy rising call.
        private static float Trumpet(float t, float d)
        {
            float freq = Mathf.Lerp(330f, 520f, Mathf.Clamp01(t / (d * 0.4f)));
            float phase = (freq * t) % 1f;
            float saw = phase * 2f - 1f;
            return saw * Envelope(t, d, 0.05f) * 0.45f;
        }

        // Two quick high chirps.
        private static float Squeak(float t, float d)
        {
            float local = t % (d / 2f);
            float freq = Mathf.Lerp(1400f, 2000f, local / (d / 2f));
            return Mathf.Sin(2f * Mathf.PI * freq * t) * Envelope(local, d / 2f, 0.01f) * 0.4f;
        }

        // Happy rising three-note chime (C5, E5, G5).
        private static float Chime(float t, float d)
        {
            float step = d / 3f;
            int note = Mathf.Min(2, (int)(t / step));
            float[] freqs = { 523.25f, 659.25f, 783.99f };
            float local = t - note * step;
            return Mathf.Sin(2f * Mathf.PI * freqs[note] * t) * Mathf.Exp(-local * 6f) * 0.5f;
        }

        // Gentle low note, never harsh.
        private static float SoftBoop(float t, float d)
        {
            float freq = Mathf.Lerp(392f, 330f, t / d);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * Envelope(t, d, 0.03f) * 0.35f;
        }
    }
}
