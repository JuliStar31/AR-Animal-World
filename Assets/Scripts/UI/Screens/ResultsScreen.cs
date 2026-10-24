using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    // Screen 6: "Kamu Hebat!" with the stars from this round. Every child
    // gets an encouraging message, whatever the score.
    public class ResultsScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.Results;

        public TMP_Text titleText;
        public TMP_Text messageText;
        public TMP_Text totalStarsText;
        public Image[] stars = new Image[0];
        public Button playAgainButton;
        public Button homeButton;

        public float starSpacing = 250f;
        public Color starOn = new Color(1f, 0.85f, 0.36f);
        public Color starOff = new Color(0.91f, 0.93f, 0.96f);

        protected override void OnInitialize()
        {
            playAgainButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                Game.StartQuiz();
            });
            homeButton.onClick.AddListener(() =>
            {
                Game.gameAudio.PlayTap();
                Game.GoHome();
            });
        }

        protected override void OnShow()
        {
            var session = Game.quizManager.Session;
            int earned = session != null ? session.StarsEarned : 0;
            int total = session != null ? session.QuestionCount : stars.Length;

            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(i < total);
                CenterInRow(stars[i].rectTransform, i, Mathf.Min(total, stars.Length), starSpacing);
                stars[i].color = i < earned ? starOn : starOff;

                var pop = stars[i].GetComponent<PopInEffect>();
                if (pop != null)
                {
                    pop.delay = 0.2f + i * 0.25f;
                    pop.Play();
                }
            }

            titleText.text = "Kamu Hebat!";
            messageText.text = MessageFor(earned, total);
            totalStarsText.text = $"Total bintangmu: {Game.appManager.starsCollected}";
            Game.gameAudio.PlayCorrect();
        }

        // Spreads the visible items evenly around the centre.
        public static void CenterInRow(RectTransform rect, int index, int count, float spacing)
        {
            var position = rect.anchoredPosition;
            position.x = (index - (count - 1) / 2f) * spacing;
            rect.anchoredPosition = position;
        }

        public static string MessageFor(int earned, int total)
        {
            if (earned >= total)
            {
                return $"Kamu dapat {earned} bintang! Semua benar!";
            }
            if (earned > 0)
            {
                return $"Kamu dapat {earned} bintang! Ayo main lagi!";
            }
            return "Kamu sudah berusaha! Ayo main lagi!";
        }
    }
}
