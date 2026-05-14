using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public GameObject canvasInicio;
    public GameObject canvasMenuPrincipal;
    public GameObject canvasOpciones;

    private void Start()
    {
        canvasInicio.SetActive(true);
        canvasMenuPrincipal.SetActive(false);
        canvasOpciones.SetActive(false);
    }

    public void MostrarMenuPrincipal()
    {
        canvasInicio.SetActive(false);
        canvasMenuPrincipal.SetActive(true);
        canvasOpciones.SetActive(false);

    }

    public void Opciones()
    {
        canvasInicio.SetActive(false);
        canvasMenuPrincipal.SetActive(false);
        canvasOpciones.SetActive(true);

    }
}