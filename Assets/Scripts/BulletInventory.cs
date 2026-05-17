using UnityEngine;

public class BulletInventory : MonoBehaviour
{
    public static BulletInventory Instance { get; private set; }

    [SerializeField] private BulletData[] allBulletData;
    [SerializeField] public GameObject bulletPrefab;

    [SerializeField] private BulletData activeData;
    [SerializeField] private int shotCount;

    public BulletData ActiveData => activeData;
    public int ShotCount => shotCount;

    private void Awake()
    {
        Instance = this;
    }

    public BulletData GetDataForType(BulletType type)
    {
        foreach (BulletData data in allBulletData)
        {
            if (data.bulletType == type) return data;
        }
        return null;
    }

    public void SetBullet(BulletType type)
    {
        BulletData data = GetDataForType(type);
        if (data == null) return;

        // NUEVO: Si recoges una bala del mismo tipo que ya tienes, sumamos la munición
        if (activeData != null && activeData.bulletType == type)
        {
            shotCount += data.shotsPerPickup;
        }
        else
        {
            // Si recoges un tipo distinto (y pulsas E en vez de F para fusionar), se reemplaza
            activeData = data;
            shotCount = data.shotsPerPickup;
        }
    }

    // NUEVO: Ahora pedimos el tipo de bala que recoges (incomingType) para saber cuántas sumar
    public void FuseBullet(BulletType fusedType, BulletType incomingType)
    {
        BulletData fusedData = GetDataForType(fusedType);
        BulletData incomingData = GetDataForType(incomingType);

        if (fusedData == null || incomingData == null) return;

        // Sumamos: Tus balas actuales + las balas que te da el pickup del suelo
        shotCount += incomingData.shotsPerPickup;
        
        // Cambiamos tu tipo de bala al nuevo tipo fusionado
        activeData = fusedData;
    }

    public bool TryConsumeBullet()
    {
        if (activeData == null || activeData.bulletType == BulletType.None || shotCount <= 0)
            return false;
        shotCount--;
        return true;
    }

    public bool CanFuse(BulletType incoming)
    {
        if (ActiveData == null) return false;
        return GetFusionResult(ActiveData.bulletType, incoming) != BulletType.None;
    }

    public static BulletType GetFusionResult(BulletType a, BulletType b)
    {
        if ((a == BulletType.Regular && b == BulletType.Heavy) ||
            (a == BulletType.Heavy   && b == BulletType.Regular))
            return BulletType.Area;

        if ((a == BulletType.Heavy  && b == BulletType.Bouncy) ||
            (a == BulletType.Bouncy && b == BulletType.Heavy))
            return BulletType.Frag;

        if ((a == BulletType.Regular && b == BulletType.Bouncy) ||
            (a == BulletType.Bouncy  && b == BulletType.Regular))
            return BulletType.Target;

        if ((a == BulletType.Area   && b == BulletType.Bouncy) ||
            (a == BulletType.Bouncy && b == BulletType.Area))
            return BulletType.Chain;

        if ((a == BulletType.Target && b == BulletType.Heavy) ||
            (a == BulletType.Heavy  && b == BulletType.Target))
            return BulletType.Piercing;

        return BulletType.None;
    }
}