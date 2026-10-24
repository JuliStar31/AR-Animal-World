using System.Collections;
using UnityEngine;
using Vuforia;

namespace ARAnimalWorld
{
    // Attach to each Vuforia Image Target (lion, elephant, rabbit card).
    // While the Scan screen is open and the card is tracked, it shows the
    // animal standing on the card, plays its sound then narration, and tells
    // ARAnimalWorldManager which animal is in view.
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
        [Tooltip("Animal size compared to the card width. 1 = as long as the card is wide.")]
        public float modelScale = 1f;
        public Vector3 modelLocalPosition = Vector3.zero;
        [Tooltip("Turn the animal on the card. 0 faces the child holding the phone.")]
        public Vector3 modelLocalEuler = new Vector3(0f, 30f, 0f);

        [Header("Audio")]
        public AudioSource audioSource;
        public float pauseBeforeNarration = 0.3f;

        private ObserverBehaviour observer;
        private AnimalData animal;
        private GameObject spawnedModel;
        private Coroutine audioRoutine;
        private bool isTracked;
        private bool isShowing;

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
            ScanScreen.ScanningChanged += HandleScanningChanged;
        }

        private void OnDisable()
        {
            if (observer != null)
            {
                observer.OnTargetStatusChanged -= HandleTargetStatusChanged;
            }
            ScanScreen.ScanningChanged -= HandleScanningChanged;
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
            isTracked = targetStatus.Status == Status.TRACKED ||
                        targetStatus.Status == Status.EXTENDED_TRACKED;
            Refresh();
        }

        // Cards stay tracked in the background, but the animal only appears
        // while the child is on the Scan screen.
        private void HandleScanningChanged(bool scanning)
        {
            Refresh();
        }

        private void Refresh()
        {
            bool show = isTracked && ScanScreen.IsScanning && animal != null;
            if (show == isShowing)
            {
                return;
            }

            isShowing = show;
            if (show)
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
                // Parented to the target so the animal stays on the card.
                float cardWidth = CardWidthInTargetSpace();
                spawnedModel = AnimalModelSpawner.Spawn(animal, transform, cardWidth * modelScale,
                    modelLocalEuler, modelLocalPosition, modelPrefab);
            }

            spawnedModel.SetActive(true);
        }

        // The card's width measured in this transform's local units, whatever
        // scale Vuforia gave the Image Target.
        private float CardWidthInTargetSpace()
        {
            float widthMeters = 0f;
            if (observer is ImageTargetBehaviour imageTarget)
            {
                widthMeters = imageTarget.GetSize().x;
            }
            if (widthMeters <= 0f)
            {
                widthMeters = 0.08f;
            }

            float worldPerLocal = Mathf.Max(transform.lossyScale.x, 1e-6f);
            return widthMeters / worldPerLocal;
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
