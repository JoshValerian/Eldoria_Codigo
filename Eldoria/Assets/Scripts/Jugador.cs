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

    public GameObject espada_1; // Cambiado de Object a GameObject
    private Renderer espadaRenderer; // Añadido para acceder al componente Renderer


    void Start()
    {
        ultimoTiempoSalto = -Mathf.Infinity; // Inicializamos en un valor negativo grande para asegurarnos de que haya pasado suficiente tiempo desde el inicio del juego.
        alturaOriginal = capsuleCollider.height;
        centroOriginal = capsuleCollider.center;

        // Obtenemos el componente Renderer de la espada
        espadaRenderer = espada_1.GetComponent<Renderer>();
        espadaRenderer.enabled = false; // Ocultamos la espada al inicio
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


        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            espadaRenderer.enabled = true; // Mostramos la espada
        }
        // Ocultar la espada cuando se suelta el clic izquierdo del ratón
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            espadaRenderer.enabled = false; // Ocultamos la espada
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
}
