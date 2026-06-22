using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LobbyMonedasUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text textoMonedas;
    [SerializeField] private Text textoGasto;

    [Header("Grupo visual opcional")]
    [SerializeField] private CanvasGroup grupoMonedas;
    [SerializeField] private bool ocultarGrupoCuandoNoHayCambio = false;

    [Header("Animacion")]
    [SerializeField] private float duracionFadeEntrada = 0.15f;
    [SerializeField] private float esperaAntesDeContar = 0.5f;
    [SerializeField] private float duracionConteo = 0.6f;
    [SerializeField] private float esperaDespuesDeContar = 0.5f;

    [Header("Colores")]
    [SerializeField] private Color colorGanancia = Color.green;
    [SerializeField] private Color colorGasto = Color.red;

    [Header("Audio - Conteo de monedas ganadas")]
    [SerializeField] private AudioSource audioSourceConteoMonedas;
    [SerializeField] private AudioMixerGroup grupoMixerMonedas;
    [SerializeField] private AudioClip sonidoConteoGanancia;
    [SerializeField, Range(0f, 1f)] private float volumenConteoGanancia = 0.7f;
    [SerializeField, Min(0.01f)] private float intervaloMinimoEntreSonidos = 0.06f;

    private Coroutine rutinaCambio;
    private bool animandoCambio = false;
    private int valorMostrado = 0;

    private float siguienteMomentoSonido = 0f;
    private int ultimoValorQueSono = int.MinValue;

    private void Awake()
    {
        ConfigurarAudioConteo();
    }

    private void Start()
    {
        ActualizarMonedas(true);
        OcultarCambio();

        if (ocultarGrupoCuandoNoHayCambio)
            OcultarGrupoInstantaneo();
        else
            MostrarGrupoInstantaneo();
    }

    private void OnEnable()
    {
        ConfigurarAudioConteo();
        ActualizarMonedas(true);
        OcultarCambio();

        if (ocultarGrupoCuandoNoHayCambio)
            OcultarGrupoInstantaneo();
        else
            MostrarGrupoInstantaneo();
    }

    private void OnDisable()
    {
        DetenerSonidoConteo();
    }

    public void ActualizarMonedas()
    {
        ActualizarMonedas(false);
    }

    public void ActualizarMonedas(bool forzar)
    {
        if (animandoCambio && !forzar)
            return;

        valorMostrado = SesionPartida.monedas;
        MostrarValor(valorMostrado);
    }

    public void MostrarGasto(int cantidad)
    {
        int destino = SesionPartida.monedas;
        int origen = destino + cantidad;

        MostrarGasto(cantidad, origen, destino);
    }

    public void MostrarGasto(int cantidad, int origen, int destino)
    {
        if (cantidad <= 0)
            return;

        IniciarAnimacionCambio(
            origen,
            destino,
            "-" + cantidad,
            colorGasto,
            reproducirSonidoConteoGanancia: false
        );
    }

    public void MostrarGanancia(int cantidad)
    {
        int destino = SesionPartida.monedas;
        int origen = Mathf.Max(0, destino - cantidad);

        MostrarGanancia(cantidad, origen, destino);
    }

    public void MostrarGanancia(int cantidad, int origen, int destino)
    {
        if (cantidad <= 0)
            return;

        IniciarAnimacionCambio(
            origen,
            destino,
            "+" + cantidad,
            colorGanancia,
            reproducirSonidoConteoGanancia: true
        );
    }

    private void IniciarAnimacionCambio(
        int origen,
        int destino,
        string textoCambio,
        Color color,
        bool reproducirSonidoConteoGanancia)
    {
        if (rutinaCambio != null)
            StopCoroutine(rutinaCambio);

        DetenerSonidoConteo();

        rutinaCambio = StartCoroutine(
            AnimarCambio(
                origen,
                destino,
                textoCambio,
                color,
                reproducirSonidoConteoGanancia
            )
        );
    }

    private IEnumerator AnimarCambio(
        int origen,
        int destino,
        string textoCambio,
        Color color,
        bool reproducirSonidoConteoGanancia)
    {
        animandoCambio = true;

        valorMostrado = origen;
        MostrarValor(valorMostrado);

        yield return StartCoroutine(MostrarGrupoConFade());
        yield return StartCoroutine(MostrarTextoCambioConFade(textoCambio, color));

        yield return new WaitForSecondsRealtime(esperaAntesDeContar);

        PrepararSonidoConteo(valorMostrado);

        float tiempo = 0f;

        while (tiempo < duracionConteo)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = duracionConteo <= 0f
                ? 1f
                : Mathf.Clamp01(tiempo / duracionConteo);

            int nuevoValor = Mathf.RoundToInt(Mathf.Lerp(origen, destino, t));

            if (nuevoValor != valorMostrado)
            {
                valorMostrado = nuevoValor;
                MostrarValor(valorMostrado);

                if (reproducirSonidoConteoGanancia && destino > origen)
                    IntentarReproducirSonidoConteo(valorMostrado);
            }

            yield return null;
        }

        valorMostrado = destino;
        MostrarValor(valorMostrado);

        DetenerSonidoConteo();

        yield return new WaitForSecondsRealtime(esperaDespuesDeContar);

        OcultarCambio();

        if (ocultarGrupoCuandoNoHayCambio)
            OcultarGrupoInstantaneo();

        animandoCambio = false;
        rutinaCambio = null;
    }

    private void ConfigurarAudioConteo()
    {
        if (audioSourceConteoMonedas == null)
            audioSourceConteoMonedas = GetComponent<AudioSource>();

        if (audioSourceConteoMonedas == null)
            return;

        audioSourceConteoMonedas.playOnAwake = false;
        audioSourceConteoMonedas.loop = false;
        audioSourceConteoMonedas.spatialBlend = 0f;
        audioSourceConteoMonedas.dopplerLevel = 0f;

        if (grupoMixerMonedas != null)
            audioSourceConteoMonedas.outputAudioMixerGroup = grupoMixerMonedas;
    }

    private void PrepararSonidoConteo(int valorInicial)
    {
        siguienteMomentoSonido = Time.unscaledTime;
        ultimoValorQueSono = valorInicial;
    }

    private void IntentarReproducirSonidoConteo(int nuevoValor)
    {
        if (audioSourceConteoMonedas == null || sonidoConteoGanancia == null)
            return;

        if (nuevoValor == ultimoValorQueSono)
            return;

        if (Time.unscaledTime < siguienteMomentoSonido)
            return;

        audioSourceConteoMonedas.PlayOneShot(
            sonidoConteoGanancia,
            volumenConteoGanancia
        );

        ultimoValorQueSono = nuevoValor;
        siguienteMomentoSonido = Time.unscaledTime + intervaloMinimoEntreSonidos;
    }

    private void DetenerSonidoConteo()
    {
        if (audioSourceConteoMonedas != null)
            audioSourceConteoMonedas.Stop();

        siguienteMomentoSonido = 0f;
        ultimoValorQueSono = int.MinValue;
    }

    private IEnumerator MostrarGrupoConFade()
    {
        if (grupoMonedas == null)
        {
            MostrarGrupoInstantaneo();
            yield break;
        }

        grupoMonedas.interactable = true;
        grupoMonedas.blocksRaycasts = true;

        float alphaInicial = grupoMonedas.alpha;
        float tiempo = 0f;

        while (tiempo < duracionFadeEntrada)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = duracionFadeEntrada <= 0f
                ? 1f
                : Mathf.Clamp01(tiempo / duracionFadeEntrada);

            grupoMonedas.alpha = Mathf.Lerp(alphaInicial, 1f, t);

            yield return null;
        }

        grupoMonedas.alpha = 1f;
    }

    private void MostrarValor(int valor)
    {
        if (textoMonedas != null)
            textoMonedas.text = valor.ToString();
    }

    private void OcultarCambio()
    {
        if (textoGasto != null)
            textoGasto.enabled = false;
    }

    private void MostrarGrupoInstantaneo()
    {
        if (grupoMonedas == null)
            return;

        grupoMonedas.alpha = 1f;
        grupoMonedas.interactable = true;
        grupoMonedas.blocksRaycasts = true;
    }

    private void OcultarGrupoInstantaneo()
    {
        if (grupoMonedas == null)
            return;

        grupoMonedas.alpha = 0f;
        grupoMonedas.interactable = false;
        grupoMonedas.blocksRaycasts = false;
    }

    private IEnumerator MostrarTextoCambioConFade(string textoCambio, Color colorBase)
    {
        if (textoGasto == null)
            yield break;

        textoGasto.text = textoCambio;
        textoGasto.enabled = true;

        Color color = colorBase;
        color.a = 0f;
        textoGasto.color = color;

        float tiempo = 0f;

        while (tiempo < duracionFadeEntrada)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = duracionFadeEntrada <= 0f
                ? 1f
                : Mathf.Clamp01(tiempo / duracionFadeEntrada);

            color.a = Mathf.Lerp(0f, 1f, t);
            textoGasto.color = color;

            yield return null;
        }

        color.a = 1f;
        textoGasto.color = color;
    }
}
