using AdventureBackpacks.Configuration;
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

        // Size/position after the grid has built so we can measure content.
        Layout(gui);

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
            _grid.UpdateInventory(inv, player, _gui.m_dragItem);
            _grid.UpdateGui(player, _gui.m_dragItem);
            Layout(_gui);
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

        var player = gui.m_player;

        // Stay in the same anchor space as the player inventory panel.
        rt.anchorMin = player.anchorMin;
        rt.anchorMax = player.anchorMax;
        rt.pivot = player.pivot;

        ApplyContentSize(rt, gui);

        var gap = ConfigRegistry.AlongsidePanelGap != null ? ConfigRegistry.AlongsidePanelGap.Value : 24f;
        var offsetY = ConfigRegistry.AlongsidePanelOffsetY != null ? ConfigRegistry.AlongsidePanelOffsetY.Value : 0f;
        var scale = ConfigRegistry.AlongsidePanelScale != null ? ConfigRegistry.AlongsidePanelScale.Value : 1f;
        scale = Mathf.Clamp(scale, 0.5f, 1.5f);

        // Place immediately to the right of the player inventory, tops aligned, with a gap.
        // Account for pivot so different UI setups still line up.
        var playerRight = player.anchoredPosition.x + player.rect.width * (1f - player.pivot.x);
        var panelLeftFromPivot = rt.rect.width * rt.pivot.x;
        var x = playerRight + gap + panelLeftFromPivot;

        var playerTop = player.anchoredPosition.y + player.rect.height * (1f - player.pivot.y);
        var panelTopFromPivot = rt.rect.height * (1f - rt.pivot.y);
        var y = playerTop - panelTopFromPivot + offsetY;

        rt.anchoredPosition = new Vector2(x, y);
        rt.localScale = Vector3.one * scale;
        rt.SetAsLastSibling();
    }

    /// <summary>
    /// Shrink the cloned chest chrome to roughly fit the backpack grid size.
    /// </summary>
    private static void ApplyContentSize(RectTransform rt, InventoryGui gui)
    {
        var inv = GetInventory();
        if (inv == null || _grid == null)
        {
            rt.sizeDelta = gui.m_container.sizeDelta;
            return;
        }

        var cell = _grid.m_elementSpace;
        if (cell <= 1f)
            cell = 64f;

        // Estimate wood-frame padding from the original container vs its grid.
        float padX = 70f;
        float padY = 110f;
        var templateGrid = gui.m_container.GetComponentInChildren<InventoryGrid>(true);
        if (templateGrid != null && templateGrid.m_width > 0 && templateGrid.m_height > 0)
        {
            var templateCell = templateGrid.m_elementSpace;
            if (templateCell > 1f)
            {
                padX = Mathf.Max(40f, gui.m_container.rect.width - templateGrid.m_width * templateCell);
                padY = Mathf.Max(70f, gui.m_container.rect.height - templateGrid.m_height * templateCell);
            }
        }

        var content = new Vector2(inv.m_width * cell + padX, inv.m_height * cell + padY);

        // Preserve how sizeDelta maps to rect for the current anchors (same as template).
        var template = gui.m_container;
        var sizeDelta = template.sizeDelta;
        var rect = template.rect;
        if (Mathf.Abs(rect.width) > 1f && Mathf.Abs(rect.height) > 1f)
        {
            // If the template uses stretch anchors, sizeDelta is inset-based; fall back to matching player-style absolute size.
            var stretchX = template.anchorMin.x != template.anchorMax.x;
            var stretchY = template.anchorMin.y != template.anchorMax.y;
            if (!stretchX && !stretchY)
            {
                // Non-stretch: sizeDelta tracks rect size (modulo pivot/anchors quirks). Scale proportionally from template.
                sizeDelta = new Vector2(
                    sizeDelta.x * (content.x / rect.width),
                    sizeDelta.y * (content.y / rect.height));
            }
            else
            {
                sizeDelta = content;
            }
        }
        else
        {
            sizeDelta = content;
        }

        rt.sizeDelta = sizeDelta;
    }
}
