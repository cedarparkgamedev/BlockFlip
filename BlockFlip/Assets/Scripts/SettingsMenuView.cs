using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsMenuView : MonoBehaviour
{
    private const string MusicKey = "Settings_Music";
    private const string SoundEffectsKey = "Settings_SoundEffects";
    private const string HapticsKey = "Settings_Haptics";
    private const string PlacementPreviewKey = "Settings_PlacementPreview";
    private const string ConfirmRestartKey = "Settings_ConfirmRestart";
    private const string ReduceMotionKey = "Settings_ReduceMotion";

    [SerializeField] private Button backButton;
    [SerializeField] private Button doneButton;
    [SerializeField] private Button languageButton;
    [SerializeField] private Button resetTutorialButton;
    [SerializeField] private Button privacyButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle soundEffectsToggle;
    [SerializeField] private Toggle hapticsToggle;
    [SerializeField] private Toggle placementPreviewToggle;
    [SerializeField] private Toggle confirmRestartToggle;
    [SerializeField] private Toggle reduceMotionToggle;

    private UnityAction onClose;

    private void Awake()
    {
        ResolveReferences();
    }

    public void Initialize(UnityAction closeAction)
    {
        ResolveReferences();
        onClose = closeAction;
        BindButton(backButton, Close);
        BindButton(doneButton, Close);
        BindButton(languageButton, () => Debug.Log("Language settings are not implemented yet."));
        BindButton(resetTutorialButton, ResetTutorial);
        BindButton(privacyButton, () => Debug.Log("Privacy screen is not implemented yet."));
        BindButton(creditsButton, () => Debug.Log("Credits screen is not implemented yet."));

        BindToggle(musicToggle, MusicKey, true);
        BindToggle(soundEffectsToggle, SoundEffectsKey, true);
        BindToggle(hapticsToggle, HapticsKey, true);
        BindToggle(placementPreviewToggle, PlacementPreviewKey, true);
        BindToggle(confirmRestartToggle, ConfirmRestartKey, false);
        BindToggle(reduceMotionToggle, ReduceMotionKey, false);
    }

    private void ResolveReferences()
    {
        backButton = FindButton("BackButton");
        doneButton = FindButton("DoneButton");
        languageButton = FindButton("LanguageButton");
        resetTutorialButton = FindButton("ResetTutorialButton");
        privacyButton = FindButton("PrivacyButton");
        creditsButton = FindButton("CreditsButton");
        musicToggle = FindToggle("MusicToggle");
        soundEffectsToggle = FindToggle("SoundEffectsToggle");
        hapticsToggle = FindToggle("HapticsToggle");
        placementPreviewToggle = FindToggle("PlacementPreviewToggle");
        confirmRestartToggle = FindToggle("ConfirmRestartToggle");
        reduceMotionToggle = FindToggle("ReduceMotionToggle");
    }

    public void Show()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Close()
    {
        Hide();
        onClose?.Invoke();
    }

    private void ResetTutorial()
    {
        PlayerPrefs.DeleteKey("TutorialCompleted");
        PlayerPrefs.Save();
        Debug.Log("Tutorial progress reset.");
    }

    private static void BindButton(Button button, UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.interactable = action != null;

        if (action != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void BindToggle(Toggle toggle, string prefsKey, bool defaultValue)
    {
        if (toggle == null)
            return;

        toggle.onValueChanged.RemoveAllListeners();
        toggle.isOn = PlayerPrefs.GetInt(prefsKey, defaultValue ? 1 : 0) == 1;
        ApplyToggleVisual(toggle, toggle.isOn);
        toggle.onValueChanged.AddListener(isOn =>
        {
            PlayerPrefs.SetInt(prefsKey, isOn ? 1 : 0);
            PlayerPrefs.Save();
            ApplyToggleVisual(toggle, isOn);
        });
    }

    private static void ApplyToggleVisual(Toggle toggle, bool isOn)
    {
        if (toggle == null)
            return;

        Image background = toggle.targetGraphic as Image;
        if (background != null)
        {
            background.color = isOn
                ? new Color(0.055f, 0.055f, 0.052f, 1f)
                : new Color(0.78f, 0.76f, 0.72f, 0.42f);
        }

        if (toggle.graphic != null)
        {
            RectTransform handle = toggle.graphic.rectTransform;
            handle.anchorMin = new Vector2(isOn ? 1f : 0f, 0.5f);
            handle.anchorMax = new Vector2(isOn ? 1f : 0f, 0.5f);
            handle.anchoredPosition = new Vector2(isOn ? -32f : 32f, 0f);
        }
    }

    private Button FindButton(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == objectName && child.TryGetComponent(out Button button))
            {
                return button;
            }
        }

        return null;
    }

    private Toggle FindToggle(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == objectName && child.TryGetComponent(out Toggle toggle))
            {
                return toggle;
            }
        }

        return null;
    }
}
