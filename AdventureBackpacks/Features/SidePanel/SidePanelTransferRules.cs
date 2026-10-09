namespace AdventureBackpacks.Features.SidePanel;

/// <summary>
/// Pure ctrl+click / move routing for the fork side-panel feature.
/// No Unity / Valheim types — unit tested so upstream merges cannot silently break behavior.
/// </summary>
public enum SidePanelInventoryKind
{
    Unknown = 0,
    Player = 1,
    Backpack = 2,
    Chest = 3
}

public enum SidePanelMoveAction
{
    /// <summary>Leave InventoryGui.OnSelectedItem alone.</summary>
    DeferToVanilla = 0,

    /// <summary>Skip vanilla and do not transfer (e.g. quest item on our path).</summary>
    Block = 1,

    /// <summary>Perform the move ourselves: destination ← source.</summary>
    Move = 2
}

public readonly struct SidePanelMoveDecision
{
    public SidePanelMoveDecision(SidePanelMoveAction action, SidePanelInventoryKind from, SidePanelInventoryKind to)
    {
        Action = action;
        From = from;
        To = to;
    }

    public SidePanelMoveAction Action { get; }
    public SidePanelInventoryKind From { get; }
    public SidePanelInventoryKind To { get; }

    public static SidePanelMoveDecision Defer =>
        new SidePanelMoveDecision(SidePanelMoveAction.DeferToVanilla, SidePanelInventoryKind.Unknown, SidePanelInventoryKind.Unknown);

    public static SidePanelMoveDecision Blocked(SidePanelInventoryKind from, SidePanelInventoryKind to) =>
        new SidePanelMoveDecision(SidePanelMoveAction.Block, from, to);

    public static SidePanelMoveDecision MoveTo(SidePanelInventoryKind from, SidePanelInventoryKind to) =>
        new SidePanelMoveDecision(SidePanelMoveAction.Move, from, to);
}

public static class SidePanelTransferRules
{
    /// <summary>
    /// Resolve ctrl+click (Modifier.Move) when the side backpack panel is open.
    /// </summary>
    /// <param name="from">Inventory that owns the clicked slot.</param>
    /// <param name="chestOpen">True when a world container (not the backpack proxy) is current.</param>
    /// <param name="playerCanAccept">Player inventory can take the full stack.</param>
    /// <param name="backpackCanAccept">Backpack inventory can take the full stack.</param>
    /// <param name="isQuestItem">Quest items must not be moved by our path.</param>
    public static SidePanelMoveDecision ResolveCtrlClick(
        SidePanelInventoryKind from,
        bool chestOpen,
        bool playerCanAccept,
        bool backpackCanAccept,
        bool isQuestItem)
    {
        if (from == SidePanelInventoryKind.Unknown)
            return SidePanelMoveDecision.Defer;

        SidePanelMoveDecision decision;

        if (from == SidePanelInventoryKind.Backpack)
        {
            // Backpack → chest (if open), else player.
            decision = SidePanelMoveDecision.MoveTo(
                SidePanelInventoryKind.Backpack,
                chestOpen ? SidePanelInventoryKind.Chest : SidePanelInventoryKind.Player);
        }
        else if (from == SidePanelInventoryKind.Player && !chestOpen)
        {
            // No chest: player → backpack.
            decision = SidePanelMoveDecision.MoveTo(SidePanelInventoryKind.Player, SidePanelInventoryKind.Backpack);
        }
        else if (from == SidePanelInventoryKind.Chest && chestOpen)
        {
            // Chest → player if it fits; otherwise → backpack (overflow).
            if (playerCanAccept)
                return SidePanelMoveDecision.Defer;

            if (!backpackCanAccept)
                return SidePanelMoveDecision.Defer;

            decision = SidePanelMoveDecision.MoveTo(SidePanelInventoryKind.Chest, SidePanelInventoryKind.Backpack);
        }
        else if (from == SidePanelInventoryKind.Player && chestOpen)
        {
            // Player → chest stays vanilla.
            return SidePanelMoveDecision.Defer;
        }
        else
        {
            return SidePanelMoveDecision.Defer;
        }

        if (decision.Action == SidePanelMoveAction.Move && decision.To == decision.From)
            return SidePanelMoveDecision.Defer;

        if (decision.Action == SidePanelMoveAction.Move && isQuestItem)
            return SidePanelMoveDecision.Blocked(decision.From, decision.To);

        return decision;
    }

    /// <summary>
    /// Whether an item should overflow into the backpack during a player-inventory AddItem
    /// that failed due to lack of space (chest Take All / ctrl+click / MoveItemToThis).
    /// </summary>
    public static bool ShouldOverflowIntoBackpack(
        bool overflowEnabled,
        bool playerCanAccept,
        bool backpackCanAccept,
        bool itemAlreadyInBackpack,
        bool isBackpackOrBackpackItem)
    {
        if (!overflowEnabled)
            return false;

        if (isBackpackOrBackpackItem)
            return false;

        if (playerCanAccept)
            return false;

        if (itemAlreadyInBackpack)
            return false;

        return backpackCanAccept;
    }

    /// <summary>
    /// Auto-store / overflow gating while the backpack UI is considered "open".
    /// Side-panel mode must still allow overflow; vanilla backpack-as-container must not.
    /// </summary>
    public static bool AllowAutoStoreWhileBackpackUiOpen(bool backpackIsOpen, bool sidePanelIsOpen)
    {
        if (!backpackIsOpen)
            return true;

        return sidePanelIsOpen;
    }
}
