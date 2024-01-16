using SaturnGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : PersistentSingleton<SceneSwitcher>
{
    [SerializeField] private MenuWipeAnimator menuWipe;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            SceneManager.LoadSceneAsync("_MaintenanceMenu");
        }
    }

    public async void LoadScene(string scenePath)
    {
        menuWipe.Anim_StartTransition();
        await Awaitable.WaitForSecondsAsync(1f);
        await SceneManager.LoadSceneAsync(scenePath);
        menuWipe.Anim_EndTransition();
    }
}
