using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : PersistentSingleton<SceneSwitcher>
{
    [SerializeField] private MenuTransition menuTransition;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            SceneManager.LoadSceneAsync("_MaintenanceMenu");
        }
    }

    private async void LoadScene(string scenePath)
    {
        menuTransition.StartTransition();
        await Awaitable.WaitForSecondsAsync(1f);
        await SceneManager.LoadSceneAsync(scenePath);
        menuTransition.EndTransition();
    }
}
