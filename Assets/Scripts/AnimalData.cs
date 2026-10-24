using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARAnimalWorld
{
    public enum AnimalId
    {
        Lion,
        Elephant,
        Rabbit
    }

    [Serializable]
    public class AnimalData
    {
        public AnimalId id;
        public string displayName;
        public string fact;
        public string narrationText;
        public string soundClipName;
        public string modelName;
        public string quizQuestion;
        public string[] quizAnswers;
        public int correctAnswerIndex;
        public Color accentColor = Color.white;

        public AnimalData(AnimalId id, string displayName, string fact, string narrationText,
            string soundClipName, string modelName, string quizQuestion,
            string[] quizAnswers, int correctAnswerIndex, Color accentColor)
        {
            this.id = id;
            this.displayName = displayName;
            this.fact = fact;
            this.narrationText = narrationText;
            this.soundClipName = soundClipName;
            this.modelName = modelName;
            this.quizQuestion = quizQuestion;
            this.quizAnswers = quizAnswers;
            this.correctAnswerIndex = correctAnswerIndex;
            this.accentColor = accentColor;
        }
    }

    public static class AnimalCatalog
    {
        public static readonly List<AnimalData> Animals = new List<AnimalData>
        {
            new AnimalData(
                AnimalId.Lion,
                "Singa",
                "Singa punya suara auman yang keras!",
                "Hai, aku singa! Aku suka berjemur di bawah sinar matahari.",
                "lion_roar",
                "LionModel",
                "Hewan mana yang bisa mengaum dengan keras?",
                new[] { "Kelinci", "Singa", "Gajah" },
                1,
                new Color(1f, 0.73f, 0.38f)
            ),
            new AnimalData(
                AnimalId.Elephant,
                "Gajah",
                "Gajah memakai belalai untuk mengambil makanan dan minum.",
                "Halo, aku gajah! Belalaiku sangat berguna untuk makan dan minum.",
                "elephant_trumpet",
                "ElephantModel",
                "Hewan mana yang punya belalai?",
                new[] { "Gajah", "Singa", "Kelinci" },
                0,
                new Color(0.55f, 0.76f, 0.95f)
            ),
            new AnimalData(
                AnimalId.Rabbit,
                "Kelinci",
                "Kelinci suka melompat dan makan wortel.",
                "Hai, aku kelinci! Aku suka melompat dengan lincah.",
                "rabbit_squeak",
                "RabbitModel",
                "Hewan mana yang suka melompat?",
                new[] { "Singa", "Gajah", "Kelinci" },
                2,
                new Color(0.52f, 0.78f, 0.48f)
            )
        };
    }
}
