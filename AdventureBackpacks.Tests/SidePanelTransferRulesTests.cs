using AdventureBackpacks.Features.SidePanel;
using Xunit;

namespace AdventureBackpacks.Tests;

public class SidePanelTransferRulesTests
{
    [Fact]
    public void CtrlClick_FromBackpack_WithChest_MovesToChest()
    {
        var d = SidePanelTransferRules.ResolveCtrlClick(
            SidePanelInventoryKind.Backpack,
            chestOpen: true,
            playerCanAccept: true,
            backpackCanAccept: true,
            isQuestItem: false);

        Assert.Equal(SidePanelMoveAction.Move, d.Action);
        Assert.Equal(SidePanelInventoryKind.Backpack, d.From);
        Assert.Equal(SidePanelInventoryKind.Chest, d.To);
    }

    [Fact]
    public void CtrlClick_FromBackpack_WithoutChest_MovesToPlayer()
    {
        var d = SidePanelTransferRules.ResolveCtrlClick(
            SidePanelInventoryKind.Backpack,
            chestOpen: false,
            playerCanAccept: true,
            backpackCanAccept: true,
            isQuestItem: false);

        Assert.Equal(SidePanelMoveAction.Move, d.Action);
        Assert.Equal(SidePanelInventoryKind.Player, d.To);
    }

    [Fact]
    public void CtrlClick_FromPlayer_WithoutChest_MovesToBackpack()
    {
        var d = SidePanelTransferRules.ResolveCtrlClick(
            SidePanelInventoryKind.Player,
            chestOpen: false,
            playerCanAccept: true,
            backpackCanAccept: true,
            isQuestItem: false);

        Assert.Equal(SidePanelMoveAction.Move, d.Action);
        Assert.Equal(SidePanelInventoryKind.Backpack, d.To);
    }

    [Fact]
    public void CtrlClick_FromPlayer_WithChest_DefersToVanilla()
    {
        var d = SidePanelTransferRules.ResolveCtrlClick(
            SidePanelInventoryKind.Player,
            chestOpen: true,
            playerCanAccept: true,
            backpackCanAccept: true,
            isQuestItem: false);

        Assert.Equal(SidePanelMoveAction.DeferToVanilla, d.Action);
    }

    [Fact]
    public void CtrlClick_FromChest_PlayerHasRoom_DefersToVanilla()
    {
        var d = SidePanelTransferRules.ResolveCtrlClick(
            SidePanelInventoryKind.Chest,
            chestOpen: true,
            playerCanAccept: true,
            backpackCanAccept: true,
            isQuestItem: false);

        Assert.Equal(SidePanelMoveAction.DeferToVanilla, d.Action);
    }

    [Fact]
    public void CtrlClick_FromChest_PlayerFull_BackpackHasRoom_MovesToBackpack()
    {
        var d = SidePanelTransferRules.ResolveCtrlClick(
            SidePanelInventoryKind.Chest,
            chestOpen: true,
            playerCanAccept: false,
            backpackCanAccept: true,
            isQuestItem: false);

        Assert.Equal(SidePanelMoveAction.Move, d.Action);
        Assert.Equal(SidePanelInventoryKind.Chest, d.From);
        Assert.Equal(SidePanelInventoryKind.Backpack, d.To);
    }

    [Fact]
    public void CtrlClick_FromChest_PlayerAndBackpackFull_DefersToVanilla()
    {
        var d = SidePanelTransferRules.ResolveCtrlClick(
            SidePanelInventoryKind.Chest,
            chestOpen: true,
            playerCanAccept: false,
            backpackCanAccept: false,
            isQuestItem: false);

        Assert.Equal(SidePanelMoveAction.DeferToVanilla, d.Action);
    }

    [Fact]
    public void CtrlClick_QuestItem_OnOurPath_Blocks()
    {
        var d = SidePanelTransferRules.ResolveCtrlClick(
            SidePanelInventoryKind.Backpack,
            chestOpen: true,
            playerCanAccept: true,
            backpackCanAccept: true,
            isQuestItem: true);

        Assert.Equal(SidePanelMoveAction.Block, d.Action);
        Assert.Equal(SidePanelInventoryKind.Chest, d.To);
    }

    [Theory]
    [InlineData(true, false, true, false, false, true)]
    [InlineData(false, false, true, false, false, false)]
    [InlineData(true, true, true, false, false, false)]
    [InlineData(true, false, false, false, false, false)]
    [InlineData(true, false, true, true, false, false)]
    [InlineData(true, false, true, false, true, false)]
    public void Overflow_Rules(
        bool overflowEnabled,
        bool playerCanAccept,
        bool backpackCanAccept,
        bool itemAlreadyInBackpack,
        bool isBackpackItem,
        bool expected)
    {
        var result = SidePanelTransferRules.ShouldOverflowIntoBackpack(
            overflowEnabled,
            playerCanAccept,
            backpackCanAccept,
            itemAlreadyInBackpack,
            isBackpackItem);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    public void AutoStore_WhileBackpackUiOpen(bool backpackIsOpen, bool sidePanelIsOpen, bool expected)
    {
        Assert.Equal(
            expected,
            SidePanelTransferRules.AllowAutoStoreWhileBackpackUiOpen(backpackIsOpen, sidePanelIsOpen));
    }
}
