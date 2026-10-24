using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARAnimalWorld
{
    public class ARAnimalWorldManager : MonoBehaviour
    {
        private const string StarsPrefsKey = "araw_stars_total";

        public AnimalId selectedAnimalId;
        public int starsCollected;

        public event Action<AnimalData> OnAnimalSelected;
        public event Action<int> OnStarsUpdated;
        public event Action<AnimalData> OnAnimalDetected;
        public event Action<AnimalData> OnAnimalLost;

        public bool IsAnimalInView { get; private set; }

        // True when the animal in view was recognised in a picture by
        // AnimalPictureRecognizer rather than found on a printed card.
        public bool IsFromPicture { get; private set; }

        public AnimalData SelectedAnimal => AnimalCatalog.Animals.Find(a => a.id == selectedAnimalId);

        private void Awake()
        {
            // Stars stay on the phone between sessions.
            starsCollected = PlayerPrefs.GetInt(StarsPrefsKey, 0);
        }

        public void SelectAnimal(AnimalId animalId)
        {
            selectedAnimalId = animalId;
            OnAnimalSelected?.Invoke(SelectedAnimal);
        }

        // Called by AnimalImageTargetHandler when a printed card is scanned,
        // and by AnimalPictureRecognizer when it recognises a picture.
        public void HandleAnimalDetected(AnimalId animalId, bool fromPicture = false)
        {
            IsAnimalInView = true;
            IsFromPicture = fromPicture;
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
            IsFromPicture = false;
            OnAnimalLost?.Invoke(GetAnimalById(animalId));
        }

        public void AddStars(int amount)
        {
            starsCollected += amount;
            SaveStars();
            OnStarsUpdated?.Invoke(starsCollected);
        }

        public AnimalData GetAnimalById(AnimalId animalId)
        {
            return AnimalCatalog.Animals.Find(a => a.id == animalId);
        }

        public void ResetProgress()
        {
            starsCollected = 0;
            SaveStars();
            OnStarsUpdated?.Invoke(starsCollected);
        }

        private void SaveStars()
        {
            PlayerPrefs.SetInt(StarsPrefsKey, starsCollected);
            PlayerPrefs.Save();
        }
    }
}
