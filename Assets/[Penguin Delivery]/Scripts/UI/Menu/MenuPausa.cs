using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    [Header("Menus")]
    public GameObject menuPausa;
    public GameObject menuOpciones;

    [Header("Fondo blur")]
    [SerializeField] private GameObject fondoBlurPausa;

    [Header("HUD")]
    [SerializeField] private GameObject hudGameplay;

    [Header("UI Mando")]
    [SerializeField] private GameObject primerBotonPausa;
    [SerializeField] private GameObject primerBotonOpciones;

    [Header("Escenas")]
    [SerializeField] private string escenaPrincipal;

    [Header("Tienda")]
    [SerializeField] private TiendaUIController tiendaUIController;

    [Header("Tutorial / Testing")]
    [SerializeField] private KeyCode teclaResetTutorial = KeyCode.Y;
    [SerializeField] private bool cargarTutorialAlResetear = false;
    [SerializeField] private string escenaTutorial = "Tutorial";

    private bool isGamePaused = false;

    private void Start()
    {
        if (menuPausa != null)
            menuPausa.SetActive(false);

        if (menuOpciones != null)
            menuOpciones.SetActive(false);

        if (fondoBlurPausa != null)
            fondoBlurPausa.SetActive(false);

        if (hudGameplay != null)
            hudGameplay.SetActive(true);
    }

    private void Update()
    {
        // TUTORIAL / TESTING
        if (Input.GetKeyDown(teclaResetTutorial))
        {
            TutorialEstado.Resetear();
            Debug.Log("Tutorial reseteado. La próxima vez se abrirá el tutorial.");

            if (cargarTutorialAlResetear)
            {
                Time.timeScale = 1f;
                SceneLoader.CargarEscena(escenaTutorial);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton9) ||
            Input.GetKeyDown(KeyCode.P) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            isGamePaused = !isGamePaused;
            PauseGame();
        }

        if (!isGamePaused || EventSystem.current == null)
            return;

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (menuOpciones != null && menuOpciones.activeInHierarchy)
            {
                SeleccionarObjeto(primerBotonOpciones);
            }
            else if (menuPausa != null && menuPausa.activeInHierarchy)
            {
                SeleccionarObjeto(primerBotonPausa);
            }
        }
    }

    public void PauseGame()
    {
        if (isGamePaused)
        {
            ActivarPausa();
        }
        else
        {
            DesactivarPausa();
        }
    }

    private void ActivarPausa()
    {
        Time.timeScale = 0f;

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

        SeleccionarObjeto(primerBotonPausa);
        StartCoroutine(SeleccionarAlFinalDelFrame(primerBotonPausa));
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

        bool tiendaAbierta = tiendaUIController != null && tiendaUIController.TiendaAbierta;

        if (!tiendaAbierta)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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

        SeleccionarObjeto(primerBotonOpciones);
        StartCoroutine(SeleccionarAlFinalDelFrame(primerBotonOpciones));
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

        SeleccionarObjeto(primerBotonPausa);
        StartCoroutine(SeleccionarAlFinalDelFrame(primerBotonPausa));
    }

    public void Salir()
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

        SceneLoader.CargarEscena(escenaPrincipal);
    }

    private void SeleccionarObjeto(GameObject objeto)
    {
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