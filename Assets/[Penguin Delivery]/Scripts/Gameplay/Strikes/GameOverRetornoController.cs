using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
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

    [Header("Jugador")]
    [SerializeField] private Player jugador;

    [Header("UI derrota")]
    [SerializeField] private CanvasGroup panelNegro;
    [SerializeField] private Text textoMensaje;
    [SerializeField] private Text textoPuntos;
    [SerializeField] private Text textoContinuar;
    [SerializeField] private Image imagenPinguinoTriste;

    [Header("Audio - Pantalla de derrota 2D")]
    [SerializeField] private AudioSource audioSourceDerrotaGlobal;
    [SerializeField] private AudioSource audioSourceTextosDerrota;
    [SerializeField] private AudioMixerGroup grupoMixerDerrota;

    [Tooltip("Empieza cuando aparece el primer texto de derrota.")]
    [SerializeField] private AudioClip sonidoDerrotaGlobal;
    [SerializeField, Range(0f, 1f)] private float volumenDerrotaGlobal = 0.9f;

    [Tooltip("Se reproduce cada vez que aparece uno de los textos: mensaje, puntos y continuar.")]
    [SerializeField] private AudioClip sonidoAparicionTexto;
    [SerializeField, Range(0f, 1f)] private float volumenAparicionTexto = 0.7f;

    [Header("Tiempos")]
    [SerializeField] private float esperaAntesCamara = 1f;
    [SerializeField] private float duracionMovimientoCamara = 2f;
    [SerializeField] private float esperaAntesFadeNegro = 2f;
    [SerializeField] private float duracionFadeNegro = 1.5f;
    [SerializeField] private float esperaTextoMensaje = 0.5f;
    [SerializeField] private float esperaTextoPuntos = 1f;
    [SerializeField] private float duracionFadeTextoContinuar = 1f;
    [SerializeField] private float velocidadParpadeoTexto = 3f;

    [Header("Musica durante la derrota")]
    [Tooltip("Empieza a bajar la musica al recibir el ultimo strike y termina al aparecer el primer texto.")]
    [SerializeField] private bool hacerFadeMusicaGameplay = true;

    [Tooltip("Valor negativo = termina antes. Valor positivo = termina despues del primer texto.")]
    [SerializeField] private float ajusteDuracionFadeMusica = 0f;

    [Header("Escena")]
    [SerializeField] private StrikeManager strikeManager;
    [SerializeField] private string escenaLobby = "Lobby";

    private bool secuenciaActiva = false;
    private Coroutine rutinaEnojo;

    private void Awake()
    {
        DerrotaActiva = false;

        ConfigurarAudioDerrota();
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

        BuscarReferencias();
        IniciarFadeMusicaDerrota();

        StartCoroutine(SecuenciaDerrota(puntosRonda));
    }

    private IEnumerator SecuenciaDerrota(int puntosRonda)
    {
        secuenciaActiva = true;

        BuscarReferencias();
        ConfigurarAudioDerrota();
        DetenerAudioPantallaDerrota();
        OcultarElementosDerrota();

        if (panelNegro != null)
        {
            panelNegro.gameObject.SetActive(true);
            panelNegro.alpha = 0f;
            panelNegro.blocksRaycasts = true;
            panelNegro.interactable = true;
        }

        yield return new WaitForSeconds(esperaAntesCamara);

        // En cuanto empieza la cinematica hacia el Rey Morsa, Guppy deja de
        // recibir movimiento y se cortan sus pasos/salto/caida.
        BloquearJugadorAlIniciarCamaraDerrota();
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

        // En este punto la pantalla de derrota ya cubre completamente la imagen.
        // Detenemos la queja del Rey Morsa y todos los sonidos de movimiento de Guppy.
        DetenerSonidosAlCompletarPantallaDerrota();

        yield return new WaitForSeconds(esperaTextoMensaje);

        if (imagenPinguinoTriste != null)
            imagenPinguinoTriste.gameObject.SetActive(true);

        if (textoMensaje != null)
        {
            textoMensaje.text = "Vuelve a tu celda a descansar...";
            textoMensaje.gameObject.SetActive(true);
            ReproducirInicioPantallaDerrota();
            ReproducirSonidoAparicionTexto();
        }

        yield return new WaitForSeconds(esperaTextoPuntos);

        if (textoPuntos != null)
        {
            textoPuntos.text = "Puntos: " + puntosRonda;
            textoPuntos.gameObject.SetActive(true);
            ReproducirSonidoAparicionTexto();
        }

        yield return new WaitForSeconds(esperaTextoPuntos);

        if (textoContinuar != null)
        {
            textoContinuar.text = "Pulsa cualquier tecla para volver a tu celda";
            textoContinuar.gameObject.SetActive(true);
            ReproducirSonidoAparicionTexto();

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


    private void IniciarFadeMusicaDerrota()
    {
        if (!hacerFadeMusicaGameplay)
            return;

        if (MusicaManager.Instancia == null)
            return;

        float tiempoMovimientoCamara = 0f;
        if (camaraDerrota != null && puntoCamaraReyMorsa != null)
            tiempoMovimientoCamara = Mathf.Max(0f, duracionMovimientoCamara);

        float tiempoFadeNegro = panelNegro != null
            ? Mathf.Max(0f, duracionFadeNegro)
            : 0f;

        float duracionHastaPrimerTexto =
            Mathf.Max(0f, esperaAntesCamara) +
            tiempoMovimientoCamara +
            Mathf.Max(0f, esperaAntesFadeNegro) +
            tiempoFadeNegro +
            Mathf.Max(0f, esperaTextoMensaje) +
            ajusteDuracionFadeMusica;

        duracionHastaPrimerTexto = Mathf.Max(0f, duracionHastaPrimerTexto);

        MusicaManager.Instancia.DesvanecerMusicaActual(duracionHastaPrimerTexto);
    }

    private void ConfigurarAudioDerrota()
    {
        if (audioSourceDerrotaGlobal == null)
            audioSourceDerrotaGlobal = gameObject.AddComponent<AudioSource>();

        if (audioSourceTextosDerrota == null)
            audioSourceTextosDerrota = gameObject.AddComponent<AudioSource>();

        ConfigurarAudioSource2D(audioSourceDerrotaGlobal);
        ConfigurarAudioSource2D(audioSourceTextosDerrota);
    }

    private void ConfigurarAudioSource2D(AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;

        if (grupoMixerDerrota != null)
            source.outputAudioMixerGroup = grupoMixerDerrota;
    }

    private void ReproducirInicioPantallaDerrota()
    {
        if (audioSourceDerrotaGlobal == null || sonidoDerrotaGlobal == null)
            return;

        audioSourceDerrotaGlobal.Stop();
        audioSourceDerrotaGlobal.clip = sonidoDerrotaGlobal;
        audioSourceDerrotaGlobal.volume = volumenDerrotaGlobal;
        audioSourceDerrotaGlobal.loop = false;
        audioSourceDerrotaGlobal.Play();
    }

    private void ReproducirSonidoAparicionTexto()
    {
        if (audioSourceTextosDerrota == null || sonidoAparicionTexto == null)
            return;

        audioSourceTextosDerrota.PlayOneShot(sonidoAparicionTexto, volumenAparicionTexto);
    }

    private void DetenerAudioPantallaDerrota()
    {
        if (audioSourceDerrotaGlobal != null)
            audioSourceDerrotaGlobal.Stop();

        if (audioSourceTextosDerrota != null)
            audioSourceTextosDerrota.Stop();
    }

    private void BloquearJugadorAlIniciarCamaraDerrota()
    {
        if (jugador == null)
            jugador = FindFirstObjectByType<Player>();

        if (jugador != null)
            jugador.BloquearMovimientoYAudioPorDerrota();
    }

    private void DetenerSonidosAlCompletarPantallaDerrota()
    {
        if (rutinaEnojo != null)
        {
            StopCoroutine(rutinaEnojo);
            rutinaEnojo = null;
        }

        if (reyMorsaAnimacion != null)
        {
            reyMorsaAnimacion.DetenerAudioPorDerrota();
        }

        if (jugador != null)
        {
            jugador.SilenciarAudioPorDerrota();
        }
    }

    private void FinalizarRutinas()
    {
        DetenerAudioPantallaDerrota();

        if (rutinaEnojo != null)
        {
            StopCoroutine(rutinaEnojo);
            rutinaEnojo = null;
        }

        if (reyMorsaAnimacion != null)
        {
            reyMorsaAnimacion.DetenerAudioPorDerrota();
        }

        if (jugador != null)
        {
            jugador.SilenciarAudioPorDerrota();
        }
    }

    private void OnDisable()
    {
        DetenerAudioPantallaDerrota();
    }

    private void BuscarReferencias()
    {
        if (camaraJugador == null)
            camaraJugador = Camera.main;

        if (reyMorsaAnimacion == null)
            reyMorsaAnimacion = FindFirstObjectByType<ReyMorsaAnimacion>();

        if (jugador == null)
            jugador = FindFirstObjectByType<Player>();

        if (strikeManager == null)
            strikeManager = FindFirstObjectByType<StrikeManager>();
    }
}