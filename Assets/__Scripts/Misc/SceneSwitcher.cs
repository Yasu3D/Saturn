using System.Threading;
using SaturnGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : PersistentSingleton<SceneSwitcher>
{
    [SerializeField] private MenuWipeAnimator menuWipe;
    public bool LoadInProgress { get; private set; }

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
        if (LoadInProgress) return;

        LoadInProgress = true;
        menuWipe.Anim_StartTransition();
        await Awaitable.WaitForSecondsAsync(1.5f);
        await SceneManager.LoadSceneAsync(scenePath);
        menuWipe.Anim_EndTransition();
        LoadInProgress = false;
    }
}
