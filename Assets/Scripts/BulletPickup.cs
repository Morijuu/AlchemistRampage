using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BulletPickup : MonoBehaviour
{
    [SerializeField] private BulletType bulletType;

    [Header("UI References")]
    [SerializeField] private GameObject promptCanvas;
    [SerializeField] private TMP_Text recogerLabel;
    [SerializeField] private TMP_Text fusionarLabel;

    private BulletType pendingFusion = BulletType.None;
    private bool playerInRange = false;

    // Evita que dos pickups muestren el prompt a la vez
    private static BulletPickup currentActive;

    private void Start()
    {
        promptCanvas.SetActive(false);
    }

    public void Initialize(BulletType type)
    {
        bulletType = type;
    }

    private void Update()
    {
        if (!playerInRange) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            OnRecoger();

        if (pendingFusion != BulletType.None && Keyboard.current.fKey.wasPressedThisFrame)
            OnFusionar();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Ocultar el prompt del pickup anterior si había otro activo
        if (currentActive != null && currentActive != this)
            currentActive.HidePrompt();

        currentActive = this;

        BulletInventory inv = BulletInventory.Instance;
        BulletType activeType = inv.ActiveData != null ? inv.ActiveData.bulletType : BulletType.None;
        pendingFusion = BulletInventory.GetFusionResult(activeType, bulletType);

        BulletData pickupData = inv.GetDataForType(bulletType);
        recogerLabel.text = "[E] Pick Up " + (pickupData != null ? pickupData.displayName : bulletType.ToString());

        if (pendingFusion != BulletType.None)
        {
            BulletData fusedData = inv.GetDataForType(pendingFusion);
            fusionarLabel.text = "[F] Merge -> " + (fusedData != null ? fusedData.displayName : pendingFusion.ToString());
            fusionarLabel.gameObject.SetActive(true);
        }
        else
        {
            fusionarLabel.gameObject.SetActive(false);
        }

        promptCanvas.SetActive(true);
        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        HidePrompt();
        if (currentActive == this) currentActive = null;
    }

    public void HidePrompt()
    {
        promptCanvas.SetActive(false);
        playerInRange = false;
    }

    private void OnRecoger()
    {
        BulletInventory.Instance.SetBullet(bulletType);
        Destroy(gameObject);
    }

    private void OnFusionar()
    {
        BulletInventory.Instance.FuseBullet(pendingFusion);
        Destroy(gameObject);
    }
}
