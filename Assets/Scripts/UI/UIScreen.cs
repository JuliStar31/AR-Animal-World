using UnityEngine;

namespace ARAnimalWorld
{
    // Base class for the six full-screen panels. ScreenRouter shows one at
    // a time; subclasses fill in their content in OnShow.
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreen : MonoBehaviour
    {
        public abstract ScreenId Id { get; }

        protected GameFlow Game { get; private set; }

        private CanvasGroup canvasGroup;

        public void Initialize(GameFlow game)
        {
            Game = game;
            canvasGroup = GetComponent<CanvasGroup>();
            OnInitialize();
            SetVisible(false);
        }

        public void Show()
        {
            SetVisible(true);
            OnShow();
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                OnHide();
            }
            SetVisible(false);
        }

        // Wire button listeners here; runs once at startup.
        protected virtual void OnInitialize() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        private void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            gameObject.SetActive(visible);
        }
    }
}
