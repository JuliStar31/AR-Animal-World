using System.Collections.Generic;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

namespace ARAnimalWorld
{
    // Recognises a lion, an elephant or a rabbit in any picture the camera
    // sees (a photo, a book, a poster, a toy) and shows that animal in front
    // of the camera, with its sound and the Scan screen's info panel.
    // It runs MobileNetV2 trained on ImageNet, which already knows these
    // animals, so nothing needs training. It is best at photos and realistic
    // pictures; simple cartoons are often missed. A printed card in view
    // (AnimalImageTargetHandler) always wins.
    public class AnimalPictureRecognizer : MonoBehaviour
    {
        public GameFlow game;
        [Tooltip("mobilenetv2-12.onnx: ImageNet classes, 1x3x224x224 input.")]
        public ModelAsset model;

        [Header("Recognition")]
        [Tooltip("Seconds between two looks at the camera image.")]
        public float interval = 0.5f;
        [Tooltip("How sure the AI must be, from 0 to 1.")]
        [Range(0f, 1f)] public float threshold = 0.4f;
        [Tooltip("Looks in a row that must agree before the animal appears.")]
        public int looksToShow = 2;
        [Tooltip("Seconds without seeing the animal before it goes away.")]
        public float secondsToHide = 2f;

        [Header("Animal in front of the camera (metres)")]
        public float distance = 0.5f;
        public float size = 0.25f;
        public Vector3 euler = new Vector3(0f, 30f, 0f);

        [Tooltip("Optional: shows what the AI sees (development builds).")]
        public RawImage debugView;

        // ImageNet classes for each animal; their chances are added up.
        private static readonly Dictionary<AnimalId, int[]> Classes = new Dictionary<AnimalId, int[]>
        {
            { AnimalId.Lion, new[] { 291 } },               // lion
            { AnimalId.Elephant, new[] { 101, 385, 386 } }, // tusker, Indian and African elephant
            { AnimalId.Rabbit, new[] { 330, 331, 332 } }    // wood rabbit, hare, Angora
        };

        private const int Side = 224;
        private const int Plane = Side * Side;
        private static readonly float[] Mean = { 0.485f, 0.456f, 0.406f };
        private static readonly float[] Std = { 0.229f, 0.224f, 0.225f };

        private Worker worker;
        private Tensor<float> pendingInput;
        private Tensor<float> pendingOutput;
        private int pendingBatch;
        private float[] inputData = new float[0];

        private PixelFormat format = PixelFormat.UNKNOWN_FORMAT;
        private float nextLook;

        private AnimalId candidate;
        private int agreeingLooks;
        private bool showing;
        private AnimalId shownAnimal;
        private float lastSeen;
        private Transform anchor;
        private GameObject shownModel;
        private Texture2D debugTexture;

        private void Start()
        {
            if (model == null || game == null)
            {
                Debug.LogWarning("AnimalPictureRecognizer needs the MobileNetV2 model and GameFlow; picture recognition is off.");
                enabled = false;
                return;
            }

            var backend = SystemInfo.supportsComputeShaders ? BackendType.GPUCompute : BackendType.CPU;
            worker = new Worker(ModelLoader.Load(model), backend);

            VuforiaApplication.Instance.OnVuforiaStarted += RegisterFormat;
            VuforiaApplication.Instance.OnVuforiaPaused += HandlePaused;
            if (VuforiaApplication.Instance.IsRunning)
            {
                RegisterFormat();
            }

            ScanScreen.ScanningChanged += HandleScanningChanged;
            game.appManager.OnAnimalDetected += HandleAnimalDetected;
        }

        private void OnDestroy()
        {
            VuforiaApplication.Instance.OnVuforiaStarted -= RegisterFormat;
            VuforiaApplication.Instance.OnVuforiaPaused -= HandlePaused;
            ScanScreen.ScanningChanged -= HandleScanningChanged;
            if (game != null && game.appManager != null)
            {
                game.appManager.OnAnimalDetected -= HandleAnimalDetected;
            }

            DisposePending();
            worker?.Dispose();
            if (debugTexture != null)
            {
                Destroy(debugTexture);
            }
        }

        // Ask Vuforia to keep a copy of each camera frame. Webcams in the
        // Editor only offer grayscale; phones give colour.
        private void RegisterFormat()
        {
            var camera = VuforiaBehaviour.Instance != null ? VuforiaBehaviour.Instance.CameraDevice : null;
            if (camera == null)
            {
                return;
            }

            var wanted = Application.isEditor ? PixelFormat.GRAYSCALE : PixelFormat.RGB888;
            if (camera.SetFrameFormat(wanted, true))
            {
                format = wanted;
            }
            else if (wanted != PixelFormat.GRAYSCALE && camera.SetFrameFormat(PixelFormat.GRAYSCALE, true))
            {
                format = PixelFormat.GRAYSCALE;
            }
        }

