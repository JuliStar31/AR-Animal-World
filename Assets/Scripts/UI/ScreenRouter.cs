using System.Collections.Generic;
using UnityEngine;

namespace ARAnimalWorld
{
    // Shows one screen at a time and remembers the path for the back
    // button (both the on-screen one and Android's system back).
    public class ScreenRouter : MonoBehaviour
    {
        public UIScreen[] screens;

        private readonly Dictionary<ScreenId, UIScreen> byId = new Dictionary<ScreenId, UIScreen>();
        private readonly Stack<ScreenId> history = new Stack<ScreenId>();

        public UIScreen Current { get; private set; }

        public void Initialize(GameFlow game)
        {
            foreach (var screen in screens)
            {
                if (screen == null)
                {
                    continue;
                }

                byId[screen.Id] = screen;
                screen.Initialize(game);
            }
        }

        public void Show(ScreenId id, bool remember = true)
        {
            if (!byId.TryGetValue(id, out var next))
            {
                Debug.LogWarning($"Screen {id} is missing from the router.");
                return;
            }

            if (Current != null)
            {
                if (remember && Current != next)
                {
                    history.Push(Current.Id);
                }
                Current.Hide();
            }

            Current = next;
            Current.Show();
        }

        // Going home clears the path, so back from the menu exits cleanly.
        public void ShowHome()
        {
            history.Clear();
            Show(ScreenId.MainMenu, false);
        }

        public void Back()
        {
            if (history.Count == 0)
            {
                if (Current != null && Current.Id == ScreenId.MainMenu)
                {
                    Application.Quit();
                }
                else
                {
                    ShowHome();
                }
                return;
            }

            Show(history.Pop(), false);
        }

        private void Update()
        {
            // Android's back button arrives as Escape.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Back();
            }
        }
    }
}
