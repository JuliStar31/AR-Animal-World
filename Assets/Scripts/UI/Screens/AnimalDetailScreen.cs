using TMPro;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    // Screen 4: big illustration of the animal, its name and fact, a button
    // to hear its sound and one to pick another animal.
    public class AnimalDetailScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.AnimalDetail;

        public Image illustration;
        public TMP_Text nameText;
        public TMP_Text factText;
        public Button listenButton;
        public Button otherAnimalButton;
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
        }

        protected override void OnShow()
        {
            var animal = Game.appManager.SelectedAnimal;
            nameText.text = animal.displayName;
            factText.text = animal.fact;

            if (illustration != null && Game.art != null)
            {
                illustration.sprite = Game.art.For(animal.id);
                var pop = illustration.GetComponent<PopInEffect>();
                if (pop != null)
                {
                    pop.Play();
                }
            }

            Game.gameAudio.PlayAnimal(animal);
        }

        protected override void OnHide()
        {
            Game.gameAudio.Stop();
        }
    }
}