        // The frame format has to be asked for again after the app resumes.
        private void HandlePaused(bool paused)
        {
            if (!paused)
            {
                RegisterFormat();
            }
        }

        private void Update()
        {
            if (showing && Time.unscaledTime - lastSeen > secondsToHide)
            {
                Hide(true);
            }

            if (pendingOutput != null)
            {
                if (pendingOutput.IsReadbackRequestDone())
                {
                    var scores = pendingOutput.DownloadToArray();
                    int images = pendingBatch;
                    DisposePending();
                    HandleScores(scores, images);
                }
                return;
            }

            if (!ScanScreen.IsScanning || format == PixelFormat.UNKNOWN_FORMAT || Time.unscaledTime < nextLook)
            {
                return;
            }
            nextLook = Time.unscaledTime + interval;

            // A card in view wins; no need to look for pictures.
            if (game.appManager.IsAnimalInView && !game.appManager.IsFromPicture)
            {
                return;
            }

            var image = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(format);
            // Vuforia 11's Image has no IsValid(); FillInput checks the pixels.
            if (image == null)
            {
                return;
            }

            int batch = FillInput(image);
            if (batch == 0)
            {
                return;
            }

            pendingBatch = batch;
            pendingInput = new Tensor<float>(new TensorShape(batch, 3, Side, Side), inputData);
            worker.Schedule(pendingInput);
            pendingOutput = worker.PeekOutput() as Tensor<float>;
            if (pendingOutput == null)
            {
                DisposePending();
                return;
            }
            pendingOutput.ReadbackRequest();
        }

        // The centre square of the camera frame, shrunk to 224 x 224 and
        // normalised the way MobileNetV2 was trained. Phone cameras deliver
        // landscape frames that the screen shows turned a quarter, and the
        // direction differs between phones, so in portrait both quarter
        // turns go in as a batch of two (see HandleScores).
        private int FillInput(Vuforia.Image image)
        {
            int width = image.Width, height = image.Height, stride = image.Stride;
            var pixels = image.Pixels;
            int channels = image.PixelFormat == PixelFormat.RGB888 ? 3 : 1;
            if (pixels == null || width <= 0 || height <= 0 || stride < width * channels || pixels.Length < stride * height)
            {
                return 0;
            }

            bool turned = !Application.isEditor && width > height && Screen.height > Screen.width;
            int batch = turned ? 2 : 1;
            if (inputData.Length != batch * 3 * Plane)
            {
                inputData = new float[batch * 3 * Plane];
            }

            int crop = Mathf.Min(width, height);
            int left = (width - crop) / 2, top = (height - crop) / 2;
            for (int b = 0; b < batch; b++)
            {
                int start = b * 3 * Plane;
                for (int y = 0; y < Side; y++)
                {
                    int v = y * crop / Side;
                    for (int x = 0; x < Side; x++)
                    {
                        int u = x * crop / Side;
                        int sx, sy;
                        if (!turned)
                        {
                            sx = u;
                            sy = v;
                        }
                        else if (b == 0)
                        {
                            // A quarter turn clockwise.
                            sx = v;
                            sy = crop - 1 - u;
                        }
                        else
                        {
                            // A quarter turn anticlockwise.
                            sx = crop - 1 - v;
                            sy = u;
                        }

                        int i = (top + sy) * stride + (left + sx) * channels;
                        float r = pixels[i] / 255f;
                        float g = channels == 3 ? pixels[i + 1] / 255f : r;
                        float bl = channels == 3 ? pixels[i + 2] / 255f : r;
                        int o = start + y * Side + x;
                        inputData[o] = (r - Mean[0]) / Std[0];
                        inputData[o + Plane] = (g - Mean[1]) / Std[1];
                        inputData[o + 2 * Plane] = (bl - Mean[2]) / Std[2];
                    }
                }
            }

            UpdateDebugView();
            return batch;
        }

