using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelHabilidadUI : MonoBehaviour
{
    [Header("Panel")]
    public Image iconoHabilidad;
    public Text textoNombre;
    public Text textoDescripcion;
    public Text textoPrecio;
    public Button botonComprar;

    [Header("Lobby UI")]
    [SerializeField] private LobbyMonedasUI lobbyMonedasUI;

    [Header("Mensajes")]
    public Text textoErrorCompra;

    [Header("Visual")]
    public Color colorComprado = Color.gray;
    public float duracionTextoError = 2f;

    private Habilidad habilidadActual;
    private BotonHabilidadUI botonActual;

    private Coroutine rutinaError;

    private void Awake()
    {
        if (botonComprar != null)
            botonComprar.onClick.AddListener(Comprar);
    }

    private void Start()
    {
        if (lobbyMonedasUI == null)
            lobbyMonedasUI = FindFirstObjectByType<LobbyMonedasUI>();

        if (textoErrorCompra != null)
            textoErrorCompra.enabled = false;

        if (lobbyMonedasUI != null)
            lobbyMonedasUI.ActualizarMonedas();

        OcultarPanelHabilidad();
    }

    private void OnEnable()
    {
        if (lobbyMonedasUI == null)
            lobbyMonedasUI = FindFirstObjectByType<LobbyMonedasUI>();

        if (textoErrorCompra != null)
            textoErrorCompra.enabled = false;

        if (lobbyMonedasUI != null)
            lobbyMonedasUI.ActualizarMonedas();

        OcultarPanelHabilidad();
    }

    public void MostrarHabilidad(Habilidad habilidadSeleccionada, BotonHabilidadUI botonSeleccionado)
    {
        if (habilidadSeleccionada == null)
            return;

        habilidadActual = habilidadSeleccionada;
        botonActual = botonSeleccionado;

        bool comprada = EstaComprada(habilidadActual.Id);

        if (iconoHabilidad != null)
        {
            iconoHabilidad.gameObject.SetActive(true);
            iconoHabilidad.sprite = habilidadActual.Icono;
            iconoHabilidad.color = comprada ? colorComprado : Color.white;
        }

        if (textoNombre != null)
            textoNombre.text = habilidadActual.Nombre;

        if (textoDescripcion != null)
            textoDescripcion.text = habilidadActual.Descripcion;

        if (textoPrecio != null)
        {
            textoPrecio.gameObject.SetActive(true);

            if (comprada)
                textoPrecio.text = "Comprado";
            else
                textoPrecio.text = "Precio: " + habilidadActual.Precio;
        }

        if (botonComprar != null)
        {
            botonComprar.gameObject.SetActive(true);
            botonComprar.interactable = !comprada;
        }

        if (lobbyMonedasUI != null)
            lobbyMonedasUI.ActualizarMonedas();
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
            MostrarError("No tienes suficiente dinero");
            return;
        }

        SesionPartida.monedas -= habilidadActual.Precio;
        SesionPartida.monedasGastadas += habilidadActual.Precio;

        MarcarComprada(habilidadActual.Id);

        if (lobbyMonedasUI != null)
        {
            lobbyMonedasUI.ActualizarMonedas();
            lobbyMonedasUI.MostrarGasto(habilidadActual.Precio);
        }

        RefrescarVistaActual();

        if (botonActual != null)
            botonActual.ActualizarVisual();
    }

    private void RefrescarVistaActual()
    {
        if (habilidadActual != null)
            MostrarHabilidad(habilidadActual, botonActual);
    }

    private void OcultarPanelHabilidad()
    {
        habilidadActual = null;
        botonActual = null;

        if (iconoHabilidad != null)
            iconoHabilidad.gameObject.SetActive(false);

        if (textoPrecio != null)
            textoPrecio.gameObject.SetActive(false);

        if (botonComprar != null)
            botonComprar.gameObject.SetActive(false);
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

        yield return new WaitForSeconds(duracionTextoError);

        textoErrorCompra.enabled = false;
        rutinaError = null;
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