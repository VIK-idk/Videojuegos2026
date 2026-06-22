using UnityEngine;

[CreateAssetMenu(fileName = "NuevoTipoSuelo", menuName = "Tipo de Suelo/Nuevo Suelo")]
public class TipoSuelo : ScriptableObject
{
    [Header("Datos")]
    public string nombre;

    [Header("Audio antiguo / fallback")]
    [Tooltip("Se mantiene para no romper lo que ya tenias. Si no asignas listas nuevas, este clip se usa como sonido de paso/salto/caida.")]
    public AudioClip efectoSonido;

    [Header("Audio pasos por este suelo")]
    public AudioClip[] sonidosPasos;

    [Header("Audio salto desde este suelo")]
    [Tooltip("Si esta vacio, se usaran los sonidos de pasos.")]
    public AudioClip[] sonidosSaltoSuelo;

    [Header("Audio caida sobre este suelo")]
    [Tooltip("Si esta vacio, se usaran los sonidos de pasos.")]
    public AudioClip[] sonidosCaidaSuelo;

    [Header("VFX")]
    public GameObject efectoVisualCaminar;

    public AudioClip ObtenerSonidoPasoAleatorio(int ultimoIndice, out int nuevoIndice)
    {
        return ObtenerAleatorioConFallback(sonidosPasos, ultimoIndice, out nuevoIndice);
    }

    public AudioClip ObtenerSonidoSaltoAleatorio(int ultimoIndice, out int nuevoIndice)
    {
        AudioClip clip = ObtenerAleatorio(sonidosSaltoSuelo, ultimoIndice, out nuevoIndice);

        if (clip != null)
            return clip;

        return ObtenerSonidoPasoAleatorio(ultimoIndice, out nuevoIndice);
    }

    public AudioClip ObtenerSonidoCaidaAleatorio(int ultimoIndice, out int nuevoIndice)
    {
        AudioClip clip = ObtenerAleatorio(sonidosCaidaSuelo, ultimoIndice, out nuevoIndice);

        if (clip != null)
            return clip;

        return ObtenerSonidoPasoAleatorio(ultimoIndice, out nuevoIndice);
    }

    private AudioClip ObtenerAleatorioConFallback(AudioClip[] clips, int ultimoIndice, out int nuevoIndice)
    {
        AudioClip clip = ObtenerAleatorio(clips, ultimoIndice, out nuevoIndice);

        if (clip != null)
            return clip;

        nuevoIndice = -1;
        return efectoSonido;
    }

    private AudioClip ObtenerAleatorio(AudioClip[] clips, int ultimoIndice, out int nuevoIndice)
    {
        nuevoIndice = -1;

        if (clips == null || clips.Length == 0)
            return null;

        if (clips.Length == 1)
        {
            nuevoIndice = 0;
            return clips[0];
        }

        int indice = Random.Range(0, clips.Length);
        int intentos = 0;

        while (indice == ultimoIndice && intentos < 10)
        {
            indice = Random.Range(0, clips.Length);
            intentos++;
        }

        nuevoIndice = indice;
        return clips[indice];
    }
}
