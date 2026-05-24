using UnityEngine;

[CreateAssetMenu(fileName = "NuevoTipoSuelo", menuName = "Penguin Delivery/Tipo Suelo")]
public class TipoSuelo : ScriptableObject
{
    [Header("Datos")]
    public string nombre;

    [Header("Audio")]
    public AudioClip efectoSonido;

    [Header("VFX")]
    public GameObject efectoVisualCaminar;
}