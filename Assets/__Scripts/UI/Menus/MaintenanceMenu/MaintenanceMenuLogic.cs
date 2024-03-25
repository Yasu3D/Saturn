using System.Collections.Generic;
using SaturnGame.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SaturnGame.UI
{
public class MaintenanceMenuLogic : MonoBehaviour
{
    [Header("BUTTONS")] [SerializeField] private List<Button> mainMenuButtons;
    [SerializeField] private List<Button> creditLogButtons;
    [SerializeField] private List<Button> hardwareTestButtons;
    [SerializeField] private List<Button> hardwareInfoButtons;
    [SerializeField] private List<Button> systemSettingsButtons;
    [Space(10)] [SerializeField] private List<Button> audioTestButtons;
    [SerializeField] private List<Button> inputTestButtons;
    [SerializeField] private List<Button> lightTestButtons;
    [SerializeField] private List<Button> displayTestButtons;
    [SerializeField] private List<Button> networkTestButtons;
    [SerializeField] private List<Button> cardReaderTestButtons;
    [Space(10)] [SerializeField] private List<Button> gameSettingsButtons;
    [SerializeField] private List<Button> audioSettingsButtons;
    [SerializeField] private List<Button> inputSettingsButtons;
    [SerializeField] private List<Button> lightSettingsButtons;
    [SerializeField] private List<Button> displaySettingsButtons;
    [SerializeField] private List<Button> displaySettingsValueButtons;
    [SerializeField] private List<Button> networkSettingsButtons;
    [SerializeField] private List<Button> graphicsSettingsButtons;

    [Header("MENUS")] [SerializeField] private List<GameObject> mainMenus;
    [SerializeField] private List<GameObject> settingsMenus;
    [SerializeField] private List<GameObject> hardwareTestMenus;
    [Header("OTHER")] [SerializeField] private AudioTester audioTester;

    private int mainMenuIndex;
    private int settingsMenuIndex;
    private int hardwareTestIndex;

    private bool changingNumericValue;
    private int numericIndex;

    private void Awake()
    {
        // Main Menu Buttons
        mainMenuButtons[0].onClick.AddListener(OpenCreditLog);
        mainMenuButtons[1].onClick.AddListener(OpenHardwareTest);
        mainMenuButtons[2].onClick.AddListener(OpenHardwareInfo);
        mainMenuButtons[3].onClick.AddListener(OpenSystemSettings);
        mainMenuButtons[4].onClick.AddListener(ReturnToGame);

        // Credit Log Buttons
        //creditLogButtons[0].onClick.AddListener();
        creditLogButtons[1].onClick.AddListener(OpenMain);

        // Hardware Test Buttons
        hardwareTestButtons[0].onClick.AddListener(delegate { OpenHardwareTestSubMenu(0); });
        hardwareTestButtons[1].onClick.AddListener(delegate { OpenHardwareTestSubMenu(1); });
        hardwareTestButtons[2].onClick.AddListener(delegate { OpenHardwareTestSubMenu(2); });
        hardwareTestButtons[3].onClick.AddListener(delegate { OpenHardwareTestSubMenu(3); });
        hardwareTestButtons[4].onClick.AddListener(delegate { OpenHardwareTestSubMenu(4); });
        hardwareTestButtons[5].onClick.AddListener(delegate { OpenHardwareTestSubMenu(5); });
        hardwareTestButtons[6].onClick.AddListener(OpenMain);

        audioTestButtons[0].onClick.AddListener(delegate { audioTester.PlayLeft(); });
        audioTestButtons[1].onClick.AddListener(delegate { audioTester.PlayRight(); });
        audioTestButtons[2].onClick.AddListener(delegate { audioTester.PlayStereo(); });
        audioTestButtons[3].onClick.AddListener(OpenHardwareTest);

        inputTestButtons[0].onClick.AddListener(OpenHardwareTest);

        lightTestButtons[0].onClick.AddListener(OpenHardwareTest);

        displayTestButtons[0].onClick.AddListener(OpenHardwareTest);

        networkTestButtons[0].onClick.AddListener(OpenHardwareTest);

        cardReaderTestButtons[0].onClick.AddListener(OpenHardwareTest);

        // Hardware Info Buttons
        hardwareInfoButtons[0].onClick.AddListener(OpenMain);

        // Settings Menu Buttons
        systemSettingsButtons[0].onClick.AddListener(delegate { OpenSettingsSubMenu(0); });
        systemSettingsButtons[1].onClick.AddListener(delegate { OpenSettingsSubMenu(1); });
        systemSettingsButtons[2].onClick.AddListener(delegate { OpenSettingsSubMenu(2); });
        systemSettingsButtons[3].onClick.AddListener(delegate { OpenSettingsSubMenu(3); });
        systemSettingsButtons[4].onClick.AddListener(delegate { OpenSettingsSubMenu(4); });
        systemSettingsButtons[5].onClick.AddListener(delegate { OpenSettingsSubMenu(5); });
        systemSettingsButtons[6].onClick.AddListener(delegate { OpenSettingsSubMenu(6); });
        systemSettingsButtons[7].onClick.AddListener(OpenMain);

        gameSettingsButtons[0].onClick.AddListener(OpenSystemSettings);

        audioSettingsButtons[0].onClick.AddListener(OpenSystemSettings);

        inputSettingsButtons[0].onClick.AddListener(OpenSystemSettings);

        lightSettingsButtons[0].onClick.AddListener(OpenSystemSettings);

        displaySettingsButtons[0].onClick.AddListener(delegate { SelectDisplayValueButton(0); });
        displaySettingsButtons[1].onClick.AddListener(delegate { SelectDisplayValueButton(1); });
        displaySettingsButtons[2].onClick.AddListener(delegate { SelectDisplayValueButton(2); });
        displaySettingsButtons[3].onClick.AddListener(delegate { SelectDisplayValueButton(3); });
        displaySettingsButtons[4].onClick.AddListener(OpenSystemSettings);
        displaySettingsValueButtons[0].onClick.AddListener(delegate { SelectDisplayButton(0); });
        displaySettingsValueButtons[1].onClick.AddListener(delegate { SelectDisplayButton(1); });
        displaySettingsValueButtons[2].onClick.AddListener(delegate { SelectDisplayButton(2); });
        displaySettingsValueButtons[3].onClick.AddListener(delegate { SelectDisplayButton(3); });

        networkSettingsButtons[0].onClick.AddListener(OpenSystemSettings);

        graphicsSettingsButtons[0].onClick.AddListener(OpenSystemSettings);
    }

    private void CloseAll()
    {
        foreach (GameObject menu in mainMenus)
            menu.SetActive(false);

        foreach (GameObject menu in settingsMenus)
            menu.SetActive(false);

        foreach (GameObject menu in hardwareTestMenus)
            menu.SetActive(false);
    }

    private static void ReturnToGame()
    {
        SceneManager.LoadSceneAsync("_TitleScreen");
    }

    private void Update()
    {
        if (!changingNumericValue) return;

        if (Input.GetButtonDown("Vertical") && Input.GetAxis("Vertical") > 0)
            NumericValueUp();

        if (Input.GetButtonDown("Vertical") && Input.GetAxis("Vertical") < 0)
            NumericValueDown();
    }

    // ==== MAIN MENUS
    private void OpenMain()
    {
        CloseAll();
        mainMenus[0].SetActive(true);
        settingsMenuIndex = 0;
        EventSystem.current.SetSelectedGameObject(mainMenuButtons[mainMenuIndex].gameObject);
    }

    private void OpenCreditLog()
    {
        CloseAll();
        mainMenus[1].SetActive(true);
        mainMenuIndex = 0;
    }

    private void OpenHardwareTest()
    {
        CloseAll();
        mainMenus[2].SetActive(true);
        mainMenuIndex = 1;
        EventSystem.current.SetSelectedGameObject(hardwareTestButtons[hardwareTestIndex].gameObject);
    }

    private void OpenHardwareInfo()
    {
        CloseAll();
        mainMenus[3].SetActive(true);
        mainMenuIndex = 2;
    }

    private void OpenSystemSettings()
    {
        CloseAll();
        mainMenus[4].SetActive(true);
        mainMenuIndex = 3;
        EventSystem.current.SetSelectedGameObject(systemSettingsButtons[settingsMenuIndex].gameObject);
    }

    // ==== SUBMENUS
    private void OpenSettingsSubMenu(int id)
    {
        CloseAll();
        settingsMenus[id].SetActive(true);
        settingsMenuIndex = id;
    }

    private void OpenHardwareTestSubMenu(int id)
    {
        CloseAll();
        hardwareTestMenus[id].SetActive(true);
        hardwareTestIndex = id;
    }

    // ==== VALUE ADJUSTMENTS
    private void SelectDisplayValueButton(int id)
    {
        changingNumericValue = true;
        numericIndex = id;
        EventSystem.current.SetSelectedGameObject(displaySettingsValueButtons[id].gameObject);
    }

    private void SelectDisplayButton(int id)
    {
        changingNumericValue = false;
        EventSystem.current.SetSelectedGameObject(displaySettingsButtons[id].gameObject);
    }

    private void NumericValueUp()
    {
        DisplaySettings display = SettingsManager.Instance.DeviceSettings.DisplaySettings;

        switch (numericIndex)
        {
            case 0:
            {
                int newPosition = display.ViewRectPosition + 1;
                if (newPosition > 100)
                    newPosition -= 101;
                display.ViewRectPosition = newPosition;
                EventManager.InvokeEvent("UpdateViewRect");
                break;
            }

            case 1:
            {
                int newScale = display.ViewRectScale + 1;
                if (newScale > 100)
                    newScale -= 51;
                display.ViewRectScale = newScale;
                EventManager.InvokeEvent("UpdateViewRect");
                break;
            }

            case 2:
            {
                break;
            }

            case 3:
            {
                break;
            }
        }
    }

    private void NumericValueDown()
    {
        DisplaySettings display = SettingsManager.Instance.DeviceSettings.DisplaySettings;

        switch (numericIndex)
        {
            case 0:
            {
                int newPosition = display.ViewRectPosition - 1;
                if (newPosition < 0)
                    newPosition += 101;
                display.ViewRectPosition = newPosition;
                EventManager.InvokeEvent("UpdateViewRect");
                break;
            }

            case 1:
            {
                int newScale = display.ViewRectScale - 1;
                if (newScale < 50)
                    newScale += 51;
                display.ViewRectScale = newScale;
                EventManager.InvokeEvent("UpdateViewRect");
                break;
            }

            case 2:
            {
                break;
            }

            case 3:
            {
                break;
            }
        }
    }
}
}