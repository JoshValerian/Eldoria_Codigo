using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Enemigo : MonoBehaviour
{
    public float VelocidadCarrera = 7;
    public float VelocidadGiro = 250;
    public float VelocidadCaminar = 0.5f;
    public float VelocidadGiroCaminando = 50;

    public Animator animatorJugador;
    public Animator animatorNPC;

    private float x, y;

    public Rigidbody rb;
    public float AlturaSalto = 10f;
    public float TiempoMaximoSalto = 0.5f;
    private float TiempoSaltoActual = 0f;
    public Transform groundCheck;
    public float groundDistance = 0.1f;
    public LayerMask groundMask;
    private bool estaSaltando;

    private float ultimoTiempoSalto;

    public CapsuleCollider capsuleCollider; // Referencia al CapsuleCollider del jugador
    private float alturaOriginal; // Almacenará la altura original del CapsuleCollider
    private Vector3 centroOriginal; // Almacenará el centro original del CapsuleCollider

    public GameObject espada_1; // Cambiado de Object a GameObject
    public GameObject espada_2;
    public GameObject espada_3;
    public GameObject hacha_1;
    public GameObject hacha_2;
    public GameObject hacha_3;
    public GameObject escudo_1;
    public GameObject escudo_2;
    private Renderer espadaRenderer; // Añadido para acceder al componente Renderer

    public float daño;
    public float bloqueo;

    public float resistenciaMax;
    public float resistenciaActual;
    public RawImage barraResistencia;
    public float vidaMax;
    public float vidaActual;
    public RawImage barraVida;
    public float resBloqueo;
    public float resAtaque;
    public float resEsquiva;
    public float resSalto;

    public CapsuleCollider ccEspada_1;
    public CapsuleCollider ccEspada_2;
    public CapsuleCollider ccEspada_3;
    public CapsuleCollider ccHacha_1;
    public CapsuleCollider ccHacha_2;
    public CapsuleCollider ccHacha_3;
    public CapsuleCollider ccEscudo_1;
    public CapsuleCollider ccEscudo_2;

    private float tiempoUltimaAccion;

    public Transform jugador; // Referencia al jugador

    public Collider campoDeVision;
    private bool visible = false;
    public LayerMask capaObstaculos;

    public AudioSource bosque;
    public AudioSource batalla;



    void Start()
    {
        batalla.enabled = false;

        ultimoTiempoSalto = -Mathf.Infinity; // Inicializamos en un valor negativo grande para asegurarnos de que haya pasado suficiente tiempo desde el inicio del juego.
        alturaOriginal = capsuleCollider.height;
        centroOriginal = capsuleCollider.center;

        resistenciaActual = resistenciaMax;
        vidaActual = vidaMax;

        resBloqueo = 10;
        resAtaque = 10;
        resEsquiva = 10;
        resSalto = 10;

        StartCoroutine(RegenerarResistencia());
        StartCoroutine(RegenerarVida());

        ocultarArmas();
        ocultarEscudos();

        // Selecciona un arma y escudo al azar
        SeleccionarArmaAleatoria();
        SeleccionarEscudoAleatorio();

        // Configura el tiempo de la última acción
        tiempoUltimaAccion = Time.time;
    }

    void Update()
    {
        // Movimiento automático del NPC hacia el jugador
        MovimientoHaciaJugador();

        // Verifica el tiempo para realizar una nueva acción
        if (Time.time - tiempoUltimaAccion > Random.Range(1.0f, 3.0f))
        {
            RealizarAccionAleatoria();
            tiempoUltimaAccion = Time.time;
        }

        estaSaltando = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Control de la duración del salto
        if (estaSaltando && TiempoSaltoActual < TiempoMaximoSalto)
        {
            rb.AddForce(Vector3.up * AlturaSalto * Time.deltaTime, ForceMode.Impulse);
            TiempoSaltoActual += Time.deltaTime;
        }
    }

    void MovimientoHaciaJugador()
    {
        if (jugador == null || !visible) return;

        Vector3 direccion = jugador.position - transform.position;
        direccion.y = 0; // Mantener la dirección en el plano horizontal

        if (!Physics.Raycast(transform.position, direccion.normalized, direccion.magnitude, capaObstaculos))
        {
            if (direccion.magnitude > 0.1f)
            {
                // Rotar hacia el jugador
                Quaternion rotacion = Quaternion.LookRotation(direccion);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacion, Time.deltaTime * VelocidadGiroCaminando);

                // Moverse hacia el jugador
                transform.position = Vector3.MoveTowards(transform.position, jugador.position, VelocidadCaminar * Time.deltaTime);

                // Actualizar los parámetros del animador
                animatorNPC.SetFloat("VelX", 0); // No hay movimiento lateral en este caso
                animatorNPC.SetFloat("VelY", 1); // Movimiento hacia adelante
            }
            else
            {
                // Si está muy cerca del jugador, no moverse
                animatorNPC.SetFloat("VelX", 0);
                animatorNPC.SetFloat("VelY", 0);
            }
        }
    }

    void RealizarAccionAleatoria()
    {
        if (!visible) return;

        int accion = Random.Range(0, 6);
        switch (accion)
        {
            case 0:
                Esquivar();
                break;
            case 1:
                Atacar();
                break;
            case 2:
                Bloquear();
                break;
            case 3:
                Atacar();
                break;
            case 4:
                Atacar();
                break;
            case 5:
                Atacar();
                break;
        }
    }

    void SeleccionarArmaAleatoria()
    {
        int arma = Random.Range(0, 6);
        switch (arma)
        {
            case 0:
                selE1();
                break;
            case 1:
                selE2();
                break;
            case 2:
                selE3();
                break;
            case 3:
                selH1();
                break;
            case 4:
                selH2();
                break;
            case 5:
                selH3();
                break;
        }
    }

    void SeleccionarEscudoAleatorio()
    {
        int escudo = Random.Range(0, 2);
        switch (escudo)
        {
            case 0:
                selB1();
                break;
            case 1:
                selB2();
                break;
        }
    }

    void Esquivar()
    {
        if (resistenciaActual > 0 && !IsAnimationPlaying("Esquivar"))
        {
            animatorNPC.Play("Esquivar");
            capsuleCollider.height = alturaOriginal * 0.5f;
            capsuleCollider.center = centroOriginal * 0.5f;
            Invoke("RestaurarCapsuleCollider", 1.8f);

            resistenciaActual = resistenciaActual - resEsquiva;
            actualizarResistencia();
        }
    }

    void Atacar()
    {
        if (resistenciaActual > 0 && !IsAnimationPlaying("Ataque"))
        {
            animatorNPC.Play("Ataque");
            resistenciaActual = resistenciaActual - resAtaque;
            actualizarResistencia();
        }
    }

    void Bloquear()
    {
        if (resistenciaActual > 0 && !IsAnimationPlaying("Bloquear"))
        {
            animatorNPC.Play("Bloquear");
            resistenciaActual = resistenciaActual - resBloqueo;
            actualizarResistencia();
        }
    }

    void Saltar()
    {
        if (estaSaltando && Time.time - ultimoTiempoSalto > 2.34f)
        {
            if (resistenciaActual > 0 && !IsAnimationPlaying("Saltar"))
            {
                animatorNPC.Play("Saltar");
                rb.AddForce(Vector3.up * AlturaSalto, ForceMode.Impulse);
                estaSaltando = false;
                TiempoSaltoActual = 0f;
                ultimoTiempoSalto = Time.time;

                resistenciaActual = resistenciaActual - resSalto;
                actualizarResistencia();
            }
        }
    }

    bool IsAnimationPlaying(string animationName)
    {
        return animatorNPC.GetCurrentAnimatorStateInfo(0).IsName(animationName) && animatorNPC.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }

    bool IsAnimationPlayingJugador(string animationName)
    {
        return animatorJugador.GetCurrentAnimatorStateInfo(0).IsName(animationName) && animatorJugador.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }

    public void ocultarArmas()
    {
        ocultarArma(espada_1, ccEspada_1);
        ocultarArma(espada_2, ccEspada_2);
        ocultarArma(espada_3, ccEspada_3);
        ocultarArma(hacha_1, ccHacha_1);
        ocultarArma(hacha_2, ccHacha_2);
        ocultarArma(hacha_3, ccHacha_3);
    }

    public void ocultarEscudos()
    {
        ocultarArma(escudo_1, ccEscudo_1);
        ocultarArma(escudo_2, ccEscudo_2);
    }

    public void ocultarArma(GameObject arma, CapsuleCollider ccArma)
    {
        espadaRenderer = arma.GetComponent<Renderer>();
        espadaRenderer.enabled = false;
        ccArma.enabled = false;
    }

    void RestaurarCapsuleCollider()
    {
        capsuleCollider.height = alturaOriginal;
        capsuleCollider.center = centroOriginal;
    }

    IEnumerator RegenerarResistencia()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (resistenciaActual < resistenciaMax)
            {
                resistenciaActual += 2;
                if (resistenciaActual > resistenciaMax)
                {
                    resistenciaActual = resistenciaMax;
                }
                actualizarResistencia();
            }
        }
    }

    IEnumerator RegenerarVida()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);

            if (vidaActual < vidaMax)
            {
                vidaActual += 5;
                if (vidaActual > vidaMax)
                {
                    vidaActual = vidaMax;
                }
                actualizarVida();
            }
        }
    }

    void actualizarResistencia()
    {
        if (resistenciaActual < 0)
        {
            resistenciaActual = 0;
        }
        float fillRatio = resistenciaActual / resistenciaMax;
        barraResistencia.transform.localScale = new Vector3(fillRatio, barraResistencia.transform.localScale.y, barraResistencia.transform.localScale.z);
    }

    void actualizarVida()
    {
        if (vidaActual <= 0)
        {
            Destroy(gameObject);
            if (gameObject.CompareTag("Boss"))
            {
                UserSession.Resultado = "¡Has ganado!";
                SceneManager.LoadScene("Resultado");
            }
        }
        float fillRatio = vidaActual / vidaMax;
        barraVida.transform.localScale = new Vector3(fillRatio, barraVida.transform.localScale.y, barraVida.transform.localScale.z);
    }

    public void mostrarArma(GameObject arma, CapsuleCollider ccArma)
    {
        espadaRenderer = arma.GetComponent<Renderer>();
        espadaRenderer.enabled = true;
        ccArma.enabled = true;
    }

    public void selE1()
    {
        ocultarArmas();
        mostrarArma(espada_1, ccEspada_1);
        resAtaque = 15;
    }

    public void selE2()
    {
        ocultarArmas();
        mostrarArma(espada_2, ccEspada_2);
        resAtaque = 20;
    }

    public void selE3()
    {
        ocultarArmas();
        mostrarArma(espada_3, ccEspada_3);
        resAtaque = 10;
    }

    public void selH1()
    {
        ocultarArmas();
        mostrarArma(hacha_1, ccHacha_1);
        resAtaque = 30;
    }

    public void selH2()
    {
        ocultarArmas();
        mostrarArma(hacha_2, ccHacha_2);
        resAtaque = 25;
    }

    public void selH3()
    {
        ocultarArmas();
        mostrarArma(hacha_3, ccHacha_3);
        resAtaque = 15;
    }

    public void selB1()
    {
        ocultarEscudos();
        mostrarArma(escudo_1, ccEscudo_1);
        resBloqueo = 15;
    }

    public void selB2()
    {
        ocultarEscudos();
        mostrarArma(escudo_2, ccEscudo_2);
        resBloqueo = 10;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 direccion = other.transform.position - transform.position;
            direccion.y = 0; // Mantener la dirección en el plano horizontal

            // Verificar si hay una línea de visión clara
            if (!Physics.Raycast(transform.position, direccion.normalized, direccion.magnitude, capaObstaculos))
            {
                visible = true;

                bosque.enabled = false;
                batalla.enabled = true;
            }
        }

        if (IsAnimationPlayingJugador("Bloquear"))
        {
            //Al entrar en contacto el escudo se bloquea no hacer nada.
            return;
        }
        else if ((other == ccEspada_1 || other == ccEspada_2 || other == ccEspada_3 ||
        other == ccHacha_1 || other == ccHacha_2 || other == ccHacha_3) && IsAnimationPlayingJugador("Ataque"))
        {
            //Daño en funcion del arma
            if (other == ccEspada_1)
            {
                daño = 20;
            }
            else if (other == ccEspada_2)
            {
                daño = 30;
            }
            else if (other == ccEspada_3)
            {
                daño = 15;
            }
            else if (other == ccHacha_1)
            {
                daño = 60;
            }
            else if (other == ccHacha_2)
            {
                daño = 40;
            }
            else if (other == ccHacha_3)
            {
                daño = 25;
            }
            vidaActual = vidaActual - daño;
            actualizarVida();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            visible = false;

            bosque.enabled = true;
            batalla.enabled = false;
        }
    }
}
