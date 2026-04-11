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
    // COMPLETADO
    // ====================
    public void MostrarCompletado()
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje("ENCARGO COMPLETADO", Color.green));
    }

    // ====================
    // FALLADO
    // ====================
    public void MostrarFallado()
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje("ENCARGO FALLIDO", Color.red));
    }

    // ====================
    // MENSAJE
    // ====================
    private IEnumerator MostrarMensaje(string mensaje, Color color)
    {
        if (textoEstado == null)
            yield break;

        textoEstado.text = mensaje;
        textoEstado.color = color;
        textoEstado.enabled = true;

        yield return new WaitForSeconds(2f);

        textoEstado.enabled = false;
    }
}