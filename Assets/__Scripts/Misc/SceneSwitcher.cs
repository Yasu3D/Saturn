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
        if (LoadInProgress) return;

        LoadInProgress = true;
        menuWipe.Anim_StartTransition();
        await Awaitable.WaitForSecondsAsync(1.5f);
        LastScene = SceneManager.GetActiveScene().name;
        await SceneManager.LoadSceneAsync(scenePath);
        menuWipe.Anim_EndTransition();
        LoadInProgress = false;
    }
}
