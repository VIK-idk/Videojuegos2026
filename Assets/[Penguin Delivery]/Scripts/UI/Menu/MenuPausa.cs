using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject menuPausa;
    [SerializeField] private GameObject menuOpciones;

    [Header("Fondo blur")]
    [SerializeField] private GameObject fondoBlurPausa;

    [Header("HUD")]
    [SerializeField] private GameObject hudGameplay;

    [Header("Primeros botones")]
    [SerializeField] private GameObject primerBotonPausa;
    [SerializeField] private GameObject primerBotonOpciones;

    [Header("Escenas")]
    [SerializeField] private string escenaPrincipal = "MainMenu";

    [Header("Tienda")]
    [SerializeField] private TiendaUIController tiendaUIController;

    [Header("Tutorial / Testing")]
    [SerializeField] private KeyCode teclaResetTutorial = KeyCode.Y;
    [SerializeField] private bool cargarTutorialAlResetear = false;
    [SerializeField] private string escenaTutorial = "Tutorial";

    private bool isGamePaused = false;
    private bool tiendaEstabaAbiertaAntesDePausar = false;

    private void Start()
    {
        InicializarMenu();
    }

    private void Update()
    {
        GestionarResetTutorial();

        if (GameOverRetornoController.DerrotaActiva)
        {
            if (isGamePaused)
            {
                isGamePaused = false;
                DesactivarPausa();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton9) ||
            Input.GetKeyDown(KeyCode.P) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            AlternarPausa();
        }

        ActualizarSeleccionSegunEntrada();
    }

    private void InicializarMenu()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        tiendaEstabaAbiertaAntesDePausar = false;

        if (menuPausa != null)
            menuPausa.SetActive(false);

        if (menuOpciones != null)
            menuOpciones.SetActive(false);

        if (fondoBlurPausa != null)
            fondoBlurPausa.SetActive(false);

        if (hudGameplay != null)
            hudGameplay.SetActive(true);
    }

    private void GestionarResetTutorial()
    {
        if (!Input.GetKeyDown(teclaResetTutorial))
            return;

        TutorialEstado.Resetear();
        Debug.Log("Tutorial reseteado. La próxima vez se abrirá el tutorial.");

        if (cargarTutorialAlResetear)
        {
            Time.timeScale = 1f;
            SceneLoader.CargarEscena(escenaTutorial);
        }
    }

    private void AlternarPausa()
    {
        isGamePaused = !isGamePaused;
        PauseGame();
    }

    public void PauseGame()
    {
        if (isGamePaused)
            ActivarPausa();
        else
            DesactivarPausa();
    }

    private void ActivarPausa()
    {
        Time.timeScale = 0f;

        tiendaEstabaAbiertaAntesDePausar =
            tiendaUIController != null && tiendaUIController.TiendaAbierta;

        if (tiendaEstabaAbiertaAntesDePausar)
            tiendaUIController.OcultarInterfazPorPausa();

        if (hudGameplay != null)
            hudGameplay.SetActive(false);

        if (fondoBlurPausa != null)
            fondoBlurPausa.SetActive(true);

        if (menuPausa != null)
            menuPausa.SetActive(true);

        if (menuOpciones != null)
            menuOpciones.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        PrepararSeleccion(primerBotonPausa);
    }

    private void DesactivarPausa()
    {
        Time.timeScale = 1f;

        if (hudGameplay != null)
            hudGameplay.SetActive(true);

        if (fondoBlurPausa != null)
            fondoBlurPausa.SetActive(false);

        if (menuPausa != null)
            menuPausa.SetActive(false);

        if (menuOpciones != null)
            menuOpciones.SetActive(false);

        if (tiendaEstabaAbiertaAntesDePausar && tiendaUIController != null)
            tiendaUIController.RestaurarInterfazTrasPausa();

        bool tiendaAbiertaAhora =
            tiendaUIController != null && tiendaUIController.TiendaAbierta;

        if (!tiendaAbiertaAhora)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        tiendaEstabaAbiertaAntesDePausar = false;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void Continuar()
    {
        isGamePaused = false;
        DesactivarPausa();
    }

    public void AbrirOpciones()
    {
        isGamePaused = true;
        Time.timeScale = 0f;

        if (hudGameplay != null)
            hudGameplay.SetActive(false);

        if (fondoBlurPausa != null)
            fondoBlurPausa.SetActive(true);

        if (menuPausa != null)
            menuPausa.SetActive(false);

        if (menuOpciones != null)
            menuOpciones.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        PrepararSeleccion(primerBotonOpciones);
    }

    public void VolverMenuPausa()
    {
        isGamePaused = true;
        Time.timeScale = 0f;

        if (hudGameplay != null)
            hudGameplay.SetActive(false);

        if (fondoBlurPausa != null)
            fondoBlurPausa.SetActive(true);

        if (menuOpciones != null)
            menuOpciones.SetActive(false);

        if (menuPausa != null)
            menuPausa.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        PrepararSeleccion(primerBotonPausa);
    }

    public void Salir()
    {
        Time.timeScale = 1f;
        tiendaEstabaAbiertaAntesDePausar = false;

        if (hudGameplay != null)
            hudGameplay.SetActive(true);

        if (fondoBlurPausa != null)
            fondoBlurPausa.SetActive(false);

        if (menuPausa != null)
            menuPausa.SetActive(false);

        if (menuOpciones != null)
            menuOpciones.SetActive(false);

        SceneLoader.CargarEscena(escenaPrincipal);
    }

    private void ActualizarSeleccionSegunEntrada()
    {
        if (!isGamePaused || EventSystem.current == null)
            return;

        if (!InputDetector.DebeMostrarSeleccionUI)
        {
            if (EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);

            return;
        }

        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        if (menuOpciones != null && menuOpciones.activeInHierarchy)
            SeleccionarObjeto(primerBotonOpciones);
        else if (menuPausa != null && menuPausa.activeInHierarchy)
            SeleccionarObjeto(primerBotonPausa);
    }

    private void PrepararSeleccion(GameObject objeto)
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (InputDetector.DebeMostrarSeleccionUI)
            StartCoroutine(SeleccionarAlFinalDelFrame(objeto));
    }

    private void SeleccionarObjeto(GameObject objeto)
    {
        if (!InputDetector.DebeMostrarSeleccionUI)
            return;

        if (EventSystem.current == null || objeto == null)
            return;

        if (!objeto.activeInHierarchy)
            return;

        Selectable selectable = objeto.GetComponent<Selectable>();

        if (selectable != null && !selectable.interactable)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(objeto);
    }

    private IEnumerator SeleccionarAlFinalDelFrame(GameObject objeto)
    {
        yield return null;
        SeleccionarObjeto(objeto);
    }
}
