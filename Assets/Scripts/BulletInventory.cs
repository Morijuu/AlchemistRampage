using UnityEngine;
using System.Collections.Generic;

public class BulletInventory : MonoBehaviour
{
    public static BulletInventory Instance { get; private set; }

    [SerializeField] private BulletData[] allBulletData;
    [SerializeField] public GameObject bulletPrefab;

    [SerializeField] private BulletData[] slotData = new BulletData[3];
    [SerializeField] private int[] slotCounts = new int[3];
    [SerializeField] private bool[] isMixedSlot = new bool[3];
    [SerializeField] private int activeSlot = 0;

    public BulletData ActiveData => slotData[activeSlot];
    public int ShotCount => slotCounts[activeSlot];
    public int ActiveSlot => activeSlot;

    public BulletData[] SlotData => slotData;
    public int[] SlotCounts => slotCounts;

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

        int sameTypeSlot = -1;
        for (int i = 0; i < 3; i++)
        {
            if (slotData[i] != null && slotData[i].bulletType == type && !isMixedSlot[i])
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
            int emptySlot = System.Array.IndexOf(slotData, null);
            if (emptySlot != -1)
            {
                slotData[emptySlot] = data;
                slotCounts[emptySlot] = data.shotsPerPickup;
                isMixedSlot[emptySlot] = false;
            }
            else
            {
                slotData[activeSlot] = data;
                slotCounts[activeSlot] = data.shotsPerPickup;
                isMixedSlot[activeSlot] = false;
            }
        }
    }

    public void FuseBullet(BulletType fusedType, BulletType incomingType)
    {
        BulletData fusedData = GetDataForType(fusedType);
        BulletData incomingData = GetDataForType(incomingType);

        if (fusedData == null || incomingData == null) return;

        slotCounts[activeSlot] += incomingData.shotsPerPickup;
        slotData[activeSlot] = fusedData;
        isMixedSlot[activeSlot] = true;
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 3 && slotData[slotIndex] != null && slotCounts[slotIndex] > 0)
        {
            activeSlot = slotIndex;
        }
    }

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
        isMixedSlot[slotA] = true;
        slotData[slotB] = null;
        slotCounts[slotB] = 0;
        isMixedSlot[slotB] = false;
    }

    public bool TryConsumeBullet()
    {
        if (slotData[activeSlot] == null || slotData[activeSlot].bulletType == BulletType.None || slotCounts[activeSlot] <= 0)
            return false;
        slotCounts[activeSlot]--;

        if (slotCounts[activeSlot] <= 0)
        {
            slotData[activeSlot] = null;
            isMixedSlot[activeSlot] = false;
        }

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