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
    [SerializeField] private float esperaPrimerEncargo = 3f;
    [SerializeField] private float tiempoMensajeRecolecta = 3f;

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
    private bool esperandoPrimerEncargo = false;
    // ====================
    // Debug y dificultad (solo para pruebas)
    // ====================
    [SerializeField] private GestorProgresoJugador gestorProgresoJugador;
    [Header("Debug")]
    [SerializeField] private bool permitirDebugTiempo = true;
    [SerializeField] private KeyCode teclaTiempoRapido = KeyCode.K; // esto es unicamente para bajar el tiempo restante y probar la parte de fallo de encargos
    [SerializeField] private float tiempoDebugForzado = 1f;

    [Header("Dificultad")]
    [SerializeField] private int pecesMinimosTotales = 5;
    [SerializeField] private int pecesMaximosTotales = 10;
    [SerializeField] private float tiempoMinimoEncargo = 15f;
    [SerializeField] private float tiempoMaximoEncargo = 24f;
    // ====================
    // INICIO
    // ====================
    private void Start()
    {
        encargoActual = null;
        tiempoRestante = 0f;

        pecesRosasActuales = 0;
        pecesAmarillosActuales = 0;
        pecesVerdesActuales = 0;

        sistemaIniciado = false;
        encargoTerminado = false;
        esperandoPrimerEncargo = false;

        if (uiEncargo != null)
            uiEncargo.OcultarInstantaneo();

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();
        }

        if (gestorProgresoJugador == null)
        {
            gestorProgresoJugador = FindFirstObjectByType<GestorProgresoJugador>();
        }

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

        if (esperandoPrimerEncargo)
            return;

        if (encargoActual == null)
            return;

        if (encargoTerminado)
            return;

        if (permitirDebugTiempo && Input.GetKeyDown(teclaTiempoRapido))
        {
            if (encargoActual != null && !encargoTerminado)
            {
                tiempoRestante = tiempoDebugForzado;

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
        }

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
        esperandoPrimerEncargo = true;

        StartCoroutine(EsperarPrimerEncargo());
    }

    // ====================
    // PRIMER ENCARGO
    // ====================
    private IEnumerator EsperarPrimerEncargo()
    {
        if (uiEstado != null)
        {
            uiEstado.MostrarRecolecta(tiempoMensajeRecolecta);
        }

        yield return new WaitForSeconds(esperaPrimerEncargo);

        esperandoPrimerEncargo = false;
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

        while (suma < pecesMinimosTotales || suma > pecesMaximosTotales)
        {
            nuevo.pecesRosas = Random.Range(0, 6);
            nuevo.pecesAmarillos = Random.Range(0, 6);
            nuevo.pecesVerdes = Random.Range(0, 6);

            suma = nuevo.pecesRosas + nuevo.pecesAmarillos + nuevo.pecesVerdes;
        }

        nuevo.tiempoLimite = Random.Range(tiempoMinimoEncargo, tiempoMaximoEncargo);

        return nuevo;
    }

    // ====================
    // REGISTRAR PEZ
    // ====================
    public void RegistrarPezRecogido(ColorPez color, int cantidad)
    {
        if (!sistemaIniciado)
            return;

        if (encargoActual == null)
            return;

        if (!encargoActual.enProceso)
            return;

        if (encargoTerminado)
            return;

        if (cantidad < 1)
            cantidad = 1;

        bool seCompletoUnColor = false;
        ColorPez colorCompletado = color;

        if (color == ColorPez.Rosa)
        {
            int antes = pecesRosasActuales;
            pecesRosasActuales = Mathf.Min(pecesRosasActuales + cantidad, encargoActual.pecesRosas);

            if (antes < encargoActual.pecesRosas && pecesRosasActuales >= encargoActual.pecesRosas)
                seCompletoUnColor = true;
        }
        else if (color == ColorPez.Amarillo)
        {
            int antes = pecesAmarillosActuales;
            pecesAmarillosActuales = Mathf.Min(pecesAmarillosActuales + cantidad, encargoActual.pecesAmarillos);

            if (antes < encargoActual.pecesAmarillos && pecesAmarillosActuales >= encargoActual.pecesAmarillos)
                seCompletoUnColor = true;
        }
        else if (color == ColorPez.Verde)
        {
            int antes = pecesVerdesActuales;
            pecesVerdesActuales = Mathf.Min(pecesVerdesActuales + cantidad, encargoActual.pecesVerdes);

            if (antes < encargoActual.pecesVerdes && pecesVerdesActuales >= encargoActual.pecesVerdes)
                seCompletoUnColor = true;
        }

        if (seCompletoUnColor)
        {
            if (pecesManager != null)
            {
                pecesManager.DesactivarPecesActivosDeColor(colorCompletado);
            }

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
        if (gestorProgresoJugador != null)
        {
            gestorProgresoJugador.DarMonedasPorEncargo();
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

        bool ultimoStrike = false;

        if (strikeManager != null)
        {
            ultimoStrike = strikeManager.GetCurrentStrikes() + 1 >= strikeManager.GetMaxStrikes();
            strikeManager.AddStrike(false);
        }

        if (uiEstado != null)
        {
            if (ultimoStrike)
            {
                uiEstado.MostrarMensajePersonalizado(
                    "ENCARGO FALLIDO\nVuelve a tu celda a descansar",
                    Color.red,
                    2f
                );
            }
            else
            {
                uiEstado.MostrarFallado();
            }
        }

        if (ultimoStrike)
        {
            StartCoroutine(EsperarYVolverALobby());
        }
        else
        {
            StartCoroutine(EsperarYSiguiente());
        }
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

    private IEnumerator EsperarYVolverALobby()
    {
        yield return new WaitForSeconds(2f);

        if (uiEncargo != null)
        {
            uiEncargo.Ocultar();
        }

        if (strikeManager != null)
        {
            strikeManager.IrALobby();
        }
    }
}