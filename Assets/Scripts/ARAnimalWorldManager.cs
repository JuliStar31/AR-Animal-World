using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARAnimalWorld
{
    public class ARAnimalWorldManager : MonoBehaviour
    {
        public AnimalId selectedAnimalId;
        public int starsCollected;
        public int quizStars;
        public int currentQuestionIndex;

        public event Action<AnimalData> OnAnimalSelected;
        public event Action<int> OnStarsUpdated;
        public event Action<int> OnQuizAnswered;

        public AnimalData SelectedAnimal => AnimalCatalog.Animals.Find(a => a.id == selectedAnimalId);

        public void SelectAnimal(AnimalId animalId)
        {
            selectedAnimalId = animalId;
            OnAnimalSelected?.Invoke(SelectedAnimal);
        }

        public void AddStars(int amount)
        {
            starsCollected += amount;
            OnStarsUpdated?.Invoke(starsCollected);
        }

        public void SubmitQuizAnswer(int answerIndex)
        {
            var selectedAnimal = SelectedAnimal;
            bool isCorrect = answerIndex == selectedAnimal.correctAnswerIndex;

            if (isCorrect)
            {
                quizStars += 1;
                AddStars(1);
            }

            OnQuizAnswered?.Invoke(isCorrect ? 1 : 0);
            currentQuestionIndex += 1;
        }

        public AnimalData GetAnimalById(AnimalId animalId)
        {
            return AnimalCatalog.Animals.Find(a => a.id == animalId);
        }

        public void ResetProgress()
        {
            starsCollected = 0;
            quizStars = 0;
            currentQuestionIndex = 0;
            OnStarsUpdated?.Invoke(starsCollected);
        }
    }
}
