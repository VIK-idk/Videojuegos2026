using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Text textoPuntos;

    [SerializeField] private int puntosPorPez = 5;
    [SerializeField] private int puntosPorEncargo = 100;

    private int puntos = 0;

    void Start()
    {
        ActualizarUI();
    }

    public void SumarPez()
    {
        puntos += puntosPorPez;
        ActualizarUI();
    }

    public void SumarPuntos(int cantidad)
    {
        puntos += cantidad;
        ActualizarUI();
    }

    public void CompletarEncargo()
    {
        puntos += puntosPorEncargo;
        ActualizarUI();
    }

    void ActualizarUI()
    {
        if (textoPuntos != null)
        {
            textoPuntos.text = "Puntos: " + puntos;
        }
    }
}