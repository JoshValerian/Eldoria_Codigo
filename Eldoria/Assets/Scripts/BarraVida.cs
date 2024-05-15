using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    public float vidaMax;
    public float vidaActual;
    public Image barraVida;
    // Start is called before the first frame update
    void Start()
    {
        vidaActual = vidaMax;
    }

    // Update is called once per frame
    void Update()
    {
        actualizarVida();
        if(vidaActual <= 0)
        {
            gameObject.SetActive(false);
            Invoke("aparecer", 10f); // Desactiva el objeto durante 10 segundos
        }
    }

    void actualizarVida()
    {
        barraVida.fillAmount = vidaActual / vidaMax;
    }

    void aparecer()
    {
        gameObject.SetActive(true); // Reactiva el objeto después de 10 segundos
        vidaActual = vidaMax; // Restaura la vida
        actualizarVida(); // Actualiza la barra de vida
    }
}
