using System.Collections;
using UnityEngine;
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

    private Coroutine rutinaCambio;
    private bool animandoCambio = false;
    private int valorMostrado = 0;

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
        ActualizarMonedas(true);
        OcultarCambio();

        if (ocultarGrupoCuandoNoHayCambio)
            OcultarGrupoInstantaneo();
        else
            MostrarGrupoInstantaneo();
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

        IniciarAnimacionCambio(origen, destino, "-" + cantidad, colorGasto);
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

        IniciarAnimacionCambio(origen, destino, "+" + cantidad, colorGanancia);
    }

    private void IniciarAnimacionCambio(int origen, int destino, string textoCambio, Color color)
    {
        if (rutinaCambio != null)
            StopCoroutine(rutinaCambio);

        rutinaCambio = StartCoroutine(AnimarCambio(origen, destino, textoCambio, color));
    }

    private IEnumerator AnimarCambio(int origen, int destino, string textoCambio, Color color)
    {
        animandoCambio = true;

        valorMostrado = origen;
        MostrarValor(valorMostrado);

        yield return StartCoroutine(MostrarGrupoConFade());
        yield return StartCoroutine(MostrarTextoCambioConFade(textoCambio, color));

        yield return new WaitForSecondsRealtime(esperaAntesDeContar);

        float tiempo = 0f;

        while (tiempo < duracionConteo)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(tiempo / duracionConteo);
            valorMostrado = Mathf.RoundToInt(Mathf.Lerp(origen, destino, t));

            MostrarValor(valorMostrado);

            yield return null;
        }

        valorMostrado = destino;
        MostrarValor(valorMostrado);

        yield return new WaitForSecondsRealtime(esperaDespuesDeContar);

        OcultarCambio();

        if (ocultarGrupoCuandoNoHayCambio)
            OcultarGrupoInstantaneo();

        animandoCambio = false;
        rutinaCambio = null;
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

            float t = Mathf.Clamp01(tiempo / duracionFadeEntrada);
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

            float t = Mathf.Clamp01(tiempo / duracionFadeEntrada);

            color.a = Mathf.Lerp(0f, 1f, t);
            textoGasto.color = color;

            yield return null;
        }

        color.a = 1f;
        textoGasto.color = color;
    }
}