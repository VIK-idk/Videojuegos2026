using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Sonido para un botón concreto de la tienda.
/// Permite elegir si el botón es de madera o de habilidad.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class SonidoBotonTiendaUI : MonoBehaviour,
    IPointerEnterHandler,
    ISelectHandler,
    IPointerDownHandler,
    ISubmitHandler
{
    [Header("Referencias")]
    [SerializeField] private Button boton;
    [SerializeField] private SonidosTiendaManager sonidosTiendaManager;

    [Header("Tipo")]
    [SerializeField] private TipoBotonTienda tipoBoton = TipoBotonTienda.Madera;

    [Header("Eventos")]
    [SerializeField] private bool reproducirHoverSelected = true;
    [SerializeField] private bool reproducirPulsar = true;

    [Header("Selección inicial")]
    [SerializeField] private bool silenciarSeleccionInicial = false;
    [SerializeField, Min(0f)] private float tiempoSilencioInicial = 0.08f;

    private float tiempoActivacion;
    private float ultimoHoverLocal = -999f;
    private const float INTERVALO_LOCAL_HOVER = 0.04f;

    private void Awake()
    {
        BuscarReferencias();
    }

    private void OnEnable()
    {
        BuscarReferencias();
        tiempoActivacion = Time.unscaledTime;
        ultimoHoverLocal = -999f;
    }

    private void Reset()
    {
        BuscarReferencias();
    }

    private void BuscarReferencias()
    {
        if (boton == null)
            boton = GetComponent<Button>();

        if (sonidosTiendaManager == null)
            sonidosTiendaManager = GetComponentInParent<SonidosTiendaManager>(true);

        if (sonidosTiendaManager == null)
            sonidosTiendaManager = FindFirstObjectByType<SonidosTiendaManager>();
    }

    private bool BotonDisponible()
    {
        return boton != null && boton.interactable && gameObject.activeInHierarchy;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!reproducirHoverSelected || !BotonDisponible())
            return;

        if (InputDetector.ModoActual != InputDetector.ModoEntrada.TecladoRaton)
            return;

        ReproducirHoverConProteccion();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!reproducirHoverSelected || !BotonDisponible())
            return;

        if (!InputDetector.DebeMostrarSeleccionUI)
            return;

        if (silenciarSeleccionInicial &&
            Time.unscaledTime - tiempoActivacion < tiempoSilencioInicial)
        {
            return;
        }

        ReproducirHoverConProteccion();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!reproducirPulsar || !BotonDisponible())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        ReproducirPulsar();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!reproducirPulsar || boton == null || !gameObject.activeInHierarchy)
            return;

        // No comprobamos interactable aquí porque BotonHabilidadUI puede
        // desactivarlo en el mismo Submit justo después de seleccionarlo.
        ReproducirPulsar();
    }

    private void ReproducirHoverConProteccion()
    {
        if (Time.unscaledTime - ultimoHoverLocal < INTERVALO_LOCAL_HOVER)
            return;

        ultimoHoverLocal = Time.unscaledTime;
        BuscarReferencias();

        if (sonidosTiendaManager != null)
            sonidosTiendaManager.ReproducirHover(tipoBoton);
    }

    private void ReproducirPulsar()
    {
        BuscarReferencias();

        if (sonidosTiendaManager != null)
            sonidosTiendaManager.ReproducirPulsar(tipoBoton);
    }

    public void Configurar(
        TipoBotonTienda nuevoTipo,
        bool usarHoverSelected,
        bool usarPulsar,
        bool silenciarPrimeraSeleccion)
    {
        tipoBoton = nuevoTipo;
        reproducirHoverSelected = usarHoverSelected;
        reproducirPulsar = usarPulsar;
        silenciarSeleccionInicial = silenciarPrimeraSeleccion;
        BuscarReferencias();
    }
}
