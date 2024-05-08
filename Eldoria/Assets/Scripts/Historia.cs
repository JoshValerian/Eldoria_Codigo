using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Historia : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void VolverMenu()
    {
        // Guardar la escena de historia visitada por el jugador
        PlayerPrefs.SetInt("LastVisitedScene", SceneManager.GetActiveScene().buildIndex);

        // Cargar la escena del menú principal
        SceneManager.LoadScene("Inicio");
    }

    public void Prueba()
    {
        // Guardar la escena de historia visitada por el jugador
        PlayerPrefs.SetInt("LastVisitedScene", SceneManager.GetActiveScene().buildIndex);

        // Cargar la escena del menú principal
        SceneManager.LoadScene("Pruebas");
    }
}
