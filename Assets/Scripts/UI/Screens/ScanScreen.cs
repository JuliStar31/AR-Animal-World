using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    public enum CameraState
    {
        Ok,
        NeedsPermission,
        Error
    }

    // Screen 3: camera view. The background stays see-through so the AR
    // camera shows behind the UI.
    //  3a  searching: guide bubble, scan frame and a hint card
    //  3b  found: "Kartu ketemu!" and an info panel with name, fact, sound
    //  3c  camera off: friendly panel with "Izinkan Kamera"
    public class ScanScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.Scan;

        // AnimalImageTargetHandler and ARCameraToggle listen to these.
        public static bool IsScanning { get; private set; }
        public static event Action<bool> ScanningChanged;
        public static event Action CameraPermissionRequested;

        public Button backButton;

        [Header("3a Searching")]
        public GameObject guidePanel;
        public GameObject scanFrame;
        public GameObject cardHint;

        [Header("3b Found")]
        public GameObject foundBadge;
        public TMP_Text foundText;
        public GameObject infoPanel;
        public TMP_Text nameText;
        public TMP_Text factText;
        public Button soundButton;
        public Button infoButton;

        [Header("3c Camera off")]
        public GameObject cameraPanel;
        public TMP_Text cameraTitle;
        public TMP_Text cameraMessage;
        public Button allowCameraButton;
        public Button cameraBackButton;

        [Header("Testing without cards (Editor and development builds only)")]
        public GameObject simulatePanel;
        public Button[] simulateButtons = new Button[0];

        private const string PermissionTitle = "Kameranya belum menyala";
        private const string PermissionMessage = "Ayo izinkan kamera supaya hewan bisa muncul!";

        private static CameraState cameraState = CameraState.Ok;
        private static string cameraText;

        protected override void OnInitialize()
        {
            backButton.onClick.AddListener(Game.Back);
            cameraBackButton.onClick.AddListener(Game.Back);
            soundButton.onClick.AddListener(() => Game.gameAudio.PlayAnimal(Game.appManager.SelectedAnimal));
            if (infoButton != null)
            {
                infoButton.onClick.AddListener(() =>
                {
                    Game.gameAudio.PlayTap();
                    Game.OpenAnimal(Game.appManager.selectedAnimalId);
                });
            }
            allowCameraButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                CameraPermissionRequested?.Invoke();
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
            // Switch AR on first, so a UI problem can never keep the
            // animals from appearing on their cards.
            IsScanning = true;
            ScanningChanged?.Invoke(true);

            if (Game.appManager.IsAnimalInView)
            {
                HandleDetected(Game.appManager.SelectedAnimal);
            }
            else
            {
                ShowSearching();
            }

            ApplyCameraState();
        }

        protected override void OnHide()
        {
            IsScanning = false;
            ScanningChanged?.Invoke(false);
        }

        public static void ReportCameraState(CameraState state, string message = null)
        {
            cameraState = state;
            cameraText = message;
            foreach (var screen in FindObjectsOfType<ScanScreen>())
            {
                screen.ApplyCameraState();
            }
        }

        private void ApplyCameraState()
        {
            bool off = cameraState != CameraState.Ok;
            cameraPanel.SetActive(off);
            if (!off)
            {
                return;
            }

            cameraTitle.text = PermissionTitle;
            cameraMessage.text = cameraState == CameraState.NeedsPermission || string.IsNullOrEmpty(cameraText)
                ? PermissionMessage
                : cameraText;
        }

        private void ShowSearching()
        {
            SetVisible(guidePanel, true);
            SetVisible(scanFrame, true);
            SetVisible(cardHint, true);
            SetVisible(foundBadge, false);
            SetVisible(infoPanel, false);
        }

        private static void SetVisible(GameObject part, bool visible)
        {
            if (part != null)
            {
                part.SetActive(visible);
            }
        }

        private void HandleDetected(AnimalData animal)
        {
            if (animal == null || !gameObject.activeInHierarchy)
            {
                return;
            }

            SetVisible(guidePanel, false);
            SetVisible(scanFrame, false);
            SetVisible(cardHint, false);
            SetVisible(foundBadge, true);
            if (foundText != null)
            {
                foundText.text = Game.appManager.IsFromPicture ? "Hewan ketemu!" : "Kartu ketemu!";
            }
            nameText.text = animal.displayName;
            factText.text = animal.fact;
            SetVisible(infoPanel, true);
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
