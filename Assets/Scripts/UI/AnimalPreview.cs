using UnityEngine;
using UnityEngine.UI;

namespace ARAnimalWorld
{
    // Shows a slowly turning 3D animal inside the UI (the detail screen),
    // without needing the AR camera. A small camera renders the animal,
    // which stands far away from the AR scene, into a RenderTexture.
    public class AnimalPreview : MonoBehaviour
    {
        public Camera previewCamera;
        public Transform spawnPoint;
        public RawImage target;
        public float turnSpeed = 25f;
        public int textureSize = 1024;

        private RenderTexture texture;
        private GameObject current;
        private AnimalId? currentId;

        // The stage stays inactive in the scene until the detail screen opens.
        private void EnsureTexture()
        {
            if (texture != null)
            {
                return;
            }

            texture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
            texture.name = "AnimalPreview";
            previewCamera.targetTexture = texture;
            if (target != null)
            {
                target.texture = texture;
            }
        }

        public void Show(AnimalData animal)
        {
            EnsureTexture();
            gameObject.SetActive(true);

            if (currentId != animal.id)
            {
                if (current != null)
                {
                    Destroy(current);
                }
                current = PlaceholderAnimalFactory.LoadOrCreate(animal, spawnPoint);
                currentId = animal.id;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            spawnPoint.Rotate(0f, turnSpeed * Time.deltaTime, 0f, Space.Self);
        }

        private void OnDestroy()
        {
            if (texture != null)
            {
                texture.Release();
            }
        }
    }
}
