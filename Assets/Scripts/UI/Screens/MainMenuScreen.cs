using TMPro;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    // Screen 1: title, tagline, "Mulai Belajar" and "Main Kuis".
    public class MainMenuScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.MainMenu;

        public Button startLearningButton;
        public Button quizButton;
        public Button scanButton;
        public TMP_Text starsLabel;

        protected override void OnInitialize()
        {
            startLearningButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                Game.OpenAnimalMenu();
            });
            quizButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                Game.StartQuiz();
            });
            if (scanButton != null)
            {
                scanButton.onClick.AddListener(() =>
                {
                    Game.gameAudio.PlayTap();
                    Game.OpenScan();
                });
            }
            Game.appManager.OnStarsUpdated += UpdateStars;
        }

        protected override void OnShow()
        {
            UpdateStars(Game.appManager.starsCollected);
        }

        private void UpdateStars(int stars)
        {
            if (starsLabel != null)
            {
                starsLabel.text = stars.ToString();
            }
        }
    }
}
