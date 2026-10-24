using UnityEngine;
using Vuforia;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace ARAnimalWorld
{
    // Put on the Vuforia ARCamera. Connects Vuforia to the Scan screen:
    // turns start-up problems (no camera permission, licence) into the
    // friendly "Kameranya belum menyala" panel, and handles its
    // "Izinkan Kamera" button.
    public class ARCameraToggle : MonoBehaviour
    {
        [Tooltip("Pause the AR camera outside the Scan screen to save battery. Off by default: pausing before Vuforia has started can stop it from starting.")]
        public bool cameraOnlyOnScanScreen;

        private bool vuforiaStarted;

        private void Awake()
        {
            VuforiaApplication.Instance.OnVuforiaInitialized += HandleInitialized;
            VuforiaApplication.Instance.OnVuforiaStarted += HandleStarted;
            ScanScreen.ScanningChanged += HandleScanningChanged;
            ScanScreen.CameraPermissionRequested += RequestCameraPermission;
        }

        private void OnDestroy()
        {
            VuforiaApplication.Instance.OnVuforiaInitialized -= HandleInitialized;
            VuforiaApplication.Instance.OnVuforiaStarted -= HandleStarted;
            ScanScreen.ScanningChanged -= HandleScanningChanged;
            ScanScreen.CameraPermissionRequested -= RequestCameraPermission;
        }

        private void HandleStarted()
        {
            vuforiaStarted = true;
            if (cameraOnlyOnScanScreen)
            {
                SetCameraRunning(ScanScreen.IsScanning);
            }
        }

        private void HandleScanningChanged(bool scanning)
        {
            if (scanning && !HasCameraPermission())
            {
                ScanScreen.ReportCameraState(CameraState.NeedsPermission);
            }

            if (cameraOnlyOnScanScreen && vuforiaStarted)
            {
                SetCameraRunning(scanning);
            }
        }

        private static void SetCameraRunning(bool running)
        {
            if (VuforiaBehaviour.Instance != null)
            {
                VuforiaBehaviour.Instance.enabled = running;
            }
        }

        private static void HandleInitialized(VuforiaInitError error)
        {
            if (error == VuforiaInitError.NONE)
            {
                ScanScreen.ReportCameraState(CameraState.Ok);
                return;
            }

            Debug.LogWarning("Vuforia could not start: " + error);
            string code = error.ToString();
            if (code.Contains("PERMISSION") || code.Contains("CAMERA") || !HasCameraPermission())
            {
                ScanScreen.ReportCameraState(CameraState.NeedsPermission);
            }
            else if (code.Contains("LICENSE"))
            {
                ScanScreen.ReportCameraState(CameraState.Error, "Kamera AR belum siap. Minta orang tua memeriksa kunci lisensi Vuforia, ya.");
            }
            else
            {
                ScanScreen.ReportCameraState(CameraState.Error, "Kamera AR belum bisa dibuka. Coba tutup dan buka lagi aplikasinya, ya.");
            }
        }

        private static bool HasCameraPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(Permission.Camera);
#else
            return true;
#endif
        }

        private void RequestCameraPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (HasCameraPermission())
            {
                StartVuforia();
                return;
            }

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => StartVuforia();
            callbacks.PermissionDenied += _ => ScanScreen.ReportCameraState(CameraState.NeedsPermission);
            callbacks.PermissionDeniedAndDontAskAgain += _ => ScanScreen.ReportCameraState(CameraState.Error,
                "Izin kamera ditolak. Minta orang tua membuka Pengaturan > Aplikasi > AR Animal World > Izin, lalu nyalakan Kamera.");
            Permission.RequestUserPermission(Permission.Camera, callbacks);
#else
            StartVuforia();
#endif
        }

        private static void StartVuforia()
        {
            ScanScreen.ReportCameraState(CameraState.Ok);
            if (!VuforiaApplication.Instance.IsInitialized)
            {
                VuforiaApplication.Instance.Initialize();
            }
        }
    }
}
