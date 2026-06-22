using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Se coloca en el panel raíz de un menú y añade SonidoBotonUI
/// automáticamente a todos los Button hijos.
/// No lo añadas al panel de la tienda si sus sonidos serán distintos.
/// </summary>
[DisallowMultipleComponent]
public class GrupoSonidosBotonesUI : MonoBehaviour
{
    [Header("Botones incluidos")]
    [SerializeField] private bool incluirBotonesInactivos = true;

    [Header("Sonidos")]
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

            SonidoBotonUI sonido = boton.GetComponent<SonidoBotonUI>();

            if (sonido == null)
                sonido = boton.gameObject.AddComponent<SonidoBotonUI>();

            sonido.Configurar(
                reproducirHoverSelected,
                reproducirPulsar,
                silenciarSeleccionInicial
            );
        }
    }
}
