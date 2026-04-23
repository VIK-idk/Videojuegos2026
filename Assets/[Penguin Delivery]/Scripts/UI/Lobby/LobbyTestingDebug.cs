using UnityEngine;

public class LobbyTestingDebug : MonoBehaviour
{
    [Header("SOLO TESTING")]
    [SerializeField] private bool permitirTestingLobby = true;
    // SOLO TESTING
    // Coma = quitar 30 monedas en el lobby
    [SerializeField] private KeyCode teclaRestarMonedas = KeyCode.Comma;

    // SOLO TESTING
    // J = reiniciar récord de puntos guardado
    [SerializeField] private KeyCode teclaReiniciarRecord = KeyCode.J;

    // SOLO TESTING
    // M = sumar monedas en la sesión actual
    [SerializeField] private KeyCode teclaSumarMonedas = KeyCode.M;

    // SOLO TESTING
    [SerializeField] private int monedasASumar = 30;

    [Header("Referencias")]
    [SerializeField] private LobbyMonedasUI lobbyMonedasUI;
    [SerializeField] private LeaderboardLobbyUI leaderboardLobbyUI;

    private void Start()
    {
        if (lobbyMonedasUI == null)
            lobbyMonedasUI = FindFirstObjectByType<LobbyMonedasUI>();

        if (leaderboardLobbyUI == null)
            leaderboardLobbyUI = FindFirstObjectByType<LeaderboardLobbyUI>();
    }

    private void Update()
    {
        if (!permitirTestingLobby)
            return;

        // ====================
        // SOLO TESTING
        // J = reinicia el récord de puntos / leaderboard del jugador
        // ====================
        if (Input.GetKeyDown(teclaReiniciarRecord))
        {
            ProgresoJugador.ResetPuntuaciones(); // SOLO TESTING

            if (leaderboardLobbyUI != null)
                leaderboardLobbyUI.ActualizarLeaderboard(); // SOLO TESTING
        }

        // ====================
        // SOLO TESTING
        // M = suma monedas en el lobby para probar la tienda
        // ====================
        if (Input.GetKeyDown(teclaSumarMonedas))
        {
            SesionPartida.monedas += monedasASumar; // SOLO TESTING

            if (lobbyMonedasUI != null)
                lobbyMonedasUI.ActualizarMonedas(); // SOLO TESTING
        }
        // ====================
        // SOLO TESTING
        // , = resta monedas en el lobby para probar compras y límites
        // ====================
        if (Input.GetKeyDown(teclaRestarMonedas))
        {
            SesionPartida.monedas -= monedasASumar; // SOLO TESTING

            if (SesionPartida.monedas < 0)
                SesionPartida.monedas = 0; // SOLO TESTING

            if (lobbyMonedasUI != null)
                lobbyMonedasUI.ActualizarMonedas(); // SOLO TESTING
        }
    }
}