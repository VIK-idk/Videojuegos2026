using UnityEngine;

public class TiendaUIController : MonoBehaviour
{
    [SerializeField] private GameObject panelTienda;
    [SerializeField] private MonoBehaviour controladorCamara;
    [SerializeField] private MonoBehaviour controladorJugador;

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