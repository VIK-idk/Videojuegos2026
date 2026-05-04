using UnityEngine;
using UnityEngine.EventSystems;

public class TiendaUIController : MonoBehaviour
{
    [SerializeField] private GameObject panelTienda;
    [SerializeField] private MonoBehaviour controladorCamara;
    [SerializeField] private MonoBehaviour controladorJugador;

    [Header("UI Mando")]
    [SerializeField] private GameObject primerBotonTienda;

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            if (panelTienda != null && panelTienda.activeSelf)
            {
                CerrarTienda();
            }
        }


        if (panelTienda != null && panelTienda.activeSelf)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(primerBotonTienda);
            }
        }
    }

    public bool TiendaAbierta
    {
        get
        {
            return panelTienda != null && panelTienda.activeSelf;
        }
    }

    public void AbrirTienda()
    {
        if (panelTienda != null)
            panelTienda.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controladorCamara != null)
            controladorCamara.enabled = false;

        if (controladorJugador != null)
            controladorJugador.enabled = false;


        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBotonTienda);
    }

    public void CerrarTienda()
    {
        if (panelTienda != null)
            panelTienda.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controladorCamara != null)
            controladorCamara.enabled = true;

        if (controladorJugador != null)
            controladorJugador.enabled = true;

        // 🔥 limpiar selección
        EventSystem.current.SetSelectedGameObject(null);
    }
}