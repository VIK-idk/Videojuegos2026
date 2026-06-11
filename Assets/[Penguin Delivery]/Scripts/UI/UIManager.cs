using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameObject menuActual;
    private GameObject primerBotonActual;
    private bool uiActiva = false;

    [Header("Control del juego")]
    [SerializeField] private MonoBehaviour controladorCamara;
    [SerializeField] private MonoBehaviour controladorJugador;

    [Header("Cerrar con mando")]
    [SerializeField] private KeyCode botonCerrarMando = KeyCode.JoystickButton1;

    private void Update()
    {
        if (!uiActiva)
            return;

        if (InputDetector.usandoMando && Input.GetKeyDown(botonCerrarMando))
        {
            CerrarMenu();
            return;
        }

        ActualizarSeleccionSegunEntrada();
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

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (InputDetector.DebeMostrarSeleccionUI)
            SeleccionarPrimerBoton();
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

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void ActualizarSeleccionSegunEntrada()
    {
        if (EventSystem.current == null)
            return;

        if (!InputDetector.DebeMostrarSeleccionUI)
        {
            if (EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);

            return;
        }

        if (EventSystem.current.currentSelectedGameObject == null)
            SeleccionarPrimerBoton();
    }

    private void SeleccionarPrimerBoton()
    {
        if (!InputDetector.DebeMostrarSeleccionUI)
            return;

        if (EventSystem.current == null || primerBotonActual == null)
            return;

        if (!primerBotonActual.activeInHierarchy)
            return;

        Selectable selectable = primerBotonActual.GetComponent<Selectable>();

        if (selectable != null && !selectable.interactable)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBotonActual);
    }
}
