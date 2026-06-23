using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MainMenuSettingsPanel : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SaveManager saveManager;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button closeButton;

    [Header("Auto Wire")]
    [SerializeField] private bool autoHideOnStart = true;

    private void Awake()
    {
        if (saveManager == null)
        {
            saveManager = SaveManager.Instance;
        }

        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }
    }

    private void OnEnable()
    {
        BindSliders();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        RefreshFromSave();
    }

    private void Start()
    {
        if (autoHideOnStart && panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void OnDisable()
    {
        UnbindSliders();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }

    public void Open()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        RefreshFromSave();
    }

    public void Close()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void Toggle()
    {
        if (panelRoot == null)
        {
            return;
        }

        panelRoot.SetActive(!panelRoot.activeSelf);
        if (panelRoot.activeSelf)
        {
            RefreshFromSave();
        }
    }

    public void RefreshFromSave()
    {
        if (saveManager == null)
        {
            return;
        }

        SetSliderValue(masterVolumeSlider, saveManager.GetMasterVolume());
        SetSliderValue(musicVolumeSlider, saveManager.GetMusicVolume());
        SetSliderValue(sfxVolumeSlider, saveManager.GetSfxVolume());
    }

    public void ApplyMasterVolume(float value)
    {
        if (saveManager != null)
        {
            saveManager.SetMasterVolume(value);
        }
    }

    public void ApplyMusicVolume(float value)
    {
        if (saveManager != null)
        {
            saveManager.SetMusicVolume(value);
        }
    }

    public void ApplySfxVolume(float value)
    {
        if (saveManager != null)
        {
            saveManager.SetSfxVolume(value);
        }
    }

    private void BindSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(ApplyMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(ApplyMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(ApplySfxVolume);
        }
    }

    private void UnbindSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(ApplyMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(ApplyMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(ApplySfxVolume);
        }
    }

    private void SetSliderValue(Slider slider, float value)
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }
}
