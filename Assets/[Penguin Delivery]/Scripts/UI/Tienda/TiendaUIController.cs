using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TiendaUIController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelTienda;

    [Header("Controladores")]
    [SerializeField] private MonoBehaviour controladorCamara;
    [SerializeField] private MonoBehaviour controladorJugador;

    [Header("UI Mando")]
    [SerializeField] private GameObject primerBotonTienda;

    private bool tiendaOcultaPorPausa = false;

    public bool TiendaAbierta
    {
        get
        {
            return panelTienda != null && panelTienda.activeSelf;
        }
    }

    public bool TiendaOcultaPorPausa
    {
        get
        {
            return tiendaOcultaPorPausa;
        }
    }

    private void Update()
    {
        if (tiendaOcultaPorPausa)
            return;

        if (TiendaAbierta)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                CerrarTienda();
                return;
            }

            MantenerSeleccionTienda();
        }
    }

    public void AbrirTienda()
    {
        tiendaOcultaPorPausa = false;

        if (panelTienda != null)
            panelTienda.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controladorCamara != null)
            controladorCamara.enabled = false;

        if (controladorJugador != null)
            controladorJugador.enabled = false;

        SeleccionarPrimerBotonTienda();
    }

    public void CerrarTienda()
    {
        tiendaOcultaPorPausa = false;

        if (panelTienda != null)
            panelTienda.SetActive(false);

        if (controladorCamara != null)
            controladorCamara.enabled = true;

        if (controladorJugador != null)
            controladorJugador.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void OcultarInterfazPorPausa()
    {
        if (!TiendaAbierta)
            return;

        tiendaOcultaPorPausa = true;

        if (panelTienda != null)
            panelTienda.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void RestaurarInterfazTrasPausa()
    {
        if (!tiendaOcultaPorPausa)
            return;

        tiendaOcultaPorPausa = false;

        if (panelTienda != null)
            panelTienda.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SeleccionarPrimerBotonTienda();
    }

    private void MantenerSeleccionTienda()
    {
        if (EventSystem.current == null)
            return;

        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        SeleccionarPrimerBotonTienda();
    }

    private void SeleccionarPrimerBotonTienda()
    {
        if (EventSystem.current == null || primerBotonTienda == null)
            return;

        if (!primerBotonTienda.activeInHierarchy)
            return;

        Selectable selectable = primerBotonTienda.GetComponent<Selectable>();

        if (selectable != null && !selectable.interactable)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBotonTienda);
    }
}