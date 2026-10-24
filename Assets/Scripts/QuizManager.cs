using System;
using UnityEngine;

namespace ARAnimalWorld
{
    // Runs a quiz round over every animal in the catalog and hands the
    // stars to ARAnimalWorldManager, which keeps the running total.
    public class QuizManager : MonoBehaviour
    {
        public const string CorrectFeedback = "Hebat sekali!";
        public const string TryAgainFeedback = "Ayo coba lagi!";

        public ARAnimalWorldManager appManager;

        public QuizSession Session { get; private set; }

        public event Action<AnimalData> OnQuestionShown;
        public event Action<string, bool> OnFeedbackShown;
        public event Action<int, int> OnQuizFinished;

        // firstAnimal puts that animal's question first, e.g. right after
        // the child has learned about it.
        public void StartQuiz(AnimalId? firstAnimal = null)
        {
            Session = new QuizSession(AnimalCatalog.Animals, firstAnimal);
            OnQuestionShown?.Invoke(Session.CurrentQuestion);
        }

        public bool AnswerCurrentQuestion(int selectedAnswerIndex)
        {
            if (Session == null || Session.IsFinished)
            {
                Debug.LogWarning("No quiz is running.");
                return false;
            }

            int starsBefore = Session.StarsEarned;
            bool isCorrect = Session.Answer(selectedAnswerIndex) == QuizAnswerResult.Correct;

            if (appManager != null && Session.StarsEarned > starsBefore)
            {
                appManager.AddStars(Session.StarsEarned - starsBefore);
            }

            OnFeedbackShown?.Invoke(isCorrect ? CorrectFeedback : TryAgainFeedback, isCorrect);
            return isCorrect;
        }

        public void GoToNextQuestion()
        {
            if (Session == null)
            {
                return;
            }

            if (Session.NextQuestion())
            {
                OnQuestionShown?.Invoke(Session.CurrentQuestion);
            }
            else
            {
                OnQuizFinished?.Invoke(Session.StarsEarned, Session.QuestionCount);
            }
        }
    }
}
