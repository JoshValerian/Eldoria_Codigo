using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class JugadorOnline : MonoBehaviourPunCallbacks
{
    public float VelocidadCarrera = 7;
    public float VelocidadGiro = 250;
    public float VelocidadCaminar = 0.5f;
    public float VelocidadGiroCaminando = 50;
    public Animator animator;

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

    public Camera playerCamera;

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

    void Start()
    {       
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
        if (photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(true);

            x = Input.GetAxis("Horizontal");
            y = Input.GetAxis("Vertical");

            transform.Rotate(0, x * Time.deltaTime * VelocidadGiroCaminando, 0);
            transform.Translate(0, 0, y * Time.deltaTime * VelocidadCaminar);

            animator.SetFloat("VelX", x);
            animator.SetFloat("VelY", y);

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
                    animator.Play("Bloquear");
                    resistenciaActual = resistenciaActual - resBloqueo;
                    actualizarResistencia();
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (resistenciaActual > 0 && !IsAnimationPlaying("Ataque") && !inventario.enabled)
                {
                    animator.Play("Ataque");
                    resistenciaActual = resistenciaActual - resAtaque;
                    actualizarResistencia();
                }
            }

            // Verificar si se presiona la tecla Shift para cambiar entre caminar y correr
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (resistenciaActual > 0 && !IsAnimationPlaying("Correr"))
                {
                    animator.Play("Correr");

                    transform.Rotate(0, x * Time.deltaTime * VelocidadGiro, 0);
                    transform.Translate(0, 0, y * Time.deltaTime * VelocidadCarrera);

                    animator.SetFloat("VelX", x);
                    animator.SetFloat("VelY", y);

                    animator.SetBool("Correr", true);
                }
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                animator.Play("Caminar");

                animator.SetBool("Correr", false);
            }
            if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                if (resistenciaActual > 0 && !IsAnimationPlaying("Esquivar"))
                {
                    animator.Play("Esquivar");
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
                animator.Play("ArribaAgachado");
                capsuleCollider.height = alturaOriginal * 0.5f;
                capsuleCollider.center = centroOriginal * 0.5f;
                animator.SetBool("Correr", false);
            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                animator.Play("AgachadoArriba");
                RestaurarCapsuleCollider();
                animator.SetBool("Correr", false);
            }


            estaSaltando = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (Input.GetKeyDown(KeyCode.Space) && estaSaltando && Time.time - ultimoTiempoSalto > 2.34f)
            {
                if (resistenciaActual > 0 && !IsAnimationPlaying("Saltar"))
                {
                    animator.Play("Saltar");
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
        else
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

    bool IsAnimationPlaying(string animationName)
    {
        if (photonView.IsMine)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
        }
        else { return false; }
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
        if (photonView.IsMine)
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
    }

    IEnumerator RegenerarVida()
    {
        if (photonView.IsMine)
        {
            while (true)
            {
                yield return new WaitForSeconds(10);

                if (resistenciaActual < resistenciaMax)
                {
                    resistenciaActual += 5;
                    if (resistenciaActual > resistenciaMax)
                    {
                        resistenciaActual = resistenciaMax;
                    }
                    actualizarVida();
                }
            }
        }
    }

    void actualizarResistencia()
    {
        if (photonView.IsMine)
        {
            if (photonView.IsMine)
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
        }
    }
    void actualizarVida()
    {
        if (photonView.IsMine)
        {
            if (vidaActual < 0)
            {
                vidaActual = 0;
            }
            // Calcular la fracción de resistencia actual respecto a la máxima
            float fillRatio = vidaActual / vidaMax;

            // Ajustar la escala de la barra de resistencia en el eje X para representar la fracción calculada
            barraVida.transform.localScale = new Vector3(fillRatio, barraVida.transform.localScale.y, barraVida.transform.localScale.z);
        }
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
        if (photonView.IsMine)
        {
            ocultarArmas();
            mostrarArma(espada_1, ccEspada_1);
            resAtaque = 15;
        }
    }
    public void selE2()
    {
        if (photonView.IsMine)
        {
            ocultarArmas();
            mostrarArma(espada_2, ccEspada_2);
            resAtaque = 20;
        }
    }
    public void selE3()
    {
        if (photonView.IsMine)
        {
            ocultarArmas();
            mostrarArma(espada_3, ccEspada_3);
            resAtaque = 10;
        }
    }
    public void selH1()
    {
        if (photonView.IsMine)
        {
            ocultarArmas();
            mostrarArma(hacha_1, ccHacha_1);
            resAtaque = 30;
        }
    }
    public void selH2()
    {
        if (photonView.IsMine)
        {
            ocultarArmas();
            mostrarArma(hacha_2, ccHacha_2);
            resAtaque = 25;
        }
    }
    public void selH3()
    {
        if (photonView.IsMine)
        {
            ocultarArmas();
            mostrarArma(hacha_3, ccHacha_3);
            resAtaque = 15;
        }
    }
    public void selB1()
    {
        if (photonView.IsMine)
        {
            ocultarEscudos();
            mostrarArma(escudo_1, ccEscudo_1);
            resBloqueo = 15;
        }
    }
    public void selB2()
    {
        if (photonView.IsMine)
        {
            ocultarEscudos();
            mostrarArma(escudo_2, ccEscudo_2);
            resBloqueo = 10;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            if (other == ccEscudo_1 || other == ccEscudo_2)
            {
                //Al entrar en contacto el escudo se bloquea no hacer nada.
            }
            else if (other == ccEspada_1 || other == ccEspada_2 || other == ccEspada_3 ||
            other == ccHacha_1 || other == ccHacha_2 || other == ccHacha_3)
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

    public void Salir()
    {
        SceneManager.LoadScene("Inicio");
    }
}