using UnityEngine;

public class TiendaUIController : MonoBehaviour
{
    [SerializeField] private GameObject panelTienda;
    [SerializeField] private MonoBehaviour controladorCamara;
    [SerializeField] private MonoBehaviour controladorJugador;
    [SerializeField] private GameObject menuUI;



        void Update()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (menuUI.activeSelf)
            {
                menuUI.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
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
    }
}