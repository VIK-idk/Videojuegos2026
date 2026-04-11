using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoPuntos;

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

    public void CompletarEncargo()
    {
        puntos += puntosPorEncargo;
        ActualizarUI();
    }

    void ActualizarUI()
    {
        textoPuntos.text = "Puntos: " + puntos;
    }
}