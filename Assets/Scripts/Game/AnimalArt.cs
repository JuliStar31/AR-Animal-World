using UnityEngine;

namespace ARAnimalWorld
{
    // The flat illustrations of each animal used by the 2D screens, in the
    // same order as AnimalCatalog.Animals. Filled in by the scene builder.
    public class AnimalArt : MonoBehaviour
    {
        public Sprite[] illustrations = new Sprite[0];
        public Sprite happyMascot;
        public Sprite starOn;
        public Sprite starOff;

        public Sprite For(AnimalId id)
        {
            int index = AnimalCatalog.Animals.FindIndex(a => a.id == id);
            return index >= 0 && index < illustrations.Length ? illustrations[index] : null;
        }

        public Sprite ForName(string displayName)
        {
            var animal = AnimalCatalog.Animals.Find(a => a.displayName == displayName);
            return animal != null ? For(animal.id) : null;
        }
    }
}
