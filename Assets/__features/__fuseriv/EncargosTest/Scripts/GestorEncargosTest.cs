using System.Collections;
using UnityEngine;

// ====================
// GESTOR ENCARGOS TEST
// ====================
public class GestorEncargosTest : MonoBehaviour
{
    // ====================
    // REFERENCIAS
    // ====================
    [Header("Managers")]
    [SerializeField] private PecesTestManager pecesManager;
    [SerializeField] private StrikeManager strikeManager;
    [SerializeField] private GameManager gameManager;

    [Header("UI")]
    [SerializeField] private UIEncargoLegacy uiEncargo;
    [SerializeField] private UIEstadoEncargoLegacy uiEstado;

    // ====================
    // AJUSTES
    // ====================
    [Header("Configuracion")]
    [SerializeField] private bool iniciarAutomaticamente = true;
    [SerializeField] private int puntosPorEncargo = 100;
    [SerializeField] private float esperaEntreEncargos = 2f;

    // ====================
    // ESTADO
    // ====================
    [Header("Estado actual")]
    [SerializeField] private EncargoData encargoActual;

    [SerializeField] private int pecesRosasActuales = 0;
    [SerializeField] private int pecesAmarillosActuales = 0;
    [SerializeField] private int pecesVerdesActuales = 0;

    [SerializeField] private float tiempoRestante = 0f;

    [SerializeField] private bool sistemaIniciado = false;

    private bool encargoTerminado = false;

    // ====================
    // INICIO
    // ====================
    private void Start()
    {
        if (uiEncargo != null)
            uiEncargo.OcultarInstantaneo();

        if (iniciarAutomaticamente)
        {
            IniciarSistema();
        }
    }

    // ====================
    // UPDATE
    // ====================
    private void Update()
    {
        if (!sistemaIniciado)
            return;

        if (encargoActual == null)
            return;

        if (encargoTerminado)
            return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            FallarEncargo();
            return;
        }

