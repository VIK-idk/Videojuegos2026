using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ====================
// UI ESTADO ENCARGO
// ====================
public class UIEstadoEncargoLegacy : MonoBehaviour
{
    [SerializeField] private Text textoEstado;

    private void Start()
    {
        if (textoEstado != null)
        {
            textoEstado.enabled = false;
        }
    }

    public void MostrarRecolecta(float duracion)
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje("¡Recolecta!", Color.white, duracion));
    }

    public void MostrarCompletado()
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje("ENCARGO COMPLETADO", Color.green, 2f));
    }

    public void MostrarFallado()
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje("ENCARGO FALLIDO", Color.red, 2f));
    }

    public void MostrarMensajePersonalizado(string mensaje, Color color, float duracion)
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje(mensaje, color, duracion));
    }

    private IEnumerator MostrarMensaje(string mensaje, Color color, float duracion)
    {
        if (textoEstado == null)
            yield break;

        textoEstado.text = mensaje;
        textoEstado.color = color;
        textoEstado.enabled = true;

        yield return new WaitForSeconds(duracion);

        textoEstado.enabled = false;
    }
}