using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BotLeaderboardData
{
    public string nombre = "Bot";
    public int puntuacion = 100;
}

public class LeaderboardLobbyUI : MonoBehaviour
{
    [SerializeField] private Text textoLeaderboard;
    [SerializeField] private int maxEntradas = 10;
    [SerializeField] private string nombreJugador = "Tú";
    [SerializeField] private Color colorJugador = Color.yellow;

    [Header("Bots falsos")]
    [SerializeField] private List<BotLeaderboardData> bots = new List<BotLeaderboardData>();

    private class EntradaLeaderboard
    {
        public string nombre;
        public int puntuacion;
        public bool esJugador;
    }

    private void Start()
    {
        ActualizarLeaderboard();
        
    }

    private void OnEnable()
    {
        ActualizarLeaderboard();
    }

    public void ActualizarLeaderboard()
    {
        if (textoLeaderboard == null)
            return;

        textoLeaderboard.supportRichText = true;

        List<EntradaLeaderboard> entradas = new List<EntradaLeaderboard>();

        for (int i = 0; i < bots.Count; i++)
        {
            if (bots[i] == null)
                continue;

            entradas.Add(new EntradaLeaderboard
            {
                nombre = bots[i].nombre,
                puntuacion = bots[i].puntuacion,
                esJugador = false
            });
        }

        int mejorPuntuacionJugador = ProgresoJugador.GetMejorPuntuacion();

        entradas.Add(new EntradaLeaderboard
        {
            nombre = nombreJugador,
            puntuacion = mejorPuntuacionJugador,
            esJugador = true
        });

        entradas.Sort((a, b) => b.puntuacion.CompareTo(a.puntuacion));

        int cantidadMostrar = Mathf.Min(maxEntradas, entradas.Count);
        string colorHexJugador = ColorUtility.ToHtmlStringRGB(colorJugador);

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < cantidadMostrar; i++)
        {
            string linea = (i + 1) + ". " + entradas[i].nombre + ": " + entradas[i].puntuacion;

            if (entradas[i].esJugador)
                sb.AppendLine("<color=#" + colorHexJugador + ">" + linea + "</color>");
            else
                sb.AppendLine(linea);
        }

        textoLeaderboard.text = sb.ToString();
    }
}