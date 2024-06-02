using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    public InputField Usuario;
    public InputField Contrasena;
    public Text Errores; // Un campo de texto para mostrar mensajes al usuario

    private string loginUrl = "https://eldoria-elorbeperdido.000webhostapp.com/apiDB/Login.php";
    private string registerUrl = "https://eldoria-elorbeperdido.000webhostapp.com/apiDB/Register.php";

    // Start is called before the first frame update
    void Start()
    {
        Errores.text = "";
    }

    public void Iniciar()
    {
        StartCoroutine(LoginCoroutine());
    }

    IEnumerator LoginCoroutine()
    {
        if (string.IsNullOrEmpty(Usuario.text) || string.IsNullOrEmpty(Contrasena.text))
        {
            Errores.text = "Rellene los campos nombre de usuario y contraseña.";
        }
        else
        {
            // Recoger los valores de los InputField
            string nombreUsuario = Usuario.text;
            string contrasena = Contrasena.text;

            // Crear el formulario para enviar los datos al servidor
            WWWForm form = new WWWForm();
            form.AddField("nombre_usuario", nombreUsuario);
            form.AddField("contrasena", contrasena);

            // Enviar la solicitud POST al servidor
            using (UnityWebRequest www = UnityWebRequest.Post(loginUrl, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    Errores.text = "Error de conexión: " + www.error;
                }
                else
                {
                    // Procesar la respuesta del servidor
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log("Respuesta del servidor: " + jsonResponse);

                    // Parsear la respuesta JSON
                    Response response = JsonUtility.FromJson<Response>(jsonResponse);

                    if (response.success)
                    {
                        // Guardar el nombre de usuario en la variable estática
                        UserSession.NombreUsuario = nombreUsuario;

                        // Inicio de sesión exitoso
                        Errores.text = "";
                        // Cambiar de escena
                        SceneManager.LoadScene("Inicio");
                    }
                    else
                    {
                        // Mostrar mensaje de error
                        Errores.text = response.message;
                    }
                }
            }
        }
    }

    IEnumerator RegisterCoroutine()
    {
        if(string.IsNullOrEmpty(Usuario.text) || string.IsNullOrEmpty(Contrasena.text))
        {
            Errores.text = "Rellene los campos nombre de usuario y contraseña.";
        }
        else
        {
            // Recoger los valores de los InputField
            string nombreUsuario = Usuario.text;
            string contrasena = Contrasena.text;

            // Crear el formulario para enviar los datos al servidor
            WWWForm form = new WWWForm();
            form.AddField("nombre_usuario", nombreUsuario);
            form.AddField("contrasena", contrasena);

            // Enviar la solicitud POST al servidor
            using (UnityWebRequest www = UnityWebRequest.Post(registerUrl, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    Errores.text = "Error de conexión: " + www.error;
                }
                else
                {
                    // Procesar la respuesta del servidor
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log("Respuesta del servidor: " + jsonResponse);

                    // Parsear la respuesta JSON
                    Response response = JsonUtility.FromJson<Response>(jsonResponse);

                    if (response.success)
                    {
                        // Registro exitoso
                        UserSession.NombreUsuario = nombreUsuario;

                        Errores.text = "";

                        SceneManager.LoadScene("Inicio");
                    }
                    else
                    {
                        // Mostrar mensaje de error
                        Errores.text = response.message;
                    }
                }
            }
        }
    }

    // Clase para mapear la respuesta JSON del servidor
    [System.Serializable]
    public class Response
    {
        public bool success;
        public string message;
    }

    public void Registrar()
    {
        StartCoroutine(RegisterCoroutine());
    }

    public void Salir()
    {
        Application.Quit();
    }
}
