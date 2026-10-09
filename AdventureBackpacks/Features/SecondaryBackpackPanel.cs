using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features.SidePanel;
using AdventureBackpacks.Patches;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdventureBackpacks.Features;

/// <summary>
/// Dedicated backpack panel cloned from InventoryGui.m_container, always placed
/// to the right of the player inventory. Used whenever Open Alongside Container
/// is enabled so empty/filled and chest/no-chest all share one window.
/// </summary>
internal static class SecondaryBackpackPanel
{
    private const string PanelName = "AB_BackpackSecondary";
    private const string GridName = "AB_BackpackGrid";

    // Stable chrome padding — do not derive from the live chest panel (that changes size).
    private const float PadX = 70f;
    private const float PadY = 110f;
    private const float DefaultCell = 64f;

    private static GameObject _panel;
    private static InventoryGrid _grid;
    private static TMP_Text _name;
    private static TMP_Text _weight;
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
            _name.text = Localization.instance.Localize(inv.GetName() ?? "$item_backpack");

        _panel.SetActive(true);

        Layout(gui);
        if (!BindGrid(gui.m_dragItem, forceRebuild: true))
        {
            Hide();
            return false;
        }

        UpdateWeightLabel();
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
            _weight = null;
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
            BindGrid(_gui.m_dragItem, forceRebuild: false);
            UpdateWeightLabel();
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
        {
            WireGridCallbacks(gui);
            return;
        }

        var parent = gui.m_container.parent != null ? gui.m_container.parent : gui.m_inventoryRoot;
        _panel = Object.Instantiate(gui.m_container.gameObject, parent);
        _panel.name = PanelName;

        _grid = _panel.GetComponentInChildren<InventoryGrid>(true);
        if (_grid != null)
        {
            _grid.name = GridName;
            ClearGridRoot(_grid);
            WireGridCallbacks(gui);
        }

        _name = null;

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

        _weight = null;
        if (gui.m_containerWeight != null)
        {
            foreach (var tmp in _panel.GetComponentsInChildren<TMP_Text>(true))
            {
                if (tmp.name == gui.m_containerWeight.name)
                {
                    _weight = tmp;
                    break;
                }
            }
        }

        // Take All / Stack All would target m_currentContainer (the chest). Hide them.
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

    private static void WireGridCallbacks(InventoryGui gui)
    {
        if (_grid == null || gui?.m_containerGrid == null)
            return;

        var src = gui.m_containerGrid;
        _grid.m_onSelected = src.m_onSelected;
        _grid.m_onRightClick = src.m_onRightClick;
        _grid.m_onReleased = src.m_onReleased;
        _grid.OnMoveToUpperInventoryGrid = src.OnMoveToUpperInventoryGrid;
        _grid.OnMoveToLowerInventoryGrid = src.OnMoveToLowerInventoryGrid;
        _grid.OnSetTouchSelection = src.OnSetTouchSelection;
        _grid.CanDropDragOntoItem = src.CanDropDragOntoItem;

        if (_grid.m_uiGroup != null)
            _grid.m_uiGroup.gameObject.SetActive(true);
    }

    /// <summary>
    /// Mirror InventoryGui.UpdateContainerWeight for the side panel badge.
    /// Shows raw contents weight (same as a chest). Player carry weight already
    /// includes contents × backpack WeightMultiplier via ItemData.GetWeight.
    /// </summary>
    private static void UpdateWeightLabel()
    {
        if (_weight == null)
            return;

        var inv = GetInventory();
        var total = inv != null ? Mathf.CeilToInt(inv.GetTotalWeight()) : 0;
        _weight.text = total.ToString();
    }

    private static bool BindGrid(ItemDrop.ItemData dragItem, bool forceRebuild)
    {
        var inv = GetInventory();
        if (_grid == null || inv == null)
            return false;

        try
        {
            if (forceRebuild)
            {
                ClearGridRoot(_grid);
                _grid.m_width = 0;
                _grid.m_height = 0;
            }

            // Vanilla chests pass null Player so hotbar binding numbers stay off.
            _grid.UpdateInventory(inv, null, dragItem);

            if (forceRebuild)
                _grid.ResetView();

            return true;
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Secondary backpack panel update failed: {ex.Message}");
            return false;
        }
    }

