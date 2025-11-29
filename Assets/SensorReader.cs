using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SensorReader : MonoBehaviour
{
    public TMP_Text txtAccel, txtGyro, txtGPS, txtBattery, txtCam;
    public RawImage cameraPreview;

    WebCamTexture camTex;
    bool gyroEnabled;
    Gyroscope gyro;

    void Start()
    {
        Application.targetFrameRate = 60;

        // Gyroscope
        Input.gyro.enabled = true;
        gyro = Input.gyro;
        gyroEnabled = SystemInfo.supportsGyroscope;

        // Caméra
        if (WebCamTexture.devices.Length > 0)
        {
            camTex = new WebCamTexture();
            cameraPreview.texture = camTex;
            camTex.Play();
        }

        // GPS
        StartCoroutine(StartLocationService());
    }

    IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            txtGPS.text = "GPS: désactivé";
            yield break;
        }

        // Précision ~1m, maj tous les 0.5m
        Input.location.Start(1f, 0.5f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait-- > 0)
            yield return new WaitForSeconds(1);

        if (Input.location.status != LocationServiceStatus.Running)
        {
            txtGPS.text = "GPS: indisponible";
            yield break;
        }
    }

    void Update()
    {
        // Accélération
        Vector3 a = Input.acceleration;
        txtAccel.text = $"Acc: {a.x:F2}, {a.y:F2}, {a.z:F2}";

        // Gyro
        if (gyroEnabled)
        {
            var g = gyro.rotationRateUnbiased;
            txtGyro.text = $"Gyro: {g.x:F2}, {g.y:F2}, {g.z:F2}";
        }
        else txtGyro.text = "Gyro: non supporté";

        // GPS
        if (Input.location.status == LocationServiceStatus.Running)
        {
            var l = Input.location.lastData;
            txtGPS.text = $"GPS: {l.latitude:F6}, {l.longitude:F6} | v={l.horizontalAccuracy:F1}m";
        }

        // Batterie
        float bl = SystemInfo.batteryLevel;
        var bs = SystemInfo.batteryStatus;
        txtBattery.text = $"Batt: {(bl < 0 ? "?" : (bl * 100f).ToString("F0") + "%")} ({bs})";

        // Cam info
        if (camTex != null)
            txtCam.text = $"Caméra: {camTex.width}x{camTex.height}";
    }

    void OnDestroy()
    {
        if (camTex != null) camTex.Stop();
        Input.location.Stop();
    }
}
