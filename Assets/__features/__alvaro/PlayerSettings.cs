using UnityEngine;
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
}