using System.Collections.Generic;
using UnityEngine;

namespace Ablabla.Habilidades
{
    // Controla activación, duración, cooldown y uso único de las habilidades.
    public class Jugador_Habilidades : MonoBehaviour
    {
        [Header("Habilidades del jugador")]
        // Lista de habilidades configuradas en el Inspector.
        [SerializeField] private List<Datos_Habilidad> habilidades = new List<Datos_Habilidad>();

        [Header("Referencias")]
        // Referencia al sistema de strikes para la habilidad QuitarStrike.
        [SerializeField] private Gestor_Strikes gestorStrikes;

        // Guarda la habilidad que está activa actualmente.
        private Datos_Habilidad habilidadActivaActual;

        // Devuelve el tipo de la habilidad activa. Si no hay ninguna, devuelve Ninguna.
        public Tipo_Habilidades TipoHabilidadActivaActual
        {
            get
            {
                if (habilidadActivaActual == null)
                {
                    return Tipo_Habilidades.Ninguna;
                }

                return habilidadActivaActual.tipoHabilidad;
            }
        }

        // Comprueba si hay al menos una habilidad desbloqueada.
        public bool TieneAlgunaHabilidadDesbloqueada()
        {
            for (int i = 0; i < habilidades.Count; i++)
            {
                if (habilidades[i] != null && habilidades[i].estaDesbloqueada)
                {
                    return true;
                }
            }

            return false;
        }

        // Indica si hay una habilidad activa en este momento.
        public bool HayAlgunaHabilidadActiva()
        {
            return habilidadActivaActual != null;
        }

        // Comprueba si una habilidad concreta está activa.
        public bool EstaActivaLaHabilidad(Tipo_Habilidades tipo)
        {
            Datos_Habilidad habilidad = ObtenerHabilidad(tipo);

            if (habilidad == null)
            {
                return false;
            }

            return habilidad.estaActiva;
        }

        // Devuelve el cooldown restante de una habilidad.
        public float ObtenerCooldownRestante(Tipo_Habilidades tipo)
        {
            Datos_Habilidad habilidad = ObtenerHabilidad(tipo);

            if (habilidad == null)
            {
                return 0f;
            }

            return habilidad.temporizadorCooldown;
        }

        // Actualiza cooldowns, duración de la habilidad activa e input.
        private void Update()
        {
            ActualizarCooldowns();
            ActualizarHabilidadActiva();
            ComprobarInput();
        }

        // Reduce el cooldown de todas las habilidades.
        private void ActualizarCooldowns()
        {
            for (int i = 0; i < habilidades.Count; i++)
            {
                Datos_Habilidad habilidad = habilidades[i];

                if (habilidad == null)
                {
                    continue;
                }

                if (habilidad.temporizadorCooldown > 0f)
                {
                    habilidad.temporizadorCooldown -= Time.deltaTime;

                    if (habilidad.temporizadorCooldown < 0f)
                    {
                        habilidad.temporizadorCooldown = 0f;
                    }
                }
            }
        }

        // Reduce la duración de la habilidad activa y la finaliza al agotarse.
        private void ActualizarHabilidadActiva()
        {
            if (habilidadActivaActual == null)
            {
                return;
            }

            habilidadActivaActual.temporizadorActivo -= Time.deltaTime;

            if (habilidadActivaActual.temporizadorActivo <= 0f)
            {
                FinalizarHabilidadActual();
            }
        }

        // Comprueba si se ha pulsado la tecla de alguna habilidad desbloqueada.
        private void ComprobarInput()
        {
            if (!TieneAlgunaHabilidadDesbloqueada())
            {
                return;
            }

            for (int i = 0; i < habilidades.Count; i++)
            {
                Datos_Habilidad habilidad = habilidades[i];

                if (habilidad == null)
                {
                    continue;
                }

                if (!habilidad.estaDesbloqueada)
                {
                    continue;
                }

                if (habilidad.teclaActivacion == KeyCode.None)
                {
                    continue;
                }

                if (Input.GetKeyDown(habilidad.teclaActivacion))
                {
                    IntentarActivarHabilidad(habilidad.tipoHabilidad);
                    return;
                }
            }
        }

