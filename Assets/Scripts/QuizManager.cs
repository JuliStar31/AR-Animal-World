using System;
using UnityEngine;

namespace ARAnimalWorld
{
    public class QuizManager : MonoBehaviour
    {
        public ARAnimalWorldManager appManager;

        public event Action<string> OnFeedbackShown;

        public void AnswerAnimalQuestion(int selectedAnswerIndex)
        {
            if (appManager == null)
            {
                Debug.LogWarning("ARAnimalWorldManager not assigned.");
                return;
            }

            var selectedAnimal = appManager.SelectedAnimal;
            if (selectedAnimal == null)
            {
                Debug.LogWarning("No animal selected.");
                return;
            }

            bool isCorrect = selectedAnswerIndex == selectedAnimal.correctAnswerIndex;
            string feedback = isCorrect ? "Hebat sekali!" : "Ayo coba lagi!";
            OnFeedbackShown?.Invoke(feedback);

            appManager.SubmitQuizAnswer(selectedAnswerIndex);
        }
    }
}
