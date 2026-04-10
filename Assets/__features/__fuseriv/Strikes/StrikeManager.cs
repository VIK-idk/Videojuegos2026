using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ====================
// STRIKES
// Controla los strikes, la UI y el cambio a la escena lobby
// ====================
public class StrikeManager : MonoBehaviour
{
    // ====================
    // AJUSTES
    // ====================
    [Header("Configuracion")]
    [SerializeField] private int maxStrikes = 3;
    [SerializeField] private string lobbySceneName = "LobbyTemporal";

    // ====================
    // UI
    // ====================
    [Header("UI de strikes")]
    [SerializeField] private Image[] strikeImages;

    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [SerializeField] private Color activeColor = new Color(1f, 0f, 0f, 1f);

    // ====================
    // DEBUG
    // ====================
    [Header("Debug / Pruebas")]
    [SerializeField] private bool useDebugStrikes = true;
    [SerializeField][Range(0, 3)] private int debugStrikes = 0;

    // ====================
    // VARIABLES
    // ====================
    private int currentStrikes = 0;
    private int lastDebugStrikes = -1;

    // ====================
    // INICIO
    // ====================
    private void Start()
    {
        currentStrikes = 0;
        UpdateStrikeUI();
        lastDebugStrikes = debugStrikes;

        // Si el modo debug está activo, aplica ese valor de prueba
        if (useDebugStrikes)
        {
            SetStrikes(debugStrikes);
        }
    }

    // ====================
    // UPDATE
    // ====================
    private void Update()
    {
        // Si el debug no está activado, no hace nada
        if (!useDebugStrikes)
            return;

        // Limita debugStrikes entre 0 y el máximo permitido
        debugStrikes = Mathf.Clamp(debugStrikes, 0, maxStrikes);

        // Si el valor del debug ha cambiado, actualiza los strikes
        if (debugStrikes != lastDebugStrikes)
        {
            lastDebugStrikes = debugStrikes;
            SetStrikes(debugStrikes);
        }
    }

    // ====================
    // SUMAR
    // ====================
    public void AddStrike()
    {
        currentStrikes++;
        UpdateStrikeUI();

        // Comprueba si ya llegó al máximo
        CheckGameOver();
    }

    // ====================
    // ASIGNAR
    // ====================
    public void SetStrikes(int amount)
    {
        // Asigna la cantidad de strikes, limitándola entre 0 y maxStrikes
        currentStrikes = Mathf.Clamp(amount, 0, maxStrikes);

        // Actualiza la UI
        UpdateStrikeUI();

        // Comprueba si ya llegó al máximo
        CheckGameOver();
    }

    // ====================
    // REINICIAR
    // ====================
    public void ResetStrikes()
    {
        // Pone los strikes actuales a 0
        currentStrikes = 0;

        // Si está en modo debug, también reinicia los valores de prueba
        if (useDebugStrikes)
        {
            debugStrikes = 0;
            lastDebugStrikes = 0;
        }

        UpdateStrikeUI();
    }

    // ====================
    // ACTUALIZAR UI
    // ====================
    private void UpdateStrikeUI()
    {
        // Si no hay imágenes asignadas, sale del método
        if (strikeImages == null || strikeImages.Length == 0)
            return;

        // Recorre todas las imágenes de strikes
        for (int i = 0; i < strikeImages.Length; i++)
        {
            // Si alguna imagen no existe, la salta
            if (strikeImages[i] == null)
                continue;

            // Si el índice es menor que los strikes actuales, la pone activa
            if (i < currentStrikes)
                strikeImages[i].color = activeColor;
            else
                // Si no, la deja inactiva
                strikeImages[i].color = inactiveColor;
        }
    }

    // ====================
    // FIN PARTIDA
    // ====================
    private void CheckGameOver()
    {
        // Si los strikes llegan o superan el máximo
        if (currentStrikes >= maxStrikes)
        {
            // Reinicia los strikes
            ResetStrikes();

            // Carga la escena del lobby
            SceneManager.LoadScene(lobbySceneName);
        }
    }
}