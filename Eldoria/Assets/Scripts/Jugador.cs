using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Jugador : MonoBehaviour
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

    public Arma armaActual; // La referencia al arma actual
    public Arma espada1;
    public Arma espada2;
    public Arma espada3;
    public Arma hacha1;
    public Arma hacha2;
    public Arma hacha3;
    public Arma escudo1;
    public Arma escudo2;

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
    public float resistencia;
    public float salud;


    void Start()
    {
        ultimoTiempoSalto = -Mathf.Infinity; // Inicializamos en un valor negativo grande para asegurarnos de que haya pasado suficiente tiempo desde el inicio del juego.
        alturaOriginal = capsuleCollider.height;
        centroOriginal = capsuleCollider.center;

        ocultarArma(espada_1);
        ocultarArma(espada_2);
        ocultarArma(espada_3);
        ocultarArma(hacha_1);
        ocultarArma(hacha_2);
        ocultarArma(hacha_3);
        ocultarArma(escudo_1);
        ocultarArma(escudo_2);

    }

    // Update is called once per frame
    void Update()
    {
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");

        transform.Rotate(0, x * Time.deltaTime * VelocidadGiroCaminando, 0);
        transform.Translate(0, 0, y * Time.deltaTime * VelocidadCaminar);

        animator.SetFloat("VelX", x);
        animator.SetFloat("VelY", y);

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            animator.Play("Bloquear");
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.Play("Ataque");
        }
     

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // SeleccionarArma(espada1);
            mostrarArma(espada_1);
        }
        // Ocultar la espada cuando se suelta el clic izquierdo del ratón
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            ocultarArma(espada_1);
        }

        // Verificar si se presiona la tecla Shift para cambiar entre caminar y correr
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.Play("Correr");

            transform.Rotate(0, x * Time.deltaTime * VelocidadGiro, 0);
            transform.Translate(0, 0, y * Time.deltaTime * VelocidadCarrera);

            animator.SetFloat("VelX", x);
            animator.SetFloat("VelY", y);

            animator.SetBool("Correr", true);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            animator.Play("Caminar");

            animator.SetBool("Correr", false);
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            animator.Play("Esquivar");
            capsuleCollider.height = alturaOriginal * 0.5f;
            capsuleCollider.center = centroOriginal * 0.5f;
            Invoke("RestaurarCapsuleCollider", 1.8f);
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
            animator.Play("Saltar");
            Invoke("Saltar", 0.45f);
            estaSaltando = false;
            TiempoSaltoActual = 0f;
            ultimoTiempoSalto = Time.time;
        }

        // Control de la duración del salto
        if (Input.GetKey(KeyCode.Space) && estaSaltando && TiempoSaltoActual < TiempoMaximoSalto)
        {
            rb.AddForce(Vector3.up * AlturaSalto * Time.deltaTime, ForceMode.Impulse);
            TiempoSaltoActual += Time.deltaTime;
        }
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

    void SeleccionarArma(Arma nuevaArma)
    {

        armaActual = nuevaArma;
        // Actualizar las estadísticas del jugador según el arma seleccionada
        daño = nuevaArma.daño;
        // Puedes hacer lo mismo con otras estadísticas del jugado

        // Activar solo el objeto del arma seleccionada
        if (nuevaArma == espada1)
        {
            mostrarArma(espada_1);
        }
        else if (nuevaArma == espada2)
        {
            mostrarArma(espada_2);
        }
        else if (nuevaArma == espada3)
        {
            mostrarArma(espada_3);
        }
        else if (nuevaArma == hacha1)
        {
            mostrarArma(hacha_1);
        }
        else if (nuevaArma == hacha2)
        {
            mostrarArma(hacha_2);
        }
        else if (nuevaArma == hacha3)
        {
            mostrarArma(hacha_3);
        }
        else if (nuevaArma == escudo1)
        {
            mostrarArma(escudo_1);
        }
        else if (nuevaArma == escudo2)
        {
            mostrarArma(escudo_2);
        }
    }

    public void mostrarArma(GameObject arma)
    {
        // Obtenemos el componente Renderer del arma
        espadaRenderer = arma.GetComponent<Renderer>();
        espadaRenderer.enabled = true;
    }

    public void ocultarArma(GameObject arma)
    {
        // Obtenemos el componente Renderer del arma
        espadaRenderer = arma.GetComponent<Renderer>();
        espadaRenderer.enabled = false;
    }
}

[System.Serializable]
public class Arma
{
    public string nombre;
    public float daño;
    public float velocidadAtaque;
    // Puedes agregar más atributos según sea necesario

    public Arma(string nombre, float daño, float velocidadAtaque)
    {
        this.nombre = nombre;
        this.daño = daño;
        this.velocidadAtaque = velocidadAtaque;
    }

    void OnTriggerEnter(Collider other)
    {
        // Verifica si el collider con el que colisionamos pertenece a un enemigo
        if (other.CompareTag("Enemigo"))
        {
            // Acciones que realizas cuando el arma colisiona con un enemigo
            Personaje enemigo = other.GetComponent<Personaje>(); // Obtén la referencia al script del enemigo
            if (enemigo != null)
            {
                // Realiza las acciones necesarias en el enemigo, como aplicar daño
                enemigo.RecibirDanio(daño);
            }
        }
    }
}

