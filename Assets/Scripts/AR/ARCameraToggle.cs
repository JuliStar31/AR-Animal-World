using UnityEngine;
using Vuforia;

namespace ARAnimalWorld
{
    // Put on the Vuforia ARCamera. Keeps the camera and tracking running
    // only while the Scan screen is open (saves battery, and the other
    // screens are fully drawn over the camera anyway). Also turns Vuforia
    // start-up errors into a gentle message on the Scan screen.
    public class ARCameraToggle : MonoBehaviour
    {
        public bool cameraOnlyOnScanScreen = true;

        private void Awake()
        {
            VuforiaApplication.Instance.OnVuforiaInitialized += HandleInitialized;
            ScanScreen.ScanningChanged += HandleScanningChanged;
        }

        private void Start()
        {
            if (cameraOnlyOnScanScreen)
            {
                SetCameraRunning(false);
            }
        }

        private void OnDestroy()
        {
            VuforiaApplication.Instance.OnVuforiaInitialized -= HandleInitialized;
            ScanScreen.ScanningChanged -= HandleScanningChanged;
        }

        private void HandleScanningChanged(bool scanning)
        {
            if (cameraOnlyOnScanScreen)
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
                ScanScreen.ReportCameraProblem(null);
                return;
            }

            Debug.LogWarning("Vuforia could not start: " + error);
            string code = error.ToString();
            if (code.Contains("LICENSE"))
            {
                ScanScreen.ReportCameraProblem("Kamera AR belum siap. Minta orang tua memeriksa kunci lisensi Vuforia, ya.");
            }
            else if (code.Contains("PERMISSION") || code.Contains("CAMERA"))
            {
                ScanScreen.ReportCameraProblem("Kamera belum diizinkan. Minta orang tua mengizinkan kamera di Pengaturan, ya.");
            }
            else
            {
                ScanScreen.ReportCameraProblem("Kamera AR belum bisa dibuka. Coba tutup dan buka lagi aplikasinya, ya.");
            }
        }
    }
}