        private void HandleScores(float[] scores, int batch)
        {
            // With two quarter turns, only the one the AI is surest about
            // counts: an upside-down cat can look like a lion.
            int classes = scores.Length / batch;
            int start = 0;
            float bestTop = -1f;
            for (int b = 0; b < batch; b++)
            {
                float top = TopChance(scores, b * classes, classes);
                if (top > bestTop)
                {
                    bestTop = top;
                    start = b * classes;
                }
            }

            // Softmax over that image's class scores.
            float max = float.MinValue;
            for (int c = 0; c < classes; c++)
            {
                max = Mathf.Max(max, scores[start + c]);
            }
            float sum = 0f;
            for (int c = 0; c < classes; c++)
            {
                sum += Mathf.Exp(scores[start + c] - max);
            }

            bool found = false;
            AnimalId best = AnimalId.Lion;
            float bestChance = 0f;
            foreach (var pair in Classes)
            {
                float chance = 0f;
                foreach (int c in pair.Value)
                {
                    chance += Mathf.Exp(scores[start + c] - max) / sum;
                }
                if (chance > bestChance)
                {
                    bestChance = chance;
                    best = pair.Key;
                    found = true;
                }
            }

            if (found && bestChance >= threshold)
            {
                agreeingLooks = candidate == best ? agreeingLooks + 1 : 1;
                candidate = best;

                if (showing && shownAnimal == best)
                {
                    lastSeen = Time.unscaledTime;
                }
                else if (agreeingLooks >= looksToShow)
                {
                    Show(best);
                }
            }
            else
            {
                agreeingLooks = 0;
            }
        }

        // Chance of the most likely class, from raw scores.
        private static float TopChance(float[] scores, int start, int count)
        {
            float max = float.MinValue;
            for (int c = 0; c < count; c++)
            {
                max = Mathf.Max(max, scores[start + c]);
            }
            float sum = 0f;
            for (int c = 0; c < count; c++)
            {
                sum += Mathf.Exp(scores[start + c] - max);
            }
            return 1f / sum;
        }

        private void Show(AnimalId id)
        {
            if (game.appManager.IsAnimalInView && !game.appManager.IsFromPicture)
            {
                return;
            }

            Hide(showing && shownAnimal != id);
            var animal = game.appManager.GetAnimalById(id);
            if (animal == null)
            {
                return;
            }

            // Hangs in front of the camera and moves with it; the model
            // faces -Z, so with no turn it looks straight at the child.
            if (anchor == null)
            {
                var cam = VuforiaBehaviour.Instance != null ? VuforiaBehaviour.Instance.transform : Camera.main.transform;
                anchor = new GameObject("Picture Animal").transform;
                anchor.SetParent(cam, false);
            }
            anchor.localPosition = new Vector3(0f, -0.35f * size, distance);
            anchor.localRotation = Quaternion.identity;

            shownModel = AnimalModelSpawner.Spawn(animal, anchor, size, euler, Vector3.zero);
            showing = true;
            shownAnimal = id;
            lastSeen = Time.unscaledTime;

            game.appManager.HandleAnimalDetected(id, true);
            game.gameAudio.PlayAnimal(animal);
        }

        private void Hide(bool tellManager)
        {
            if (shownModel != null)
            {
                Destroy(shownModel);
                shownModel = null;
            }

            if (showing && tellManager && game.appManager.IsFromPicture)
            {
                game.appManager.HandleAnimalLost(shownAnimal);
            }
            showing = false;
        }

        private void HandleScanningChanged(bool scanning)
        {
            if (!scanning)
            {
                Hide(true);
                agreeingLooks = 0;
            }
        }

        // A printed card was found: its animal takes over.
        private void HandleAnimalDetected(AnimalData animal)
        {
            if (showing && !game.appManager.IsFromPicture)
            {
                Hide(false);
            }
        }

        private void DisposePending()
        {
            pendingInput?.Dispose();
            pendingInput = null;
            pendingOutput = null;
        }

        private void UpdateDebugView()
        {
            if (debugView == null || !debugView.isActiveAndEnabled)
            {
                return;
            }

            if (debugTexture == null)
            {
                debugTexture = new Texture2D(Side, Side, TextureFormat.RGB24, false);
                debugView.texture = debugTexture;
            }

            // First image of the batch; texture rows go bottom to top.
            var colors = new Color32[Plane];
            for (int y = 0; y < Side; y++)
            {
                for (int x = 0; x < Side; x++)
                {
                    int o = y * Side + x;
                    colors[(Side - 1 - y) * Side + x] = new Color32(
                        ToByte(inputData[o], 0), ToByte(inputData[o + Plane], 1), ToByte(inputData[o + 2 * Plane], 2), 255);
                }
            }
            debugTexture.SetPixels32(colors);
            debugTexture.Apply();
        }

        private static byte ToByte(float value, int channel)
        {
            return (byte)Mathf.Clamp(Mathf.RoundToInt((value * Std[channel] + Mean[channel]) * 255f), 0, 255);
        }
    }
}
