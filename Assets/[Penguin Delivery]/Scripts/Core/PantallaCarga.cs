using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PantallaCarga : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform barraContenedor;
    [SerializeField] private RectTransform barraRelleno;

    [Header("Carga")]
    [SerializeField] private float duracionMinimaPantalla = 1f;
    [SerializeField] private string escenaPruebaSiNoHayDestino = "Gameplay";

    private float anchoMaximoBarra;

    private void Start()
    {
        Canvas.ForceUpdateCanvases();

        if (barraContenedor != null)
        {
            anchoMaximoBarra = barraContenedor.rect.width;
        }

        ActualizarBarra(0f);

        StartCoroutine(CargarEscenaDestino());
    }

    private IEnumerator CargarEscenaDestino()
    {
        string escenaDestino = SceneLoader.EscenaDestino;

        if (string.IsNullOrEmpty(escenaDestino))
        {
            escenaDestino = escenaPruebaSiNoHayDestino;
        }

        float tiempoInicio = Time.unscaledTime;

        AsyncOperation operacion = SceneManager.LoadSceneAsync(escenaDestino);
        operacion.allowSceneActivation = false;

        while (!operacion.isDone)
        {
            float progresoReal = Mathf.Clamp01(operacion.progress / 0.9f);

            float tiempoTranscurrido = Time.unscaledTime - tiempoInicio;
            float progresoVisual = Mathf.Clamp01(tiempoTranscurrido / duracionMinimaPantalla);

            float progreso = Mathf.Min(progresoReal, progresoVisual);

            ActualizarBarra(progreso);

            bool cargaTerminada = operacion.progress >= 0.9f;
            bool tiempoMinimoCumplido = tiempoTranscurrido >= duracionMinimaPantalla;

            if (cargaTerminada && tiempoMinimoCumplido)
            {
                ActualizarBarra(1f);

                yield return new WaitForSecondsRealtime(0.15f);

                operacion.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private void ActualizarBarra(float progreso)
    {
        if (barraRelleno == null)
            return;

        float anchoActual = anchoMaximoBarra * Mathf.Clamp01(progreso);
        barraRelleno.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, anchoActual);
    }
}