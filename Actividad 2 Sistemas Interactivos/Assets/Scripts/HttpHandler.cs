using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public class HttpHandler : MonoBehaviour
{
    [SerializeField] GameObject login,scoreboard;
    [SerializeField] string ServerApiURL;

    public string Token { get; set; }
    public string Username { get; set; }

    [SerializeField] string token;

    public TMP_Text[] Positions;

    public TMP_Text Name,ErrorName;


    void Start()
    {
        InitialSetup();
    }

    public void Register()
    {
        User user = new User();
        user.username = GameObject.Find("Username").GetComponent<InputField>().text;
        user.password = GameObject.Find("Password").GetComponent<InputField>().text;
        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Registry(postData));
    }

    public void Login()
    {
        User user = new User();
        user.username = GameObject.Find("Username").GetComponent<InputField>().text;
        user.password = GameObject.Find("Password").GetComponent<InputField>().text;
        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Login(postData));
    }

    public void InitialSetup()
    {
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");

        List<User> lista = new List<User>();
        List<User> listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<User>();
        if (string.IsNullOrEmpty(Token))
        {
            scoreboard.SetActive(false);
            login.SetActive(true);
        }
        else
        {
            login.SetActive(false);
            scoreboard.SetActive(true);
            token = Token;
            Debug.Log(Token);
            Debug.Log(Username);
            StartCoroutine(GetUserProfile());
        }

    }
    public void UpdateData()
    {
        User user = new User();
        user.username = Username;
        if (int.TryParse(GameObject.Find("Datascore").GetComponent<InputField>().text,out _))
        {
            user.data.score = int.Parse(GameObject.Find("Datascore").GetComponent<InputField>().text);
        }
        string postData = JsonUtility.ToJson(user);
        Debug.Log(postData);
        StartCoroutine(updateDate(postData));
    }
  
    IEnumerator Registry(string postData)
    {

        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/usuarios", postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);
            }
            else
            {
                string message = "Status :" + www.responseCode;
                message += "\ncontent-type:" + www.GetResponseHeader("content-type");
                message += "\nError :" + www.error;
                Debug.Log(message);
                ErrorName.text = "Error : User already exists  ";
                StartCoroutine(Message());
            }

        }
    }
    IEnumerator Login(string postData)
    {

        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/auth/login", postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " logged in");

                Token = jsonData.token;
                Username = jsonData.usuario.username;

                PlayerPrefs.SetString("token", Token);
                PlayerPrefs.SetString("username", Username);
                login.SetActive(false);
                scoreboard.SetActive(true);
                StartCoroutine(MoreInfo());
                Name.text = "User :" + jsonData.usuario.username;

            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
                ErrorName.text = "Error : User or password is wrong  ";
                StartCoroutine(Message());
            }

        }
    }
    IEnumerator GetUserProfile()
    {
        UnityWebRequest www = UnityWebRequest.Get(ServerApiURL + "/api/usuarios/" + Username);
        www.SetRequestHeader("x-token", Token);



        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " You are still logged in");
                Name.text = "User :" + jsonData.usuario.username;

                //hola 
                StartCoroutine(MoreInfo());

            }
            else
            {
                scoreboard.SetActive(false);
                login.SetActive(true);
                string message = "Status :" + www.responseCode;
                message += "\ncontent-type:" + www.GetResponseHeader("content-type");
                message += "\nError :" + www.error;
                ErrorName.text = "Error : The previous user has logged off ";


                StartCoroutine(Message());
                Debug.Log(message);
            }

        }
    }
    public void Out()
    {
        PlayerPrefs.DeleteAll();
        scoreboard.SetActive(false);
        login.SetActive(true);
    }

    IEnumerator MoreInfo()
    {
        UnityWebRequest www = UnityWebRequest.Get(ServerApiURL + "/api/usuarios");
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {
                userlist jsonList = JsonUtility.FromJson<userlist>(www.downloadHandler.text);
                Debug.Log(jsonList.usuarios.Count);
                foreach (User user in jsonList.usuarios)
                {
                    Debug.Log(user.username);
                }
                List<User> userList = jsonList.usuarios;
                List<User> organizedList = userList.OrderByDescending(u => u.data.score).ToList<User>();
                int spot=0;
                foreach (User user in organizedList)
                {
                    if (spot > 4) { }
                    else
                    {
                        string nombre = spot + 1 + "." + " User: " + user.username + ", Score: " + user.data.score;
                        Positions[spot].text = nombre;
                        spot++;
                    }
                }
            }
            else
            {
                scoreboard.SetActive(false);
                login.SetActive(true);


                string message = "Status :" + www.responseCode;
                message += "\ncontent-type:" + www.GetResponseHeader("content-type");
                message += "\nError :" + www.error;
                Debug.Log(message);
            }

        }
    }
    IEnumerator updateDate(string postData)
    {

        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/usuarios/", postData);
        www.method = "PATCH";
        www.SetRequestHeader("x-token", Token);
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();
        if (www.isNetworkError)
        {
            scoreboard.SetActive(false);
            login.SetActive(true);
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);
                StartCoroutine(MoreInfo());
            }
            else
            {
                string message = "Status :" + www.responseCode;
                message += "\ncontent-type:" + www.GetResponseHeader("content-type");
                message += "\nError :" + www.error;
                Debug.Log(message);
            }

        }
    } 
    IEnumerator Message()
    {
      yield return new WaitForSeconds(5f);
      ErrorName.text = "";
    }
}

[System.Serializable]
public class User
{
    public string _id;
    public string username;
    public string password;
   
    public userData data;

    public User()
    {
        data = new userData();
    }
    public User(string username, string password)
    {
        this.username = username;
        this.password = password;
        data = new userData();
    }
}
[System.Serializable]
public class userData
{
    public int score;
    public userData()
    {
        
    }

}

public class AuthJsonData
{
    public User usuario;
    public userData data;
    public string token;
}

[System.Serializable]
public class userlist
{
    public List<User> usuarios;
}
