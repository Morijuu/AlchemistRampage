using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraFlash : MonoBehaviour
{
    public static CameraFlash Instance { get; private set; }

    private Volume    volume;
    private Vignette  vignette;

    private float flashAlpha  = 0f;
    private float pulseAlpha  = 0f;
    private float pulseTimer  = 0f;
    private bool  isLowHealth = false;

    private const float FLASH_START  = 0.58f;
    private const float FLASH_DECAY  = 1.8f;   // alpha units per second
    private const float LOW_MAX      = 0.30f;
    private const float LOW_MIN      = 0.06f;
    private const float PULSE_SPEED  = 1.5f;
    private const int   LOW_THRESHOLD = 30;

    private void Awake()
    {
        Instance = this;

        if (Camera.main != null)
        {
            var cam = Camera.main.GetUniversalAdditionalCameraData();
            if (cam != null) cam.renderPostProcessing = true;
        }

        GameObject volGO = new GameObject("CameraFlashVolume");
        volume = volGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 50f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.profile = profile;

        vignette = profile.Add<Vignette>();
        vignette.active = true;
        vignette.color.Override(new Color(0.75f, 0f, 0f, 1f));
        vignette.intensity.Override(0f);
        vignette.smoothness.Override(0.35f);
        vignette.rounded.Override(true);
    }

    private void OnDestroy()
    {
        if (volume != null) Destroy(volume.gameObject);
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (vignette == null) return;

        // Flash decays quickly back to zero (or to the low-health floor)
        flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, FLASH_DECAY * Time.deltaTime);

        // Low-health pulses slowly
        if (isLowHealth)
        {
            pulseTimer += Time.deltaTime * PULSE_SPEED;
            pulseAlpha  = Mathf.Lerp(LOW_MIN, LOW_MAX, (Mathf.Sin(pulseTimer) + 1f) * 0.5f);
        }
        else
        {
            pulseAlpha = Mathf.MoveTowards(pulseAlpha, 0f, Time.deltaTime * 2f);
        }

        vignette.intensity.Override(Mathf.Max(flashAlpha, pulseAlpha));
    }

    public void TriggerFlash()
    {
        flashAlpha = FLASH_START;
    }

    public void SetLowHealth(bool low)
    {
        isLowHealth = low;
        if (!low) pulseTimer = 0f;
    }
}
