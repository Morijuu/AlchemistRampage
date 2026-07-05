using UnityEngine;
using System.Collections.Generic;

public class BulletInventory : MonoBehaviour
{
    public static BulletInventory Instance { get; private set; }

    [SerializeField] private BulletData[] allBulletData;
    public GameObject bulletPrefab;

    [SerializeField] private BulletData[] slotData = new BulletData[3];
    [SerializeField] private int[] slotCounts = new int[3];
    [SerializeField] private int activeSlot = 0;

    public BulletData ActiveData
    {
        get
        {
            if (activeSlot >= 0 && activeSlot < slotData.Length)
                return slotData[activeSlot];
            return null;
        }
    }

    public int ShotCount
    {
        get
        {
            if (activeSlot >= 0 && activeSlot < slotCounts.Length)
                return slotCounts[activeSlot];
            return 0;
        }
    }

    public int ActiveSlot => activeSlot;
    public BulletData[] SlotData => slotData;
    public int[] SlotCounts => slotCounts;

    private void Awake()
    {
        Instance = this;
    }

    // Obtener datos de una bala por tipo
    public BulletData GetDataForType(BulletType type)
    {
        if (allBulletData == null) return null;

        foreach (BulletData data in allBulletData)
        {
            if (data != null && data.bulletType == type) return data;
        }
        return null;
    }

    // Consumir una bala
    public bool TryConsumeBullet()
    {
        if (slotData[activeSlot] == null || slotCounts[activeSlot] <= 0)
            return false;

        slotCounts[activeSlot]--;

        if (slotCounts[activeSlot] <= 0)
        {
            slotData[activeSlot] = null;
        }

        return true;
    }

    // Establecer una bala en el slot activo
    public void SetBullet(BulletType type)
    {
        BulletData data = GetDataForType(type);
        if (data == null) return;

        // Buscar si ya hay balas de este tipo
        int sameTypeSlot = -1;
        for (int i = 0; i < 3; i++)
        {
            if (slotData[i] != null && slotData[i].bulletType == type)
            {
                sameTypeSlot = i;
                break;
            }
        }

        if (sameTypeSlot != -1)
        {
            slotCounts[sameTypeSlot] += data.shotsPerPickup;
        }
        else
        {
            // Buscar slot vacío
            int emptySlot = -1;
            for (int i = 0; i < 3; i++)
            {
                if (slotData[i] == null)
                {
                    emptySlot = i;
                    break;
                }
            }

            if (emptySlot != -1)
            {
                slotData[emptySlot] = data;
                slotCounts[emptySlot] = data.shotsPerPickup;
            }
            else
            {
                // Reemplazar slot activo si no hay vacíos
                slotData[activeSlot] = data;
                slotCounts[activeSlot] = data.shotsPerPickup;
            }
        }
    }

    // Mezclar balas
    public void FuseBullet(BulletType fusedType, BulletType incomingType)
    {
        BulletData fusedData = GetDataForType(fusedType);
        BulletData incomingData = GetDataForType(incomingType);

        if (fusedData == null || incomingData == null) return;

        slotCounts[activeSlot] += incomingData.shotsPerPickup;
        slotData[activeSlot] = fusedData;
    }

    // Seleccionar slot
    public void SelectSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 3 && slotData[slotIndex] != null && slotCounts[slotIndex] > 0)
        {
            activeSlot = slotIndex;
        }
    }

    // Mezclar dos slots
    public void MixSlots(int slotA, int slotB)
    {
        if (slotData[slotA] == null || slotData[slotB] == null) return;

        BulletType resultType = GetFusionResult(slotData[slotA].bulletType, slotData[slotB].bulletType);
        if (resultType == BulletType.None) return;

        BulletData resultData = GetDataForType(resultType);
        if (resultData == null) return;

        int totalAmmo = slotCounts[slotA] + slotCounts[slotB];
        slotData[slotA] = resultData;
        slotCounts[slotA] = totalAmmo;
        slotData[slotB] = null;
        slotCounts[slotB] = 0;
    }

    // Tabla de fusión
    public static BulletType GetFusionResult(BulletType a, BulletType b)
    {
        if ((a == BulletType.Regular && b == BulletType.Heavy) ||
            (a == BulletType.Heavy && b == BulletType.Regular))
            return BulletType.Area;

        if ((a == BulletType.Heavy && b == BulletType.Bouncy) ||
            (a == BulletType.Bouncy && b == BulletType.Heavy))
            return BulletType.Frag;

        if ((a == BulletType.Regular && b == BulletType.Bouncy) ||
            (a == BulletType.Bouncy && b == BulletType.Regular))
            return BulletType.Target;

        if ((a == BulletType.Area && b == BulletType.Bouncy) ||
            (a == BulletType.Bouncy && b == BulletType.Area))
            return BulletType.Chain;

        if ((a == BulletType.Target && b == BulletType.Heavy) ||
            (a == BulletType.Heavy && b == BulletType.Target))
            return BulletType.Piercing;

        return BulletType.None;
    }

    // Obtener todas las recetas
    public static List<(BulletType a, BulletType b, BulletType result)> GetAllRecipes()
    {
        return new List<(BulletType, BulletType, BulletType)>
        {
            (BulletType.Regular, BulletType.Heavy, BulletType.Area),
            (BulletType.Heavy, BulletType.Bouncy, BulletType.Frag),
            (BulletType.Regular, BulletType.Bouncy, BulletType.Target),
            (BulletType.Area, BulletType.Bouncy, BulletType.Chain),
            (BulletType.Target, BulletType.Heavy, BulletType.Piercing)
        };
    }
}