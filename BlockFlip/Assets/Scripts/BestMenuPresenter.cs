using UnityEngine;

public static class BestMenuPresenter
{
    private const string PrefabResourcePath = "BestMenu";

    public static BestMenuView Show(Transform parent)
    {
        if (parent == null)
            return null;

        BestMenuView existing = parent.Find("BestMenu")?.GetComponent<BestMenuView>();
        if (existing != null)
        {
            existing.Initialize();
            existing.Show();
            return existing;
        }

        BestMenuView prefab = Resources.Load<BestMenuView>(PrefabResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning("Best menu prefab could not be loaded from Resources/BestMenu.");
            return null;
        }

        BestMenuView view = Object.Instantiate(prefab, parent, false);
        view.name = "BestMenu";
        GameManager.ApplyShadowStyleToHierarchy(view.gameObject);
        view.Initialize();
        view.Show();
        return view;
    }
}
