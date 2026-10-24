using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    // Screen 5: "Tebak Hewan!" One question per animal with three big
    // answer cards. A wrong answer only fades that card and says
    // "Ayo coba lagi!"; a right answer celebrates and moves on.
    public class QuizScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.Quiz;

        public TMP_Text questionText;
        public TMP_Text progressText;
        public TMP_Text starsText;
        public TMP_Text feedbackText;
        public Image[] progressDots = new Image[0];
        public Button[] answerButtons = new Button[3];
        public TMP_Text[] answerLabels = new TMP_Text[3];
        public Image[] answerIcons = new Image[3];
        public Sprite[] animalIcons = new Sprite[0];
        public Button backButton;
        public float delayAfterCorrect = 1.4f;
        public float dotSpacing = 80f;

        public Color dotDone = new Color(1f, 0.85f, 0.36f);
        public Color dotTodo = new Color(0.91f, 0.93f, 0.96f);
        public Color correctColor = new Color(0.45f, 0.79f, 0.43f);
        public Color tryAgainColor = new Color(1f, 0.7f, 0.42f);

        private Coroutine advance;

        protected override void OnInitialize()
        {
            backButton.onClick.AddListener(Game.Back);
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                answerButtons[i].onClick.AddListener(() => Answer(index));
            }

            Game.quizManager.OnQuestionShown += ShowQuestion;
            Game.quizManager.OnFeedbackShown += ShowFeedback;
            Game.quizManager.OnQuizFinished += (stars, total) => Game.ShowResults();
        }

        protected override void OnShow()
        {
            var session = Game.quizManager.Session;
            if (session != null && !session.IsFinished)
            {
                ShowQuestion(session.CurrentQuestion);
            }
        }

        protected override void OnHide()
        {
            if (advance != null)
            {
                StopCoroutine(advance);
                advance = null;
            }
        }

        private void ShowQuestion(AnimalData question)
        {
            if (question == null)
            {
                return;
            }

            var session = Game.quizManager.Session;
            questionText.text = question.quizQuestion;
            progressText.text = $"Soal {session.CurrentIndex + 1} dari {session.QuestionCount}";
            starsText.text = session.StarsEarned.ToString();
            feedbackText.text = string.Empty;

            for (int i = 0; i < progressDots.Length; i++)
            {
                progressDots[i].gameObject.SetActive(i < session.QuestionCount);
                ResultsScreen.CenterInRow(progressDots[i].rectTransform, i, Mathf.Min(session.QuestionCount, progressDots.Length), dotSpacing);
                progressDots[i].color = i < session.CurrentIndex ? dotDone : dotTodo;
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                bool hasAnswer = i < question.quizAnswers.Length;
                answerButtons[i].gameObject.SetActive(hasAnswer);
                if (!hasAnswer)
                {
                    continue;
                }

                string answer = question.quizAnswers[i];
                answerButtons[i].interactable = true;
                SetButtonAlpha(answerButtons[i], 1f);
                answerLabels[i].text = answer;

                if (i < answerIcons.Length && answerIcons[i] != null)
                {
                    var icon = IconForName(answer);
                    answerIcons[i].sprite = icon;
                    answerIcons[i].enabled = icon != null;
                }
            }
        }

        private void Answer(int index)
        {
            if (advance != null)
            {
                return;
            }

            bool correct = Game.quizManager.AnswerCurrentQuestion(index);
            if (correct)
            {
                Game.gameAudio.PlayCorrect();
                foreach (var button in answerButtons)
                {
                    button.interactable = false;
                }
                SetButtonAlpha(answerButtons[index], 1f);
                advance = StartCoroutine(AdvanceAfterDelay());
            }
            else
            {
                Game.gameAudio.PlayTryAgain();
                answerButtons[index].interactable = false;
                SetButtonAlpha(answerButtons[index], 0.45f);
            }
        }

        private void ShowFeedback(string message, bool correct)
        {
            feedbackText.text = message;
            feedbackText.color = correct ? correctColor : tryAgainColor;
            starsText.text = Game.quizManager.Session.StarsEarned.ToString();

            var pop = feedbackText.GetComponent<PopInEffect>();
            if (pop != null)
            {
                pop.Play();
            }
        }

        private IEnumerator AdvanceAfterDelay()
        {
            yield return new WaitForSeconds(delayAfterCorrect);
            advance = null;
            Game.quizManager.GoToNextQuestion();
        }

        private Sprite IconForName(string displayName)
        {
            int index = AnimalCatalog.Animals.FindIndex(a => a.displayName == displayName);
            return index >= 0 && index < animalIcons.Length ? animalIcons[index] : null;
        }

        // Keeps disabled answers soft instead of the default grey.
        private static void SetButtonAlpha(Button button, float alpha)
        {
            var group = button.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = button.gameObject.AddComponent<CanvasGroup>();
            }
            group.alpha = alpha;
        }
    }
}
