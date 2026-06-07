using UnityEngine;
using UnityEngine.UI;

public class BotonHabilidadUI : MonoBehaviour
{
    [Header("Datos")]
    public Habilidad habilidad;
    public PanelHabilidadUI panelHabilidadUI;

    [Header("UI")]
    public Text textoPrecio;
    public Button boton;

    [Header("Orden visual")]
    [SerializeField] private bool ponerArribaMientrasSeleccionado = true;

    private int indiceOriginal;
    private bool seleccionado = false;
    private bool referenciasPreparadas = false;

    private void Awake()
    {
        PrepararReferencias();
    }

    private void OnEnable()
    {
        PrepararReferencias();

        seleccionado = false;

        if (boton != null)
            boton.interactable = true;

        ActualizarVisual();
    }

    private void OnDisable()
    {
        seleccionado = false;

        if (boton != null)
            boton.interactable = true;
    }

    private void OnDestroy()
    {
        if (boton != null)
            boton.onClick.RemoveListener(SeleccionarHabilidad);
    }

    private void PrepararReferencias()
    {
        if (referenciasPreparadas)
            return;

        if (boton == null)
            boton = GetComponentInChildren<Button>(true);

        if (boton != null)
        {
            boton.onClick.RemoveListener(SeleccionarHabilidad);
            boton.onClick.AddListener(SeleccionarHabilidad);
        }

        indiceOriginal = transform.GetSiblingIndex();

        referenciasPreparadas = true;
    }

    private void SeleccionarHabilidad()
    {
        if (panelHabilidadUI != null)
            panelHabilidadUI.MostrarHabilidad(habilidad, this);
    }

    public void SetSeleccionado(bool valor)
    {
        PrepararReferencias();

        seleccionado = valor;

        if (boton == null)
            return;

        if (seleccionado)
        {
            if (ponerArribaMientrasSeleccionado && gameObject.activeInHierarchy)
                transform.SetAsLastSibling();

            boton.interactable = false;
        }
        else
        {
            boton.interactable = true;

            if (ponerArribaMientrasSeleccionado && gameObject.activeInHierarchy && transform.parent != null)
            {
                int cantidadHermanos = transform.parent.childCount;
                int indiceSeguro = Mathf.Clamp(indiceOriginal, 0, Mathf.Max(0, cantidadHermanos - 1));
                transform.SetSiblingIndex(indiceSeguro);
            }
        }
    }

    public void ActualizarVisual()
    {
        if (habilidad == null || textoPrecio == null)
            return;

        textoPrecio.text = habilidad.Precio.ToString();
    }
}