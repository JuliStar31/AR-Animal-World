using System;
using System.Collections.Generic;

namespace ARAnimalWorld
{
    public enum QuizAnswerResult
    {
        Correct,
        TryAgain
    }

    // One round of the quiz: one question per animal. Kids may retry a
    // question until they find the right answer, but only a correct answer
    // on the first try earns a star. Plain C# so it stays easy to test.
    public class QuizSession
    {
        private readonly List<AnimalData> questions;
        private bool missedCurrent;

        public int CurrentIndex { get; private set; }
        public int StarsEarned { get; private set; }
        public int QuestionCount => questions.Count;
        public bool IsFinished => CurrentIndex >= questions.Count;
        public AnimalData CurrentQuestion => IsFinished ? null : questions[CurrentIndex];

        public QuizSession(IEnumerable<AnimalData> animals, AnimalId? firstAnimal = null, Random random = null)
        {
            questions = new List<AnimalData>(animals);
            Shuffle(questions, random ?? new Random());

            if (firstAnimal.HasValue)
            {
                int index = questions.FindIndex(a => a.id == firstAnimal.Value);
                if (index > 0)
                {
                    var first = questions[index];
                    questions.RemoveAt(index);
                    questions.Insert(0, first);
                }
            }
        }

        public QuizAnswerResult Answer(int answerIndex)
        {
            if (IsFinished)
            {
                throw new InvalidOperationException("The quiz is already finished.");
            }

            if (answerIndex != CurrentQuestion.correctAnswerIndex)
            {
                missedCurrent = true;
                return QuizAnswerResult.TryAgain;
            }

            if (!missedCurrent)
            {
                StarsEarned += 1;
            }

            return QuizAnswerResult.Correct;
        }

        // Moves on after a correct answer. Returns false when the quiz is over.
        public bool NextQuestion()
        {
            if (IsFinished)
            {
                return false;
            }

            CurrentIndex += 1;
            missedCurrent = false;
            return !IsFinished;
        }

        private static void Shuffle(List<AnimalData> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
