using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ====================
// UI ENCARGO
// ====================
public class UIEncargoLegacy : MonoBehaviour
{
    // ====================
    // REFERENCIAS
    // ====================
    [Header("Tiempo")]
    [SerializeField] private Text textoTiempo;

    [Header("Filas")]
    [SerializeField] private GameObject filaRosa;
    [SerializeField] private GameObject filaAmarilla;
    [SerializeField] private GameObject filaVerde;

    [Header("Textos")]
    [SerializeField] private Text textoRosa;
    [SerializeField] private Text textoAmarillo;
    [SerializeField] private Text textoVerde;

    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;

    // ====================
    // ACTUALIZAR
    // ====================
    public void ActualizarUI(
        EncargoData encargo,
        float tiempo,
        int rosasActuales,
        int amarillosActuales,
        int verdesActuales)
    {
        if (encargo == null)
            return;

        if (textoTiempo != null)
        {
            if (float.IsInfinity(tiempo))
                textoTiempo.text = "∞";
            else
                textoTiempo.text = tiempo.ToString("F1") + "s";
        }

        if (filaRosa != null)
            filaRosa.SetActive(encargo.pecesRosas > 0);

        if (filaAmarilla != null)
            filaAmarilla.SetActive(encargo.pecesAmarillos > 0);

        if (filaVerde != null)
            filaVerde.SetActive(encargo.pecesVerdes > 0);

        if (textoRosa != null)
            textoRosa.text = rosasActuales + "/" + encargo.pecesRosas;

        if (textoAmarillo != null)
            textoAmarillo.text = amarillosActuales + "/" + encargo.pecesAmarillos;

        if (textoVerde != null)
            textoVerde.text = verdesActuales + "/" + encargo.pecesVerdes;
    }

    // ====================
    // MOSTRAR
    // ====================
    public void Mostrar()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0f, 1f));
    }

    // ====================
    // OCULTAR
    // ====================
    public void Ocultar()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1f, 0f));
    }

    // ====================
    // OCULTAR YA
    // ====================
    public void OcultarInstantaneo()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    // ====================
    // FADE
    // ====================
    private IEnumerator Fade(float inicio, float fin)
    {
        if (canvasGroup == null)
            yield break;

        float tiempo = 0f;
        float duracion = 0.25f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(inicio, fin, tiempo / duracion);
            yield return null;
        }

        canvasGroup.alpha = fin;
    }
}