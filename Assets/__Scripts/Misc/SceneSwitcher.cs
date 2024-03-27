using System.Threading;
using JetBrains.Annotations;
using SaturnGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : PersistentSingleton<SceneSwitcher>
{
    [SerializeField] private MenuWipeAnimator menuWipe;
    // Warning: atomicity only guaranteed on main thread.
    public bool LoadInProgress { get; private set; }
    // Warning: may be inaccurate if a load is in progress. Recommended to read on Awake() only.
    [CanBeNull] public string LastScene { get; private set; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            LoadMaintenanceMenu();
        }
    }

    public void LoadMaintenanceMenu()
    {
        menuWipe.Anim_ForceEnd();
        SceneManager.LoadSceneAsync("_MaintenanceMenu");
    }

    public async void LoadScene(string scenePath)
    {
        await Awaitable.MainThreadAsync();
        Debug.Log($"Loading scene: {scenePath}");
        if (LoadInProgress)
        {
            Debug.LogWarning("Aborting scene load: another load is in progress.");
            return;
        }

        LoadInProgress = true;
        menuWipe.Anim_StartTransition();
        await Awaitable.WaitForSecondsAsync(1.5f);
        LastScene = SceneManager.GetActiveScene().name;
        Debug.Log($"Previous scene was {LastScene}, now loading {scenePath}");
        await SceneManager.LoadSceneAsync(scenePath);
        Debug.Log($"Scene {scenePath} loaded, finishing animation.");
        menuWipe.Anim_EndTransition();
        Debug.Log($"Load animation finished.");
        LoadInProgress = false;
    }
}
