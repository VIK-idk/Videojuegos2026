using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelHabilidadUI : MonoBehaviour
{
    [Header("Panel")]
    public Image iconoHabilidad;
    public Text textoNombre;
    public Text textoDescripcion;
    public Text textoPrecio;
    public Button botonComprar;

    [Header("Texto inicial tienda")]
    [SerializeField] private bool usarTextoInicialDelInspector = true;
    [SerializeField] private string nombreInicialTienda = "";
    [SerializeField, TextArea(2, 5)] private string descripcionInicialTienda = "";

    [Header("Precio")]
    [SerializeField] private GameObject grupoPrecio;
    [SerializeField] private Image iconoMonedaPrecio;

    [Header("Estado comprado")]
    [SerializeField] private GameObject imagenComprado;

    [Header("Lobby UI")]
    [SerializeField] private LobbyMonedasUI lobbyMonedasUI;

    [Header("Mercader")]
    [SerializeField] private MercaderAnimacion mercaderAnimacion;

    [Header("Sonidos tienda")]
    [SerializeField] private SonidosTiendaManager sonidosTiendaManager;

    [Header("Mensajes")]
    public Text textoErrorCompra;

    [Header("Visual")]
    public float duracionTextoError = 2f;

    [Header("UI Mando")]
    [SerializeField] private bool seleccionarComprarAlElegirHabilidad = true;

    private Habilidad habilidadActual;
    private BotonHabilidadUI botonActual;
    private Coroutine rutinaError;
    private bool textosInicialesPreparados = false;

    private void Awake()
    {
        PrepararTextosInicialesTienda();

        if (botonComprar != null)
        {
            botonComprar.onClick.RemoveListener(Comprar);
            botonComprar.onClick.AddListener(Comprar);
        }
    }

    private void Start()
    {
        BuscarReferencias();

        if (textoErrorCompra != null)
            textoErrorCompra.enabled = false;

        if (lobbyMonedasUI != null)
            lobbyMonedasUI.ActualizarMonedas();

        OcultarPanelHabilidad();
    }

    private void OnEnable()
    {
        PrepararTextosInicialesTienda();
        BuscarReferencias();

        if (textoErrorCompra != null)
            textoErrorCompra.enabled = false;

        if (lobbyMonedasUI != null)
            lobbyMonedasUI.ActualizarMonedas();

        OcultarPanelHabilidad();
    }

    private void OnDestroy()
    {
        if (botonComprar != null)
            botonComprar.onClick.RemoveListener(Comprar);
    }

    private void PrepararTextosInicialesTienda()
    {
        if (textosInicialesPreparados)
            return;

        if (usarTextoInicialDelInspector)
        {
            if (textoNombre != null)
                nombreInicialTienda = textoNombre.text;

            if (textoDescripcion != null)
                descripcionInicialTienda = textoDescripcion.text;
        }

        textosInicialesPreparados = true;
    }

    private void BuscarReferencias()
    {
        if (lobbyMonedasUI == null)
            lobbyMonedasUI = FindFirstObjectByType<LobbyMonedasUI>();

        if (mercaderAnimacion == null)
            mercaderAnimacion = FindFirstObjectByType<MercaderAnimacion>();

        if (sonidosTiendaManager == null)
            sonidosTiendaManager = FindFirstObjectByType<SonidosTiendaManager>();
    }

    public void MostrarHabilidad(Habilidad habilidadSeleccionada, BotonHabilidadUI botonSeleccionado)
    {
        if (habilidadSeleccionada == null)
            return;

        if (botonActual != null && botonActual != botonSeleccionado)
            botonActual.SetSeleccionado(false);

        habilidadActual = habilidadSeleccionada;
        botonActual = botonSeleccionado;

        if (botonActual != null)
            botonActual.SetSeleccionado(true);

        bool comprada = EstaComprada(habilidadActual.Id);

        if (iconoHabilidad != null)
        {
            iconoHabilidad.gameObject.SetActive(true);
            iconoHabilidad.sprite = habilidadActual.Icono;
            iconoHabilidad.color = Color.white;
        }

        if (textoNombre != null)
            textoNombre.text = habilidadActual.Nombre;

        if (textoDescripcion != null)
            textoDescripcion.text = habilidadActual.Descripcion;

        ActualizarPrecioPanel(comprada);
        ActualizarEstadoCompra(comprada);

        if (lobbyMonedasUI != null)
            lobbyMonedasUI.ActualizarMonedas();

        if (seleccionarComprarAlElegirHabilidad &&
            !comprada &&
            InputDetector.DebeMostrarSeleccionUI)
        {
            StartCoroutine(SeleccionarComprarAlFinalDelFrame());
        }
    }

    private void ActualizarPrecioPanel(bool comprada)
    {
        if (comprada)
        {
            if (grupoPrecio != null)
                grupoPrecio.SetActive(false);

            if (textoPrecio != null)
                textoPrecio.gameObject.SetActive(false);

            if (iconoMonedaPrecio != null)
                iconoMonedaPrecio.gameObject.SetActive(false);

            return;
        }

        if (grupoPrecio != null)
            grupoPrecio.SetActive(true);

        if (textoPrecio != null)
        {
            textoPrecio.gameObject.SetActive(true);
            textoPrecio.text = habilidadActual.Precio.ToString();
        }

        if (iconoMonedaPrecio != null)
            iconoMonedaPrecio.gameObject.SetActive(true);
    }

    private void ActualizarEstadoCompra(bool comprada)
    {
        if (botonComprar != null)
        {
            botonComprar.gameObject.SetActive(!comprada);
            botonComprar.interactable = !comprada;
        }

        if (imagenComprado != null)
            imagenComprado.SetActive(comprada);
    }

    private void Comprar()
    {
        if (habilidadActual == null)
            return;

        if (EstaComprada(habilidadActual.Id))
        {
            RefrescarVistaActual();
            return;
        }

        if (SesionPartida.monedas < habilidadActual.Precio)
        {
            if (sonidosTiendaManager != null)
                sonidosTiendaManager.ReproducirCompraSinDinero();

            if (mercaderAnimacion != null)
                mercaderAnimacion.PedirEnojado();

            MostrarError("No tienes suficiente dinero");
            return;
        }

        int monedasAntes = SesionPartida.monedas;

        SesionPartida.monedas -= habilidadActual.Precio;
        SesionPartida.monedasGastadas += habilidadActual.Precio;

        int monedasDespues = SesionPartida.monedas;

        MarcarComprada(habilidadActual.Id);

        if (sonidosTiendaManager != null)
            sonidosTiendaManager.ReproducirCompraExitosa();

        if (mercaderAnimacion != null)
            mercaderAnimacion.PedirAlegre();

        if (lobbyMonedasUI != null)
            lobbyMonedasUI.MostrarGasto(habilidadActual.Precio, monedasAntes, monedasDespues);

        if (botonActual != null)
            botonActual.ActualizarVisual();

        RefrescarVistaActual();
    }

    private void RefrescarVistaActual()
    {
        if (habilidadActual != null)
            MostrarHabilidad(habilidadActual, botonActual);
    }

    public void LimpiarSeleccion()
    {
        OcultarPanelHabilidad();
    }

    private void OcultarPanelHabilidad()
    {
        if (botonActual != null)
            botonActual.SetSeleccionado(false);

        habilidadActual = null;
        botonActual = null;

        RestaurarTextoInicialTienda();

        if (iconoHabilidad != null)
            iconoHabilidad.gameObject.SetActive(false);

        if (grupoPrecio != null)
            grupoPrecio.SetActive(false);

        if (textoPrecio != null)
            textoPrecio.gameObject.SetActive(false);

        if (iconoMonedaPrecio != null)
            iconoMonedaPrecio.gameObject.SetActive(false);

        if (botonComprar != null)
            botonComprar.gameObject.SetActive(false);

        if (imagenComprado != null)
            imagenComprado.SetActive(false);

        if (rutinaError != null)
        {
            StopCoroutine(rutinaError);
            rutinaError = null;
        }

        if (textoErrorCompra != null)
            textoErrorCompra.enabled = false;
    }

    private void RestaurarTextoInicialTienda()
    {
        if (textoNombre != null)
            textoNombre.text = nombreInicialTienda;

        if (textoDescripcion != null)
            textoDescripcion.text = descripcionInicialTienda;
    }

    private void MostrarError(string mensaje)
    {
        if (textoErrorCompra == null)
            return;

        if (rutinaError != null)
            StopCoroutine(rutinaError);

        rutinaError = StartCoroutine(MostrarErrorCoroutine(mensaje));
    }

    private IEnumerator MostrarErrorCoroutine(string mensaje)
    {
        textoErrorCompra.text = mensaje;
        textoErrorCompra.color = Color.red;
        textoErrorCompra.enabled = true;

        yield return new WaitForSecondsRealtime(duracionTextoError);

        textoErrorCompra.enabled = false;
        rutinaError = null;
    }

    private IEnumerator SeleccionarComprarAlFinalDelFrame()
    {
        yield return null;

        if (!InputDetector.DebeMostrarSeleccionUI)
            yield break;

        if (EventSystem.current == null)
            yield break;

        if (botonComprar == null)
            yield break;

        if (!botonComprar.gameObject.activeInHierarchy)
            yield break;

        if (!botonComprar.interactable)
            yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(botonComprar.gameObject);
    }

    private bool EstaComprada(string id)
    {
        if (id == "x2_peces")
            return SesionPartida.habilidadX2Comprada;

        if (id == "iman")
            return SesionPartida.habilidadImanComprada;

        if (id == "quitar_strike")
            return SesionPartida.habilidadQuitarStrikeComprada;

        return false;
    }

    private void MarcarComprada(string id)
    {
        if (id == "x2_peces")
            SesionPartida.habilidadX2Comprada = true;

        if (id == "iman")
            SesionPartida.habilidadImanComprada = true;

        if (id == "quitar_strike")
            SesionPartida.habilidadQuitarStrikeComprada = true;
    }
}
