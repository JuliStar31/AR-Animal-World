using UnityEngine;

namespace ARAnimalWorld
{
    // Entry point of the game. Holds the shared managers and moves the
    // child between the six screens.
    public class GameFlow : MonoBehaviour
    {
        public ARAnimalWorldManager appManager;
        public QuizManager quizManager;
        public GameAudio gameAudio;
        public ScreenRouter router;
        public AnimalPreview animalPreview;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            router.Initialize(this);
            router.ShowHome();
        }

        public void GoHome()
        {
            gameAudio.Stop();
            router.ShowHome();
        }

        public void OpenAnimalMenu()
        {
            router.Show(ScreenId.AnimalMenu);
        }

        public void OpenScan()
        {
            router.Show(ScreenId.Scan);
        }

        public void OpenAnimal(AnimalId id)
        {
            appManager.SelectAnimal(id);
            router.Show(ScreenId.AnimalDetail);
        }

        public void StartQuiz(AnimalId? firstAnimal = null)
        {
            gameAudio.Stop();
            quizManager.StartQuiz(firstAnimal);
            router.Show(ScreenId.Quiz);
        }

        public void ShowResults()
        {
            // Back from the results goes home rather than into a finished quiz.
            router.Show(ScreenId.Results, false);
        }

        public void Back()
        {
            gameAudio.PlayTap();
            router.Back();
        }
    }
}
