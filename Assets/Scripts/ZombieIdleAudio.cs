using UnityEngine;

public class ZombieIdleAudio :
    MonoBehaviour
{
    private float timer;

    void Start()
    {
        timer =
            Random.Range(
                2f,
                8f
            );
    }

    void Update()
    {
        timer -=
            Time.deltaTime;

        if (
            timer <= 0
        )
        {
            if (
                AudioManager
                .Instance
                != null
            )
            {
                AudioManager
                .Instance
                .PlayZombieIdle();
            }

            timer =
                Random.Range(
                    5f,
                    12f
                );
        }
    }
}