using UnityEngine;

public class GameStats : MonoBehaviour
{
    public static GameStats Instance { get; private set; }

    public float damageDealt = 0f;
    public float damageTaken = 0f;
    public float damageHealed = 0f;
    public int zombiesKilled = 0;
    private float startTime = 0f;
    private float endTime = 0f;

    private void Awake()
    {
        Instance = this;
        startTime = Time.time;
    }

    public void AddDamageDealt(float damage)
    {
        damageDealt += damage;
    }

    public void AddDamageTaken(float damage)
    {
        damageTaken += damage;
    }

    public void AddDamageHealed(float damage)
    {
        damageHealed += damage;
    }

    public void AddZombieKilled()
    {
        zombiesKilled++;
    }

    public void EndGame()
    {
        endTime = Time.time;
    }

    public float GetGameDuration()
    {
        if (endTime == 0f)
            return Time.time - startTime;
        return endTime - startTime;
    }

    public string GetFormattedTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        int secs = (int)(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }
}
