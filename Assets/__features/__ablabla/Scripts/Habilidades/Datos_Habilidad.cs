using UnityEngine;

namespace Ablabla.Habilidades
{
    // Clase que guarda la configuración y el estado de una habilidad.
    [System.Serializable]
    public class Datos_Habilidad
    {
        [Header("Configuracion")]
        // Tipo de habilidad: imán, doble pez, quitar strike, etc.
        public Tipo_Habilidades tipoHabilidad = Tipo_Habilidades.Ninguna;

        // Tecla que activa esta habilidad.
        public KeyCode teclaActivacion = KeyCode.None;

        // Tiempo que la habilidad permanece activa.
        [Min(0f)]
        public float duracion = 0f;

        // Tiempo que debe esperar antes de poder volver a usarse.
        [Min(0f)]
        public float cooldown = 0f;

        // Indica si la habilidad está desbloqueada y se puede usar.
        public bool estaDesbloqueada = true;

        // Indica si esta habilidad solo puede usarse una vez por partida.
        public bool soloUnUsoPorPartida = false;

        [Header("Estado en tiempo de ejecucion")]
        // Indica si la habilidad está activa en este momento.
        [HideInInspector] public bool estaActiva = false;

        // Tiempo restante de duración mientras la habilidad está activa.
        [HideInInspector] public float temporizadorActivo = 0f;

        // Tiempo restante de cooldown antes de poder volver a usarla.
        [HideInInspector] public float temporizadorCooldown = 0f;

        // Indica si la habilidad ya se ha usado en esta partida.
        [HideInInspector] public bool yaUsadaEnEstaPartida = false;
    }
}