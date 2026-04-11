using UnityEngine;

namespace Ablabla.Habilidades
{
    // Sistema simple para gestionar los strikes del jugador.
    public class Gestor_Strikes : MonoBehaviour
    {
        [Header("Configuracion de strikes")]
        [SerializeField] private int strikesActuales = 0;
        [SerializeField] private int strikesMaximos = 3;

        // Devuelve el número actual de strikes.
        public int ObtenerStrikesActuales()
        {
            return strikesActuales;
        }

        // Comprueba si el jugador tiene al menos un strike.
        public bool TieneStrikes()
        {
            return strikesActuales > 0;
        }

        // Añade un strike si no se ha alcanzado el máximo.
        public void AgregarStrike()
        {
            if (strikesActuales >= strikesMaximos)
            {
                Debug.Log("No se puede agregar mas strikes. Ya esta en el maximo.");
                return;
            }

            strikesActuales++;
            Debug.Log("Strike agregado. Total actual: " + strikesActuales);
        }

        // Quita un strike si hay alguno disponible.
        public bool QuitarUnStrike()
        {
            if (strikesActuales <= 0)
            {
                Debug.Log("No hay strikes para quitar.");
                return false;
            }

            strikesActuales--;
            Debug.Log("Se ha quitado 1 strike. Total actual: " + strikesActuales);
            return true;
        }

        // Solo para pruebas en el Inspector o desde otros scripts.
        public void EstablecerStrikes(int cantidad)
        {
            strikesActuales = Mathf.Clamp(cantidad, 0, strikesMaximos);
            Debug.Log("Strikes establecidos manualmente. Total actual: " + strikesActuales);
        }
    }
}