        if (uiEncargo != null)
        {
            uiEncargo.ActualizarUI(
                encargoActual,
                tiempoRestante,
                pecesRosasActuales,
                pecesAmarillosActuales,
                pecesVerdesActuales);
        }
    }

    // ====================
    // INICIAR
    // ====================
    public void IniciarSistema()
    {
        if (sistemaIniciado)
            return;

        sistemaIniciado = true;
        IniciarNuevoEncargo();
    }

    // ====================
    // TERMINADO
    // ====================
    public bool EstaEncargoTerminado()
    {
        return encargoTerminado;
    }

    // ====================
    // NUEVO ENCARGO
    // ====================
    private void IniciarNuevoEncargo()
    {
        encargoActual = GenerarEncargoAleatorio();

        encargoActual.enProceso = true;
        encargoActual.completado = false;
        encargoActual.fallado = false;

        pecesRosasActuales = 0;
        pecesAmarillosActuales = 0;
        pecesVerdesActuales = 0;

        tiempoRestante = encargoActual.tiempoLimite;
        encargoTerminado = false;

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();

            pecesManager.SetColoresActivos(
                encargoActual.pecesRosas > 0,
                encargoActual.pecesAmarillos > 0,
                encargoActual.pecesVerdes > 0);

            pecesManager.ActivarPecesAleatorios();
        }

        if (uiEncargo != null)
        {
            uiEncargo.Mostrar();
            uiEncargo.ActualizarUI(
                encargoActual,
                tiempoRestante,
                pecesRosasActuales,
                pecesAmarillosActuales,
                pecesVerdesActuales);
        }
    }

    // ====================
    // GENERAR
    // ====================
    private EncargoData GenerarEncargoAleatorio()
    {
        EncargoData nuevo = new EncargoData();

        int suma = 0;

        while (suma < 3 || suma > 8)
        {
            nuevo.pecesRosas = Random.Range(0, 5);
            nuevo.pecesAmarillos = Random.Range(0, 5);
            nuevo.pecesVerdes = Random.Range(0, 5);

            suma = nuevo.pecesRosas + nuevo.pecesAmarillos + nuevo.pecesVerdes;
        }

        nuevo.tiempoLimite = Random.Range(20f, 35f);

        return nuevo;
    }

    // ====================
    // REGISTRAR PEZ
    // ====================
    public void RegistrarPezRecogido(ColorPez color)
    {
        if (!sistemaIniciado)
            return;

        if (encargoActual == null)
            return;

        if (!encargoActual.enProceso)
            return;

        if (encargoTerminado)
            return;

        bool seCompletoUnColor = false;

        if (color == ColorPez.Rosa)
        {
            if (pecesRosasActuales < encargoActual.pecesRosas)
            {
                pecesRosasActuales++;

                if (pecesRosasActuales >= encargoActual.pecesRosas)
                {
                    seCompletoUnColor = true;
                }
            }
        }
        else if (color == ColorPez.Amarillo)
        {
            if (pecesAmarillosActuales < encargoActual.pecesAmarillos)
            {
                pecesAmarillosActuales++;

                if (pecesAmarillosActuales >= encargoActual.pecesAmarillos)
                {
                    seCompletoUnColor = true;
                }
            }
        }
        else if (color == ColorPez.Verde)
        {
            if (pecesVerdesActuales < encargoActual.pecesVerdes)
            {
                pecesVerdesActuales++;

                if (pecesVerdesActuales >= encargoActual.pecesVerdes)
                {
                    seCompletoUnColor = true;
                }
            }
        }

        if (seCompletoUnColor)
        {
            ActualizarColoresPendientes();
        }

        if (uiEncargo != null)
        {
            uiEncargo.ActualizarUI(
                encargoActual,
                tiempoRestante,
                pecesRosasActuales,
                pecesAmarillosActuales,
                pecesVerdesActuales);
        }

        ComprobarEncargo();
    }

    // ====================
    // PENDIENTES
    // ====================
    private void ActualizarColoresPendientes()
    {
        if (pecesManager == null || encargoActual == null)
            return;

        bool rosaPendiente = pecesRosasActuales < encargoActual.pecesRosas;
        bool amarilloPendiente = pecesAmarillosActuales < encargoActual.pecesAmarillos;
        bool verdePendiente = pecesVerdesActuales < encargoActual.pecesVerdes;

        pecesManager.SetColoresActivos(rosaPendiente, amarilloPendiente, verdePendiente);
    }

    // ====================
    // COMPROBAR
    // ====================
    private void ComprobarEncargo()
    {
        if (pecesRosasActuales >= encargoActual.pecesRosas &&
            pecesAmarillosActuales >= encargoActual.pecesAmarillos &&
            pecesVerdesActuales >= encargoActual.pecesVerdes)
        {
            CompletarEncargo();
        }
    }

    // ====================
    // COMPLETAR
    // ====================
    private void CompletarEncargo()
    {
        if (encargoActual == null)
            return;

        if (encargoTerminado)
            return;

        encargoTerminado = true;

        encargoActual.enProceso = false;
        encargoActual.completado = true;

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();
        }

        if (gameManager != null)
        {
            gameManager.SumarPuntos(puntosPorEncargo);
        }

        if (uiEstado != null)
        {
            uiEstado.MostrarCompletado();
        }

        StartCoroutine(EsperarYSiguiente());
    }

    // ====================
    // FALLAR
    // ====================
    private void FallarEncargo()
    {
        if (encargoActual == null)
            return;

        if (encargoTerminado)
            return;

        encargoTerminado = true;

        encargoActual.enProceso = false;
        encargoActual.fallado = true;

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();
        }

        if (strikeManager != null)
        {
            strikeManager.AddStrike();
        }

        if (uiEstado != null)
        {
            uiEstado.MostrarFallado();
        }

        StartCoroutine(EsperarYSiguiente());
    }

    // ====================
    // ESPERAR
    // ====================
    private IEnumerator EsperarYSiguiente()
    {
        yield return new WaitForSeconds(esperaEntreEncargos);

        if (uiEncargo != null)
        {
            uiEncargo.Ocultar();
        }

        yield return new WaitForSeconds(0.5f);

        IniciarNuevoEncargo();
    }
}