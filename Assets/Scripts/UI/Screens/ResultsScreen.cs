using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    // Screen 6: "Kamu Hebat!" with up to three stars for this round. Every
    // child gets an encouraging message, whatever the score.
    public class ResultsScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.Results;

        public Image[] stars = new Image[3];
        public TMP_Text headlineText;
        public TMP_Text messageText;
        public Button playAgainButton;
        public Button homeButton;

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
            int lit = StarsToLight(earned, total, stars.Length);

            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].sprite = i < lit ? Game.art.starOn : Game.art.starOff;

                var pop = stars[i].GetComponent<PopInEffect>();
                if (pop != null)
                {
                    pop.delay = 0.2f + i * 0.25f;
                    pop.Play();
                }
            }

            headlineText.text = $"{earned} Bintang!";
            messageText.text = MessageFor(earned, total);
            Game.gameAudio.PlayCorrect();
        }

        // The screen always shows three stars; the score is spread over them.
        public static int StarsToLight(int earned, int total, int shown)
        {
            if (total <= 0 || earned <= 0)
            {
                return 0;
            }
            return Mathf.Clamp(Mathf.RoundToInt(shown * (float)earned / total), 1, shown);
        }

        public static string MessageFor(int earned, int total)
        {
            if (earned >= total)
            {
                return "Kamu pintar mengenal hewan. Ayo main lagi!";
            }
            if (earned > 0)
            {
                return "Hebat! Ayo main lagi supaya bintangnya makin banyak!";
            }
            return "Kamu sudah berusaha! Ayo main lagi!";
        }
    }
}
