using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour {

    private InputField usernameField;
    private InputField passwordField;
    private Button loginButton;

    public static User localUser = new User();

    private Dictionary<string, string> responseHeaders = new Dictionary<string,string>();
    
    void Start () {
        usernameField = GameObject.Find("usernameField").GetComponent<InputField>();
        passwordField = GameObject.Find("passwordField").GetComponent<InputField>();
        loginButton = GameObject.Find("loginButton").GetComponent<Button>();
        loginButton.onClick.AddListener(login);
    }
	
	private void login() {
        string username = usernameField.text;
        string password = passwordField.text;

        if(username.Length != 0 && password.Length != 0) {
            StartCoroutine(Identify(username,password));
        }
    }

    IEnumerator Identify(string username, string password) {

        string authorization = authenticate(username, password);
        string url = "http://vps.studio-otsu.fr:8080";

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("AUTHORIZATION", authorization);
        yield return www.Send();

        if (www.isError) {
            Debug.Log(www.error);
        }
        else {

            if(www.responseCode == 200) {
                responseHeaders.Clear();
                if (www.GetResponseHeaders().Count > 0) {
                    foreach (KeyValuePair<string, string> entry in www.GetResponseHeaders()) {
                        responseHeaders.Add(entry.Key, entry.Value);
                    }
                }
                localUser.userToken = responseHeaders["Auth-Token"];
                if (localUser.userToken != null) {
                    StartCoroutine(GetUserInfo(username, localUser.userToken));
                }
            }
            else {
                Debug.Log("Could not login : invalid credentials.");
            }

        }
    }

    IEnumerator GetUserInfo(string username, string password) {

        string authorization = authenticate(username, password);
        string url = "http://vps.studio-otsu.fr:8080//gdwc/users?filter={'userName':'" + username + "'}";

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("AUTHORIZATION", authorization);
        yield return www.Send();

        if (www.isError) {
            Debug.Log(www.error);
        }
        else {
            if (www.responseCode == 200) {
                string response = www.downloadHandler.text;
                ObjectJSON json = new ObjectJSON(response);
                ObjectJSON userInfo = json.getArrayJSON("_embedded").getObjectJSONAt(0);
                localUser.userId = (ulong) userInfo.getInt("id");
                localUser.userName = userInfo.getString("playerName");
                SceneManager.LoadScene("main");
            }
        }
    }

    string authenticate(string username, string password) {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    }
}
