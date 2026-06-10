using UnityEngine;

public class EventosAnimacionJugador : MonoBehaviour
{
    [SerializeField] private Player player;

    private void Awake()
    {
        BuscarPlayer();
    }

    private void Reset()
    {
        BuscarPlayer();
    }

    private void BuscarPlayer()
    {
        if (player == null)
        {
            player = GetComponentInParent<Player>();
        }
    }

    // Usa estos nombres en los Animation Events.
    public void PasoIzquierdo()
    {
        if (player == null)
            BuscarPlayer();

        if (player != null)
            player.ReproducirPasoIzquierdoDesdeAnimacion();
    }

    public void PasoDerecho()
    {
        if (player == null)
            BuscarPlayer();

        if (player != null)
            player.ReproducirPasoDerechoDesdeAnimacion();
    }

    // Alias por si prefieres estos nombres en los eventos.
    public void PieIzquierdo()
    {
        PasoIzquierdo();
    }

    public void PieDerecho()
    {
        PasoDerecho();
    }
}
