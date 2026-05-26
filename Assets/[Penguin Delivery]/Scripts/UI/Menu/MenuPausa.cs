using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    public GameObject menuPausa;
    public GameObject menuOpciones;

    [Header("UI Mando")]
    [SerializeField] private GameObject primerBotonPausa;
    [SerializeField] private GameObject primerBotonOpciones;

    [SerializeField] private string escenaPrincipal;
    [SerializeField] private TiendaUIController tiendaUIController;

    private bool isGamePaused = false;

    void Update()
    {
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
            Time.timeScale = 0;

            if (menuPausa != null)
                menuPausa.SetActive(true);

            if (menuOpciones != null)
                menuOpciones.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            SeleccionarObjeto(primerBotonPausa);
            StartCoroutine(SeleccionarAlFinalDelFrame(primerBotonPausa));
        }
        else
        {
            Time.timeScale = 1;

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
    }

    public void Continuar()
    {
        isGamePaused = false;
        PauseGame();
    }

    public void AbrirOpciones()
    {
        if (!isGamePaused)
            isGamePaused = true;

        if (menuPausa != null)
            menuPausa.SetActive(false);

        if (menuOpciones != null)
            menuOpciones.SetActive(true);

        SeleccionarObjeto(primerBotonOpciones);
        StartCoroutine(SeleccionarAlFinalDelFrame(primerBotonOpciones));
    }

    public void VolverMenuPausa()
    {
        if (menuOpciones != null)
            menuOpciones.SetActive(false);

        if (menuPausa != null)
            menuPausa.SetActive(true);

        SeleccionarObjeto(primerBotonPausa);
        StartCoroutine(SeleccionarAlFinalDelFrame(primerBotonPausa));
    }

    public void Salir()
    {
        Time.timeScale = 1;
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
