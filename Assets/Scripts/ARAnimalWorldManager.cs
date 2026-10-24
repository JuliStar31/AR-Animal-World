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
        public event Action<AnimalData> OnAnimalDetected;
        public event Action<AnimalData> OnAnimalLost;

        public bool IsAnimalInView { get; private set; }

        public AnimalData SelectedAnimal => AnimalCatalog.Animals.Find(a => a.id == selectedAnimalId);

        public void SelectAnimal(AnimalId animalId)
        {
            selectedAnimalId = animalId;
            OnAnimalSelected?.Invoke(SelectedAnimal);
        }

        // Called by AnimalImageTargetHandler when a printed card is scanned.
        public void HandleAnimalDetected(AnimalId animalId)
        {
            IsAnimalInView = true;
            SelectAnimal(animalId);
            OnAnimalDetected?.Invoke(SelectedAnimal);
        }

        public void HandleAnimalLost(AnimalId animalId)
        {
            if (animalId != selectedAnimalId)
            {
                return;
            }

            IsAnimalInView = false;
            OnAnimalLost?.Invoke(GetAnimalById(animalId));
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
