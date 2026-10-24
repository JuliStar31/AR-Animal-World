using System.Collections;
using UnityEngine;
using Vuforia;

namespace ARAnimalWorld
{
    // Attach to each Vuforia Image Target (lion, elephant, rabbit card).
    // When the card is detected it spawns the animal model on the card,
    // plays the animal sound followed by the narration, and tells
    // ARAnimalWorldManager which animal is now active.
    [RequireComponent(typeof(ObserverBehaviour))]
    public class AnimalImageTargetHandler : MonoBehaviour
    {
        public ARAnimalWorldManager appManager;
        public AnimalId animalId;

        [Header("Assets (optional, falls back to Resources by AnimalData names)")]
        public GameObject modelPrefab;
        public AudioClip soundClip;
        public AudioClip narrationClip;

        [Header("Placement")]
        public Vector3 modelLocalPosition = Vector3.zero;
        public Vector3 modelLocalEuler = Vector3.zero;
        public float modelScale = 1f;

        [Header("Audio")]
        public AudioSource audioSource;
        public float pauseBeforeNarration = 0.3f;

        private ObserverBehaviour observer;
        private AnimalData animal;
        private GameObject spawnedModel;
        private Coroutine audioRoutine;
        private bool isTracked;

        private void Awake()
        {
            observer = GetComponent<ObserverBehaviour>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            if (observer != null)
            {
                observer.OnTargetStatusChanged += HandleTargetStatusChanged;
            }
        }

        private void OnDisable()
        {
            if (observer != null)
            {
                observer.OnTargetStatusChanged -= HandleTargetStatusChanged;
            }
        }

        private void Start()
        {
            animal = appManager != null
                ? appManager.GetAnimalById(animalId)
                : AnimalCatalog.Animals.Find(a => a.id == animalId);

            if (animal == null)
            {
                Debug.LogWarning($"No AnimalData found for {animalId} on {name}.");
                return;
            }

            if (modelPrefab == null)
            {
                modelPrefab = Resources.Load<GameObject>("Models/" + animal.modelName);
            }

            if (soundClip == null)
            {
                soundClip = GameAudio.LoadAnimalSound(animal);
            }

            if (narrationClip == null)
            {
                narrationClip = GameAudio.LoadNarration(animal);
            }
        }

        private void HandleTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
        {
            bool tracked = targetStatus.Status == Status.TRACKED ||
                           targetStatus.Status == Status.EXTENDED_TRACKED;

            if (tracked == isTracked)
            {
                return;
            }

            isTracked = tracked;

            if (tracked)
            {
                OnCardFound();
            }
            else
            {
                OnCardLost();
            }
        }

        private void OnCardFound()
        {
            if (animal == null)
            {
                return;
            }

            ShowModel();

            if (audioRoutine != null)
            {
                StopCoroutine(audioRoutine);
            }
            audioRoutine = StartCoroutine(PlaySoundThenNarration());

            if (appManager != null)
            {
                appManager.HandleAnimalDetected(animalId);
            }
        }

        private void OnCardLost()
        {
            if (spawnedModel != null)
            {
                spawnedModel.SetActive(false);
            }

            if (audioRoutine != null)
            {
                StopCoroutine(audioRoutine);
                audioRoutine = null;
            }
            audioSource.Stop();

            if (appManager != null)
            {
                appManager.HandleAnimalLost(animalId);
            }
        }

        private void ShowModel()
        {
            if (spawnedModel == null)
            {
                // Parent to the target so the animal stays anchored to the card.
                // Without a prefab, a simple placeholder animal is built instead.
                spawnedModel = modelPrefab != null
                    ? Instantiate(modelPrefab, transform)
                    : PlaceholderAnimalFactory.Create(animalId, transform);
                spawnedModel.transform.localPosition = modelLocalPosition;
                spawnedModel.transform.localRotation = Quaternion.Euler(modelLocalEuler);
                spawnedModel.transform.localScale = Vector3.one * modelScale;

                var idle = spawnedModel.GetComponent<AnimalIdleMotion>();
                if (idle == null)
                {
                    idle = spawnedModel.AddComponent<AnimalIdleMotion>();
                }
                idle.Scale = modelScale;
            }

            spawnedModel.SetActive(true);
        }

        private IEnumerator PlaySoundThenNarration()
        {
            if (soundClip != null)
            {
                audioSource.PlayOneShot(soundClip);
                yield return new WaitForSeconds(soundClip.length + pauseBeforeNarration);
            }

            if (narrationClip != null)
            {
                audioSource.PlayOneShot(narrationClip);
            }

            audioRoutine = null;
        }
    }
}
