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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private Toggle pantallaCompleta;
    [SerializeField] private Slider volumen;
    [SerializeField] private Dropdown calidadDrop;
    [SerializeField] private AudioMixer audioMixer;

    void Start()
    {
        CargarYAplicarSettings();
    }

    private void CargarYAplicarSettings()
    {
        bool pantallaCompletaGuardada = PlayerPrefs.GetInt("PantallaCompleta", Screen.fullScreen ? 1 : 0) == 1;
        float volumenGuardado = PlayerPrefs.GetFloat("Volumen", 0f);
        int calidadGuardada = PlayerPrefs.GetInt("Calidad", QualitySettings.GetQualityLevel());

        Screen.fullScreen = pantallaCompletaGuardada;
        QualitySettings.SetQualityLevel(calidadGuardada, true);

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

        if (calidadDrop != null)
        {
            calidadDrop.SetValueWithoutNotify(calidadGuardada);
            calidadDrop.RefreshShownValue();
        }
    }

    public void SetPantallaCompletaPref(bool valor)
    {
        Screen.fullScreen = valor;
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

    public void SetCalidadPref(int valor)
    {
        QualitySettings.SetQualityLevel(valor, true);
        PlayerPrefs.SetInt("Calidad", valor);
        PlayerPrefs.Save();
    }
}