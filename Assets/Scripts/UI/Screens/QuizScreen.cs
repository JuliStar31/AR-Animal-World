using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    // Screen 5: "Tebak Hewan!" One question per animal with three answer
    // cards. A wrong answer only fades that card and says "Ayo coba lagi!";
    // a right answer turns the card green and unlocks "Lanjut".
    public class QuizScreen : UIScreen
    {
        public override ScreenId Id => ScreenId.Quiz;

        public Image[] progressSegments = new Image[0];
        public TMP_Text starsText;
        public Image clueImage;
        public TMP_Text questionText;
        public Button[] answerButtons = new Button[3];
        public Image[] answerBorders = new Image[3];
        public TMP_Text[] answerLabels = new TMP_Text[3];
        public Image[] answerIcons = new Image[3];
        public Image feedbackPanel;
        public TMP_Text feedbackText;
        public Button replayButton;
        public Button nextButton;
        public Image nextFace;
        public Image nextShadow;
        public TMP_Text nextLabel;
        public Button backButton;

        public float segmentGap = 16f;
        public Color segmentDone = new Color32(27, 127, 59, 255);
        public Color segmentTodo = new Color32(236, 248, 254, 255);
        public Color borderIdle = new Color32(245, 190, 72, 255);
        public Color borderCorrect = new Color32(27, 127, 59, 255);
        public Color feedbackIdle = new Color32(232, 246, 254, 255);
        public Color feedbackCorrect = new Color32(214, 240, 214, 255);
        public Color feedbackTryAgain = new Color32(255, 236, 214, 255);
        public Color textIdle = new Color32(18, 74, 115, 255);
        public Color textCorrect = new Color32(27, 127, 59, 255);
        public Color textTryAgain = new Color32(201, 116, 31, 255);
        public Color nextOff = new Color32(214, 208, 192, 255);
        public Color nextOffShadow = new Color32(186, 178, 160, 255);
        public Color nextOffText = new Color32(92, 85, 70, 255);
        public Color nextOn = new Color32(27, 127, 59, 255);
        public Color nextOnShadow = new Color32(17, 89, 42, 255);

        private const string Prompt = "Pilih satu jawaban ya!";
        private bool answered;

        protected override void OnInitialize()
        {
            backButton.onClick.AddListener(Game.Back);
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                answerButtons[i].onClick.AddListener(() => Answer(index));
            }

            replayButton.onClick.AddListener(PlayHint);
            nextButton.onClick.AddListener(() =>
            {
                if (!answered)
                {
                    return;
                }
                Game.gameAudio.PlayTap();
                Game.quizManager.GoToNextQuestion();
            });

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

        private void ShowQuestion(AnimalData question)
        {
            if (question == null)
            {
                return;
            }

            var session = Game.quizManager.Session;
            answered = false;
            questionText.text = question.quizQuestion;
            starsText.text = Game.appManager.starsCollected.ToString();

            if (clueImage != null && Game.art != null)
            {
                // The clue shows the animal the question is about.
                var answer = question.quizAnswers[question.correctAnswerIndex];
                clueImage.sprite = Game.art.ForName(answer);
                clueImage.enabled = clueImage.sprite != null;
            }

            LayoutSegments(session.QuestionCount, session.CurrentIndex);
            SetFeedback(Prompt, feedbackIdle, textIdle);
            SetNextEnabled(false);

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
                SetAlpha(answerButtons[i], 1f);
                answerBorders[i].color = borderIdle;
                answerLabels[i].text = answer;

                if (i < answerIcons.Length && answerIcons[i] != null && Game.art != null)
                {
                    answerIcons[i].sprite = Game.art.ForName(answer);
                    answerIcons[i].enabled = answerIcons[i].sprite != null;
                }
            }
        }

        private void Answer(int index)
        {
            if (answered)
            {
                return;
            }

            bool correct = Game.quizManager.AnswerCurrentQuestion(index);
            if (correct)
            {
                answered = true;
                Game.gameAudio.PlayCorrect();
                foreach (var button in answerButtons)
                {
                    button.interactable = false;
                }
                answerBorders[index].color = borderCorrect;
                SetNextEnabled(true);
            }
            else
            {
                Game.gameAudio.PlayTryAgain();
                answerButtons[index].interactable = false;
                SetAlpha(answerButtons[index], 0.45f);
            }
        }

        private void ShowFeedback(string message, bool correct)
        {
            SetFeedback(message, correct ? feedbackCorrect : feedbackTryAgain, correct ? textCorrect : textTryAgain);
            starsText.text = Game.appManager.starsCollected.ToString();

            var pop = feedbackPanel.GetComponent<PopInEffect>();
            if (pop != null)
            {
                pop.Play();
            }
        }

        // "Ulangi contoh": plays the sound of the animal the question is
        // about, as a hint.
        private void PlayHint()
        {
            var question = Game.quizManager.Session?.CurrentQuestion;
            if (question == null)
            {
                return;
            }
            var answer = question.quizAnswers[question.correctAnswerIndex];
            var animal = AnimalCatalog.Animals.Find(a => a.displayName == answer);
            Game.gameAudio.PlayAnimal(animal ?? question, false);
        }

        private void SetFeedback(string message, Color panel, Color text)
        {
            feedbackText.text = message;
            feedbackText.color = text;
            feedbackPanel.color = panel;
        }

        private void SetNextEnabled(bool enabled)
        {
            nextButton.interactable = enabled;
            nextFace.color = enabled ? nextOn : nextOff;
            nextShadow.color = enabled ? nextOnShadow : nextOffShadow;
            nextLabel.color = enabled ? Color.white : nextOffText;
        }

        // Splits the bar into one segment per question.
        private void LayoutSegments(int count, int done)
        {
            int shown = Mathf.Min(count, progressSegments.Length);
            if (shown == 0)
            {
                return;
            }

            var parent = (RectTransform)progressSegments[0].rectTransform.parent;
            float width = (parent.rect.width - segmentGap * (shown - 1)) / shown;
            for (int i = 0; i < progressSegments.Length; i++)
            {
                var segment = progressSegments[i];
                segment.gameObject.SetActive(i < shown);
                if (i >= shown)
                {
                    continue;
                }

                var rect = segment.rectTransform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2(width, 0f);
                rect.anchoredPosition = new Vector2(i * (width + segmentGap), 0f);
                segment.color = i < done ? segmentDone : segmentTodo;
            }
        }

        // Keeps disabled answers soft instead of the default grey.
        private static void SetAlpha(Button button, float alpha)
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