        // Intenta activar una habilidad si cumple todas las condiciones.
        public bool IntentarActivarHabilidad(Tipo_Habilidades tipo)
        {
            Datos_Habilidad habilidad = ObtenerHabilidad(tipo);

            if (habilidad == null)
            {
                Debug.LogWarning("No existe la habilidad: " + tipo);
                return false;
            }

            if (!habilidad.estaDesbloqueada)
            {
                Debug.Log("La habilidad " + tipo + " no esta desbloqueada.");
                return false;
            }

            if (habilidad.soloUnUsoPorPartida && habilidad.yaUsadaEnEstaPartida)
            {
                Debug.Log("La habilidad " + tipo + " ya se ha usado en esta partida.");
                return false;
            }

            if (habilidad.estaActiva)
            {
                Debug.Log("La habilidad " + tipo + " ya esta activa.");
                return false;
            }

            if (habilidad.temporizadorCooldown > 0f)
            {
                Debug.Log("La habilidad " + tipo + " esta en cooldown.");
                return false;
            }

            if (habilidadActivaActual != null)
            {
                Debug.Log("Ya hay una habilidad activa: " + habilidadActivaActual.tipoHabilidad);
                return false;
            }

            return ActivarHabilidad(habilidad);
        }

        // Activa la habilidad y solo la da por válida si su efecto se aplica correctamente.
        private bool ActivarHabilidad(Datos_Habilidad habilidad)
        {
            habilidadActivaActual = habilidad;
            habilidadActivaActual.estaActiva = true;
            habilidadActivaActual.temporizadorActivo = habilidadActivaActual.duracion;

            bool efectoAplicado = AlEmpezarHabilidad(habilidadActivaActual.tipoHabilidad);

            // Si el efecto falla, se cancela la activación.
            if (!efectoAplicado)
            {
                habilidadActivaActual.estaActiva = false;
                habilidadActivaActual.temporizadorActivo = 0f;
                habilidadActivaActual = null;
                return false;
            }

            // Solo se marca como usada si el efecto se ha aplicado de verdad.
            if (habilidadActivaActual.soloUnUsoPorPartida)
            {
                habilidadActivaActual.yaUsadaEnEstaPartida = true;
            }

            Debug.Log("Habilidad activada: " + habilidadActivaActual.tipoHabilidad);
            return true;
        }

        // Finaliza la habilidad actual y le aplica su cooldown.
        private void FinalizarHabilidadActual()
        {
            if (habilidadActivaActual == null)
            {
                return;
            }

            Tipo_Habilidades tipoFinalizado = habilidadActivaActual.tipoHabilidad;

            habilidadActivaActual.estaActiva = false;
            habilidadActivaActual.temporizadorActivo = 0f;
            habilidadActivaActual.temporizadorCooldown = habilidadActivaActual.cooldown;

            AlTerminarHabilidad(tipoFinalizado);

            Debug.Log("Habilidad terminada: " + tipoFinalizado);

            habilidadActivaActual = null;
        }

        // Busca una habilidad concreta dentro de la lista.
        private Datos_Habilidad ObtenerHabilidad(Tipo_Habilidades tipo)
        {
            for (int i = 0; i < habilidades.Count; i++)
            {
                if (habilidades[i] != null && habilidades[i].tipoHabilidad == tipo)
                {
                    return habilidades[i];
                }
            }

            return null;
        }

        // Ejecuta el efecto inicial de la habilidad.
        protected virtual bool AlEmpezarHabilidad(Tipo_Habilidades tipo)
        {
            if (tipo == Tipo_Habilidades.QuitarStrike)
            {
                if (gestorStrikes == null)
                {
                    Debug.LogWarning("No hay Gestor_Strikes asignado en Jugador_Habilidades.");
                    return false;
                }

                return gestorStrikes.QuitarUnStrike();
            }

            return true;
        }

        // Punto de salida para lógica al terminar una habilidad.
        protected virtual void AlTerminarHabilidad(Tipo_Habilidades tipo)
        {
        }
    }
}