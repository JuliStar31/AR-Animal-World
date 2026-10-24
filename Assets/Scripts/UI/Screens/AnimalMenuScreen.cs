using System;
using TMPro;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    [Serializable]
    public class AnimalCardView
    {
        public AnimalId animalId;
        public Button cardButton;
        public Button openButton;
        public TMP_Text nameLabel;
        public Image icon;
    }

    // Screen 2: "Pilih Hewan" with one card per animal, and a button to
    // scan a printed card.
    public class AnimalMenuScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.AnimalMenu;

        public AnimalCardView[] cards;
        public Button scanButton;
        public Button backButton;

        protected override void OnInitialize()
        {
            backButton.onClick.AddListener(Game.Back);
            scanButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                Game.OpenScan();
            });

            foreach (var card in cards)
            {
                var id = card.animalId;
                if (card.nameLabel != null)
                {
                    card.nameLabel.text = Game.appManager.GetAnimalById(id).displayName;
                }
                if (card.icon != null && Game.art != null)
                {
                    card.icon.sprite = Game.art.For(id);
                }

                void Open()
                {
                    Game.gameAudio.PlayTap();
                    Game.OpenAnimal(id);
                }

                if (card.cardButton != null)
                {
                    card.cardButton.onClick.AddListener(Open);
                }
                if (card.openButton != null)
                {
                    card.openButton.onClick.AddListener(Open);
                }
            }
        }
    }
}
