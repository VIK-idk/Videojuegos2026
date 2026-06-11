using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Añade el sonido general de hover/selected a cualquier Selectable de opciones
/// (Toggle, Dropdown, etc.) sin cambiar su comportamiento original.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Selectable))]
public class SonidoControlOpcionesUI : MonoBehaviour,
    IPointerEnterHandler,
    ISelectHandler
{
    [Header("Control")]
    [SerializeField] private Selectable selectable;

    [Header("Sonido")]
    [SerializeField] private bool reproducirHoverSelected = true;
    [SerializeField, Min(0f)] private float intervaloLocal = 0.06f;

    private float ultimoSonido = -999f;

    private void Awake()
    {
        BuscarSelectable();
    }

    private void Reset()
    {
        BuscarSelectable();
    }

    private void OnEnable()
    {
        BuscarSelectable();
        ultimoSonido = -999f;
    }

    private void BuscarSelectable()
    {
        if (selectable == null)
            selectable = GetComponent<Selectable>();
    }

    private bool Disponible()
    {
        return selectable != null &&
               selectable.interactable &&
               gameObject.activeInHierarchy;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ReproducirHover();
    }

    public void OnSelect(BaseEventData eventData)
    {
        ReproducirHover();
    }

    private void ReproducirHover()
    {
        if (!reproducirHoverSelected || !Disponible())
            return;

        if (Time.unscaledTime - ultimoSonido < intervaloLocal)
            return;

        ultimoSonido = Time.unscaledTime;
        SonidosUIManager.ReproducirHoverSelected();
    }

    public void Configurar(bool usarHoverSelected)
    {
        reproducirHoverSelected = usarHoverSelected;
        BuscarSelectable();
    }
}
