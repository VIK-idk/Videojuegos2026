using UnityEngine;
using TMPro;
using System.Collections;

public class UiEstadoEncargo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoEstado;

    public void MostrarCompletado()
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje("ENCARGO COMPLETADO", Color.green));
    }

    public void MostrarFallado()
    {
        StopAllCoroutines();
        StartCoroutine(MostrarMensaje("ENCARGO FALLIDO", Color.red));
    }

    IEnumerator MostrarMensaje(string mensaje, Color color)
    {
        textoEstado.text = mensaje;
        textoEstado.color = color;
        textoEstado.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        textoEstado.gameObject.SetActive(false);
    }
}