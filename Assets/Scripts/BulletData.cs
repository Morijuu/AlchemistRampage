using UnityEngine;

[CreateAssetMenu(menuName = "AlchemistRampage/BulletData")]
public class BulletData : ScriptableObject
{
    public BulletType bulletType;
    public string displayName;
    public int damage = 20;
    public float speed = 15f;
    public float fireRate = 3f;     // disparos por segundo
    public int shotsPerPickup = 5;

    [Header("Area Bullet")]
    public float splashRadius = 2f;
    public int splashDamage = 10;
    public float areaDuration = 4f;
    public float areaTickRate = 0.5f;      // daño cada X segundos
    public GameObject areaEffectPrefab;    // prefab de la zona visual

    [Header("Frag Bullet")]
    public int fragCount = 6;
    public float fragSpeed = 6f;
    public int fragDamage = 10;

    [Header("Target Bullet")]
    public float homingTurnSpeed = 180f;
    public float homingStopDistance = 2f;
    public float homingDelay = 0.4f;       // segundos antes de empezar a corregir

    [Header("Chain Bullet")]
    public int chainCount = 3;
    public float chainRange = 8f;
    public int chainDamage = 15;

    [Header("Piercing Bullet")]
    public int maxPierceCount = 3;
}
