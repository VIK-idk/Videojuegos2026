using System.Collections.Generic;
using UnityEngine;

// ====================
// MANAGER PECES TEST
// ====================
public class PecesTestManager : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private string fishTag = "Orbe";
    [SerializeField] private int porcentajeMinimoActivos = 65;

    [Header("Puntos")]
    [SerializeField] private bool sumarPuntosAlRecoger = false;
    [SerializeField] private int puntosPorPez = 5;

    [Header("Colores activos")]
    [SerializeField] private bool rosaActivo = true;
    [SerializeField] private bool amarilloActivo = false;
    [SerializeField] private bool verdeActivo = false;

    private Pez[] todosLosPeces;
    private GameManager gm;
    private GestorEncargosTest gestorEncargos;
    private HabilidadesManager habilidadesManager;

    private bool ultimoRosaActivo;
    private bool ultimoAmarilloActivo;
    private bool ultimoVerdeActivo;

    private float multiplicadorRecogidaActual = 1f;



    private void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
        gestorEncargos = FindFirstObjectByType<GestorEncargosTest>();
        habilidadesManager = FindFirstObjectByType<HabilidadesManager>();

        BuscarPeces();
        SetMultiplicadorRecogida(1f);
        ActivarPecesAleatorios();

        ultimoRosaActivo = rosaActivo;
        ultimoAmarilloActivo = amarilloActivo;
        ultimoVerdeActivo = verdeActivo;
    }

    private void Update()
    {
        if (rosaActivo != ultimoRosaActivo ||
            amarilloActivo != ultimoAmarilloActivo ||
            verdeActivo != ultimoVerdeActivo)
        {
            ultimoRosaActivo = rosaActivo;
            ultimoAmarilloActivo = amarilloActivo;
            ultimoVerdeActivo = verdeActivo;

            ActualizarPecesActivos();
        }
    }

    public void ReiniciarTodosLosPeces()
    {
        if (todosLosPeces == null || todosLosPeces.Length == 0)
            return;

        for (int i = 0; i < todosLosPeces.Length; i++)
        {
            if (todosLosPeces[i] != null)
                todosLosPeces[i].gameObject.SetActive(false);
        }
    }

    public void SetColoresActivos(bool rosa, bool amarillo, bool verde)
    {
        rosaActivo = rosa;
        amarilloActivo = amarillo;
        verdeActivo = verde;

        ultimoRosaActivo = rosaActivo;
        ultimoAmarilloActivo = amarilloActivo;
        ultimoVerdeActivo = verdeActivo;

        ActualizarPecesActivos();
    }

    public void SetMultiplicadorRecogida(float multiplicador)
    {
        multiplicadorRecogidaActual = multiplicador;

        if (todosLosPeces == null || todosLosPeces.Length == 0)
            return;

        for (int i = 0; i < todosLosPeces.Length; i++)
        {
            if (todosLosPeces[i] == null)
                continue;

            PezPickupTest pickup = todosLosPeces[i].GetComponentInChildren<PezPickupTest>(true);

            if (pickup != null)
                pickup.SetMultiplicadorRecogida(multiplicadorRecogidaActual);
        }
    }

    private void BuscarPeces()
    {
        GameObject[] objetos = GameObject.FindGameObjectsWithTag(fishTag);
        List<Pez> lista = new List<Pez>();

        for (int i = 0; i < objetos.Length; i++)
        {
            Pez pez = objetos[i].GetComponent<Pez>();

            if (pez != null)
                lista.Add(pez);
        }

        todosLosPeces = lista.ToArray();
    }

    public void ActivarPecesAleatorios()
    {
        if (todosLosPeces == null || todosLosPeces.Length == 0)
            return;

        int totalPeces = todosLosPeces.Length;
        int minimoActivos = (totalPeces * porcentajeMinimoActivos) / 100;

        if (minimoActivos < 1)
            minimoActivos = 1;

        int cantidadActivos = Random.Range(minimoActivos, totalPeces + 1);

        List<int> indicesDisponibles = new List<int>();

        for (int i = 0; i < totalPeces; i++)
        {
            indicesDisponibles.Add(i);
            todosLosPeces[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < cantidadActivos; i++)
        {
            int indiceRandomLista = Random.Range(0, indicesDisponibles.Count);
            int indicePez = indicesDisponibles[indiceRandomLista];

            indicesDisponibles.RemoveAt(indiceRandomLista);

            todosLosPeces[indicePez].ConfigurarPez(ObtenerColorAleatorio());
            todosLosPeces[indicePez].gameObject.SetActive(true);
        }

        SetMultiplicadorRecogida(multiplicadorRecogidaActual);
    }

    private void ActualizarPecesActivos()
    {
        if (todosLosPeces == null || todosLosPeces.Length == 0)
            return;

        for (int i = 0; i < todosLosPeces.Length; i++)
        {
            if (todosLosPeces[i] == null)
                continue;

            if (todosLosPeces[i].gameObject.activeSelf)
                todosLosPeces[i].ConfigurarPez(ObtenerColorAleatorio());
        }

        SetMultiplicadorRecogida(multiplicadorRecogidaActual);
    }

    public void ProcesarRecogida(Pez pezRecogido)
    {
        if (pezRecogido == null)
            return;

        if (gestorEncargos == null)
            gestorEncargos = FindFirstObjectByType<GestorEncargosTest>();

        if (habilidadesManager == null)
            habilidadesManager = FindFirstObjectByType<HabilidadesManager>();

        ColorPez colorRecogido = pezRecogido.GetColorPez();
        int cantidadPeces = 1;

        if (habilidadesManager != null)
            cantidadPeces = habilidadesManager.GetCantidadPecesPorRecogida();

        pezRecogido.gameObject.SetActive(false);

        if (gestorEncargos != null)
            gestorEncargos.RegistrarPezRecogido(colorRecogido, cantidadPeces);

        if (sumarPuntosAlRecoger && gm != null)
            gm.SumarPuntos(puntosPorPez);

        if (gestorEncargos != null && gestorEncargos.EstaEncargoTerminado())
            return;

        ReponerPezEnOtroSitio(pezRecogido);
    }

    private void ReponerPezEnOtroSitio(Pez pezRecogido)
    {
        List<Pez> pecesInactivos = new List<Pez>();

        for (int i = 0; i < todosLosPeces.Length; i++)
        {
            if (todosLosPeces[i] == null)
                continue;

            if (!todosLosPeces[i].gameObject.activeSelf && todosLosPeces[i] != pezRecogido)
                pecesInactivos.Add(todosLosPeces[i]);
        }

        if (pecesInactivos.Count == 0)
            return;

        int indice = Random.Range(0, pecesInactivos.Count);
        Pez nuevoPez = pecesInactivos[indice];

        nuevoPez.ConfigurarPez(ObtenerColorAleatorio());
        nuevoPez.gameObject.SetActive(true);

        PezPickupTest pickup = nuevoPez.GetComponentInChildren<PezPickupTest>(true);

        if (pickup != null)
            pickup.SetMultiplicadorRecogida(multiplicadorRecogidaActual);
    }

    private ColorPez ObtenerColorAleatorio()
    {
        List<ColorPez> coloresDisponibles = new List<ColorPez>();

        if (rosaActivo)
            coloresDisponibles.Add(ColorPez.Rosa);

        if (amarilloActivo)
            coloresDisponibles.Add(ColorPez.Amarillo);

        if (verdeActivo)
            coloresDisponibles.Add(ColorPez.Verde);

        if (coloresDisponibles.Count == 0)
            return ColorPez.Rosa;

        int indice = Random.Range(0, coloresDisponibles.Count);
        return coloresDisponibles[indice];
    }
}