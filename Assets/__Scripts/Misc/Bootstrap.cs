using UnityEngine;

public class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadStartup()
    {
        GameObject startup = GameObject.Instantiate(Resources.Load("Startup")) as GameObject;
        GameObject.DontDestroyOnLoad(startup);
    }
}
