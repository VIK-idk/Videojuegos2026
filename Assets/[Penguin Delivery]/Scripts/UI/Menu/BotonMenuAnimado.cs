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

    [Header("Comportamiento al terminar")]
    [SerializeField] private bool restaurarSpriteAlFinal = true;

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

        if (imagenBoton != null && spriteNormal != null)
        {
            imagenBoton.sprite = spriteNormal;
        }

        if (boton != null)
        {
            boton.onClick.AddListener(ReproducirClick);
        }
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

        if (restaurarSpriteAlFinal)
        {
            ResetearVisual();
        }

        accionAlTerminar.Invoke();

        reproduciendoClick = false;
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
        if (reproduciendoClick)
            return;

        ResetearVisual();
    }

    private void ResetearVisual()
    {
        escalaObjetivo = escalaNormal;
        rectTransform.localScale = escalaNormal;

        if (imagenBoton != null)
        {
            imagenBoton.color = colorNormal;

            if (spriteNormal != null)
            {
                imagenBoton.sprite = spriteNormal;
            }
        }
    }

    public void ResetearVisualCompleto()
    {
        reproduciendoClick = false;
        ResetearVisual();
    }
}