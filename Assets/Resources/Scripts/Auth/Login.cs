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
        localUser.userId = 1;
        localUser.userName = "Bob";
        SceneManager.LoadScene("main");
    }
}
