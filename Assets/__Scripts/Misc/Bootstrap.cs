using UnityEngine;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadStartup()
    {
        Object startup = Object.Instantiate(Resources.Load("Startup"));
        Object.DontDestroyOnLoad(startup);
    }
}