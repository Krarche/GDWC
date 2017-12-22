using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMaster : MonoBehaviour {

    public static SceneMaster singleton;

    public static float asyncProgress = 0;
    public static Dictionary<string, object> args;


    void Awake() {
        singleton = this;
    }

    void Start() {
        singleton = this;
    }

    public void SwitchToServerMain(Dictionary<string, object> args = null) {
        SceneMaster.args = args;
        StartCoroutine(DoSwitchToScene("Main"));
    }

    public void SwitchToLobby(Dictionary<string, object> args = null) {
        SceneMaster.args = args;
        StartCoroutine(DoSwitchToScene("Lobby"));
    }

    public void SwitchToGame1v1(Dictionary<string, object> args = null) {
        SceneMaster.args = args;
        StartCoroutine(DoSwitchToScene("Game1v1"));
    }

    // http://t-machine.org/index.php/2017/06/05/changing-scenelevel-smoothly-in-unity-5-5-5-unitytips/

    private IEnumerator DoSwitchToScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
        yield return null;
        /*SceneManager.LoadScene("LoadingScreen");
        Scene loadingScene = SceneManager.GetActiveScene();
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        async.allowSceneActivation = true;
        asyncProgress = 0;
        // progressing ...
        while (async.progress != 0.9f) {
            asyncProgress = async.progress;
            yield return null;
        }
        // done !
        asyncProgress = 1;
        foreach (GameObject go in loadingScene.GetRootGameObjects())
            Destroy(go);
        SceneManager.MergeScenes(SceneManager.GetSceneByName(sceneName), loadingScene);*/
    }


}
