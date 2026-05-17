using UnityEngine;

public static class SettingsMenuPresenter
{
    private const string PrefabResourcePath = "SettingsMenu";

    public static SettingsMenuView Show(Transform parent)
    {
        if (parent == null)
            return null;

        SettingsMenuView existing = parent.Find("SettingsMenu")?.GetComponent<SettingsMenuView>();
        if (existing != null)
        {
            existing.Initialize(null);
            existing.Show();
            return existing;
        }

        SettingsMenuView prefab = Resources.Load<SettingsMenuView>(PrefabResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning("Settings menu prefab could not be loaded from Resources/SettingsMenu.");
            return null;
        }

        SettingsMenuView view = Object.Instantiate(prefab, parent, false);
        view.name = "SettingsMenu";
        GameManager.ApplyShadowStyleToHierarchy(view.gameObject);
        view.Initialize(null);
        view.Show();
        return view;
    }
}
