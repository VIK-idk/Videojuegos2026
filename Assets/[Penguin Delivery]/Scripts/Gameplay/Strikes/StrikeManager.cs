using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private Sprite spriteStrikeActivado;
    [SerializeField] private Sprite spriteStrikeDesactivado;

    [Header("Opacidad")]
    [SerializeField] private float opacidadStrikeActivado = 1f;
    [SerializeField] private float opacidadStrikeDesactivado = 0.35f;

    [Header("Debug / Pruebas")]
    [SerializeField] private bool useDebugStrikes = true;
    [SerializeField][Range(0, 3)] private int debugStrikes = 0;

    private int currentStrikes = 0;
    private int lastDebugStrikes = -1;
    private Coroutine rutinaParpadeoDemoContinuo;

    private GestorProgresoJugador gestorProgresoJugador;

    private void Start()
    {
        gestorProgresoJugador = FindFirstObjectByType<GestorProgresoJugador>();

        currentStrikes = 0;
        ActualizarStrikeUI();
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

        if (currentStrikes > maxStrikes)
            currentStrikes = maxStrikes;

        if (useDebugStrikes)
        {
            debugStrikes = currentStrikes;
            lastDebugStrikes = currentStrikes;
        }

        ActualizarStrikeUI();

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

        if (useDebugStrikes)
        {
            debugStrikes = currentStrikes;
            lastDebugStrikes = currentStrikes;
        }

        ActualizarStrikeUI();
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

        ActualizarStrikeUI();
    }

    // ====================
    // QUITAR STRIKE
    // ====================
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

        ActualizarStrikeUI();
        return true;
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
        SceneLoader.CargarEscena(lobbySceneName);
    }

    // ====================
    // UI
    // ====================
    private void ActualizarStrikeUI()
    {
        if (strikeImages == null || strikeImages.Length == 0)
            return;

        for (int i = 0; i < strikeImages.Length; i++)
        {
            if (strikeImages[i] == null)
                continue;

            bool strikeActivo = i < currentStrikes;

            if (strikeActivo)
            {
                if (spriteStrikeActivado != null)
                    strikeImages[i].sprite = spriteStrikeActivado;

                AplicarOpacidad(strikeImages[i], opacidadStrikeActivado);
            }
            else
            {
                if (spriteStrikeDesactivado != null)
                    strikeImages[i].sprite = spriteStrikeDesactivado;

                AplicarOpacidad(strikeImages[i], opacidadStrikeDesactivado);
            }
        }
    }

    private void AplicarOpacidad(Image imagen, float opacidad)
    {
        if (imagen == null)
            return;

        Color color = imagen.color;
        color.r = 1f;
        color.g = 1f;
        color.b = 1f;
        color.a = opacidad;
        imagen.color = color;
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
            SceneLoader.CargarEscena(lobbySceneName);
        }
    }

    // ====================
    // DEMO TUTORIAL
    // ====================
    public IEnumerator ParpadearStrikeDemo(float duracion)
    {
        if (strikeImages == null || strikeImages.Length == 0)
            yield break;

        Image imagenDemo = strikeImages[0];

        if (imagenDemo == null)
            yield break;

        float tiempo = 0f;
        float intervalo = 0.25f;
        bool activo = false;

        while (tiempo < duracion)
        {
            activo = !activo;
            AplicarEstadoDemoStrike(imagenDemo, activo);

            yield return new WaitForSeconds(intervalo);
            tiempo += intervalo;
        }

        ActualizarStrikeUI();
    }

    public void IniciarParpadeoStrikeDemoContinuo()
    {
        if (strikeImages == null || strikeImages.Length == 0)
            return;

        if (rutinaParpadeoDemoContinuo != null)
            StopCoroutine(rutinaParpadeoDemoContinuo);

        rutinaParpadeoDemoContinuo = StartCoroutine(ParpadeoStrikeDemoContinuo());
    }

    public void DetenerParpadeoStrikeDemo()
    {
        if (rutinaParpadeoDemoContinuo != null)
        {
            StopCoroutine(rutinaParpadeoDemoContinuo);
            rutinaParpadeoDemoContinuo = null;
        }

        ActualizarStrikeUI();
    }

    private IEnumerator ParpadeoStrikeDemoContinuo()
    {
        if (strikeImages == null || strikeImages.Length == 0)
            yield break;

        Image imagenDemo = strikeImages[0];

        if (imagenDemo == null)
            yield break;

        float intervalo = 0.25f;
        bool activo = false;

        while (true)
        {
            activo = !activo;
            AplicarEstadoDemoStrike(imagenDemo, activo);
            yield return new WaitForSeconds(intervalo);
        }
    }

    private void AplicarEstadoDemoStrike(Image imagenDemo, bool activo)
    {
        if (imagenDemo == null)
            return;

        if (activo)
        {
            if (spriteStrikeActivado != null)
                imagenDemo.sprite = spriteStrikeActivado;

            AplicarOpacidad(imagenDemo, opacidadStrikeActivado);
        }
        else
        {
            if (spriteStrikeDesactivado != null)
                imagenDemo.sprite = spriteStrikeDesactivado;

            AplicarOpacidad(imagenDemo, opacidadStrikeDesactivado);
        }
    }
}
