using UnityEngine;
using System.Collections;

public class GestorEncargos : MonoBehaviour
{
    [Header("Encargo actual")]
    [SerializeField] private Encargo encargoActual;

    [Header("Progreso")]
    [SerializeField] private int pecesAmarillosActual = 0;
    [SerializeField] private int pecesRosasActual = 0;
    [SerializeField] private int pecesVerdesActual = 0;

    [Header("Tiempo")]
    [SerializeField] private float tiempoRestante;

    [Header("Control")]
    [SerializeField] private bool sistemaIniciado = false;
    [SerializeField] private int encargosCompletados = 0;

    private UiEstadoEncargo uiEstado;
    private UIEncargo ui;

    void Start()
    {
        ui = FindObjectOfType<UIEncargo>();
        uiEstado = FindObjectOfType<UiEstadoEncargo>();


        if (ui != null)
            ui.OcultarInstantaneo();
    }

    void Update()
    {
        if (!sistemaIniciado) return;
        if (encargoActual == null) return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0)
        {
            FallarEncargo();
            return;
        }

        ComprobarEncargo();

        ui.ActualizarUI(encargoActual, tiempoRestante,
            pecesAmarillosActual, pecesRosasActual, pecesVerdesActual);
    }

    // 🚀 Se llama cuando el jugador se mueve por primera vez
    public void IniciarSistema()
    {
        if (sistemaIniciado) return;

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
        encargoActual.enProceso = false;
        encargoActual.completado = true;
        encargosCompletados++;
        uiEstado.MostrarCompletado();

        ui.ActualizarUI(encargoActual, tiempoRestante,
            pecesAmarillosActual, pecesRosasActual, pecesVerdesActual);

        StartCoroutine(EsperarYSiguiente());
    }

    void FallarEncargo()
    {
        encargoActual.enProceso = false;
        encargoActual.fallado = true;
        uiEstado.MostrarFallado();
        ui.ActualizarUI(encargoActual, tiempoRestante,
            pecesAmarillosActual, pecesRosasActual, pecesVerdesActual);

        StartCoroutine(EsperarYSiguiente());
    }

    IEnumerator EsperarYSiguiente()
    {
        yield return new WaitForSeconds(2f);

        ui.Ocultar();

        yield return new WaitForSeconds(0.5f);

        IniciarEncargo();
    }
}