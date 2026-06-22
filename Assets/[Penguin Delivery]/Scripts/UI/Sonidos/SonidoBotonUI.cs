using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Reproduce los sonidos generales de un botón.
/// Ratón: sonido al entrar con el cursor.
/// Teclado/mando: sonido al quedar seleccionado.
/// Todos: sonido al pulsarlo.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class SonidoBotonUI : MonoBehaviour,
    IPointerEnterHandler,
    ISelectHandler,
    IPointerClickHandler,
    ISubmitHandler
{
    [Header("Botón")]
    [SerializeField] private Button boton;

    [Header("Sonidos")]
    [SerializeField] private bool reproducirHoverSelected = true;
    [SerializeField] private bool reproducirPulsar = true;

    [Header("Selección inicial")]
    [Tooltip("Si está activado, no suena cuando el menú selecciona automáticamente el primer botón al abrirse.")]
    [SerializeField] private bool silenciarSeleccionInicial = false;

    [SerializeField, Min(0f)] private float tiempoSilencioInicial = 0.08f;

    private float tiempoActivacion;
    private float ultimoHoverLocal = -999f;
    private const float INTERVALO_LOCAL_HOVER = 0.04f;

    private void Awake()
    {
        BuscarBoton();
    }

    private void OnEnable()
    {
        BuscarBoton();
        tiempoActivacion = Time.unscaledTime;
        ultimoHoverLocal = -999f;
    }

    private void Reset()
    {
        BuscarBoton();
    }

    private void BuscarBoton()
    {
        if (boton == null)
            boton = GetComponent<Button>();
    }

    private bool BotonDisponible()
    {
        return boton != null && boton.interactable && gameObject.activeInHierarchy;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!reproducirHoverSelected || !BotonDisponible())
            return;

        // El hover del ratón solo suena en modo teclado/ratón.
        // Así no se duplica con OnSelect cuando se usa mando o navegación por teclado.
        if (InputDetector.ModoActual != InputDetector.ModoEntrada.TecladoRaton)
            return;

        ReproducirHoverConProteccion();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!reproducirHoverSelected || !BotonDisponible())
            return;

        // OnSelect solo genera sonido cuando la selección se está usando realmente
        // con mando o con WASD/flechas.
        if (!InputDetector.DebeMostrarSeleccionUI)
            return;

        if (silenciarSeleccionInicial &&
            Time.unscaledTime - tiempoActivacion < tiempoSilencioInicial)
        {
            return;
        }

        ReproducirHoverConProteccion();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!reproducirPulsar || !BotonDisponible())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        SonidosUIManager.ReproducirPulsar();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!reproducirPulsar || !BotonDisponible())
            return;

        SonidosUIManager.ReproducirPulsar();
    }

    private void ReproducirHoverConProteccion()
    {
        if (Time.unscaledTime - ultimoHoverLocal < INTERVALO_LOCAL_HOVER)
            return;

        ultimoHoverLocal = Time.unscaledTime;
        SonidosUIManager.ReproducirHoverSelected();
    }

    public void Configurar(bool usarHoverSelected, bool usarPulsar, bool silenciarPrimeraSeleccion)
    {
        reproducirHoverSelected = usarHoverSelected;
        reproducirPulsar = usarPulsar;
        silenciarSeleccionInicial = silenciarPrimeraSeleccion;
    }
}
