using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("Estado UI")]
    private GameObject menuActual;
    private GameObject primerBotonActual;
    private bool uiActiva = false;

    [Header("Control del juego")]
    [SerializeField] private MonoBehaviour controladorCamara;
    [SerializeField] private MonoBehaviour controladorJugador;

    void Update()
    {
        if (!uiActiva) return;

        if (Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            CerrarMenu();
        }


        if (EventSystem.current.currentSelectedGameObject == null && primerBotonActual != null)
        {
            EventSystem.current.SetSelectedGameObject(primerBotonActual);
        }
    }

    public void AbrirMenu(GameObject menu, GameObject primerBoton)
    {
        menuActual = menu;
        primerBotonActual = primerBoton;

        if (menuActual != null)
            menuActual.SetActive(true);

        uiActiva = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controladorCamara != null)
            controladorCamara.enabled = false;

        if (controladorJugador != null)
            controladorJugador.enabled = false;


        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBotonActual);
    }


    public void CerrarMenu()
    {
        if (menuActual != null)
            menuActual.SetActive(false);

        uiActiva = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controladorCamara != null)
            controladorCamara.enabled = true;

        if (controladorJugador != null)
            controladorJugador.enabled = true;

        EventSystem.current.SetSelectedGameObject(null);
    }
}