using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Añade automáticamente SonidoBotonTiendaUI a todos los Button hijos.
/// Colócalo en un padre que contenga solamente botones del mismo tipo.
/// </summary>
[DisallowMultipleComponent]
public class GrupoSonidosBotonesTiendaUI : MonoBehaviour
{
    [Header("Tipo de botones del grupo")]
    [SerializeField] private TipoBotonTienda tipoBoton = TipoBotonTienda.Habilidad;

    [Header("Botones incluidos")]
    [SerializeField] private bool incluirBotonesInactivos = true;

    [Header("Eventos")]
    [SerializeField] private bool reproducirHoverSelected = true;
    [SerializeField] private bool reproducirPulsar = true;

    [Header("Selección automática")]
    [SerializeField] private bool silenciarSeleccionInicial = false;

    private void Awake()
    {
        InstalarEnBotones();
    }

    private void OnEnable()
    {
        InstalarEnBotones();
    }

    [ContextMenu("Instalar sonidos en los botones hijos")]
    public void InstalarEnBotones()
    {
        Button[] botones = GetComponentsInChildren<Button>(incluirBotonesInactivos);

        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];

            if (boton == null)
                continue;

            SonidoBotonTiendaUI sonido = boton.GetComponent<SonidoBotonTiendaUI>();

            if (sonido == null)
                sonido = boton.gameObject.AddComponent<SonidoBotonTiendaUI>();

            sonido.Configurar(
                tipoBoton,
                reproducirHoverSelected,
                reproducirPulsar,
                silenciarSeleccionInicial
            );
        }
    }
}
