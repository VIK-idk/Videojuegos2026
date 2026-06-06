using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverRetornoController : MonoBehaviour
{
    public static bool DerrotaActiva { get; private set; } = false;

    [Header("Camaras")]
    [SerializeField] private Camera camaraJugador;
    [SerializeField] private Camera camaraDerrota;
    [SerializeField] private Transform puntoCamaraReyMorsa;

    [Header("Rey Morsa")]
    [SerializeField] private ReyMorsaAnimacion reyMorsaAnimacion;
    [SerializeField] private float intervaloEnojoReyMorsa = 1.2f;

    [Header("UI derrota")]
    [SerializeField] private CanvasGroup panelNegro;
    [SerializeField] private Text textoMensaje;
    [SerializeField] private Text textoPuntos;
    [SerializeField] private Text textoContinuar;
    [SerializeField] private Image imagenPinguinoTriste;

    [Header("Tiempos")]
    [SerializeField] private float esperaAntesCamara = 1f;
    [SerializeField] private float duracionMovimientoCamara = 2f;
    [SerializeField] private float esperaAntesFadeNegro = 2f;
    [SerializeField] private float duracionFadeNegro = 1.5f;
    [SerializeField] private float esperaTextoMensaje = 0.5f;
    [SerializeField] private float esperaTextoPuntos = 1f;
    [SerializeField] private float duracionFadeTextoContinuar = 1f;
    [SerializeField] private float velocidadParpadeoTexto = 3f;

    [Header("Escena")]
    [SerializeField] private StrikeManager strikeManager;
    [SerializeField] private string escenaLobby = "Lobby";

    private bool secuenciaActiva = false;
    private Coroutine rutinaEnojo;

    private void Awake()
    {
        DerrotaActiva = false;

        BuscarReferencias();
        PrepararEstadoInicial();
    }

    private void PrepararEstadoInicial()
    {
        if (camaraDerrota != null)
        {
            camaraDerrota.gameObject.SetActive(true);
            camaraDerrota.enabled = false;
        }

        if (panelNegro != null)
        {
            panelNegro.alpha = 0f;
            panelNegro.blocksRaycasts = false;
            panelNegro.interactable = false;
            panelNegro.gameObject.SetActive(false);
        }

        OcultarElementosDerrota();
    }

    public void IniciarSecuenciaDerrota(int puntosRonda)
    {
        if (secuenciaActiva)
            return;

        DerrotaActiva = true;
        Time.timeScale = 1f;

        StartCoroutine(SecuenciaDerrota(puntosRonda));
    }

    private IEnumerator SecuenciaDerrota(int puntosRonda)
    {
        secuenciaActiva = true;

        BuscarReferencias();
        OcultarElementosDerrota();

        if (panelNegro != null)
        {
            panelNegro.gameObject.SetActive(true);
            panelNegro.alpha = 0f;
            panelNegro.blocksRaycasts = true;
            panelNegro.interactable = true;
        }

        yield return new WaitForSeconds(esperaAntesCamara);

        PrepararCamaraDerrota();

        if (camaraDerrota != null && puntoCamaraReyMorsa != null)
        {
            yield return StartCoroutine(MoverCamaraDerrotaAlReyMorsa());
        }

        rutinaEnojo = StartCoroutine(RepetirEnojoReyMorsa());

        yield return new WaitForSeconds(esperaAntesFadeNegro);

        if (panelNegro != null)
        {
            yield return StartCoroutine(FadeNegro(0f, 1f, duracionFadeNegro));
        }

        yield return new WaitForSeconds(esperaTextoMensaje);

        if (imagenPinguinoTriste != null)
            imagenPinguinoTriste.gameObject.SetActive(true);

        if (textoMensaje != null)
        {
            textoMensaje.text = "Vuelve a tu celda a descansar...";
            textoMensaje.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(esperaTextoPuntos);

        if (textoPuntos != null)
        {
            textoPuntos.text = "Puntos: " + puntosRonda;
            textoPuntos.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(esperaTextoPuntos);

        if (textoContinuar != null)
        {
            textoContinuar.text = "Pulsa cualquier tecla para volver a tu celda";
            textoContinuar.gameObject.SetActive(true);

            CambiarAlphaTexto(textoContinuar, 0f);
            yield return StartCoroutine(FadeTexto(textoContinuar, 0f, 1f, duracionFadeTextoContinuar));
        }

        yield return null;

        while (!Input.anyKeyDown &&
               !Input.GetButtonDown("Submit") &&
               !Input.GetButtonDown("Cancel"))
        {
            ParpadearTextoContinuar();
            yield return null;
        }

        FinalizarRutinas();

        DerrotaActiva = false;
        secuenciaActiva = false;

        if (strikeManager != null)
        {
            strikeManager.IrALobby();
        }
        else
        {
            SceneLoader.CargarEscena(escenaLobby);
        }
    }

    private void PrepararCamaraDerrota()
    {
        if (camaraJugador == null)
            camaraJugador = Camera.main;

        if (camaraJugador == null || camaraDerrota == null)
            return;

        camaraDerrota.transform.position = camaraJugador.transform.position;
        camaraDerrota.transform.rotation = camaraJugador.transform.rotation;
        camaraDerrota.fieldOfView = camaraJugador.fieldOfView;

        camaraDerrota.gameObject.SetActive(true);
        camaraDerrota.enabled = true;

        camaraJugador.enabled = false;
    }

    private IEnumerator MoverCamaraDerrotaAlReyMorsa()
    {
        Vector3 posicionInicial = camaraDerrota.transform.position;
        Quaternion rotacionInicial = camaraDerrota.transform.rotation;

        Vector3 posicionFinal = puntoCamaraReyMorsa.position;
        Quaternion rotacionFinal = puntoCamaraReyMorsa.rotation;

        float tiempo = 0f;

        while (tiempo < duracionMovimientoCamara)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionMovimientoCamara;
            t = Mathf.SmoothStep(0f, 1f, t);

            camaraDerrota.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);
            camaraDerrota.transform.rotation = Quaternion.Slerp(rotacionInicial, rotacionFinal, t);

            yield return null;
        }

        camaraDerrota.transform.position = posicionFinal;
        camaraDerrota.transform.rotation = rotacionFinal;
    }

    private IEnumerator FadeNegro(float alphaInicial, float alphaFinal, float duracion)
    {
        if (panelNegro == null)
            yield break;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracion;
            panelNegro.alpha = Mathf.Lerp(alphaInicial, alphaFinal, t);

            yield return null;
        }

        panelNegro.alpha = alphaFinal;
    }

    private IEnumerator FadeTexto(Text texto, float alphaInicial, float alphaFinal, float duracion)
    {
        if (texto == null)
            yield break;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracion;
            float alpha = Mathf.Lerp(alphaInicial, alphaFinal, t);

            CambiarAlphaTexto(texto, alpha);

            yield return null;
        }

        CambiarAlphaTexto(texto, alphaFinal);
    }

    private IEnumerator RepetirEnojoReyMorsa()
    {
        while (secuenciaActiva)
        {
            if (reyMorsaAnimacion != null)
                reyMorsaAnimacion.Enojar();

            yield return new WaitForSeconds(intervaloEnojoReyMorsa);
        }
    }

    private void ParpadearTextoContinuar()
    {
        if (textoContinuar == null)
            return;

        Color color = textoContinuar.color;
        color.a = Mathf.Abs(Mathf.Sin(Time.time * velocidadParpadeoTexto));
        textoContinuar.color = color;
    }

    private void CambiarAlphaTexto(Text texto, float alpha)
    {
        if (texto == null)
            return;

        Color color = texto.color;
        color.a = alpha;
        texto.color = color;
    }

    private void OcultarElementosDerrota()
    {
        if (textoMensaje != null)
            textoMensaje.gameObject.SetActive(false);

        if (textoPuntos != null)
            textoPuntos.gameObject.SetActive(false);

        if (textoContinuar != null)
        {
            CambiarAlphaTexto(textoContinuar, 0f);
            textoContinuar.gameObject.SetActive(false);
        }

        if (imagenPinguinoTriste != null)
            imagenPinguinoTriste.gameObject.SetActive(false);
    }

    private void FinalizarRutinas()
    {
        if (rutinaEnojo != null)
        {
            StopCoroutine(rutinaEnojo);
            rutinaEnojo = null;
        }
    }

    private void BuscarReferencias()
    {
        if (camaraJugador == null)
            camaraJugador = Camera.main;

        if (reyMorsaAnimacion == null)
            reyMorsaAnimacion = FindFirstObjectByType<ReyMorsaAnimacion>();

        if (strikeManager == null)
            strikeManager = FindFirstObjectByType<StrikeManager>();
    }
}