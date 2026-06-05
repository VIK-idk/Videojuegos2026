using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // ====================
    // ESCENAS
    // ====================
    [Header("Escenas")]
    [SerializeField] private string nombreEscena = "Gameplay";
    [SerializeField] private string nombreEscenaTutorial = "Tutorial";

    // ====================
    // PANELES
    // ====================
    [Header("Paneles")]
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelOpciones;

    [Header("Fondo blur")]
    [SerializeField] private GameObject fondoBlurMenu;

    // ====================
    // UI MANDO
    // ====================
    [Header("UI Mando")]
    [SerializeField] private GameObject primerBotonMenu;
    [SerializeField] private GameObject primerBotonOpciones;

    // ====================
    // UNITY
    // ====================
    private void Start()
    {
        InicializarMenu();
    }

    private void Update()
    {
        MantenerSeleccionMando();
    }

    // ====================
    // INICIALIZAR
    // ====================
    private void InicializarMenu()
    {
        Time.timeScale = 1f;

        if (panelMenuPrincipal != null)
            panelMenuPrincipal.SetActive(true);

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        if (fondoBlurMenu != null)
            fondoBlurMenu.SetActive(false);

        ResetearVisuales(panelMenuPrincipal);
        ResetearVisuales(panelOpciones);

        SeleccionarObjeto(primerBotonMenu);
        StartCoroutine(SeleccionarAlFinalDelFrame(primerBotonMenu));
    }

    // ====================
    // BOTONES
    // ====================
    public void Jugar()
    {
        SesionPartida.ReiniciarSesion();

        if (TutorialEstado.EstaCompletado())
        {
            SceneLoader.CargarEscena(nombreEscena);
        }
        else
        {
            SceneLoader.CargarEscena(nombreEscenaTutorial);
        }
    }

    public void AbrirOpciones()
    {
        ResetearVisuales(panelMenuPrincipal);
        ResetearVisuales(panelOpciones);

        if (panelMenuPrincipal != null)
            panelMenuPrincipal.SetActive(false);

        if (fondoBlurMenu != null)
            fondoBlurMenu.SetActive(true);

        if (panelOpciones != null)
            panelOpciones.SetActive(true);

        SeleccionarObjeto(primerBotonOpciones);
        StartCoroutine(SeleccionarAlFinalDelFrame(primerBotonOpciones));
    }

    public void VolverMenuPrincipal()
    {
        ResetearVisuales(panelOpciones);
        ResetearVisuales(panelMenuPrincipal);

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        if (fondoBlurMenu != null)
            fondoBlurMenu.SetActive(false);

        if (panelMenuPrincipal != null)
            panelMenuPrincipal.SetActive(true);

        SeleccionarObjeto(primerBotonMenu);
        StartCoroutine(SeleccionarAlFinalDelFrame(primerBotonMenu));
    }

    public void Salir()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }

    // ====================
    // SELECCION MANDO
    // ====================
    private void MantenerSeleccionMando()
    {
        if (EventSystem.current == null)
            return;

        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        if (panelOpciones != null && panelOpciones.activeInHierarchy)
        {
            SeleccionarObjeto(primerBotonOpciones);
        }
        else if (panelMenuPrincipal != null && panelMenuPrincipal.activeInHierarchy)
        {
            SeleccionarObjeto(primerBotonMenu);
        }
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

    // ====================
    // VISUALES
    // ====================
    private void ResetearVisuales(GameObject panel)
    {
        if (panel == null)
            return;

        BotonMenuAnimado[] botones = panel.GetComponentsInChildren<BotonMenuAnimado>(true);

        for (int i = 0; i < botones.Length; i++)
        {
            botones[i].ResetearVisualCompleto();
        }
    }

    private void OnApplicationQuit()
    {
        SesionPartida.ReiniciarSesion();
    }
}