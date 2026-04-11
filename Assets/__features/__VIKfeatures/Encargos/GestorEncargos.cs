using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GestorEncargos : MonoBehaviour
{
    [SerializeField] private string nombreEscenaActiva;

    [SerializeField] private UIEncargo ui;
    [SerializeField] private UiEstadoEncargo uiEstado;

    [SerializeField] private Encargo encargoActual;

    [SerializeField] private int pecesAmarillosActual = 0;
    [SerializeField] private int pecesRosasActual = 0;
    [SerializeField] private int pecesVerdesActual = 0;

    [SerializeField] private float tiempoRestante;

    [SerializeField] private bool sistemaIniciado = false;
    [SerializeField] private int encargosCompletados = 0;

    private bool encargoTerminado = false;

    void Start()
    {
        if (SceneManager.GetActiveScene().name != nombreEscenaActiva)
        {
            gameObject.SetActive(false);
            return;
        }

        if (ui != null)
            ui.OcultarInstantaneo();
    }

    void Update()
    {
        if (!sistemaIniciado) return;
        if (encargoActual == null) return;
        if (encargoTerminado) return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0)
        {
            FallarEncargo();
            return;
        }

        ComprobarEncargo();

        if (ui != null)
        {
            ui.ActualizarUI(encargoActual, tiempoRestante,
                pecesAmarillosActual,
                pecesRosasActual,
                pecesVerdesActual);
        }
    }

    public void IniciarSistema()
    {
        if (sistemaIniciado) return;

        if (SceneManager.GetActiveScene().name != nombreEscenaActiva) return;

        sistemaIniciado = true;
        IniciarEncargo();
    }

    void IniciarEncargo()
    {
        encargoActual = GenerarEncargoRandom();

        encargoActual.enProceso = true;
        encargoActual.completado = false;
        encargoActual.fallado = false;

        tiempoRestante = encargoActual.tiempoLimite;

        pecesAmarillosActual = 0;
        pecesRosasActual = 0;
        pecesVerdesActual = 0;

        encargoTerminado = false;

        if (ui != null)
            ui.Mostrar();
    }

    Encargo GenerarEncargoRandom()
    {
        Encargo nuevo = new Encargo();

        int suma = 0;

        while (suma < 8)
        {
            nuevo.pecesAmarillos = Random.Range(0, 6);
            nuevo.pecesRosas = Random.Range(0, 6);
            nuevo.pecesVerdes = Random.Range(0, 6);

            suma = nuevo.pecesAmarillos + nuevo.pecesRosas + nuevo.pecesVerdes;
        }

        nuevo.tiempoLimite = Random.Range(20f, 40f);

        return nuevo;
    }

    public void SumarPez(string tipo)
    {
        if (encargoActual == null) return;
        if (!encargoActual.enProceso) return;

        if (tipo == "Amarillo") pecesAmarillosActual++;
        if (tipo == "Rosa") pecesRosasActual++;
        if (tipo == "Verde") pecesVerdesActual++;
    }

    void ComprobarEncargo()
    {
        if (pecesAmarillosActual >= encargoActual.pecesAmarillos &&
            pecesRosasActual >= encargoActual.pecesRosas &&
            pecesVerdesActual >= encargoActual.pecesVerdes)
        {
            CompletarEncargo();
        }
    }

    void CompletarEncargo()
    {
        if (encargoActual == null) return;
        if (encargoTerminado) return;

        encargoTerminado = true;

        encargoActual.enProceso = false;
        encargoActual.completado = true;
        encargosCompletados++;

        if (uiEstado != null)
            uiEstado.MostrarCompletado();

        StartCoroutine(EsperarYSiguiente());
    }

    void FallarEncargo()
    {
        if (encargoActual == null) return;
        if (encargoTerminado) return;

        encargoTerminado = true;

        encargoActual.enProceso = false;
        encargoActual.fallado = true;

        if (uiEstado != null)
            uiEstado.MostrarFallado();

        StartCoroutine(EsperarYSiguiente());
    }

    IEnumerator EsperarYSiguiente()
    {
        yield return new WaitForSeconds(2f);

        if (ui != null)
            ui.Ocultar();

        yield return new WaitForSeconds(0.5f);

        IniciarEncargo();
    }
}