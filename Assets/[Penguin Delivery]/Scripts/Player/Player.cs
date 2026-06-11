using UnityEngine;
using UnityEngine.Audio;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private bool estaEnSuelo = true;
    [SerializeField] private float jumpTrampolin = 40f;
    [SerializeField] private float multiplicadorCaida = 2.5f;
    [SerializeField] private float multiplicadorSaltoBajo = 2f;

    [Header("Animaciones")]
    [SerializeField] private Animator animator;
    [SerializeField] private float deadzoneAnimacion = 0.1f;
    [SerializeField] private float tiempoParaFidget = 5f;

    private float temporizadorQuieto = 0f;

    private const string PARAM_CAMINANDO = "Caminando";
    private const string PARAM_EN_SUELO = "EnSuelo";
    private const string PARAM_VELOCIDAD_Y = "VelocidadY";
    private const string PARAM_SALTAR = "Saltar";
    private const string PARAM_FIDGET = "Fidget";

    [Header("Rotacion modelo")]
    [SerializeField] private Transform modeloVisual;
    [SerializeField] private float velocidadRotacionModelo = 10f;
    [SerializeField] private float offsetRotacionModelo = 0f;

    [Header("Suelos / VFX pasos")]
    [SerializeField] private Transform puntoPieIzquierdo;
    [SerializeField] private Transform puntoPieDerecho;

    [Header("Deteccion de suelo")]
    [Tooltip("Activa una deteccion extra hacia abajo para conocer el tipo de suelo aunque el jugador ya empiece encima del suelo y no se dispare OnCollisionEnter.")]
    [SerializeField] private bool detectarTipoSueloPorRaycast = true;
    [SerializeField] private LayerMask capasDeteccionSuelo = ~0;
    [SerializeField] private float distanciaRaycastSuelo = 1.8f;
    [SerializeField] private float radioSphereCastSuelo = 0.25f;
    [SerializeField] private float alturaOrigenRaycastSuelo = 0.8f;

    private Suelo sueloActual;
    private TipoSuelo tipoSueloActual;
    private ParticleSystem efectoPasosIzquierdo;
    private ParticleSystem efectoPasosDerecho;

    [Header("VFX impulso morsa / salto")]
    [SerializeField] private GameObject vfxImpulsoMorsa;
    [SerializeField] private float duracionVFXImpulsoMorsa = 1f;

    [Header("Audio - Player")]
    [Tooltip("Grupo para la voz/sonido general de salto de Guppy, por ejemplo el viento o quejido de salto.")]
    [SerializeField] private AudioMixerGroup grupoMixerPlayer;

    [Tooltip("Grupo para pisadas, salto del suelo y caida. Pon aqui SFX > Pasos.")]
    [SerializeField] private AudioMixerGroup grupoMixerPasos;

    [SerializeField] private AudioSource audioSourcePieIzquierdo;
    [SerializeField] private AudioSource audioSourcePieDerecho;
    [SerializeField] private AudioSource audioSourceSaltoGeneral;

    [Header("Audio pasos")]
    [SerializeField, Range(0f, 1f)] private float volumenPasos = 0.85f;
    [SerializeField, Range(0f, 0.5f)] private float variacionPitchPasos = 0.04f;
    [SerializeField] private float tiempoMinimoEntrePasosMismoPie = 0.08f;
    [SerializeField] private bool pasosSoloSiEstaCaminando = true;

    [Header("Fallback pasos sin Animation Events")]
    [Tooltip("Si todavia no has puesto eventos PasoIzquierdo/PasoDerecho en la animacion, esto reproduce pasos automaticamente al caminar.")]
    [SerializeField] private bool usarPasosAutomaticosSiNoHayEventos = true;
    [SerializeField] private float intervaloPasoAutomatico = 0.28f;

    [Header("Audio salto y caida")]
    [Tooltip("Sonido comun del salto, por ejemplo un viento. Si metes varios, elige uno aleatorio.")]
    [SerializeField] private AudioClip[] sonidosSalto;
    [SerializeField, Range(0f, 1f)] private float volumenSalto = 1f;
    [SerializeField, Range(0f, 1f)] private float volumenSaltoSuelo = 0.85f;
    [SerializeField, Range(0f, 1f)] private float volumenCaidaSuelo = 1f;
    [SerializeField] private bool evitarRepetirMismoSalto = true;
    [SerializeField] private bool reproducirSonidoSueloAlSaltar = true;
    [SerializeField] private bool reproducirSonidoVientoAlSaltar = true;
    [SerializeField] private bool reproducirSonidoCaidaAlAterrizar = true;
    [SerializeField] private bool reproducirSaltoTambienEnReboteMorsa = false;

    [Header("Audio 2D / 3D")]
    [Tooltip("0 = 2D. Recomendado para Guppy porque suele estar a la misma distancia de la camara.")]
    [SerializeField, Range(0f, 1f)] private float spatialBlendPlayer = 0f;

    private int ultimoIndicePasoIzquierdo = -1;
    private int ultimoIndicePasoDerecho = -1;
    private int ultimoIndiceSaltoSuelo = -1;
    private int ultimoIndiceCaida = -1;
    private int ultimoIndiceSalto = -1;
    private float tiempoUltimoPasoIzquierdo = -999f;
    private float tiempoUltimoPasoDerecho = -999f;
    private float temporizadorPasoAutomatico = 0f;
    private bool siguientePasoAutomaticoIzquierdo = true;
    private float tiempoUltimoEventoPasoAnimacion = -999f;
    private bool audioJugadorBloqueado = false;

    private TutorialManager tutorialManager;
    private Rigidbody rb;

    private float inputX;
    private float inputZ;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        tutorialManager = FindFirstObjectByType<TutorialManager>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.SetBool(PARAM_EN_SUELO, estaEnSuelo);
        }

        if (modeloVisual == null)
        {
            Transform encontrado = transform.Find("Pinguino");

            if (encontrado != null)
            {
                modeloVisual = encontrado;
            }
        }

        if (puntoPieIzquierdo == null)
        {
            puntoPieIzquierdo = transform;
        }

        if (puntoPieDerecho == null)
        {
            puntoPieDerecho = transform;
        }

        ConfigurarAudioSourcesPlayer();
        ActualizarTipoSueloPorRaycast();
    }

    private void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        ActualizarAnimacionMovimiento();
        ActualizarAnimacionFidget();
        ActualizarRotacionModelo();
        ActualizarTipoSueloPorRaycast();
        ActualizarEfectoPasos();
        ActualizarPasosAutomaticos();

        if (Input.GetButtonDown("Saltar") && estaEnSuelo)
        {
            SaltarDesdeSuelo();
        }
    }

    private void FixedUpdate()
    {
        Vector3 direccion = transform.forward * inputZ + transform.right * inputX;
        direccion = Vector3.ClampMagnitude(direccion, 1f);

        Vector3 velocidad = direccion * speed;
        velocidad.y = rb.linearVelocity.y;
        rb.linearVelocity = velocidad;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorCaida - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Saltar"))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorSaltoBajo - 1) * Time.fixedDeltaTime;
        }

        ActualizarAnimacionAire();
    }

    private void SaltarDesdeSuelo()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        estaEnSuelo = false;

        ReproducirVFXImpulsoMorsa();
        ReproducirSonidoSaltoCompleto();
        DetenerEfectoPasos();

        temporizadorQuieto = 0f;

        if (animator != null)
        {
            animator.ResetTrigger(PARAM_FIDGET);
            animator.SetBool(PARAM_EN_SUELO, false);
            animator.SetTrigger(PARAM_SALTAR);
        }
    }

    private void ActualizarAnimacionMovimiento()
    {
        if (animator == null)
            return;

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);
        bool estaCaminando = inputMovimiento.magnitude > deadzoneAnimacion;

        animator.SetBool(PARAM_CAMINANDO, estaCaminando);
    }

    private void ActualizarAnimacionFidget()
    {
        if (animator == null || rb == null)
            return;

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);

        bool estaQuieto = inputMovimiento.magnitude <= deadzoneAnimacion &&
                          estaEnSuelo &&
                          Mathf.Abs(rb.linearVelocity.y) < 0.05f;

        if (estaQuieto)
        {
            temporizadorQuieto += Time.deltaTime;

            if (temporizadorQuieto >= tiempoParaFidget)
            {
                animator.ResetTrigger(PARAM_FIDGET);
                animator.SetTrigger(PARAM_FIDGET);
                temporizadorQuieto = 0f;
            }
        }
        else
        {
            temporizadorQuieto = 0f;
            animator.ResetTrigger(PARAM_FIDGET);
        }
    }

    private void ActualizarAnimacionAire()
    {
        if (animator == null || rb == null)
            return;

        animator.SetBool(PARAM_EN_SUELO, estaEnSuelo);
        animator.SetFloat(PARAM_VELOCIDAD_Y, rb.linearVelocity.y);
    }

    private void ActualizarRotacionModelo()
    {
        if (modeloVisual == null)
            return;

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);

        if (inputMovimiento.magnitude <= deadzoneAnimacion)
            return;

        float anguloObjetivo = Mathf.Atan2(inputX, inputZ) * Mathf.Rad2Deg;
        Quaternion rotacionObjetivo = Quaternion.Euler(0f, anguloObjetivo + offsetRotacionModelo, 0f);

        modeloVisual.localRotation = Quaternion.Slerp(
            modeloVisual.localRotation,
            rotacionObjetivo,
            velocidadRotacionModelo * Time.deltaTime
        );
    }

    private void ActualizarEfectoPasos()
    {
        if (!estaEnSuelo)
        {
            DetenerEfectoPasos();
            return;
        }

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);
        bool estaCaminando = inputMovimiento.magnitude > deadzoneAnimacion;

        if (!estaCaminando)
        {
            DetenerEfectoPasos();
            return;
        }

        ReproducirEfecto(efectoPasosIzquierdo);
        ReproducirEfecto(efectoPasosDerecho);
    }

    private void CambiarTipoSuelo(Suelo nuevoSuelo)
    {
        if (nuevoSuelo == null || nuevoSuelo.tipo == null)
            return;

        sueloActual = nuevoSuelo;

        if (tipoSueloActual == nuevoSuelo.tipo)
            return;

        tipoSueloActual = nuevoSuelo.tipo;
        DestruirEfectosPasos();

        if (tipoSueloActual.efectoVisualCaminar != null)
        {
            efectoPasosIzquierdo = CrearEfectoPasos(puntoPieIzquierdo);
            efectoPasosDerecho = CrearEfectoPasos(puntoPieDerecho);

            Debug.Log("Suelo actual: " + tipoSueloActual.nombre);
        }
    }

    private ParticleSystem CrearEfectoPasos(Transform punto)
    {
        if (punto == null || tipoSueloActual == null || tipoSueloActual.efectoVisualCaminar == null)
            return null;

        GameObject nuevoObjetoEfecto = Instantiate(
            tipoSueloActual.efectoVisualCaminar,
            punto,
            false
        );

        nuevoObjetoEfecto.transform.localPosition = Vector3.zero;

        ParticleSystem particulas = nuevoObjetoEfecto.GetComponent<ParticleSystem>();

        if (particulas == null)
        {
            particulas = nuevoObjetoEfecto.GetComponentInChildren<ParticleSystem>();
        }

        if (particulas != null)
        {
            particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        return particulas;
    }

    private void DestruirEfectosPasos()
    {
        if (efectoPasosIzquierdo != null)
        {
            Destroy(efectoPasosIzquierdo.gameObject);
            efectoPasosIzquierdo = null;
        }

        if (efectoPasosDerecho != null)
        {
            Destroy(efectoPasosDerecho.gameObject);
            efectoPasosDerecho = null;
        }
    }

    private void ReproducirEfecto(ParticleSystem efecto)
    {
        if (efecto != null && !efecto.isPlaying)
        {
            efecto.Play();
        }
    }

    private void DetenerEfecto(ParticleSystem efecto)
    {
        if (efecto != null && efecto.isPlaying)
        {
            efecto.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void DetenerEfectoPasos()
    {
        DetenerEfecto(efectoPasosIzquierdo);
        DetenerEfecto(efectoPasosDerecho);
    }

    private void ReproducirVFXImpulsoMorsa()
    {
        CrearVFXImpulsoEnPie(puntoPieIzquierdo);
        CrearVFXImpulsoEnPie(puntoPieDerecho);
    }

    private void CrearVFXImpulsoEnPie(Transform puntoPie)
    {
        if (vfxImpulsoMorsa == null || puntoPie == null)
            return;

        GameObject nuevoVFX = Instantiate(
            vfxImpulsoMorsa,
            puntoPie.position,
            vfxImpulsoMorsa.transform.rotation
        );

        ParticleSystem particulas = nuevoVFX.GetComponent<ParticleSystem>();

        if (particulas == null)
        {
            particulas = nuevoVFX.GetComponentInChildren<ParticleSystem>();
        }

        if (particulas != null)
        {
            particulas.Play();
        }

        Destroy(nuevoVFX, duracionVFXImpulsoMorsa);
    }

    private void ConfigurarAudioSourcesPlayer()
    {
        AudioMixerGroup grupoPasosFinal = grupoMixerPasos != null ? grupoMixerPasos : grupoMixerPlayer;

        audioSourcePieIzquierdo = ConfigurarAudioSource(audioSourcePieIzquierdo, puntoPieIzquierdo, "Audio_Pie_Izquierdo", grupoPasosFinal);
        audioSourcePieDerecho = ConfigurarAudioSource(audioSourcePieDerecho, puntoPieDerecho, "Audio_Pie_Derecho", grupoPasosFinal);
        audioSourceSaltoGeneral = ConfigurarAudioSource(audioSourceSaltoGeneral, transform, "Audio_Salto_General", grupoMixerPlayer);
    }

    private AudioSource ConfigurarAudioSource(AudioSource source, Transform punto, string nombre, AudioMixerGroup grupoMixer)
    {
        if (punto == null)
        {
            punto = transform;
        }

        if (source == null)
        {
            source = punto.GetComponent<AudioSource>();
        }

        if (source == null)
        {
            GameObject objetoAudio = new GameObject(nombre);
            objetoAudio.transform.SetParent(punto, false);
            objetoAudio.transform.localPosition = Vector3.zero;
            source = objetoAudio.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = spatialBlendPlayer;
        source.dopplerLevel = 0f;

        if (grupoMixer != null)
        {
            source.outputAudioMixerGroup = grupoMixer;
        }

        return source;
    }

    public void ReproducirPasoIzquierdoDesdeAnimacion()
    {
        tiempoUltimoEventoPasoAnimacion = Time.time;
        ReproducirSonidoPaso(true);
    }

    public void ReproducirPasoDerechoDesdeAnimacion()
    {
        tiempoUltimoEventoPasoAnimacion = Time.time;
        ReproducirSonidoPaso(false);
    }

    private void ActualizarPasosAutomaticos()
    {
        if (audioJugadorBloqueado)
            return;

        if (!usarPasosAutomaticosSiNoHayEventos)
            return;

        if (!estaEnSuelo || tipoSueloActual == null)
        {
            temporizadorPasoAutomatico = 0f;
            return;
        }

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);

        if (inputMovimiento.magnitude <= deadzoneAnimacion)
        {
            temporizadorPasoAutomatico = 0f;
            return;
        }

        // Si ya hay eventos reales de animacion llegando, no duplicamos pasos automaticos.
        if (Time.time - tiempoUltimoEventoPasoAnimacion < intervaloPasoAutomatico * 1.5f)
            return;

        temporizadorPasoAutomatico += Time.deltaTime;

        float intensidadMovimiento = Mathf.Clamp01(inputMovimiento.magnitude);
        float intervaloActual = Mathf.Lerp(intervaloPasoAutomatico * 1.15f, intervaloPasoAutomatico * 0.8f, intensidadMovimiento);

        if (temporizadorPasoAutomatico < intervaloActual)
            return;

        temporizadorPasoAutomatico = 0f;
        ReproducirSonidoPaso(siguientePasoAutomaticoIzquierdo);
        siguientePasoAutomaticoIzquierdo = !siguientePasoAutomaticoIzquierdo;
    }

    private void ReproducirSonidoPaso(bool pieIzquierdo)
    {
        if (audioJugadorBloqueado)
            return;

        if (tipoSueloActual == null)
            return;

        if (!estaEnSuelo)
            return;

        if (pasosSoloSiEstaCaminando)
        {
            Vector2 inputMovimiento = new Vector2(inputX, inputZ);

            if (inputMovimiento.magnitude <= deadzoneAnimacion)
                return;
        }

        AudioSource source = pieIzquierdo ? audioSourcePieIzquierdo : audioSourcePieDerecho;

        if (source == null)
            return;

        float tiempoUltimo = pieIzquierdo ? tiempoUltimoPasoIzquierdo : tiempoUltimoPasoDerecho;

        if (Time.time - tiempoUltimo < tiempoMinimoEntrePasosMismoPie)
            return;

        int ultimoIndice = pieIzquierdo ? ultimoIndicePasoIzquierdo : ultimoIndicePasoDerecho;
        int nuevoIndice;
        AudioClip clip = tipoSueloActual.ObtenerSonidoPasoAleatorio(ultimoIndice, out nuevoIndice);

        if (clip == null)
            return;

        if (pieIzquierdo)
        {
            ultimoIndicePasoIzquierdo = nuevoIndice;
            tiempoUltimoPasoIzquierdo = Time.time;
        }
        else
        {
            ultimoIndicePasoDerecho = nuevoIndice;
            tiempoUltimoPasoDerecho = Time.time;
        }

        ReproducirClipEnSource(source, clip, volumenPasos, true);
    }

    private void ReproducirSonidoSaltoCompleto()
    {
        if (audioJugadorBloqueado)
            return;

        if (reproducirSonidoSueloAlSaltar)
        {
            ReproducirSonidoSaltoSueloEnAmbosPies();
        }

        if (reproducirSonidoVientoAlSaltar)
        {
            ReproducirSonidoSaltoGeneral();
        }
    }

    private void ReproducirSonidoSaltoSueloEnAmbosPies()
    {
        if (tipoSueloActual == null)
            return;

        int nuevoIndice;
        AudioClip clip = tipoSueloActual.ObtenerSonidoSaltoAleatorio(ultimoIndiceSaltoSuelo, out nuevoIndice);

        if (clip == null)
            return;

        ultimoIndiceSaltoSuelo = nuevoIndice;

        ReproducirClipEnSource(audioSourcePieIzquierdo, clip, volumenSaltoSuelo, true);
        ReproducirClipEnSource(audioSourcePieDerecho, clip, volumenSaltoSuelo, true);
    }

    private void ReproducirSonidoCaidaEnAmbosPies()
    {
        if (audioJugadorBloqueado)
            return;

        if (!reproducirSonidoCaidaAlAterrizar)
            return;

        if (tipoSueloActual == null)
            return;

        int nuevoIndice;
        AudioClip clip = tipoSueloActual.ObtenerSonidoCaidaAleatorio(ultimoIndiceCaida, out nuevoIndice);

        if (clip == null)
            return;

        ultimoIndiceCaida = nuevoIndice;

        ReproducirClipEnSource(audioSourcePieIzquierdo, clip, volumenCaidaSuelo, true);
        ReproducirClipEnSource(audioSourcePieDerecho, clip, volumenCaidaSuelo, true);
    }

    private void ReproducirSonidoSaltoGeneral()
    {
        if (audioJugadorBloqueado)
            return;

        if (sonidosSalto == null || sonidosSalto.Length == 0 || audioSourceSaltoGeneral == null)
            return;

        AudioClip clip = ObtenerSonidoSaltoAleatorio();

        if (clip == null)
            return;

        ReproducirClipEnSource(audioSourceSaltoGeneral, clip, volumenSalto, false);
    }

    private AudioClip ObtenerSonidoSaltoAleatorio()
    {
        if (sonidosSalto == null || sonidosSalto.Length == 0)
            return null;

        if (sonidosSalto.Length == 1)
        {
            ultimoIndiceSalto = 0;
            return sonidosSalto[0];
        }

        int indice = Random.Range(0, sonidosSalto.Length);

        if (evitarRepetirMismoSalto)
        {
            int intentos = 0;

            while (indice == ultimoIndiceSalto && intentos < 10)
            {
                indice = Random.Range(0, sonidosSalto.Length);
                intentos++;
            }
        }

        ultimoIndiceSalto = indice;
        return sonidosSalto[indice];
    }

    private void ReproducirClipEnSource(AudioSource source, AudioClip clip, float volumen, bool aplicarPitch)
    {
        if (audioJugadorBloqueado)
            return;

        if (source == null || clip == null)
            return;

        if (aplicarPitch)
        {
            source.pitch = Random.Range(1f - variacionPitchPasos, 1f + variacionPitchPasos);
        }
        else
        {
            source.pitch = 1f;
        }

        source.PlayOneShot(clip, volumen);
    }


    /// <summary>
    /// Detiene inmediatamente los pasos, salto y caida de Guppy y evita que
    /// los Animation Events o los pasos automaticos vuelvan a reproducirlos.
    /// Se usa cuando la pantalla de derrota ya cubre toda la pantalla.
    /// </summary>
    public void SilenciarAudioPorDerrota()
    {
        audioJugadorBloqueado = true;
        temporizadorPasoAutomatico = 0f;

        DetenerAudioSource(audioSourcePieIzquierdo);
        DetenerAudioSource(audioSourcePieDerecho);
        DetenerAudioSource(audioSourceSaltoGeneral);

        DetenerEfectoPasos();
    }

    /// <summary>
    /// Permite volver a activar el audio del jugador si alguna secuencia
    /// necesita reutilizar el mismo Player sin cambiar de escena.
    /// </summary>
    public void ReactivarAudioJugador()
    {
        audioJugadorBloqueado = false;
    }

    private void DetenerAudioSource(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        source.pitch = 1f;
    }


    private void ActualizarTipoSueloPorRaycast()
    {
        if (!detectarTipoSueloPorRaycast)
            return;

        Vector3 origen = transform.position + Vector3.up * alturaOrigenRaycastSuelo;

        RaycastHit[] hits = Physics.SphereCastAll(
            origen,
            radioSphereCastSuelo,
            Vector3.down,
            distanciaRaycastSuelo,
            capasDeteccionSuelo,
            QueryTriggerInteraction.Ignore
        );

        if (hits == null || hits.Length == 0)
            return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            Collider colliderDetectado = hits[i].collider;

            if (colliderDetectado == null)
                continue;

            Player playerDetectado = colliderDetectado.GetComponentInParent<Player>();

            if (playerDetectado == this)
                continue;

            Suelo suelo = colliderDetectado.GetComponent<Suelo>();

            if (suelo == null)
            {
                suelo = colliderDetectado.GetComponentInParent<Suelo>();
            }

            if (suelo == null || suelo.tipo == null)
                continue;

            CambiarTipoSuelo(suelo);
            return;
        }
    }

    private bool IntentarDetectarSuelo(Collision collision)
    {
        bool contactoSuperior = false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contacto = collision.GetContact(i);

            if (contacto.normal.y > 0.5f)
            {
                contactoSuperior = true;
                break;
            }
        }

        if (!contactoSuperior)
            return false;

        Suelo suelo = collision.gameObject.GetComponent<Suelo>();

        if (suelo == null)
        {
            suelo = collision.gameObject.GetComponentInParent<Suelo>();
        }

        if (suelo == null || suelo.tipo == null)
            return false;

        estaEnSuelo = true;
        CambiarTipoSuelo(suelo);

        if (animator != null)
        {
            animator.SetBool(PARAM_EN_SUELO, true);
        }

        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Trampolin"))
        {
            RebotarEnMorsa(collision);
            return;
        }

        bool estabaEnSueloAntes = estaEnSuelo;
        bool detectoSuelo = IntentarDetectarSuelo(collision);

        if (detectoSuelo && !estabaEnSueloAntes)
        {
            ReproducirSonidoCaidaEnAmbosPies();
        }
    }

    private void RebotarEnMorsa(Collision collision)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpTrampolin, ForceMode.Impulse);

        ReproducirVFXImpulsoMorsa();

        if (reproducirSaltoTambienEnReboteMorsa)
        {
            ReproducirSonidoSaltoGeneral();
        }

        MorsaAnimacion morsaAnimacion = collision.gameObject.GetComponentInParent<MorsaAnimacion>();

        if (morsaAnimacion != null)
        {
            morsaAnimacion.ReproducirRebote();
        }

        estaEnSuelo = false;
        DetenerEfectoPasos();
        temporizadorQuieto = 0f;

        if (animator != null)
        {
            animator.ResetTrigger(PARAM_FIDGET);
            animator.SetBool(PARAM_EN_SUELO, false);
            animator.SetTrigger(PARAM_SALTAR);
        }

        if (tutorialManager != null)
        {
            tutorialManager.NotificarReboteEnMorsa();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Suelo sueloSalida = collision.gameObject.GetComponent<Suelo>();

        if (sueloSalida == null)
        {
            sueloSalida = collision.gameObject.GetComponentInParent<Suelo>();
        }

        if (sueloActual != null && sueloSalida == sueloActual)
        {
            sueloActual = null;
            DetenerEfectoPasos();
        }
    }
}
