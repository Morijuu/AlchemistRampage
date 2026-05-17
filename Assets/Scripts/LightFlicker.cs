using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    private Light2D myLight;

    [Header("Intensidad")]
    public float normalIntensity = 1.0f;
    public float lowIntensity = 0.3f; // Intensidad de cuando está intentando encenderse

    [Header("Tiempos de la secuencia")]
    public float minTimeOn = 3f;  // Tiempo mínimo que aguanta encendida
    public float maxTimeOn = 6f;  // Tiempo máximo que aguanta encendida
    public float timeOff = 2f;    // Tiempo que se queda totalmente apagada

    void Start()
    {
        myLight = GetComponent<Light2D>();
        
        // Iniciamos la rutina cíclica
        if (myLight != null)
        {
            StartCoroutine(BrokenLightRoutine());
        }
    }

    IEnumerator BrokenLightRoutine()
    {
        // Este bucle infinito hace que la secuencia se repita para siempre
        while (true) 
        {
            // --- FASE 1: LUZ NORMAL ---
            myLight.intensity = normalIntensity;
            // Espera encendida un tiempo aleatorio (ej: entre 3 y 6 segundos)
            yield return new WaitForSeconds(Random.Range(minTimeOn, maxTimeOn));


            // --- FASE 2: SE APAGA ---
            myLight.intensity = 0f;
            // Se queda a oscuras (ej: 2 segundos, con una pequeñísima variación para que no sea robótico)
            yield return new WaitForSeconds(Random.Range(timeOff - 0.2f, timeOff + 0.2f));


            // --- FASE 3: LOS PARPADEOS ROTOS ---
            // Elegimos entre 2 y 4 parpadeos
            int blinkCount = Random.Range(2, 5); 
            
            for (int i = 0; i < blinkCount; i++)
            {
                // Da un chispazo de luz baja
                myLight.intensity = lowIntensity;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                
                // Se vuelve a apagar
                myLight.intensity = 0f;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }

            // Una pausa muy cortita (medio segundo) antes de volver a la normalidad de golpe
            yield return new WaitForSeconds(0.5f);
        }
    }
}