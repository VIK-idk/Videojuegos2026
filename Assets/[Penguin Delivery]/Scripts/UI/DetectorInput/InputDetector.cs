using System;
using UnityEngine;

/// <summary>
/// Detecta el último método de entrada usado con el Input Manager antiguo.
/// Distingue entre ratón, navegación por teclado y mando.
/// Se crea automáticamente y permanece entre escenas.
/// </summary>
public class InputDetector : MonoBehaviour
{
    public enum ModoEntrada
    {
        TecladoRaton,
        NavegacionTeclado,
        Mando
    }

    public static InputDetector Instancia { get; private set; }

    // Se conserva este nombre para no romper TextoDinamicoUI ni HabilidadSlotUI.
    public static bool usandoMando { get; private set; }

    public static ModoEntrada ModoActual { get; private set; } = ModoEntrada.TecladoRaton;

    /// <summary>
    /// True cuando la UI debe mostrar un objeto seleccionado:
    /// mando o navegación con WASD/flechas.
    /// </summary>
    public static bool DebeMostrarSeleccionUI
    {
        get
        {
            return ModoActual == ModoEntrada.Mando ||
                   ModoActual == ModoEntrada.NavegacionTeclado;
        }
    }

    public static event Action<ModoEntrada> AlCambiarModoEntrada;

    [Header("Detección de mando")]
    [SerializeField, Range(0.1f, 1f)] private float zonaMuertaMando = 0.45f;

    [Header("Detección de ratón")]
    [SerializeField] private float pixelesMinimosMovimientoRaton = 1.5f;

    private Vector3 ultimaPosicionRaton;
    private bool posicionRatonInicializada;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CrearAutomaticamente()
    {
        if (Instancia != null)
            return;

        GameObject objeto = new GameObject("InputDetector");
        objeto.AddComponent<InputDetector>();
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            // El InputDetector de algunas escenas comparte GameObject con otros managers.
            // Destruimos solo este componente para no borrar UIManager u otros scripts.
            Destroy(this);
            return;
        }

        Instancia = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        ultimaPosicionRaton = Input.mousePosition;
        posicionRatonInicializada = true;
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }

    private void Update()
    {
        bool botonMando = SePulsoBotonMando();
        bool navegacionTeclado = SePulsoNavegacionTeclado();
        bool ejeMando = SeMovioEjeMando();
        bool usoRaton = SeUsoRaton();

        // Prioridad: mando > navegación por teclado > ratón/otras teclas.
        if (botonMando || ejeMando)
        {
            CambiarModo(ModoEntrada.Mando);
            return;
        }

        if (navegacionTeclado)
        {
            CambiarModo(ModoEntrada.NavegacionTeclado);
            return;
        }

        if (usoRaton)
        {
            CambiarModo(ModoEntrada.TecladoRaton);
            return;
        }

        // E, Espacio, TAB, Escape, números, etc. cuentan como teclado,
        // pero no activan el selector de navegación.
        if (Input.anyKeyDown)
            CambiarModo(ModoEntrada.TecladoRaton);
    }

    private bool SePulsoBotonMando()
    {
        for (int i = 0; i <= 19; i++)
        {
            KeyCode boton = KeyCode.JoystickButton0 + i;

            if (Input.GetKeyDown(boton))
                return true;
        }

        return false;
    }

    private bool SePulsoNavegacionTeclado()
    {
        return Input.GetKeyDown(KeyCode.W) ||
               Input.GetKeyDown(KeyCode.A) ||
               Input.GetKeyDown(KeyCode.S) ||
               Input.GetKeyDown(KeyCode.D) ||
               Input.GetKeyDown(KeyCode.UpArrow) ||
               Input.GetKeyDown(KeyCode.DownArrow) ||
               Input.GetKeyDown(KeyCode.LeftArrow) ||
               Input.GetKeyDown(KeyCode.RightArrow);
    }

    private bool HayNavegacionTecladoMantenida()
    {
        return Input.GetKey(KeyCode.W) ||
               Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.S) ||
               Input.GetKey(KeyCode.D) ||
               Input.GetKey(KeyCode.UpArrow) ||
               Input.GetKey(KeyCode.DownArrow) ||
               Input.GetKey(KeyCode.LeftArrow) ||
               Input.GetKey(KeyCode.RightArrow);
    }

    private bool SeMovioEjeMando()
    {
        if (!HayMandoConectado())
            return false;

        // Evita confundir WASD/flechas con los ejes Horizontal/Vertical,
        // porque el Input Manager antiguo combina teclado y stick.
        if (HayNavegacionTecladoMantenida())
            return false;

        if (Mathf.Abs(LeerEjeSeguro("Horizontal")) >= zonaMuertaMando)
            return true;

        if (Mathf.Abs(LeerEjeSeguro("Vertical")) >= zonaMuertaMando)
            return true;

        if (Mathf.Abs(LeerEjeSeguro("RightStickX")) >= zonaMuertaMando)
            return true;

        if (Mathf.Abs(LeerEjeSeguro("RightStickY")) >= zonaMuertaMando)
            return true;

        return false;
    }

    private bool SeUsoRaton()
    {
        if (Input.GetMouseButtonDown(0) ||
            Input.GetMouseButtonDown(1) ||
            Input.GetMouseButtonDown(2) ||
            Mathf.Abs(Input.mouseScrollDelta.y) > 0.01f)
        {
            ultimaPosicionRaton = Input.mousePosition;
            posicionRatonInicializada = true;
            return true;
        }

        Vector3 posicionActual = Input.mousePosition;

        if (!posicionRatonInicializada)
        {
            ultimaPosicionRaton = posicionActual;
            posicionRatonInicializada = true;
            return false;
        }

        float distancia = Vector3.Distance(posicionActual, ultimaPosicionRaton);
        ultimaPosicionRaton = posicionActual;

        return distancia >= pixelesMinimosMovimientoRaton;
    }

    private bool HayMandoConectado()
    {
        string[] nombres = Input.GetJoystickNames();

        for (int i = 0; i < nombres.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(nombres[i]))
                return true;
        }

        return false;
    }

    private float LeerEjeSeguro(string nombreEje)
    {
        try
        {
            return Input.GetAxisRaw(nombreEje);
        }
        catch
        {
            return 0f;
        }
    }

    private void CambiarModo(ModoEntrada nuevoModo)
    {
        if (ModoActual == nuevoModo)
            return;

        ModoActual = nuevoModo;
        usandoMando = nuevoModo == ModoEntrada.Mando;
        AlCambiarModoEntrada?.Invoke(nuevoModo);
    }
}
