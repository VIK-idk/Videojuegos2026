using System.Collections.Generic;
using UnityEngine;

// ====================
// MANAGER PECES TEST
// ====================
public class PecesTestManager : MonoBehaviour
{
    // ====================
    // SPAWN
    // ====================
    [Header("Spawn")]
    [SerializeField] private string fishTag = "Orbe";
    [SerializeField] private int porcentajeMinimoActivos = 65;

    // ====================
    // PUNTOS
    // ====================
    [Header("Puntos")]
    [SerializeField] private int puntosPorPez = 5;

    // ====================
    // COLORES CORRECTOS
    // ====================
    [Header("Colores correctos del test")]
    [SerializeField] private bool rosaActivo = true;
    [SerializeField] private bool amarilloActivo = false;
    [SerializeField] private bool verdeActivo = false;

    // ====================
    // VARIABLES
    // ====================
    private Pez[] todosLosPeces;
    private GameManager gm;

    private bool ultimoRosaActivo;
    private bool ultimoAmarilloActivo;
    private bool ultimoVerdeActivo;

    // ====================
    // INICIO
    // ====================
    private void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
        BuscarPeces();
        ActivarPecesAleatorios();

        ultimoRosaActivo = rosaActivo;
        ultimoAmarilloActivo = amarilloActivo;
        ultimoVerdeActivo = verdeActivo;
    }

    // ====================
    // UPDATE
    // ====================
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

    // ====================
    // BUSCAR
    // ====================
    private void BuscarPeces()
    {
        GameObject[] objetos = GameObject.FindGameObjectsWithTag(fishTag);
        List<Pez> lista = new List<Pez>();

        for (int i = 0; i < objetos.Length; i++)
        {
            Pez pez = objetos[i].GetComponent<Pez>();

            if (pez != null)
            {
                lista.Add(pez);
            }
        }

        todosLosPeces = lista.ToArray();
    }

    // ====================
    // ACTIVAR INICIAL
    // ====================
    public void ActivarPecesAleatorios()
    {
        if (todosLosPeces == null || todosLosPeces.Length == 0)
            return;

        int totalPeces = todosLosPeces.Length;
        int minimoActivos = (totalPeces * porcentajeMinimoActivos) / 100;

        if (minimoActivos < 1)
        {
            minimoActivos = 1;
        }

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
    }

    // ====================
    // ACTUALIZAR ACTIVOS
    // ====================
    private void ActualizarPecesActivos()
    {
        if (todosLosPeces == null || todosLosPeces.Length == 0)
            return;

        for (int i = 0; i < todosLosPeces.Length; i++)
        {
            if (todosLosPeces[i] == null)
                continue;

            if (todosLosPeces[i].gameObject.activeSelf)
            {
                todosLosPeces[i].ConfigurarPez(ObtenerColorAleatorio());
            }
        }
    }

    // ====================
    // RECOGER
    // ====================
    public void ProcesarRecogida(Pez pezRecogido)
    {
        if (pezRecogido == null)
            return;

        bool esCorrecto = EsColorCorrecto(pezRecogido.GetColorPez());

        if (esCorrecto)
        {
            Debug.Log("Pez correcto recogido. Color: " + pezRecogido.GetColorPez() + " | ID: " + pezRecogido.GetColorId());
        }
        else
        {
            Debug.Log("Pez incorrecto recogido. Color: " + pezRecogido.GetColorPez() + " | ID: " + pezRecogido.GetColorId());
        }

        if (gm != null)
        {
            gm.SumarPuntos(puntosPorPez);
        }

        pezRecogido.gameObject.SetActive(false);
        ReponerPezEnOtroSitio(pezRecogido);
    }

    // ====================
    // REPONER
    // ====================
    private void ReponerPezEnOtroSitio(Pez pezRecogido)
    {
        List<Pez> pecesInactivos = new List<Pez>();

        for (int i = 0; i < todosLosPeces.Length; i++)
        {
            if (todosLosPeces[i] == null)
                continue;

            if (!todosLosPeces[i].gameObject.activeSelf && todosLosPeces[i] != pezRecogido)
            {
                pecesInactivos.Add(todosLosPeces[i]);
            }
        }

        if (pecesInactivos.Count == 0)
            return;

        int indice = Random.Range(0, pecesInactivos.Count);
        Pez nuevoPez = pecesInactivos[indice];

        nuevoPez.ConfigurarPez(ObtenerColorAleatorio());
        nuevoPez.gameObject.SetActive(true);
    }

    // ====================
    // COLOR RANDOM
    // ====================
    private ColorPez ObtenerColorAleatorio()
    {
        List<ColorPez> coloresDisponibles = new List<ColorPez>();

        if (rosaActivo)
        {
            coloresDisponibles.Add(ColorPez.Rosa);
        }

        if (amarilloActivo)
        {
            coloresDisponibles.Add(ColorPez.Amarillo);
        }

        if (verdeActivo)
        {
            coloresDisponibles.Add(ColorPez.Verde);
        }

        if (coloresDisponibles.Count == 0)
        {
            return ColorPez.Rosa;
        }

        int indice = Random.Range(0, coloresDisponibles.Count);
        return coloresDisponibles[indice];
    }

    // ====================
    // COLOR CORRECTO
    // ====================
    public bool EsColorCorrecto(ColorPez color)
    {
        if (color == ColorPez.Rosa && rosaActivo) return true;
        if (color == ColorPez.Amarillo && amarilloActivo) return true;
        if (color == ColorPez.Verde && verdeActivo) return true;

        return false;
    }
}