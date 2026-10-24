using System.Collections;
using UnityEngine;

namespace ARAnimalWorld
{
    // Plays animal sounds, narration and UI feedback for the screens.
    // Real clips are loaded from Resources/Audio:
    //   {soundClipName}           e.g. lion_roar
    //   narration_{animal id}     e.g. narration_lion (optional voice-over)
    // Missing sounds fall back to PlaceholderSounds.
    [RequireComponent(typeof(AudioSource))]
    public class GameAudio : MonoBehaviour
    {
        public AudioClip correctClip;
        public AudioClip tryAgainClip;
        public AudioClip tapClip;
        public float pauseBeforeNarration = 0.3f;

        private AudioSource source;
        private Coroutine sequence;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
        }

        public static AudioClip LoadAnimalSound(AnimalData animal)
        {
            var clip = Resources.Load<AudioClip>("Audio/" + animal.soundClipName);
            return clip != null ? clip : PlaceholderSounds.ForAnimal(animal.id);
        }

        public static AudioClip LoadNarration(AnimalData animal)
        {
            return Resources.Load<AudioClip>("Audio/narration_" + animal.id.ToString().ToLowerInvariant());
        }

        public void PlayAnimal(AnimalData animal, bool withNarration = true)
        {
            if (animal == null)
            {
                return;
            }

            Stop();
            sequence = StartCoroutine(PlaySequence(LoadAnimalSound(animal), withNarration ? LoadNarration(animal) : null));
        }

        public void PlayCorrect()
        {
            source.PlayOneShot(correctClip != null ? correctClip : PlaceholderSounds.Correct());
        }

        public void PlayTryAgain()
        {
            source.PlayOneShot(tryAgainClip != null ? tryAgainClip : PlaceholderSounds.TryAgain());
        }

        public void PlayTap()
        {
            source.PlayOneShot(tapClip != null ? tapClip : PlaceholderSounds.Tap());
        }

        public void Stop()
        {
            if (sequence != null)
            {
                StopCoroutine(sequence);
                sequence = null;
            }
            source.Stop();
        }

        private IEnumerator PlaySequence(AudioClip sound, AudioClip narration)
        {
            source.clip = sound;
            source.Play();
            yield return new WaitForSeconds(sound.length + pauseBeforeNarration);

            if (narration != null)
            {
                source.clip = narration;
                source.Play();
            }

            sequence = null;
        }
    }
}
