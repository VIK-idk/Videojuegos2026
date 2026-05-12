using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string nombreEscena = "Gameplay";

    [Header("Paneles")]
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelOpciones;

    [Header("UI Mando")]
    [SerializeField] private GameObject primerBotonMenu;
    [SerializeField] private GameObject primerBotonOpciones;

    void Start()
    {
        if (panelMenuPrincipal != null)
            panelMenuPrincipal.SetActive(true);

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        ResetearVisuales(panelMenuPrincipal);
        ResetearVisuales(panelOpciones);
        SeleccionarObjeto(primerBotonMenu);
    }

    void Update()
    {
        if (EventSystem.current == null)
            return;

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (panelOpciones != null && panelOpciones.activeInHierarchy)
            {
                SeleccionarObjeto(primerBotonOpciones);
            }
            else if (panelMenuPrincipal != null && panelMenuPrincipal.activeInHierarchy)
            {
                SeleccionarObjeto(primerBotonMenu);
            }
        }
    }

    public void Jugar()
    {
        SesionPartida.ReiniciarSesion();
        SceneManager.LoadScene(nombreEscena);
    }

    public void AbrirOpciones()
    {
        ResetearVisuales(panelMenuPrincipal);
        ResetearVisuales(panelOpciones);

        if (panelMenuPrincipal != null)
            panelMenuPrincipal.SetActive(false);

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
