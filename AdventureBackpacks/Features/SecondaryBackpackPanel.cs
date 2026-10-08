using AdventureBackpacks.Extensions;
using AdventureBackpacks.Patches;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdventureBackpacks.Features;

/// <summary>
/// Shows the equipped backpack inventory as a cloned container panel while a
/// world container (chest, etc.) remains InventoryGui.m_currentContainer.
/// </summary>
internal static class SecondaryBackpackPanel
{
    private const string PanelName = "AB_BackpackSecondary";

    private static GameObject _panel;
    private static InventoryGrid _grid;
    private static TMP_Text _name;
    private static InventoryGui _gui;
    private static Container _backpackContainer;

    public static bool IsOpen => _panel != null && _panel.activeSelf;

    public static InventoryGrid Grid => _grid;

    public static Container BackpackContainer => _backpackContainer;

    public static Inventory GetInventory() => _backpackContainer != null ? _backpackContainer.GetInventory() : null;

    public static bool Show(InventoryGui gui, Player player)
    {
        if (gui == null || player == null || gui.m_container == null)
            return false;

        if (!player.CanOpenBackpack())
            return false;

        var backpack = player.GetEquippedBackpack();
        if (backpack == null)
            return false;

        var backpackContainer = player.GetBackpackContainerProxy();
        if (backpackContainer == null)
            return false;

        backpack.UpdateContainerSizing(ref backpackContainer);
        _backpackContainer = backpackContainer;
        _gui = gui;

        EnsurePanel(gui);

        if (_panel == null || _grid == null)
            return false;

        var inv = backpackContainer.GetInventory();
        if (inv == null)
            return false;

        if (_name != null)
            _name.text = Localization.instance.Localize(backpackContainer.GetInventory()?.GetName() ?? "$item_backpack");

        _panel.SetActive(true);
        Layout(gui);

        try
        {
            _grid.UpdateInventory(inv, player, gui.m_dragItem);
            _grid.UpdateGui(player, gui.m_dragItem);
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Secondary backpack panel update failed: {ex.Message}");
            Hide();
            return false;
        }

        InventoryGuiPatches.BackpackIsOpen = true;
        return true;
    }

    public static void Hide()
    {
        if (_panel != null)
            _panel.SetActive(false);

        _backpackContainer = null;
    }

    public static void Destroy()
    {
        Hide();
        if (_panel != null)
        {
            Object.Destroy(_panel);
            _panel = null;
            _grid = null;
            _name = null;
            _gui = null;
        }
    }

    public static void Refresh(Player player)
    {
        if (!IsOpen || _gui == null || _grid == null || player == null)
            return;

        var inv = GetInventory();
        if (inv == null)
        {
            Hide();
            InventoryGuiPatches.BackpackIsOpen = false;
            return;
        }

        try
        {
            Layout(_gui);
            _grid.UpdateInventory(inv, player, _gui.m_dragItem);
            _grid.UpdateGui(player, _gui.m_dragItem);
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Secondary backpack panel refresh failed: {ex.Message}");
        }
    }

    private static void EnsurePanel(InventoryGui gui)
    {
        if (_panel != null)
            return;

        var parent = gui.m_container.parent != null ? gui.m_container.parent : gui.m_inventoryRoot;
        _panel = Object.Instantiate(gui.m_container.gameObject, parent);
        _panel.name = PanelName;

        _grid = _panel.GetComponentInChildren<InventoryGrid>(true);
        _name = null;

        // Prefer the same name label the container panel uses when present.
        foreach (var tmp in _panel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (gui.m_containerName != null && tmp.name == gui.m_containerName.name)
            {
                _name = tmp;
                break;
            }
        }

        if (_name == null)
            _name = _panel.GetComponentInChildren<TMP_Text>(true);

        // Take All / Stack All on the clone would hit the chest (m_currentContainer). Hide them.
        foreach (var button in _panel.GetComponentsInChildren<Button>(true))
        {
            var n = button.gameObject.name;
            if (n.IndexOf("Take", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Stack", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Drop", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    private static void Layout(InventoryGui gui)
    {
        if (_panel == null || gui.m_player == null || gui.m_container == null)
            return;

        var rt = _panel.GetComponent<RectTransform>();
        if (rt == null)
            return;

        // Sit under the player inventory, left of the open chest panel.
        var player = gui.m_player;
        rt.anchorMin = player.anchorMin;
        rt.anchorMax = player.anchorMax;
        rt.pivot = player.pivot;
        rt.sizeDelta = gui.m_container.sizeDelta;

        var height = Mathf.Max(player.rect.height, 200f);
        rt.anchoredPosition = player.anchoredPosition + new Vector2(0f, -(height + 16f));
        rt.localScale = Vector3.one;
        rt.SetAsLastSibling();
    }
}
