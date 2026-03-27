using UnityEngine;
using TMPro; 

public class GameManager : MonoBehaviour
{
    
    public TextMeshProUGUI textoPuntos; 
    private int puntos = 0;

    void Start()
    {
        ActualizarInterfaz();
    }

    public void SumarPuntos(int cantidad)
    {
        puntos += cantidad;
        ActualizarInterfaz();
    }

    void ActualizarInterfaz()
    {
        textoPuntos.text = "Puntos: " + puntos;
    }
}
