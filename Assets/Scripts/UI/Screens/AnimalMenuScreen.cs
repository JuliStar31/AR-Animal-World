using System;
using TMPro;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    [Serializable]
    public class AnimalCardView
    {
        public AnimalId animalId;
        public Button learnButton;
        public Button scanButton;
        public TMP_Text nameLabel;
    }

    // Screen 2: "Pilih Hewan" with one card per animal.
    public class AnimalMenuScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.AnimalMenu;

        public AnimalCardView[] cards;
        public Button backButton;

        protected override void OnInitialize()
        {
            backButton.onClick.AddListener(Game.Back);

            foreach (var card in cards)
            {
                var id = card.animalId;
                if (card.nameLabel != null)
                {
                    card.nameLabel.text = Game.appManager.GetAnimalById(id).displayName;
                }
                card.learnButton.onClick.AddListener(() =>
                {
                    Game.gameAudio.PlayTap();
                    Game.OpenAnimal(id);
                });
                if (card.scanButton != null)
                {
                    card.scanButton.onClick.AddListener(() =>
                    {
                        Game.gameAudio.PlayTap();
                        Game.appManager.SelectAnimal(id);
                        Game.OpenScan();
                    });
                }
            }
        }
    }
}
