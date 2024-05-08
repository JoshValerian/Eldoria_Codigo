using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{
    public Button continuar; // Referencia al botón de continuar en el Inspector de Unity

    void Start()
    {
        // Obtener la última escena visitada por el jugador
        int lastVisitedScene = PlayerPrefs.GetInt("LastVisitedScene", 0);

        // Si el jugador ha visitado alguna escena, habilitar el botón de continuar
        if (lastVisitedScene != 0)
        {
            continuar.interactable = true;
        }
        else
        {
            continuar.interactable = false;
        }
    }

    public void Historia()
    {
        // Cargar la escena de historia
        SceneManager.LoadScene("Historia");
    }

    public void Continuar()
    {
        // Obtener la última escena visitada por el jugador
        int lastVisitedScene = PlayerPrefs.GetInt("LastVisitedScene", 0);

        // Cargar la última escena visitada por el jugador
        SceneManager.LoadScene(lastVisitedScene);
    }

    public void Arena()
    {
        // Cargar la escena de arena
        SceneManager.LoadScene("PruebasArena");
    }

    public void Salir()
    {
        Application.Quit();
    }
}
