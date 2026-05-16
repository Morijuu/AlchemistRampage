using UnityEngine;
using System.Collections;

public class CanvasTexto : MonoBehaviour
{
    public GameObject canvasTexto;
    public float tiempo = 10f;

    void Start()
    {
        StartCoroutine(DesactivarCanvas());
    }

    IEnumerator DesactivarCanvas()
    {
        yield return new WaitForSeconds(tiempo);

        canvasTexto.SetActive(false);
    }
}