using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BotonMenuAnimado : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Imagenes")]
    [SerializeField] private Image imagenBoton;
    [SerializeField] private Sprite spriteNormal;
    [SerializeField] private Sprite[] spritesExplosion;

    [Header("Animacion click")]
    [SerializeField] private float duracionExplosion = 0.07f;

    [Header("Hover / Selected")]
    [SerializeField] private float escalaSeleccionado = 1.08f;
    [SerializeField] private float velocidadEscala = 12f;
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorSeleccionado = new Color(0.75f, 0.9f, 1f, 1f);

    [Header("Accion al terminar")]
    [SerializeField] private UnityEvent accionAlTerminar;

    private Button boton;
    private RectTransform rectTransform;
    private Vector3 escalaNormal;
    private Vector3 escalaObjetivo;
    private bool reproduciendoClick = false;

    private void Awake()
    {
        boton = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();

        if (imagenBoton == null)
        {
            imagenBoton = GetComponent<Image>();
        }

        escalaNormal = rectTransform.localScale;
        escalaObjetivo = escalaNormal;

        ResetearVisualCompleto();

        if (boton != null)
        {
            boton.onClick.AddListener(ReproducirClick);
        }
    }

    private void OnEnable()
    {
        reproduciendoClick = false;
        ResetearVisualCompleto();
    }

    private void OnDisable()
    {
        reproduciendoClick = false;
        ResetearVisualCompleto();
    }

    private void Update()
    {
        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            escalaObjetivo,
            velocidadEscala * Time.unscaledDeltaTime
        );
    }

    private void ReproducirClick()
    {
        if (reproduciendoClick)
            return;

        StartCoroutine(AnimacionClick());
    }

    private IEnumerator AnimacionClick()
    {
        reproduciendoClick = true;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (spritesExplosion != null && spritesExplosion.Length > 0 && imagenBoton != null)
        {
            float tiempoPorSprite = duracionExplosion / spritesExplosion.Length;

            for (int i = 0; i < spritesExplosion.Length; i++)
            {
                imagenBoton.sprite = spritesExplosion[i];

                if (i < spritesExplosion.Length - 1)
                {
                    yield return new WaitForSecondsRealtime(tiempoPorSprite);
                }
            }
        }
        else
        {
            yield return new WaitForSecondsRealtime(duracionExplosion);
        }

        // Se resetea ANTES de ejecutar la accion, porque la accion puede ocultar/desactivar este boton.
        reproduciendoClick = false;
        ResetearVisualCompleto();

        accionAlTerminar.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SeleccionarVisualmente();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DeseleccionarVisualmente();
    }

    public void OnSelect(BaseEventData eventData)
    {
        SeleccionarVisualmente();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DeseleccionarVisualmente();
    }

    private void SeleccionarVisualmente()
    {
        if (reproduciendoClick)
            return;

        escalaObjetivo = escalaNormal * escalaSeleccionado;

        if (imagenBoton != null)
        {
            imagenBoton.color = colorSeleccionado;
        }
    }

    private void DeseleccionarVisualmente()
    {
        escalaObjetivo = escalaNormal;

        if (imagenBoton != null)
        {
            imagenBoton.color = colorNormal;
        }
    }

    public void ResetearVisualCompleto()
    {
        escalaObjetivo = escalaNormal;

        if (rectTransform != null)
        {
            rectTransform.localScale = escalaNormal;
        }

        if (imagenBoton != null)
        {
            imagenBoton.color = colorNormal;

            if (spriteNormal != null)
            {
                imagenBoton.sprite = spriteNormal;
            }
        }
    }
}
