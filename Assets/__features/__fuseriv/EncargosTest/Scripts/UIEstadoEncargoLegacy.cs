using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ====================
// UI ESTADO ENCARGO
// ====================
public class UIEstadoEncargoLegacy : MonoBehaviour
{
    // ====================
    // REFERENCIA
    // ====================
    [SerializeField] private Text textoEstado;

    // ====================
    // INICIO
    // ====================
    private void Start()
    {
        if (textoEstado != null)
        {
            textoEstado.enabled = false;
        }
    }

    // ====================
    // RECOLECTA
    // ====================
    public void MostrarRecolecta(float duracion)
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje("¡Salta sobre las morsas para recolectar los peces!", Color.white, duracion));
    }

    // ====================
    // COMPLETADO
    // ====================
    public void MostrarCompletado()
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje("ENCARGO COMPLETADO", Color.green, 2f));
    }

    // ====================
    // FALLADO
    // ====================
    public void MostrarFallado()
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje("ENCARGO FALLIDO", Color.red, 2f));
    }

    // ====================
    // MENSAJE
    // ====================
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