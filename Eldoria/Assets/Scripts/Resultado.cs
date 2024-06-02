using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Resultado : MonoBehaviour
{
    public Text resultado;
    // Start is called before the first frame update
    void Start()
    {
        resultado.text = UserSession.Resultado;
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Volver()
    {
        // Cargar la escena de historia
        SceneManager.LoadScene("Inicio");
    }

    public void Reintentar()
    {
        // Obtener la última escena visitada por el jugador
        int lastVisitedScene = PlayerPrefs.GetInt("LastVisitedScene", 0);

        // Cargar la última escena visitada por el jugador
        SceneManager.LoadScene(lastVisitedScene);
    }
}
