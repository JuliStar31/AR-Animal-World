using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vuforia;
using static ARAnimalWorld.EditorTools.UIKit;

namespace ARAnimalWorld.EditorTools
{
    // Menu: AR Animal World > Bangun Game di Scene Ini
    // Builds the whole game into the open scene: managers, the six UI
    // screens, the 3D preview stage, and wires any Vuforia Image Targets
    // already in the scene to the right animal. Safe to run again; it
    // replaces what it built before.
    public static class ARAnimalWorldSceneBuilder
    {
        private const string ManagersName = "AR Animal World";
        private const string CanvasName = "AR Animal World UI";
        private const string PreviewName = "AR Animal World Preview";

        private static readonly Vector2 Top = new Vector2(0.5f, 1f);
        private static readonly Vector2 Bottom = new Vector2(0.5f, 0f);
        private static readonly Vector2 Middle = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 TopRight = new Vector2(1f, 1f);

        private static Sprite[] icons;

        [MenuItem("AR Animal World/Bangun Game di Scene Ini", priority = 0)]
        public static void BuildGame()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") == null)
            {
                EditorUtility.DisplayDialog("TextMeshPro belum siap",
                    "Buka Window > TextMeshPro > Import TMP Essential Resources, klik Import, lalu jalankan menu ini lagi.",
                    "OK");
                return;
            }

            GeneratedArt.CreateShapes();
            icons = GeneratedArt.RenderAnimalIcons();

            DestroyExisting(ManagersName);
            DestroyExisting(CanvasName);
            DestroyExisting(PreviewName);

            var flow = BuildManagers();
            flow.animalPreview = BuildPreviewStage(out var previewImageHolder);
            EnsureEventSystem();
            EnsureCamera();

            var canvas = BuildCanvas();
            var screens = new List<UIScreen>
            {
                BuildMainMenu(canvas, flow),
                BuildAnimalMenu(canvas),
                BuildScan(canvas),
                BuildDetail(canvas, flow),
                BuildQuiz(canvas),
                BuildResults(canvas)
            };

            var router = canvas.gameObject.AddComponent<ScreenRouter>();
            router.screens = screens.ToArray();
            flow.router = router;

            int wired = WireVuforia(flow.appManager);

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = flow.gameObject;

            string vuforiaNote = wired > 0
                ? $"{wired} Image Target terhubung ke hewan."
                : "Belum ada Vuforia Image Target di scene. Tambahkan ARCamera dan Image Target (lihat docs/Panduan-Build-APK.md), lalu jalankan menu ini lagi untuk menghubungkannya.";
            EditorUtility.DisplayDialog("AR Animal World siap",
                "Semua layar sudah dibuat. Tekan Play untuk mencoba, lalu simpan scene (Ctrl+S).\n\n" + vuforiaNote,
                "Mantap");
        }

        [MenuItem("AR Animal World/Hapus Bintang Tersimpan", priority = 20)]
        public static void ResetStars()
        {
            PlayerPrefs.DeleteKey("araw_stars_total");
            PlayerPrefs.Save();
            Debug.Log("Bintang tersimpan sudah dihapus.");
        }

        private static void DestroyExisting(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == name)
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static GameFlow BuildManagers()
        {
            var go = new GameObject(ManagersName);
            var app = go.AddComponent<ARAnimalWorldManager>();
            var quiz = go.AddComponent<QuizManager>();
            quiz.appManager = app;
            go.AddComponent<AudioSource>();
            var audio = go.AddComponent<GameAudio>();
            var flow = go.AddComponent<GameFlow>();
            flow.appManager = app;
            flow.quizManager = quiz;
            flow.gameAudio = audio;
            return flow;
        }

