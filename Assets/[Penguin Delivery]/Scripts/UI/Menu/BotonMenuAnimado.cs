using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BotonMenuAnimado : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler,
    ISubmitHandler
{
    [Header("Referencias")]
    [SerializeField] private Button boton;
    [SerializeField] private Image imagenBoton;
    [SerializeField] private RectTransform rectTransformBoton;

    [Header("Grieta")]
    [SerializeField] private Image imagenGrieta;
    [SerializeField] private RectTransform rectTransformGrieta;
    [SerializeField] private float duracionFadeGrieta = 0.2f;
    [SerializeField] private float escalaGrieta = 1f;

    [Header("Animacion escala")]
    [SerializeField] private float escalaHoverSelected = 1.06f;
    [SerializeField] private float escalaPressed = 0.96f;
    [SerializeField] private float velocidadEscala = 14f;

    [Header("Click")]
    [SerializeField] private float tiempoAntesDeAccion = 0.2f;

    [Header("Accion al terminar")]
    [SerializeField] private UnityEvent accionAlTerminar;

    private Vector3 escalaNormal;
    private Vector3 escalaObjetivo;

    private bool mouseEncima = false;
    private bool seleccionado = false;
    private bool presionado = false;
    private bool ejecutandoClick = false;

    private Sprite spriteNormalInicial;
    private Vector2 ultimaPosicionClickLocal = Vector2.zero;
    private Coroutine rutinaGrieta;

    private void Awake()
    {
        BuscarReferencias();
        InicializarVisual();
    }

    private void OnEnable()
    {
        ResetearVisualCompleto();
    }

    private void Update()
    {
        ActualizarEscalaSuave();
        MantenerSpritePressedSiHaceFalta();
    }

    private void BuscarReferencias()
    {
        if (boton == null)
            boton = GetComponent<Button>();

        if (imagenBoton == null)
            imagenBoton = GetComponent<Image>();

        if (rectTransformBoton == null)
            rectTransformBoton = GetComponent<RectTransform>();

        if (imagenGrieta != null && rectTransformGrieta == null)
            rectTransformGrieta = imagenGrieta.GetComponent<RectTransform>();
    }

    private void InicializarVisual()
    {
        if (rectTransformBoton != null)
        {
            escalaNormal = rectTransformBoton.localScale;
            escalaObjetivo = escalaNormal;
        }

        if (imagenBoton != null)
            spriteNormalInicial = imagenBoton.sprite;

        OcultarGrietaInstantaneo();
    }

    private void ActualizarEscalaSuave()
    {
        if (rectTransformBoton == null)
            return;

        rectTransformBoton.localScale = Vector3.Lerp(
            rectTransformBoton.localScale,
            escalaObjetivo,
            velocidadEscala * Time.unscaledDeltaTime
        );
    }

    private void MantenerSpritePressedSiHaceFalta()
    {
        if (!presionado && !ejecutandoClick)
            return;

        AplicarSpritePressed();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEncima = true;
        ActualizarEscala();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEncima = false;

        if (!ejecutandoClick)
            ActualizarEscala();
    }

    public void OnSelect(BaseEventData eventData)
    {
        seleccionado = true;
        ActualizarEscala();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        seleccionado = false;

        if (!ejecutandoClick)
            ActualizarEscala();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (boton != null && !boton.interactable)
            return;

        GuardarPosicionClick(eventData);

        presionado = true;
        ActualizarEscala();
        AplicarSpritePressed();

        MostrarGrietaFija();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        DesvanecerGrieta();
        StartCoroutine(ResetearPressedSiNoFueClick());
    }

    private IEnumerator ResetearPressedSiNoFueClick()
    {
        yield return null;

        if (ejecutandoClick)
            yield break;

        presionado = false;
        ActualizarEscala();
        RestaurarSpriteSegunEstado();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GuardarPosicionClick(eventData);
        EjecutarBoton();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (boton != null && !boton.interactable)
            return;

        ultimaPosicionClickLocal = Vector2.zero;

        presionado = true;
        ActualizarEscala();
        AplicarSpritePressed();

        MostrarGrietaFija();
        DesvanecerGrieta();

        EjecutarBoton();
    }

    private void EjecutarBoton()
    {
        if (ejecutandoClick)
            return;

        if (boton != null && !boton.interactable)
            return;

        StartCoroutine(SecuenciaClick());
    }

    private IEnumerator SecuenciaClick()
    {
        ejecutandoClick = true;
        presionado = true;

        ActualizarEscala();
        AplicarSpritePressed();

        yield return new WaitForSecondsRealtime(tiempoAntesDeAccion);

        // Antes de ejecutar la acción lo dejamos preparado.
        // Si la acción desactiva este botón, ya no intentamos lanzar ninguna coroutine después.
        presionado = false;
        ejecutandoClick = false;
        ActualizarEscala();
        RestaurarSpriteSegunEstado();

        accionAlTerminar?.Invoke();
    }

    private void GuardarPosicionClick(PointerEventData eventData)
    {
        if (rectTransformBoton == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransformBoton,
            eventData.position,
            eventData.pressEventCamera,
            out ultimaPosicionClickLocal
        );
    }

    private void ActualizarEscala()
    {
        if (rectTransformBoton == null)
            return;

        if (presionado || ejecutandoClick)
        {
            escalaObjetivo = escalaNormal * escalaPressed;
            return;
        }

        if (mouseEncima || seleccionado)
        {
            escalaObjetivo = escalaNormal * escalaHoverSelected;
            return;
        }

        escalaObjetivo = escalaNormal;
    }

    private void AplicarSpritePressed()
    {
        if (boton == null || imagenBoton == null)
            return;

        Sprite spritePressed = boton.spriteState.pressedSprite;

        if (spritePressed != null)
            imagenBoton.sprite = spritePressed;
    }

    private void RestaurarSpriteSegunEstado()
    {
        if (boton == null || imagenBoton == null)
            return;

        SpriteState estados = boton.spriteState;

        if (seleccionado && estados.selectedSprite != null)
        {
            imagenBoton.sprite = estados.selectedSprite;
            return;
        }

        if (mouseEncima && estados.highlightedSprite != null)
        {
            imagenBoton.sprite = estados.highlightedSprite;
            return;
        }

        if (spriteNormalInicial != null)
            imagenBoton.sprite = spriteNormalInicial;
    }

    private void MostrarGrietaFija()
    {
        if (imagenGrieta == null || rectTransformGrieta == null)
            return;

        if (rutinaGrieta != null)
        {
            StopCoroutine(rutinaGrieta);
            rutinaGrieta = null;
        }

        rectTransformGrieta.anchoredPosition = ultimaPosicionClickLocal;
        rectTransformGrieta.localScale = Vector3.one * escalaGrieta;

        imagenGrieta.gameObject.SetActive(true);

        Color color = imagenGrieta.color;
        color.a = 1f;
        imagenGrieta.color = color;
    }

    private void DesvanecerGrieta()
    {
        if (imagenGrieta == null)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        if (rutinaGrieta != null)
            StopCoroutine(rutinaGrieta);

        rutinaGrieta = StartCoroutine(FadeGrieta());
    }

    private IEnumerator FadeGrieta()
    {
        if (imagenGrieta == null)
            yield break;

        float tiempo = 0f;
        float alphaInicial = imagenGrieta.color.a;

        while (tiempo < duracionFadeGrieta)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = tiempo / duracionFadeGrieta;

            Color color = imagenGrieta.color;
            color.a = Mathf.Lerp(alphaInicial, 0f, t);
            imagenGrieta.color = color;

            yield return null;
        }

        OcultarGrietaInstantaneo();
    }

    private void OcultarGrietaInstantaneo()
    {
        if (imagenGrieta == null)
            return;

        Color color = imagenGrieta.color;
        color.a = 0f;
        imagenGrieta.color = color;

        imagenGrieta.gameObject.SetActive(false);
    }

    public void ResetearVisualCompleto()
    {
        ejecutandoClick = false;
        presionado = false;
        mouseEncima = false;
        seleccionado = false;

        if (rectTransformBoton != null)
        {
            rectTransformBoton.localScale = escalaNormal;
            escalaObjetivo = escalaNormal;
        }

        RestaurarSpriteSegunEstado();
        OcultarGrietaInstantaneo();
    }
}