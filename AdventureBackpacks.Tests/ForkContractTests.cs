using Xunit;

namespace AdventureBackpacks.Tests;

/// <summary>
/// Source-level contracts so an upstream mirror/merge cannot drop the fork feature files
/// or rename critical APIs without failing CI.
/// </summary>
public class ForkContractTests
{
    private static string RepoRoot
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "AdventureBackpacks.sln")))
                    return dir.FullName;
                dir = dir.Parent;
            }

            throw new InvalidOperationException("Could not locate AdventureBackpacks.sln from test output path.");
        }
    }

    private static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(RepoRoot, relativePath));

    [Theory]
    [InlineData("AdventureBackpacks/Features/SecondaryBackpackPanel.cs")]
    [InlineData("AdventureBackpacks/Features/SidePanel/SidePanelTransferRules.cs")]
    [InlineData("AdventureBackpacks/Features/SidePanel/SidePanelLayoutMath.cs")]
    public void ForkFeatureFile_Exists(string relativePath)
    {
        Assert.True(File.Exists(Path.Combine(RepoRoot, relativePath)), $"Missing {relativePath}");
    }

    [Fact]
    public void ConfigRegistry_DefinesOpenAlongsideSettings()
    {
        var text = Read("AdventureBackpacks/Configuration/ConfigRegistry.cs");
        Assert.Contains("OpenAlongsideContainer", text);
        Assert.Contains("AlongsidePanelGap", text);
        Assert.Contains("AlongsidePanelOffsetY", text);
        Assert.Contains("AlongsidePanelScale", text);
        Assert.Contains("Open Alongside Container", text);
    }

    [Fact]
    public void PlayerExtensions_RoutesOpenAlongsideToSidePanel()
    {
        var text = Read("AdventureBackpacks/Extensions/PlayerExtensions.cs");
        Assert.Contains("OpenAlongsideContainer", text);
        Assert.Contains("SecondaryBackpackPanel.Show", text);
    }

    [Fact]
    public void InventoryGui_UsesSidePanelTransferRules()
    {
        var text = Read("AdventureBackpacks/Patches/InventoryGui.cs");
        Assert.Contains("TryOpenSidePanel", text);
        Assert.Contains("SidePanelTransferRules.ResolveCtrlClick", text);
        Assert.Contains("SecondaryBackpackPanel", text);
    }

    [Fact]
    public void StoreToBackpack_UsesOverflowRules()
    {
        var text = Read("AdventureBackpacks/Features/StoreToBackpack.cs");
        Assert.Contains("TryOverflowIntoBackpack", text);
        Assert.Contains("SidePanelTransferRules.ShouldOverflowIntoBackpack", text);
        Assert.Contains("AllowAutoStoreWhileBackpackUiOpen", text);
    }

    [Fact]
    public void Project_IncludesSidePanelSources()
    {
        var text = Read("AdventureBackpacks/AdventureBackpacks.csproj");
        Assert.Contains("Features\\SecondaryBackpackPanel.cs", text);
        Assert.Contains("Features\\SidePanel\\SidePanelTransferRules.cs", text);
        Assert.Contains("Features\\SidePanel\\SidePanelLayoutMath.cs", text);
    }

    [Fact]
    public void Readme_DeclaresForkAndFeature()
    {
        var text = Read("README.md");
        Assert.Contains("unofficial fork", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Open Alongside Container", text);
        Assert.Contains("Vapok/AdventureBackpacks", text);
    }
}