        private static AnimalPreview BuildPreviewStage(out GameObject holder)
        {
            // Far below the AR scene, and only active on the detail screen.
            holder = new GameObject(PreviewName);
            holder.transform.position = new Vector3(0f, -500f, 0f);

            var spawn = new GameObject("SpawnPoint").transform;
            spawn.SetParent(holder.transform, false);
            spawn.localRotation = Quaternion.Euler(0f, -25f, 0f);

            var camGo = new GameObject("PreviewCamera");
            camGo.transform.SetParent(holder.transform, false);
            var cam = camGo.AddComponent<Camera>();
            GeneratedArt.ConfigurePreviewCamera(cam);
            cam.depth = -10f;

            var lightGo = new GameObject("PreviewLight");
            lightGo.transform.SetParent(holder.transform, false);
            lightGo.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.6f;

            var preview = holder.AddComponent<AnimalPreview>();
            preview.previewCamera = cam;
            preview.spawnPoint = spawn;
            holder.SetActive(false);
            return preview;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        private static void EnsureCamera()
        {
            // Vuforia's ARCamera counts; this only helps a scene with no camera.
            if (Object.FindObjectOfType<Camera>() == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camGo.tag = "MainCamera";
                var cam = camGo.GetComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = SkyBlue;
            }

            if (Object.FindObjectOfType<AudioListener>() == null)
            {
                Object.FindObjectOfType<Camera>().gameObject.AddComponent<AudioListener>();
            }
        }

        private static Transform BuildCanvas()
        {
            var go = new GameObject(CanvasName, typeof(RectTransform));
            go.layer = 5;
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            go.AddComponent<GraphicRaycaster>();
            return go.transform;
        }

        // Full-screen panel with an optional background and a safe-area child
        // that holds the content.
        private static T NewScreen<T>(Transform canvas, string name, Color? background, out Transform content) where T : UIScreen
        {
            var rect = Stretch(Rect(name, canvas));
            rect.gameObject.AddComponent<CanvasGroup>();
            if (background.HasValue)
            {
                var bg = rect.gameObject.AddComponent<Image>();
                bg.color = background.Value;
            }

            var safe = Stretch(Rect("SafeArea", rect));
            safe.gameObject.AddComponent<SafeAreaFitter>();
            content = safe;
            return rect.gameObject.AddComponent<T>();
        }

        private static Button BackButton(Transform parent)
        {
            return CircleButton("BackButton", parent, "<", White, BlueInk, TopLeft, new Vector2(110f, -110f), 140f, 80f);
        }

        private static TextMeshProUGUI Title(Transform parent, string text, float y = -110f)
        {
            var title = Text("Title", parent, text, 76f, BlueInk, FontStyles.Bold);
            Place(title.rectTransform, Top, new Vector2(0f, y), new Vector2(640f, 140f));
            return title;
        }

        private static Sprite IconFor(AnimalId id)
        {
            int index = AnimalCatalog.Animals.FindIndex(a => a.id == id);
            return icons != null && index >= 0 && index < icons.Length ? icons[index] : null;
        }

        // 1. Welcome / Main Menu
        private static UIScreen BuildMainMenu(Transform canvas, GameFlow flow)
        {
            var screen = NewScreen<MainMenuScreen>(canvas, "1 MainMenu", SkyBlue, out var c);

            var sun = Panel("Sun", c, SunYellow, GeneratedArt.Circle);
            Place(sun.rectTransform, TopLeft, new Vector2(140f, -150f), new Vector2(200f, 200f));
            var grass = Panel("Grass", c.parent, FreshGreen, GeneratedArt.Rounded);
            grass.rectTransform.SetSiblingIndex(0);
            grass.rectTransform.anchorMin = Vector2.zero;
            grass.rectTransform.anchorMax = new Vector2(1f, 0f);
            grass.rectTransform.offsetMin = new Vector2(-80f, -80f);
            grass.rectTransform.offsetMax = new Vector2(80f, 720f);

            screen.starsLabel = StarBadge(c, TopRight, new Vector2(-170f, -110f));

            var title = Text("Title", c, "AR Animal World", 112f, BlueInk, FontStyles.Bold);
            Place(title.rectTransform, Top, new Vector2(0f, -390f), new Vector2(1000f, 160f));
            var subtitle = Text("Subtitle", c, "Yuk, kenali dunia hewan!", 60f, TextDark);
            Place(subtitle.rectTransform, Top, new Vector2(0f, -520f), new Vector2(1000f, 100f));

            var hero = Icon("HeroLion", c, IconFor(AnimalId.Lion), Middle, new Vector2(0f, 120f), 640f);
            hero.gameObject.AddComponent<PulseEffect>().amount = 0.03f;

            screen.startLearningButton = Button("StartLearningButton", c, "Mulai Belajar", GrassDark, White, Bottom, new Vector2(0f, 560f), new Vector2(780f, 170f), 66f);
            screen.quizButton = Button("QuizButton", c, "Main Kuis", BlueInk, White, Bottom, new Vector2(0f, 370f), new Vector2(780f, 150f), 60f);
            screen.scanButton = SecondaryButton("ScanButton", c, "Scan Kartu", SoftOrange, Bottom, new Vector2(0f, 190f), new Vector2(780f, 140f));
            return screen;
        }

        // 2. Animal Learning Menu
        private static UIScreen BuildAnimalMenu(Transform canvas)
        {
            var screen = NewScreen<AnimalMenuScreen>(canvas, "2 AnimalMenu", SkyBlue, out var c);
            screen.backButton = BackButton(c);
            Title(c, "Pilih Hewan");

            var cards = new List<AnimalCardView>();
            for (int i = 0; i < AnimalCatalog.Animals.Count; i++)
            {
                var animal = AnimalCatalog.Animals[i];
                var card = Panel(animal.displayName + "Card", c, White, GeneratedArt.Rounded, true);
                Place(card.rectTransform, Top, new Vector2(0f, -470f - i * 470f), new Vector2(960f, 420f));

                var circle = Panel("IconBackground", card.transform, animal.accentColor, GeneratedArt.Circle);
                Place(circle.rectTransform, new Vector2(0f, 0.5f), new Vector2(200f, 0f), new Vector2(320f, 320f));
                Icon("Icon", card.transform, IconFor(animal.id), new Vector2(0f, 0.5f), new Vector2(200f, 10f), 330f);

                var name = Text("Name", card.transform, animal.displayName, 84f, TextDark, FontStyles.Bold, TextAlignmentOptions.Left);
                Place(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(660f, 90f), new Vector2(520f, 120f));

                var learn = Button("LearnButton", card.transform, "Belajar", GrassDark, White, new Vector2(0f, 0.5f), new Vector2(535f, -80f), new Vector2(250f, 130f), 50f);
                var scan = Button("ScanButton", card.transform, "Scan AR", BlueInk, White, new Vector2(0f, 0.5f), new Vector2(800f, -80f), new Vector2(250f, 130f), 50f);

                cards.Add(new AnimalCardView { animalId = animal.id, learnButton = learn, scanButton = scan, nameLabel = name });
            }

            screen.cards = cards.ToArray();
            return screen;
        }

        // 3. AR Camera / Scan Screen (transparent so the camera shows through)
        private static UIScreen BuildScan(Transform canvas)
        {
            var screen = NewScreen<ScanScreen>(canvas, "3 Scan", null, out var c);
            screen.backButton = BackButton(c);

            var banner = Panel("GuideBanner", c, White, GeneratedArt.Rounded, true);
            Place(banner.rectTransform, Top, new Vector2(70f, -110f), new Vector2(780f, 150f));
            screen.guideText = Text("GuideText", banner.transform, "Arahkan kamera ke kartu hewan!", 48f, BlueInk, FontStyles.Bold);
            Stretch(screen.guideText.rectTransform, 30f, 30f, 10f, 10f);

            var frame = Panel("ScanFrame", c, White, GeneratedArt.Frame);
            Place(frame.rectTransform, Middle, new Vector2(0f, 60f), new Vector2(760f, 760f));
            frame.raycastTarget = false;
            frame.gameObject.AddComponent<PulseEffect>();
            var hints = Rect("CardHints", frame.transform);
            Place(hints, Bottom, new Vector2(0f, -110f), new Vector2(600f, 160f));
            for (int i = 0; i < AnimalCatalog.Animals.Count; i++)
            {
                var hintCard = Panel("Hint" + AnimalCatalog.Animals[i].displayName, hints, new Color(1f, 1f, 1f, 0.85f), GeneratedArt.Rounded);
                Place(hintCard.rectTransform, Middle, new Vector2((i - 1) * 200f, 0f), new Vector2(160f, 160f));
                hintCard.raycastTarget = false;
                Icon("Icon", hintCard.transform, icons[i], Middle, Vector2.zero, 140f);
            }
            screen.scanFrame = frame.gameObject;

            // Bottom info panel, shown when a card is found.
            var info = Panel("InfoPanel", c, White, GeneratedArt.Rounded, true);
            Place(info.rectTransform, Bottom, new Vector2(0f, 360f), new Vector2(1000f, 640f));
            info.gameObject.AddComponent<PopInEffect>();
            screen.infoPanel = info.gameObject;
            screen.nameText = Text("Name", info.transform, "Singa", 84f, GrassDark, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(screen.nameText.rectTransform, Top, new Vector2(-100f, -90f), new Vector2(720f, 120f));
            screen.factText = Text("Fact", info.transform, "", 48f, TextDark, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            Place(screen.factText.rectTransform, Top, new Vector2(-60f, -270f), new Vector2(800f, 220f));
            screen.soundButton = CircleButton("SoundButton", info.transform, "", SunYellow, TextDark, TopRight, new Vector2(-120f, -120f), 180f, 40f);
            SoundIcon(screen.soundButton.transform);
            screen.learnButton = Button("LearnButton", info.transform, "Pelajari", GrassDark, White, Bottom, new Vector2(-235f, 110f), new Vector2(440f, 140f), 54f);
            screen.quizButton = Button("QuizButton", info.transform, "Kuis", BlueInk, White, Bottom, new Vector2(235f, 110f), new Vector2(440f, 140f), 54f);
            info.gameObject.SetActive(false);

            // Gentle camera problem message.
            var message = Panel("CameraMessage", c, Cream, GeneratedArt.Rounded, true);
            Place(message.rectTransform, Middle, new Vector2(0f, 60f), new Vector2(900f, 420f));
            screen.cameraMessagePanel = message.gameObject;
            screen.cameraMessageText = Text("Text", message.transform, "", 50f, TextDark);
            Stretch(screen.cameraMessageText.rectTransform, 50f, 50f, 40f, 40f);
            message.gameObject.SetActive(false);

            // Test buttons (Editor and development builds only).
            var simulate = Panel("SimulatePanel", c, new Color(1f, 1f, 1f, 0.8f), GeneratedArt.Rounded);
            Place(simulate.rectTransform, Top, new Vector2(0f, -300f), new Vector2(960f, 150f));
            var simLabel = Text("Label", simulate.transform, "Tes:", 40f, TextDark, FontStyles.Bold);
            Place(simLabel.rectTransform, new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(130f, 100f));
            var simButtons = new List<Button>();
            for (int i = 0; i < AnimalCatalog.Animals.Count; i++)
            {
                simButtons.Add(Button("Simulate" + AnimalCatalog.Animals[i].id, simulate.transform, AnimalCatalog.Animals[i].displayName,
                    Peach, White, new Vector2(0f, 0.5f), new Vector2(285f + i * 255f, 0f), new Vector2(235f, 110f), 42f));
            }
            screen.simulatePanel = simulate.gameObject;
            screen.simulateButtons = simButtons.ToArray();
            return screen;
        }

        // 4. Animal Detail / Learning Screen
        private static UIScreen BuildDetail(Transform canvas, GameFlow flow)
        {
            var screen = NewScreen<AnimalDetailScreen>(canvas, "4 AnimalDetail", SkyBlue, out var c);
            screen.backButton = BackButton(c);
            screen.nameText = Title(c, "Singa");

            var stageCard = Panel("PreviewCard", c, Peach, GeneratedArt.Rounded);
            Place(stageCard.rectTransform, Top, new Vector2(0f, -620f), new Vector2(900f, 760f));
            screen.accentBackground = stageCard;

            var raw = Rect("Preview", stageCard.transform).gameObject.AddComponent<RawImage>();
            Stretch(raw.rectTransform);
            raw.raycastTarget = false;
            flow.animalPreview.target = raw;

            screen.factText = Text("Fact", c, "", 54f, TextDark, FontStyles.Bold);
            Place(screen.factText.rectTransform, Top, new Vector2(0f, -1100f), new Vector2(960f, 170f));

            var bubble = Panel("NarrationBubble", c, Cream, GeneratedArt.Rounded, true);
            Place(bubble.rectTransform, Top, new Vector2(0f, -1290f), new Vector2(960f, 180f));
            screen.narrationText = Text("Narration", bubble.transform, "", 42f, BlueInk, FontStyles.Italic);
            Stretch(screen.narrationText.rectTransform, 40f, 40f, 15f, 15f);

            screen.listenButton = Button("ListenButton", c, "Dengarkan Suara", SunYellow, TextDark, Bottom, new Vector2(0f, 460f), new Vector2(960f, 150f), 58f);
            screen.scanButton = Button("ScanButton", c, "Scan Kartu", BlueInk, White, Bottom, new Vector2(-245f, 285f), new Vector2(470f, 140f), 52f);
            screen.quizButton = Button("QuizButton", c, "Kuis", GrassDark, White, Bottom, new Vector2(245f, 285f), new Vector2(470f, 140f), 52f);
            screen.otherAnimalButton = SecondaryButton("OtherAnimalButton", c, "Coba Hewan Lain", BlueInk, Bottom, new Vector2(0f, 120f), new Vector2(960f, 130f), 50f);
            return screen;
        }

        // 5. Quiz Screen
        private static UIScreen BuildQuiz(Transform canvas)
        {
            var screen = NewScreen<QuizScreen>(canvas, "5 Quiz", SkyBlue, out var c);
            screen.backButton = BackButton(c);
            Title(c, "Tebak Hewan!");
            screen.starsText = StarBadge(c, TopRight, new Vector2(-170f, -110f));

            screen.progressText = Text("Progress", c, "Soal 1 dari 3", 44f, TextDark);
            Place(screen.progressText.rectTransform, Top, new Vector2(0f, -260f), new Vector2(600f, 70f));
            var dots = new List<Image>();
            for (int i = 0; i < 5; i++)
            {
                var dot = Panel("Dot" + (i + 1), c, SoftGray, GeneratedArt.Circle);
                Place(dot.rectTransform, Top, new Vector2((i - 1) * 80f, -335f), new Vector2(50f, 50f));
                dots.Add(dot);
            }
            screen.progressDots = dots.ToArray();

            var clue = Panel("Clue", c, SunYellow, GeneratedArt.Circle, true);
            Place(clue.rectTransform, Top, new Vector2(0f, -600f), new Vector2(380f, 380f));
            clue.gameObject.AddComponent<PulseEffect>().amount = 0.04f;
            var mark = Text("QuestionMark", clue.transform, "?", 260f, White, FontStyles.Bold);
            Stretch(mark.rectTransform);

            screen.questionText = Text("Question", c, "", 64f, TextDark, FontStyles.Bold);
            Place(screen.questionText.rectTransform, Top, new Vector2(0f, -930f), new Vector2(980f, 220f));

            var answerColors = new[] { Peach, Hex("#8CC3F2"), FreshGreen };
            screen.answerButtons = new Button[3];
            screen.answerLabels = new TMP_Text[3];
            screen.answerIcons = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var button = Button("Answer" + (i + 1), c, null, White, TextDark, Bottom, new Vector2((i - 1) * 340f, 520f), new Vector2(310f, 400f));
                var strip = Panel("Color", button.transform, answerColors[i], GeneratedArt.Rounded);
                Place(strip.rectTransform, Top, new Vector2(0f, -130f), new Vector2(270f, 230f));
                strip.raycastTarget = false;
                screen.answerIcons[i] = Icon("Icon", button.transform, null, Top, new Vector2(0f, -130f), 240f);
                var label = Text("Label", button.transform, "", 54f, TextDark, FontStyles.Bold);
                Place(label.rectTransform, Bottom, new Vector2(0f, 70f), new Vector2(290f, 100f));
                screen.answerButtons[i] = button;
                screen.answerLabels[i] = label;
            }
            screen.animalIcons = icons;

            screen.feedbackText = Text("Feedback", c, "", 80f, GrassDark, FontStyles.Bold);
            Place(screen.feedbackText.rectTransform, Bottom, new Vector2(0f, 220f), new Vector2(980f, 140f));
            screen.feedbackText.gameObject.AddComponent<PopInEffect>();
            return screen;
        }

        // 6. Quiz Results Screen
        private static UIScreen BuildResults(Transform canvas)
        {
            var screen = NewScreen<ResultsScreen>(canvas, "6 Results", Cream, out var c);

            for (int i = 0; i < 8; i++)
            {
                // Soft confetti dots.
                var confetti = Panel("Confetti" + i, c, i % 2 == 0 ? SunYellow : Hex("#8CC3F2"), GeneratedArt.Circle);
                float x = -440f + i * 125f;
                float y = -200f - (i % 3) * 90f;
                Place(confetti.rectTransform, Top, new Vector2(x, y), new Vector2(40f + (i % 3) * 14f, 40f + (i % 3) * 14f));
                confetti.raycastTarget = false;
            }

            screen.titleText = Text("Title", c, "Kamu Hebat!", 116f, GrassDark, FontStyles.Bold);
            Place(screen.titleText.rectTransform, Top, new Vector2(0f, -400f), new Vector2(1000f, 170f));

            var mascot = Icon("Mascot", c, IconFor(AnimalId.Lion), Middle, new Vector2(0f, 200f), 480f);
            mascot.gameObject.AddComponent<PulseEffect>().amount = 0.04f;

            var stars = new List<Image>();
            for (int i = 0; i < 5; i++)
            {
                var star = Icon("Star" + (i + 1), c, GeneratedArt.Star, Middle, new Vector2((i - 1) * 250f, -170f), 220f);
                star.color = SunYellow;
                star.gameObject.AddComponent<PopInEffect>();
                stars.Add(star);
            }
            screen.stars = stars.ToArray();

            screen.messageText = Text("Message", c, "", 56f, TextDark, FontStyles.Bold);
            Place(screen.messageText.rectTransform, Middle, new Vector2(0f, -370f), new Vector2(980f, 120f));
            screen.totalStarsText = Text("TotalStars", c, "", 44f, BlueInk);
            Place(screen.totalStarsText.rectTransform, Middle, new Vector2(0f, -470f), new Vector2(980f, 80f));

            screen.playAgainButton = Button("PlayAgainButton", c, "Main Lagi", GrassDark, White, Bottom, new Vector2(0f, 330f), new Vector2(780f, 170f), 66f);
            screen.homeButton = SecondaryButton("HomeButton", c, "Kembali ke Beranda", BlueInk, Bottom, new Vector2(0f, 140f), new Vector2(780f, 140f), 52f);
            return screen;
        }

        // A simple speaker: a box, a cone and two waves made from shapes.
        private static void SoundIcon(Transform parent)
        {
            var body = Panel("Speaker", parent, TextDark, GeneratedArt.Rounded);
            Place(body.rectTransform, Middle, new Vector2(-25f, 0f), new Vector2(50f, 60f));
            body.raycastTarget = false;
            var wave1 = Panel("Wave1", parent, TextDark, GeneratedArt.Frame);
            Place(wave1.rectTransform, Middle, new Vector2(25f, 0f), new Vector2(50f, 70f));
            wave1.raycastTarget = false;
            var wave2 = Panel("Wave2", parent, TextDark, GeneratedArt.Frame);
            Place(wave2.rectTransform, Middle, new Vector2(40f, 0f), new Vector2(80f, 110f));
            wave2.raycastTarget = false;
        }

        // Adds AnimalImageTargetHandler to each Image Target, guessing the
        // animal from the target or object name (lion/singa, elephant/gajah,
        // rabbit/kelinci), and adds ARCameraToggle to the ARCamera.
        private static int WireVuforia(ARAnimalWorldManager app)
        {
            foreach (var behaviour in Object.FindObjectsOfType<VuforiaBehaviour>())
            {
                if (behaviour.GetComponent<ARCameraToggle>() == null)
                {
                    behaviour.gameObject.AddComponent<ARCameraToggle>();
                }
            }

            int wired = 0;
            foreach (var target in Object.FindObjectsOfType<ImageTargetBehaviour>(true))
            {
                var handler = target.GetComponent<AnimalImageTargetHandler>();
                if (handler == null)
                {
                    if (!TryGuessAnimal(target.TargetName + " " + target.name, out var id))
                    {
                        Debug.LogWarning($"Tidak tahu hewan untuk Image Target '{target.name}'. Beri nama yang mengandung singa/gajah/kelinci, atau pasang AnimalImageTargetHandler manual.", target);
                        continue;
                    }

                    handler = target.gameObject.AddComponent<AnimalImageTargetHandler>();
                    handler.animalId = id;
                }

                // The default handler would fight ours over showing children.
                var defaultHandler = target.GetComponent<DefaultObserverEventHandler>();
                if (defaultHandler != null)
                {
                    Object.DestroyImmediate(defaultHandler);
                }

                handler.appManager = app;
                EditorUtility.SetDirty(handler);
                wired++;
            }
            return wired;
        }

        private static bool TryGuessAnimal(string name, out AnimalId id)
        {
            string lower = name.ToLowerInvariant();
            if (lower.Contains("lion") || lower.Contains("singa"))
            {
                id = AnimalId.Lion;
                return true;
            }
            if (lower.Contains("elephant") || lower.Contains("gajah"))
            {
                id = AnimalId.Elephant;
                return true;
            }
            if (lower.Contains("rabbit") || lower.Contains("kelinci"))
            {
                id = AnimalId.Rabbit;
                return true;
            }
            id = AnimalId.Lion;
            return false;
        }
    }
}
