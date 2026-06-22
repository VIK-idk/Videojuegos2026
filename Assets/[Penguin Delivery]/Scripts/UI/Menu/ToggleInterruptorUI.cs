using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleInterruptorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Toggle")]
    [SerializeField] private Toggle toggle;

    [Header("Fondo")]
    [SerializeField] private Image imagenFondo;
    [SerializeField] private Sprite spriteFondoApagado;
    [SerializeField] private Sprite spriteFondoEncendido;

    [Header("Bolita")]
    [SerializeField] private RectTransform bolita;
    [SerializeField] private Image imagenBolita;
    [SerializeField] private Sprite spriteBolitaNormal;
    [SerializeField] private Sprite spriteBolitaHover;

    [Header("Posiciones bolita")]
    [SerializeField] private Vector2 posicionApagado = new Vector2(-17f, 0f);
    [SerializeField] private Vector2 posicionEncendido = new Vector2(17f, 0f);

    [Header("Hover / Selected")]
    [SerializeField] private Vector3 escalaNormal = Vector3.one;
    [SerializeField] private Vector3 escalaHover = new Vector3(1.12f, 1.12f, 1f);

    [Header("Animacion")]
    [SerializeField] private float velocidadMovimiento = 15f;
    [SerializeField] private float velocidadHover = 12f;

    [Header("Sonido")]
    [SerializeField] private bool reproducirSonidoAlCambiar = true;

    private Vector2 posicionObjetivo;
    private Vector3 escalaObjetivo;

    private bool mouseEncima = false;
    private bool seleccionado = false;
    private bool sonidoHabilitado = false;

    private void Awake()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();

        if (imagenBolita == null && bolita != null)
            imagenBolita = bolita.GetComponent<Image>();

        if (toggle != null)
            toggle.onValueChanged.AddListener(AlCambiarValorToggle);

        RefrescarVisual();

        // Evita que suene al inicializar el menú o cargar el valor guardado.
        sonidoHabilitado = true;
    }

    private void OnEnable()
    {
        RefrescarVisual();
    }

    private void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(AlCambiarValorToggle);
    }

    private void Update()
    {
        MoverBolita();
    }

    public void RefrescarVisual()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();

        if (toggle == null)
            return;

        ActualizarEstadoToggle(toggle.isOn);
        ActualizarEstadoInteractivo();

        if (bolita != null)
            bolita.anchoredPosition = posicionObjetivo;
    }

    private void AlCambiarValorToggle(bool encendido)
    {
        ActualizarEstadoToggle(encendido);

        if (sonidoHabilitado && reproducirSonidoAlCambiar && isActiveAndEnabled)
            SonidosUIManager.ReproducirToggle();
    }

    private void ActualizarEstadoToggle(bool encendido)
    {
        posicionObjetivo = encendido ? posicionEncendido : posicionApagado;

        if (imagenFondo != null)
        {
            imagenFondo.sprite = encendido ? spriteFondoEncendido : spriteFondoApagado;
            imagenFondo.color = Color.white;
        }
    }

    private void ActualizarEstadoInteractivo()
    {
        bool activo = mouseEncima || seleccionado;

        escalaObjetivo = activo ? escalaHover : escalaNormal;

        if (imagenBolita != null && spriteBolitaNormal != null && spriteBolitaHover != null)
            imagenBolita.sprite = activo ? spriteBolitaHover : spriteBolitaNormal;
    }

    private void MoverBolita()
    {
        if (bolita == null)
            return;

        bolita.anchoredPosition = Vector2.Lerp(
            bolita.anchoredPosition,
            posicionObjetivo,
            velocidadMovimiento * Time.unscaledDeltaTime
        );

        bolita.localScale = Vector3.Lerp(
            bolita.localScale,
            escalaObjetivo,
            velocidadHover * Time.unscaledDeltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEncima = true;
        ActualizarEstadoInteractivo();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEncima = false;
        ActualizarEstadoInteractivo();
    }

    public void OnSelect(BaseEventData eventData)
    {
        seleccionado = true;
        ActualizarEstadoInteractivo();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        seleccionado = false;
        ActualizarEstadoInteractivo();
    }
}
