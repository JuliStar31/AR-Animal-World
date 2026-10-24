using TMPro;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    // Screen 4: big turning 3D animal, its fact and narration, and
    // buttons to hear its sound, scan it, quiz, or pick another animal.
    public class AnimalDetailScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.AnimalDetail;

        public TMP_Text nameText;
        public TMP_Text factText;
        public TMP_Text narrationText;
        public Image accentBackground;
        public Button listenButton;
        public Button otherAnimalButton;
        public Button scanButton;
        public Button quizButton;
        public Button backButton;

        protected override void OnInitialize()
        {
            backButton.onClick.AddListener(Game.Back);
            listenButton.onClick.AddListener(() => Game.gameAudio.PlayAnimal(Game.appManager.SelectedAnimal));
            otherAnimalButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                Game.OpenAnimalMenu();
            });
            scanButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                Game.OpenScan();
            });
            quizButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                Game.StartQuiz(Game.appManager.selectedAnimalId);
            });
        }

        protected override void OnShow()
        {
            var animal = Game.appManager.SelectedAnimal;
            nameText.text = animal.displayName;
            factText.text = animal.fact;
            narrationText.text = animal.narrationText;

            if (accentBackground != null)
            {
                var color = animal.accentColor;
                color.a = 0.35f;
                accentBackground.color = color;
            }

            if (Game.animalPreview != null)
            {
                Game.animalPreview.Show(animal);
            }

            Game.gameAudio.PlayAnimal(animal);
        }

        protected override void OnHide()
        {
            Game.gameAudio.Stop();
            if (Game.animalPreview != null)
            {
                Game.animalPreview.Hide();
            }
        }
    }
}
