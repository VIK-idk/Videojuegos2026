using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private Toggle pantallaCompleta;
    [SerializeField] private Slider volumen;
    [SerializeField] private Dropdown resolucionDrop;
    [SerializeField] private Slider sensibilidadSlider;
    [SerializeField] private Text textoValorSensibilidad;
    [SerializeField] private AudioMixer audioMixer;

    private readonly Vector2Int[] resoluciones = new Vector2Int[]
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080)
    };

    private void Start()
    {
        ConfigurarDropdownResolucion();
        ConfigurarSliderSensibilidad();
        CargarYAplicarSettings();
    }

    private void ConfigurarDropdownResolucion()
    {
        if (resolucionDrop == null)
            return;

        resolucionDrop.ClearOptions();

        List<string> opciones = new List<string>();

        for (int i = 0; i < resoluciones.Length; i++)
        {
            opciones.Add(resoluciones[i].x + "x" + resoluciones[i].y);
        }

        resolucionDrop.AddOptions(opciones);
    }

    private void ConfigurarSliderSensibilidad()
    {
        if (sensibilidadSlider == null)
            return;

        sensibilidadSlider.minValue = 200f;
        sensibilidadSlider.maxValue = 1000f;
        sensibilidadSlider.wholeNumbers = true;
    }

    private void CargarYAplicarSettings()
    {
        bool pantallaCompletaGuardada = PlayerPrefs.GetInt("PantallaCompleta", Screen.fullScreen ? 1 : 0) == 1;
        float volumenGuardado = PlayerPrefs.GetFloat("Volumen", 0f);
        float sensibilidadGuardada = PlayerPrefs.GetFloat("Sensibilidad", 550f);

        int resolucionIndexGuardado = PlayerPrefs.GetInt("ResolucionIndex", BuscarIndiceResolucionActual());
        resolucionIndexGuardado = Mathf.Clamp(resolucionIndexGuardado, 0, resoluciones.Length - 1);

        AplicarResolucion(resolucionIndexGuardado, pantallaCompletaGuardada);

        if (audioMixer != null)
        {
            audioMixer.SetFloat("Volumen", volumenGuardado);
        }

        if (pantallaCompleta != null)
        {
            pantallaCompleta.SetIsOnWithoutNotify(pantallaCompletaGuardada);
        }

        if (volumen != null)
        {
            volumen.SetValueWithoutNotify(volumenGuardado);
        }

        if (resolucionDrop != null)
        {
            resolucionDrop.SetValueWithoutNotify(resolucionIndexGuardado);
            resolucionDrop.RefreshShownValue();
        }

        if (sensibilidadSlider != null)
        {
            sensibilidadSlider.SetValueWithoutNotify(sensibilidadGuardada);
        }

        ActualizarTextoSensibilidad(sensibilidadGuardada);
    }

    private int BuscarIndiceResolucionActual()
    {
        for (int i = 0; i < resoluciones.Length; i++)
        {
            if (resoluciones[i].x == Screen.width && resoluciones[i].y == Screen.height)
                return i;
        }

        return 0;
    }

    private void AplicarResolucion(int index, bool fullscreen)
    {
        if (index < 0 || index >= resoluciones.Length)
            return;

        Screen.SetResolution(resoluciones[index].x, resoluciones[index].y, fullscreen);
    }

    private void ActualizarTextoSensibilidad(float valor)
    {
        if (textoValorSensibilidad != null)
        {
            textoValorSensibilidad.text = Mathf.RoundToInt(valor).ToString();
        }
    }

    public void SetPantallaCompletaPref(bool valor)
    {
        int indiceActual = PlayerPrefs.GetInt("ResolucionIndex", BuscarIndiceResolucionActual());
        indiceActual = Mathf.Clamp(indiceActual, 0, resoluciones.Length - 1);

        AplicarResolucion(indiceActual, valor);

        PlayerPrefs.SetInt("PantallaCompleta", valor ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVolumePref(float valor)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("Volumen", valor);
        }

        PlayerPrefs.SetFloat("Volumen", valor);
        PlayerPrefs.Save();
    }

    public void SetResolucionPref(int index)
    {
        index = Mathf.Clamp(index, 0, resoluciones.Length - 1);

        bool fullscreenActual = PlayerPrefs.GetInt("PantallaCompleta", Screen.fullScreen ? 1 : 0) == 1;
        AplicarResolucion(index, fullscreenActual);

        PlayerPrefs.SetInt("ResolucionIndex", index);
        PlayerPrefs.Save();
    }

    public void SetSensibilidadPref(float valor)
    {
        PlayerPrefs.SetFloat("Sensibilidad", valor);
        PlayerPrefs.Save();

        ActualizarTextoSensibilidad(valor);
    }
}

//=================================================================================================================
//Codigo anterior de pruebas temporalmente guardado, no borrar por ahora, puede ser útil para comparar o recuperar ideas
//=================================================================================================================

/*using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private Toggle pantallaCompleta;
    [SerializeField] private Slider volumen;
    [SerializeField] private Dropdown calidadDrop;


    void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        volumen.value = PlayerPrefs.GetFloat("Volumen");
        calidadDrop.value = PlayerPrefs.GetInt("Calidad");

        // ?? Cargar correctamente el toggle SIN disparar eventos + valor por defecto
        int valorToggle = PlayerPrefs.GetInt("PantallaCompleta", 0);
        pantallaCompleta.SetIsOnWithoutNotify(valorToggle == 1);
    }

    public void SetVolumePref()
    {
        PlayerPrefs.SetFloat("Volumen", volumen.value);
    }

    public void setCalidadPref()
    {
        PlayerPrefs.SetInt("Calidad", calidadDrop.value);
    }

    // ?? Usar el bool que manda el Toggle directamente + guardar seguro
    public void SetPantallaCompletaPref(bool valor)
    {
        PlayerPrefs.SetInt("PantallaCompleta", valor ? 1 : 0);
        PlayerPrefs.Save(); // ?? Fuerza el guardado inmediato
    }
}*/
