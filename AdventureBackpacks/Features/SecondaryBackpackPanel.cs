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
    private const string GridName = "AB_BackpackGrid";

    private static GameObject _panel;
    private static InventoryGrid _grid;
    private static TMP_Text _name;
    private static InventoryGui _gui;
    private static Container _backpackContainer;
    private static readonly Vector3[] CornerBuffer = new Vector3[4];

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

        // Size the chrome first, then rebuild the grid into that space.
        Layout(gui);
        if (!RebuildGrid(player, gui.m_dragItem))
        {
            Hide();
            return false;
        }

        // Re-apply position after size/grid settle.
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
            Layout(_gui);
            RebuildGrid(player, _gui.m_dragItem);
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
        if (_grid != null)
            _grid.name = GridName;

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

    private static bool RebuildGrid(Player player, ItemDrop.ItemData dragItem)
    {
        var inv = GetInventory();
        if (_grid == null || inv == null)
            return false;

        try
        {
            // Drop any cloned chest slots so UpdateGui lays out fresh for backpack dims.
            if (_grid.m_elements != null)
            {
                foreach (var element in _grid.m_elements)
                {
                    if (element != null)
                        Object.Destroy(element.gameObject);
                }

                _grid.m_elements.Clear();
            }

            // Force UpdateGui's rebuild path (it skips when width/height already match).
            _grid.m_width = 0;
            _grid.m_height = 0;

            // Match the grid widget to the backpack content area before centering slots.
            var gridRt = _grid.transform as RectTransform;
            if (gridRt != null)
            {
                var cell = _grid.m_elementSpace > 1f ? _grid.m_elementSpace : 64f;
                gridRt.sizeDelta = new Vector2(inv.m_width * cell, inv.m_height * cell);
            }

            Canvas.ForceUpdateCanvases();

            _grid.UpdateInventory(inv, player, dragItem);
            _grid.UpdateGui(player, dragItem);
            _grid.ResetView();
            return true;
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Secondary backpack panel update failed: {ex.Message}");
            return false;
        }
    }

    private static void Layout(InventoryGui gui)
    {
        if (_panel == null || gui.m_player == null || gui.m_container == null)
            return;

        var rt = _panel.GetComponent<RectTransform>();
        if (rt == null)
            return;

        var parent = rt.parent as RectTransform;
        if (parent == null)
            return;

        var player = gui.m_player;

        // Stay in the same anchor space as the player inventory panel.
        rt.anchorMin = player.anchorMin;
        rt.anchorMax = player.anchorMax;
        rt.pivot = player.pivot;

        var contentSize = ApplyContentSize(rt, gui);
        Canvas.ForceUpdateCanvases();

        var gap = ConfigRegistry.AlongsidePanelGap != null ? ConfigRegistry.AlongsidePanelGap.Value : 72f;
        var offsetY = ConfigRegistry.AlongsidePanelOffsetY != null ? ConfigRegistry.AlongsidePanelOffsetY.Value : 0f;
        var scale = ConfigRegistry.AlongsidePanelScale != null ? ConfigRegistry.AlongsidePanelScale.Value : 1f;
        scale = Mathf.Clamp(scale, 0.5f, 1.5f);

        // Top of the player inventory background box (not the taller player root).
        var bkg = player.Find("Bkg") as RectTransform ?? player;
        var topY = GetLocalTop(parent, bkg);

        // Clear weight / armor icons that sit to the right of the inventory.
        var rightX = GetLocalRight(parent, player);
        rightX = Mathf.Max(rightX, GetLocalRight(parent, gui.m_weight != null ? gui.m_weight.rectTransform : null));
        rightX = Mathf.Max(rightX, GetLocalRight(parent, gui.m_armor != null ? gui.m_armor.rectTransform : null));

        // Desired top-left of our panel in parent local space.
        var width = contentSize.x > 1f ? contentSize.x : rt.rect.width;
        var height = contentSize.y > 1f ? contentSize.y : rt.rect.height;
        var localLeft = rightX + gap;
        var localTop = topY + offsetY;

        // Convert top-left → anchoredPosition for the current pivot.
        var pivotLocal = new Vector2(
            localLeft + width * rt.pivot.x,
            localTop - height * (1f - rt.pivot.y));

        rt.anchoredPosition = ParentLocalToAnchored(rt, parent, pivotLocal);
        rt.localScale = Vector3.one * scale;
        rt.SetAsLastSibling();
    }

    /// <summary>
    /// Shrink the cloned chest chrome to roughly fit the backpack grid size.
    /// Returns the intended on-screen content size used for positioning.
    /// </summary>
    private static Vector2 ApplyContentSize(RectTransform rt, InventoryGui gui)
    {
        var inv = GetInventory();
        if (inv == null || _grid == null)
        {
            rt.sizeDelta = gui.m_container.sizeDelta;
            return rt.rect.size;
        }

        var cell = _grid.m_elementSpace;
        if (cell <= 1f)
            cell = 64f;

        // Estimate wood-frame padding from the original container vs its grid.
        float padX = 70f;
        float padY = 110f;
        var templateGrid = gui.m_containerGrid != null
            ? gui.m_containerGrid
            : gui.m_container.GetComponentInChildren<InventoryGrid>(true);
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

        var template = gui.m_container;
        var sizeDelta = template.sizeDelta;
        var rect = template.rect;
        if (Mathf.Abs(rect.width) > 1f && Mathf.Abs(rect.height) > 1f)
        {
            var stretchX = template.anchorMin.x != template.anchorMax.x;
            var stretchY = template.anchorMin.y != template.anchorMax.y;
            if (!stretchX && !stretchY)
            {
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
        return content;
    }

    private static float GetLocalRight(RectTransform parent, RectTransform target)
    {
        if (parent == null || target == null || !target.gameObject.activeInHierarchy)
            return float.MinValue;

        target.GetWorldCorners(CornerBuffer);
        // corners: 0=bl, 1=tl, 2=tr, 3=br
        var a = parent.InverseTransformPoint(CornerBuffer[2]).x;
        var b = parent.InverseTransformPoint(CornerBuffer[3]).x;
        return Mathf.Max(a, b);
    }

    private static float GetLocalTop(RectTransform parent, RectTransform target)
    {
        if (parent == null || target == null)
            return 0f;

        target.GetWorldCorners(CornerBuffer);
        var a = parent.InverseTransformPoint(CornerBuffer[1]).y;
        var b = parent.InverseTransformPoint(CornerBuffer[2]).y;
        return Mathf.Max(a, b);
    }

    private static Vector2 ParentLocalToAnchored(RectTransform rt, RectTransform parent, Vector2 parentLocal)
    {
        // Mirror Unity's anchoredPosition mapping for non-stretch and stretch anchors.
        var parentRect = parent.rect;
        var anchorMin = rt.anchorMin;
        var anchorMax = rt.anchorMax;
        var anchorRef = new Vector2(
            Mathf.Lerp(parentRect.xMin, parentRect.xMax, (anchorMin.x + anchorMax.x) * 0.5f),
            Mathf.Lerp(parentRect.yMin, parentRect.yMax, (anchorMin.y + anchorMax.y) * 0.5f));

        return parentLocal - anchorRef;
    }
}
