using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PauseMenuView : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button homeButton;

    public void Initialize(
        UnityAction onResume,
        UnityAction onRestart,
        UnityAction onSettings,
        UnityAction onHome)
    {
        BindButton(resumeButton, onResume);
        BindButton(restartButton, onRestart);
        BindButton(settingsButton, onSettings);
        BindButton(homeButton, onHome);
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
}
