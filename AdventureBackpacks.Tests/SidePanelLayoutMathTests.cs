using AdventureBackpacks.Features.SidePanel;
using Xunit;

namespace AdventureBackpacks.Tests;

public class SidePanelLayoutMathTests
{
    [Fact]
    public void ContentSize_MatchesBlackForestLevel2_Defaults()
    {
        // RegisterBackpackSize(2, 4, 2) — Rugged Backpack Level 2
        const int cols = 4;
        const int rows = 2;
        const float cell = 64f;
        const float padX = 70f;
        const float padY = 110f;

        Assert.Equal(4 * 64f + 70f, SidePanelLayoutMath.ContentWidth(cols, cell, padX));
        Assert.Equal(2 * 64f + 110f, SidePanelLayoutMath.ContentHeight(rows, cell, padY));
    }

    [Fact]
    public void TopLeft_SitsRightOfInventoryWithGap()
    {
        SidePanelLayoutMath.TopLeftFromAnchors(
            rightEdgeX: 100f,
            topEdgeY: 50f,
            gap: 72f,
            offsetY: 10f,
            out var left,
            out var top);

        Assert.Equal(172f, left);
        Assert.Equal(60f, top);
    }

    [Fact]
    public void PivotFromTopLeft_CenterPivot()
    {
        SidePanelLayoutMath.PivotFromTopLeft(
            localLeft: 0f,
            localTop: 100f,
            width: 200f,
            height: 80f,
            pivotX: 0.5f,
            pivotY: 0.5f,
            out var x,
            out var y);

        Assert.Equal(100f, x);
        Assert.Equal(60f, y); // 100 - 80 * 0.5
    }

    [Fact]
    public void ParentLocalToAnchored_ZeroAnchors_MatchesLocal()
    {
        // parent rect [-100,100] x [-50,50], anchors at 0.5/0.5 → ref is (0,0)
        SidePanelLayoutMath.ParentLocalToAnchored(
            parentLocalX: 40f,
            parentLocalY: -10f,
            parentXMin: -100f,
            parentXMax: 100f,
            parentYMin: -50f,
            parentYMax: 50f,
            anchorMinX: 0.5f,
            anchorMaxX: 0.5f,
            anchorMinY: 0.5f,
            anchorMaxY: 0.5f,
            out var ax,
            out var ay);

        Assert.Equal(40f, ax);
        Assert.Equal(-10f, ay);
    }
}
