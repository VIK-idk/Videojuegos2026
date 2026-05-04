using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// ====================
// STRIKES
// ====================
public class StrikeManager : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private int maxStrikes = 3;
    [SerializeField] private string lobbySceneName = "Lobby";

    [Header("UI de strikes")]
    [SerializeField] private Image[] strikeImages;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [SerializeField] private Color activeColor = new Color(1f, 0f, 0f, 1f);

    [Header("Debug / Pruebas")]
    [SerializeField] private bool useDebugStrikes = true;
    [SerializeField][Range(0, 3)] private int debugStrikes = 0;

    private int currentStrikes = 0;
    private int lastDebugStrikes = -1;

    private GestorProgresoJugador gestorProgresoJugador;

    private void Start()
    {
        gestorProgresoJugador = FindFirstObjectByType<GestorProgresoJugador>();

        currentStrikes = 0;
        UpdateStrikeUI();
        lastDebugStrikes = debugStrikes;

        if (useDebugStrikes)
        {
            SetStrikes(debugStrikes);
        }
    }

    private void Update()
    {
        if (!useDebugStrikes)
            return;

        debugStrikes = Mathf.Clamp(debugStrikes, 0, maxStrikes);

        if (debugStrikes != lastDebugStrikes)
        {
            lastDebugStrikes = debugStrikes;
            SetStrikes(debugStrikes);
        }
    }

    // ====================
    // SUMAR
    // ====================
    public void AddStrike(bool cargarLobbySiPierde = true)
    {
        currentStrikes++;
        UpdateStrikeUI();

        if (cargarLobbySiPierde)
        {
            CheckGameOver();
        }
    }

    // ====================
    // ASIGNAR
    // ====================
    public void SetStrikes(int amount)
    {
        currentStrikes = Mathf.Clamp(amount, 0, maxStrikes);
        UpdateStrikeUI();
        CheckGameOver();
    }

    // ====================
    // REINICIAR
    // ====================
    public void ResetStrikes()
    {
        currentStrikes = 0;

        if (useDebugStrikes)
        {
            debugStrikes = 0;
            lastDebugStrikes = 0;
        }

        UpdateStrikeUI();
    }

    // ====================
    // HELPERS
    // ====================
    public int GetCurrentStrikes()
    {
        return currentStrikes;
    }

    public int GetMaxStrikes()
    {
        return maxStrikes;
    }

    public void IrALobby()
    {
        if (gestorProgresoJugador == null)
            gestorProgresoJugador = FindFirstObjectByType<GestorProgresoJugador>();

        if (gestorProgresoJugador != null)
        {
            gestorProgresoJugador.RegistrarIntentoActual();
        }

        ResetStrikes();
        SceneManager.LoadScene(lobbySceneName);
    }

    // ====================
    // UI
    // ====================
    private void UpdateStrikeUI()
    {
        if (strikeImages == null || strikeImages.Length == 0)
            return;

        for (int i = 0; i < strikeImages.Length; i++)
        {
            if (strikeImages[i] == null)
                continue;

            if (i < currentStrikes)
                strikeImages[i].color = activeColor;
            else
                strikeImages[i].color = inactiveColor;
        }
    }

    // ====================
    // FIN PARTIDA
    // ====================
    private void CheckGameOver()
    {
        if (currentStrikes >= maxStrikes)
        {
            if (gestorProgresoJugador == null)
                gestorProgresoJugador = FindFirstObjectByType<GestorProgresoJugador>();

            if (gestorProgresoJugador != null)
            {
                gestorProgresoJugador.RegistrarIntentoActual();
            }

            ResetStrikes();
            SceneManager.LoadScene(lobbySceneName);
        }
    }

    public bool RemoveStrike()
    {
        if (currentStrikes <= 0)
            return false;

        currentStrikes--;

        if (useDebugStrikes)
        {
            debugStrikes = currentStrikes;
            lastDebugStrikes = currentStrikes;
        }

        UpdateStrikeUI();
        return true;
    }
    public IEnumerator ParpadearStrikeDemo(float duracion)
    {
        if (strikeImages == null || strikeImages.Length == 0)
            yield break;

        float tiempo = 0f;
        float intervalo = 0.25f;
        bool rojo = false;

        while (tiempo < duracion)
        {
            rojo = !rojo;

            if (strikeImages[0] != null)
                strikeImages[0].color = rojo ? activeColor : inactiveColor;

            yield return new WaitForSeconds(intervalo);
            tiempo += intervalo;
        }

        UpdateStrikeUI();
    }
}