    private static void ClearGridRoot(InventoryGrid grid)
    {
        if (grid == null)
            return;

        if (grid.m_elements != null)
        {
            foreach (var element in grid.m_elements)
            {
                if (element != null)
                    Object.DestroyImmediate(element.gameObject);
            }

            grid.m_elements.Clear();
        }

        if (grid.m_gridRoot != null)
        {
            for (var i = grid.m_gridRoot.childCount - 1; i >= 0; i--)
            {
                var child = grid.m_gridRoot.GetChild(i);
                if (child != null)
                    Object.DestroyImmediate(child.gameObject);
            }
        }

        grid.m_width = 0;
        grid.m_height = 0;
    }

    private static void Layout(InventoryGui gui)
    {
        if (_panel == null || gui.m_player == null)
            return;

        var rt = _panel.GetComponent<RectTransform>();
        if (rt == null)
            return;

        var parent = rt.parent as RectTransform;
        if (parent == null)
            return;

        var player = gui.m_player;

        rt.anchorMin = player.anchorMin;
        rt.anchorMax = player.anchorMax;
        rt.pivot = player.pivot;

        var contentSize = ApplyContentSize(rt);
        Canvas.ForceUpdateCanvases();

        var gap = ConfigRegistry.AlongsidePanelGap != null ? ConfigRegistry.AlongsidePanelGap.Value : 72f;
        var offsetY = ConfigRegistry.AlongsidePanelOffsetY != null ? ConfigRegistry.AlongsidePanelOffsetY.Value : 0f;
        var scale = ConfigRegistry.AlongsidePanelScale != null ? ConfigRegistry.AlongsidePanelScale.Value : 1f;
        scale = Mathf.Clamp(scale, 0.5f, 1.5f);

        // Top of inventory background; right past player + weight/armor chrome.
        var bkg = player.Find("Bkg") as RectTransform ?? player;
        var topY = GetLocalTop(parent, bkg);

        var rightX = GetLocalRight(parent, player);
        rightX = Mathf.Max(rightX, GetLocalRight(parent, gui.m_weight != null ? gui.m_weight.rectTransform : null));
        rightX = Mathf.Max(rightX, GetLocalRight(parent, gui.m_armor != null ? gui.m_armor.rectTransform : null));

        var width = contentSize.x;
        var height = contentSize.y;
        SidePanelLayoutMath.TopLeftFromAnchors(rightX, topY, gap, offsetY, out var localLeft, out var localTop);
        SidePanelLayoutMath.PivotFromTopLeft(
            localLeft, localTop, width, height, rt.pivot.x, rt.pivot.y,
            out var pivotLocalX, out var pivotLocalY);

        var parentRect = parent.rect;
        SidePanelLayoutMath.ParentLocalToAnchored(
            pivotLocalX, pivotLocalY,
            parentRect.xMin, parentRect.xMax, parentRect.yMin, parentRect.yMax,
            rt.anchorMin.x, rt.anchorMax.x, rt.anchorMin.y, rt.anchorMax.y,
            out var anchoredX, out var anchoredY);

        rt.anchoredPosition = new Vector2(anchoredX, anchoredY);
        rt.localScale = Vector3.one * scale;
        rt.SetAsLastSibling();
    }

    private static Vector2 ApplyContentSize(RectTransform rt)
    {
        var inv = GetInventory();
        var cell = _grid != null && _grid.m_elementSpace > 1f ? _grid.m_elementSpace : DefaultCell;

        int cols = inv != null && inv.m_width > 0 ? inv.m_width : 4;
        int rows = inv != null && inv.m_height > 0 ? inv.m_height : 2;

        var content = new Vector2(
            SidePanelLayoutMath.ContentWidth(cols, cell, PadX),
            SidePanelLayoutMath.ContentHeight(rows, cell, PadY));
        rt.sizeDelta = content;
        return content;
    }

    private static float GetLocalRight(RectTransform parent, RectTransform target)
    {
        if (parent == null || target == null || !target.gameObject.activeInHierarchy)
            return float.MinValue;

        target.GetWorldCorners(CornerBuffer);
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

}
