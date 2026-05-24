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
    // ICONOS
    // ====================
    private Image iconoRosa;
    private Image iconoAmarillo;
    private Image iconoVerde;

    // ====================
    // EFECTO RECOGIDA
    // ====================
    [Header("Efecto recogida")]
    [SerializeField] private float escalaResalte = 1.35f;
    [SerializeField] private float tiempoResaltado = 0.35f;
    [SerializeField] private float duracionAnimacionResalte = 0.12f;

    private Vector3 escalaOriginalRosa;
    private Vector3 escalaOriginalAmarillo;
    private Vector3 escalaOriginalVerde;

    private Coroutine rutinaRosa;
    private Coroutine rutinaAmarilla;
    private Coroutine rutinaVerde;
    private Coroutine rutinaFade;

    // ====================
    // INICIO
    // ====================
    private void Awake()
    {
        BuscarIconosAutomaticamente();
        GuardarEscalasOriginales();
    }

    // ====================
    // BUSCAR ICONOS
    // ====================
    private void BuscarIconosAutomaticamente()
    {
        if (filaRosa != null)
        {
            Transform icono = filaRosa.transform.Find("IconoRosa");

            if (icono != null)
                iconoRosa = icono.GetComponent<Image>();

            if (iconoRosa == null)
                iconoRosa = filaRosa.GetComponentInChildren<Image>(true);
        }

        if (filaAmarilla != null)
        {
            Transform icono = filaAmarilla.transform.Find("IconoAmarillo");

            if (icono != null)
                iconoAmarillo = icono.GetComponent<Image>();

            if (iconoAmarillo == null)
                iconoAmarillo = filaAmarilla.GetComponentInChildren<Image>(true);
        }

        if (filaVerde != null)
        {
            Transform icono = filaVerde.transform.Find("IconoVerde");

            if (icono != null)
                iconoVerde = icono.GetComponent<Image>();

            if (iconoVerde == null)
                iconoVerde = filaVerde.GetComponentInChildren<Image>(true);
        }
    }

    // ====================
    // GUARDAR ESCALAS
    // ====================
    private void GuardarEscalasOriginales()
    {
        if (iconoRosa != null)
            escalaOriginalRosa = iconoRosa.transform.localScale;

        if (iconoAmarillo != null)
            escalaOriginalAmarillo = iconoAmarillo.transform.localScale;

        if (iconoVerde != null)
            escalaOriginalVerde = iconoVerde.transform.localScale;
    }

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
    // RESALTAR PEZ
    // ====================
    public void ResaltarPezRecogido(ColorPez color)
    {
        if (color == ColorPez.Rosa)
        {
            IniciarResalte(iconoRosa, escalaOriginalRosa, ref rutinaRosa);
        }
        else if (color == ColorPez.Amarillo)
        {
            IniciarResalte(iconoAmarillo, escalaOriginalAmarillo, ref rutinaAmarilla);
        }
        else if (color == ColorPez.Verde)
        {
            IniciarResalte(iconoVerde, escalaOriginalVerde, ref rutinaVerde);
        }
    }

    // ====================
    // INICIAR RESALTE
    // ====================
    private void IniciarResalte(Image icono, Vector3 escalaOriginal, ref Coroutine rutina)
    {
        if (icono == null)
            return;

        if (rutina != null)
            StopCoroutine(rutina);

        icono.transform.localScale = escalaOriginal;

        rutina = StartCoroutine(AnimarResalte(icono, escalaOriginal));
    }

    // ====================
    // ANIMAR RESALTE
    // ====================
    private IEnumerator AnimarResalte(Image icono, Vector3 escalaOriginal)
    {
        Vector3 escalaGrande = escalaOriginal * escalaResalte;

        float tiempo = 0f;

        while (tiempo < duracionAnimacionResalte)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionAnimacionResalte;

            icono.transform.localScale = Vector3.Lerp(escalaOriginal, escalaGrande, t);

            yield return null;
        }

        icono.transform.localScale = escalaGrande;

        yield return new WaitForSeconds(tiempoResaltado);

        tiempo = 0f;

        while (tiempo < duracionAnimacionResalte)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionAnimacionResalte;

            icono.transform.localScale = Vector3.Lerp(escalaGrande, escalaOriginal, t);

            yield return null;
        }

        icono.transform.localScale = escalaOriginal;
    }

    // ====================
    // MOSTRAR
    // ====================
    public void Mostrar()
    {
        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

        rutinaFade = StartCoroutine(Fade(0f, 1f));
    }

    // ====================
    // OCULTAR
    // ====================
    public void Ocultar()
    {
        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

        rutinaFade = StartCoroutine(Fade(1f, 0f));
    }

    // ====================
    // OCULTAR YA
    // ====================
    public void OcultarInstantaneo()
    {
        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

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