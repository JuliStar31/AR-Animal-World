using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    // Screen 3: camera view. The background stays see-through so the AR
    // camera shows behind the UI. When a card is found (via
    // AnimalImageTargetHandler -> ARAnimalWorldManager) the info panel
    // slides in with the name, fact and buttons.
    public class ScanScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.Scan;

        // ARCameraToggle listens to this to turn the AR camera on and off.
        public static event Action<bool> ScanningChanged;

        public TMP_Text guideText;
        public GameObject scanFrame;
        public GameObject infoPanel;
        public TMP_Text nameText;
        public TMP_Text factText;
        public Button soundButton;
        public Button learnButton;
        public Button quizButton;
        public Button backButton;
        public GameObject cameraMessagePanel;
        public TMP_Text cameraMessageText;

        [Header("Testing without cards (Editor and development builds only)")]
        public GameObject simulatePanel;
        public Button[] simulateButtons = new Button[0];

        private const string GuideSearching = "Arahkan kamera ke kartu hewan!";
        private const string GuideFound = "Wah, ketemu!";

        private static string pendingCameraMessage;

        protected override void OnInitialize()
        {
            backButton.onClick.AddListener(Game.Back);
            soundButton.onClick.AddListener(() => Game.gameAudio.PlayAnimal(Game.appManager.SelectedAnimal));
            learnButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                Game.OpenAnimal(Game.appManager.selectedAnimalId);
            });
            quizButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                Game.StartQuiz(Game.appManager.selectedAnimalId);
            });

            Game.appManager.OnAnimalDetected += HandleDetected;
            Game.appManager.OnAnimalLost += HandleLost;

            bool allowSimulation = Application.isEditor || Debug.isDebugBuild;
            if (simulatePanel != null)
            {
                simulatePanel.SetActive(allowSimulation);
            }

            for (int i = 0; i < simulateButtons.Length && i < AnimalCatalog.Animals.Count; i++)
            {
                var id = AnimalCatalog.Animals[i].id;
                simulateButtons[i].onClick.AddListener(() => SimulateCard(id));
            }
        }

        protected override void OnShow()
        {
            if (Game.appManager.IsAnimalInView)
            {
                HandleDetected(Game.appManager.SelectedAnimal);
            }
            else
            {
                ShowSearching();
            }

            ShowCameraMessage(pendingCameraMessage);
            ScanningChanged?.Invoke(true);
        }

        protected override void OnHide()
        {
            ScanningChanged?.Invoke(false);
        }

        // Shown gently when the camera cannot start (no permission, no
        // Vuforia key, etc.). Pass null to hide it.
        public static void ReportCameraProblem(string message)
        {
            pendingCameraMessage = message;
            foreach (var screen in FindObjectsOfType<ScanScreen>())
            {
                screen.ShowCameraMessage(message);
            }
        }

        private void ShowCameraMessage(string message)
        {
            if (cameraMessagePanel == null)
            {
                return;
            }

            cameraMessagePanel.SetActive(!string.IsNullOrEmpty(message));
            if (cameraMessageText != null && !string.IsNullOrEmpty(message))
            {
                cameraMessageText.text = message;
            }
        }

        private void ShowSearching()
        {
            guideText.text = GuideSearching;
            scanFrame.SetActive(true);
            infoPanel.SetActive(false);
        }

        private void HandleDetected(AnimalData animal)
        {
            if (animal == null || !gameObject.activeInHierarchy)
            {
                return;
            }

            guideText.text = GuideFound;
            scanFrame.SetActive(false);
            nameText.text = animal.displayName;
            factText.text = animal.fact;
            infoPanel.SetActive(true);
        }

        private void HandleLost(AnimalData animal)
        {
            if (gameObject.activeInHierarchy)
            {
                ShowSearching();
            }
        }

        private void SimulateCard(AnimalId id)
        {
            if (Game.appManager.IsAnimalInView)
            {
                Game.appManager.HandleAnimalLost(Game.appManager.selectedAnimalId);
            }
            Game.appManager.HandleAnimalDetected(id);
            Game.gameAudio.PlayAnimal(Game.appManager.SelectedAnimal);
        }
    }
}
