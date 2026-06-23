using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MainMenuController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SaveManager saveManager;

    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Buttons")]
    [SerializeField] private Button newKingdomButton;
    [SerializeField] private Button continueKingdomButton;
    [SerializeField] private Button settingsButton;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private MainMenuSettingsPanel settingsPanelController;

    [Header("Auto Wire")]
    [SerializeField] private bool autoFindSceneButtons = true;

    private readonly List<Button> trackedButtons = new List<Button>();

    private void Awake()
    {
        if (saveManager == null)
        {
            saveManager = SaveManager.Instance;
        }

        if (autoFindSceneButtons)
        {
            AutoWireButtons();
            AutoWireSettingsPanel();
        }
    }

    private void OnEnable()
    {
        BindButtons();
        RefreshContinueButtonState();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void Start()
    {
        RefreshContinueButtonState();
    }

    private void OnDisable()
    {
        UnbindButtons();
    }

    public void StartNewKingdom()
    {
        if (saveManager != null)
        {
            saveManager.DeleteLocalSave();
        }

        LoadGameScene();
    }

    public void ContinueKingdom()
    {
        if (saveManager != null && !saveManager.HasLocalSaveData)
        {
            return;
        }

        if (saveManager != null && !saveManager.HasLoadedSave)
        {
            saveManager.LoadGame();
        }

        LoadGameScene();
    }

    public void ToggleSettings()
    {
        if (settingsPanel == null)
        {
            return;
        }

        bool isVisible = settingsPanel.activeSelf;
        settingsPanel.SetActive(!isVisible);

        if (!isVisible && settingsPanelController != null)
        {
            settingsPanelController.RefreshFromSave();
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            return;
        }

        settingsPanel.SetActive(true);
        if (settingsPanelController != null)
        {
            settingsPanelController.RefreshFromSave();
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void RefreshContinueButtonState()
    {
        if (continueKingdomButton != null)
        {
            continueKingdomButton.interactable = saveManager != null && saveManager.HasLocalSaveData;
        }
    }

    private void LoadGameScene()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogWarning("MainMenuController: game scene name is not configured.");
            return;
        }

        SceneManager.LoadSceneAsync(gameSceneName);
    }

    private void AutoWireButtons()
    {
        if (newKingdomButton == null)
        {
            newKingdomButton = FindButtonByName("Novo Reino");
        }

        if (continueKingdomButton == null)
        {
            continueKingdomButton = FindButtonByName("ContinuarReino");
        }

        if (settingsButton == null)
        {
            settingsButton = FindButtonByName("Config");
            if (settingsButton == null)
            {
                settingsButton = FindButtonByName("Settings");
            }
        }
    }

    private void AutoWireSettingsPanel()
    {
        if (settingsPanel == null)
        {
            settingsPanel = FindObjectByName("SettingsPanel");
        }

        if (settingsPanelController == null && settingsPanel != null)
        {
            settingsPanelController = settingsPanel.GetComponent<MainMenuSettingsPanel>();
        }
    }

    private Button FindButtonByName(string objectName)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && buttons[i].gameObject.name == objectName)
            {
                return buttons[i];
            }
        }

        return null;
    }

    private GameObject FindObjectByName(string objectName)
    {
        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != null && transforms[i].name == objectName)
            {
                return transforms[i].gameObject;
            }
        }

        return null;
    }

    private void BindButtons()
    {
        UnbindButtons();

        if (newKingdomButton != null)
        {
            newKingdomButton.onClick.AddListener(StartNewKingdom);
            trackedButtons.Add(newKingdomButton);
        }

        if (continueKingdomButton != null)
        {
            continueKingdomButton.onClick.AddListener(ContinueKingdom);
            trackedButtons.Add(continueKingdomButton);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ToggleSettings);
            trackedButtons.Add(settingsButton);
        }
    }

    private void UnbindButtons()
    {
        for (int i = 0; i < trackedButtons.Count; i++)
        {
            Button button = trackedButtons[i];
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveListener(StartNewKingdom);
            button.onClick.RemoveListener(ContinueKingdom);
            button.onClick.RemoveListener(ToggleSettings);
        }

        trackedButtons.Clear();
    }
}
