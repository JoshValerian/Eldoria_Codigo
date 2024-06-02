using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Jugador : MonoBehaviour
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
    public Text nomUsu;
    public float resBloqueo;
    public float resAtaque;
    public float resEsquiva;
    public float resSalto;

    public Canvas inventario;
    public bool invStat;

    public CapsuleCollider ccEspada_1;
    public CapsuleCollider ccEspada_2;
    public CapsuleCollider ccEspada_3;
    public CapsuleCollider ccHacha_1;
    public CapsuleCollider ccHacha_2;
    public CapsuleCollider ccHacha_3;
    public CapsuleCollider ccEscudo_1;
    public CapsuleCollider ccEscudo_2;

    public CapsuleCollider salida;

    void Start()
    {
        // Guardar la escena de historia visitada por el jugador
        PlayerPrefs.SetInt("LastVisitedScene", SceneManager.GetActiveScene().buildIndex);

        ultimoTiempoSalto = -Mathf.Infinity; // Inicializamos en un valor negativo grande para asegurarnos de que haya pasado suficiente tiempo desde el inicio del juego.
        alturaOriginal = capsuleCollider.height;
        centroOriginal = capsuleCollider.center;

        resistenciaActual = resistenciaMax;
        vidaActual = vidaMax;

        inventario.enabled = false;
        invStat = true;

        nomUsu.text = UserSession.NombreUsuario;

        resBloqueo = 10;
        resAtaque = 10;
        resEsquiva = 10;
        resSalto = 10;

        StartCoroutine(RegenerarResistencia());
        StartCoroutine(RegenerarVida());

        ocultarArmas();
        ocultarEscudos();

    }

    // Update is called once per frame
    void Update()
    {
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");

        transform.Rotate(0, x * Time.deltaTime * VelocidadGiroCaminando, 0);
        transform.Translate(0, 0, y * Time.deltaTime * VelocidadCaminar);

        animatorJugador.SetFloat("VelX", x);
        animatorJugador.SetFloat("VelY", y);

        //Mostrar inventario
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (invStat)
            {
                inventario.enabled = true;
                invStat = false;
            }
            else
            {
                inventario.enabled = false;
                invStat = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (resistenciaActual > 0 && !IsAnimationPlaying("Bloquear"))
            {
                animatorJugador.Play("Bloquear");
                resistenciaActual = resistenciaActual - resBloqueo;
                actualizarResistencia();
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (resistenciaActual > 0 && !IsAnimationPlaying("Ataque") && !inventario.enabled)
            {
                animatorJugador.Play("Ataque");
                resistenciaActual = resistenciaActual - resAtaque;
                actualizarResistencia();
            }
        }

        // Verificar si se presiona la tecla Shift para cambiar entre caminar y correr
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (resistenciaActual > 0 && !IsAnimationPlaying("Correr"))
            {
                animatorJugador.Play("Correr");

                transform.Rotate(0, x * Time.deltaTime * VelocidadGiro, 0);
                transform.Translate(0, 0, y * Time.deltaTime * VelocidadCarrera);

                animatorJugador.SetFloat("VelX", x);
                animatorJugador.SetFloat("VelY", y);

                animatorJugador.SetBool("Correr", true);
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            animatorJugador.Play("Caminar");

            animatorJugador.SetBool("Correr", false);
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            if (resistenciaActual > 0 && !IsAnimationPlaying("Esquivar"))
            {
                animatorJugador.Play("Esquivar");
                capsuleCollider.height = alturaOriginal * 0.5f;
                capsuleCollider.center = centroOriginal * 0.5f;
                Invoke("RestaurarCapsuleCollider", 1.8f);

                resistenciaActual = resistenciaActual - resEsquiva;
                actualizarResistencia();
            }
        }

        // Verificar si se presiona la tecla ctrl para cambiar entre caminar y agacharse
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            animatorJugador.Play("ArribaAgachado");
            capsuleCollider.height = alturaOriginal * 0.5f;
            capsuleCollider.center = centroOriginal * 0.5f;
            animatorJugador.SetBool("Correr", false);
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            animatorJugador.Play("AgachadoArriba");
            RestaurarCapsuleCollider();
            animatorJugador.SetBool("Correr", false);
        }


        estaSaltando = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (Input.GetKeyDown(KeyCode.Space) && estaSaltando && Time.time - ultimoTiempoSalto > 2.34f)
        {
            if (resistenciaActual > 0 && !IsAnimationPlaying("Saltar"))
            {
                animatorJugador.Play("Saltar");
                Invoke("Saltar", 0.45f);
                estaSaltando = false;
                TiempoSaltoActual = 0f;
                ultimoTiempoSalto = Time.time;

                resistenciaActual = resistenciaActual - resSalto;
                actualizarResistencia();
            }
        }

        // Control de la duración del salto
        if (Input.GetKey(KeyCode.Space) && estaSaltando && TiempoSaltoActual < TiempoMaximoSalto)
        {
            rb.AddForce(Vector3.up * AlturaSalto * Time.deltaTime, ForceMode.Impulse);
            TiempoSaltoActual += Time.deltaTime;
        }
    }

    bool IsAnimationPlaying(string animationName)
    {
        return animatorJugador.GetCurrentAnimatorStateInfo(0).IsName(animationName) && animatorJugador.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;    
    }

    bool IsAnimationPlayingNPC(string animationName)
    {
        return animatorNPC.GetCurrentAnimatorStateInfo(0).IsName(animationName) && animatorNPC.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
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

    //Metodo para ocultar el equipo
    public void ocultarArma(GameObject arma, CapsuleCollider ccArma)
    {
        // Obtenemos el componente Renderer del arma
        espadaRenderer = arma.GetComponent<Renderer>();
        espadaRenderer.enabled = false;
        ccArma.enabled = false;
    }

    public void Saltar()
    {
        rb.AddForce(Vector3.up * AlturaSalto, ForceMode.Impulse);
    }

    void RestaurarCapsuleCollider()
    {
        capsuleCollider.height = alturaOriginal;
        capsuleCollider.center = centroOriginal;
    }

    //Rutinas
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
            yield return new WaitForSeconds(5);

            if (vidaActual < vidaMax)
            {
                vidaActual += 10;
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
        // Calcular la fracción de resistencia actual respecto a la máxima
        float fillRatio = resistenciaActual / resistenciaMax;

        // Ajustar la escala de la barra de resistencia en el eje X para representar la fracción calculada
        barraResistencia.transform.localScale = new Vector3(fillRatio, barraResistencia.transform.localScale.y, barraResistencia.transform.localScale.z);
    }
    void actualizarVida()
    {
        if (vidaActual <= 0)
        {
            UserSession.Resultado = "¡Has perdido!";
            SceneManager.LoadScene("Resultado");
        }

        // Calcular la fracción de resistencia actual respecto a la máxima
        float fillRatio = vidaActual / vidaMax;

        // Ajustar la escala de la barra de resistencia en el eje X para representar la fracción calculada
        barraVida.transform.localScale = new Vector3(fillRatio, barraVida.transform.localScale.y, barraVida.transform.localScale.z);
    }

    public void mostrarArma(GameObject arma, CapsuleCollider ccArma)
    {
        // Obtenemos el componente Renderer del arma
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

    public void Salir()
    {
        SceneManager.LoadScene("Inicio");
    }

    void OnTriggerEnter(Collider other)
    {
        if(other == salida)
        {
            SceneManager.LoadScene("Ciudad");
        }
        if (IsAnimationPlayingNPC("Bloquear"))
        {
            //Al entrar en contacto el escudo se bloquea no hacer nada.
            return;
        }
        else if ((other == ccEspada_1 || other == ccEspada_2 || other == ccEspada_3 ||
        other == ccHacha_1 || other == ccHacha_2 || other == ccHacha_3) && IsAnimationPlayingNPC("Ataque"))
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
}