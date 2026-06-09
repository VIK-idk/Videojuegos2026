using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ====================
// UI ESTADO ENCARGO
// ====================
public class UIEstadoEncargoLegacy : MonoBehaviour
{
    [Header("Texto")]
    [SerializeField] private Text textoEstado;
    [SerializeField] private RectTransform textoRect;

    [Header("Animacion")]
    [SerializeField] private float duracionEntrada = 0.18f;
    [SerializeField] private float duracionSalida = 0.18f;
    [SerializeField] private float escalaInicial = 0.65f;
    [SerializeField] private float escalaVisible = 1.15f;
    [SerializeField] private float escalaFinal = 0.65f;

    private Coroutine rutinaMensaje;
    private Vector3 escalaOriginal = Vector3.one;

    private void Awake()
    {
        if (textoEstado == null)
            textoEstado = GetComponent<Text>();

        if (textoRect == null && textoEstado != null)
            textoRect = textoEstado.GetComponent<RectTransform>();

        if (textoRect != null)
            escalaOriginal = textoRect.localScale;
    }

    private void Start()
    {
        OcultarInstantaneo();
    }

    public void MostrarRecolecta(float duracion)
    {
        MostrarMensajeAnimado(
            "¡Recolecta peces rebotando sobre las morsas!",
            Color.white,
            duracion
        );
    }

    public void MostrarCompletado()
    {
        MostrarMensajeAnimado(
            "ENCARGO COMPLETADO",
            Color.green,
            2f
        );
    }

    public void MostrarFallado()
    {
        MostrarMensajeAnimado(
            "ENCARGO FALLIDO",
            Color.red,
            2f
        );
    }

    public void MostrarMensajePersonalizado(string mensaje, Color color, float duracion)
    {
        MostrarMensajeAnimado(mensaje, color, duracion);
    }

    private void MostrarMensajeAnimado(string mensaje, Color color, float duracion)
    {
        if (textoEstado == null)
            return;

        if (rutinaMensaje != null)
            StopCoroutine(rutinaMensaje);

        rutinaMensaje = StartCoroutine(MostrarMensajeCoroutine(mensaje, color, duracion));
    }

    private IEnumerator MostrarMensajeCoroutine(string mensaje, Color colorBase, float duracion)
    {
        textoEstado.text = mensaje;
        textoEstado.enabled = true;

        Color color = colorBase;
        color.a = 0f;
        textoEstado.color = color;

        if (textoRect != null)
            textoRect.localScale = escalaOriginal * escalaInicial;

        yield return StartCoroutine(AnimarTexto(
            0f,
            1f,
            escalaInicial,
            escalaVisible,
            duracionEntrada,
            colorBase
        ));

        float tiempoVisible = Mathf.Max(0f, duracion);

        yield return new WaitForSeconds(tiempoVisible);

        yield return StartCoroutine(AnimarTexto(
            1f,
            0f,
            escalaVisible,
            escalaFinal,
            duracionSalida,
            colorBase
        ));

        textoEstado.enabled = false;

        if (textoRect != null)
            textoRect.localScale = escalaOriginal;

        rutinaMensaje = null;
    }

    private IEnumerator AnimarTexto(
        float alphaInicial,
        float alphaFinal,
        float escalaInicialAnim,
        float escalaFinalAnim,
        float duracion,
        Color colorBase)
    {
        if (textoEstado == null)
            yield break;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);
            t = Mathf.SmoothStep(0f, 1f, t);

            Color color = colorBase;
            color.a = Mathf.Lerp(alphaInicial, alphaFinal, t);
            textoEstado.color = color;

            if (textoRect != null)
            {
                float escala = Mathf.Lerp(escalaInicialAnim, escalaFinalAnim, t);
                textoRect.localScale = escalaOriginal * escala;
            }

            yield return null;
        }

        Color colorFinal = colorBase;
        colorFinal.a = alphaFinal;
        textoEstado.color = colorFinal;

        if (textoRect != null)
            textoRect.localScale = escalaOriginal * escalaFinalAnim;
    }

    public void Ocultar()
    {
        if (rutinaMensaje != null)
            StopCoroutine(rutinaMensaje);

        if (textoEstado != null && textoEstado.enabled)
            rutinaMensaje = StartCoroutine(OcultarConAnimacion());
        else
            OcultarInstantaneo();
    }

    private IEnumerator OcultarConAnimacion()
    {
        if (textoEstado == null)
            yield break;

        Color colorActual = textoEstado.color;
        Color colorBase = colorActual;
        colorBase.a = 1f;

        yield return StartCoroutine(AnimarTexto(
            colorActual.a,
            0f,
            escalaVisible,
            escalaFinal,
            duracionSalida,
            colorBase
        ));

        textoEstado.enabled = false;

        if (textoRect != null)
            textoRect.localScale = escalaOriginal;

        rutinaMensaje = null;
    }

    private void OcultarInstantaneo()
    {
        if (rutinaMensaje != null)
            StopCoroutine(rutinaMensaje);

        if (textoEstado != null)
        {
            Color color = textoEstado.color;
            color.a = 0f;
            textoEstado.color = color;
            textoEstado.enabled = false;
        }

        if (textoRect != null)
            textoRect.localScale = escalaOriginal;
    }
}