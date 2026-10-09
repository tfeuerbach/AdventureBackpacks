namespace AdventureBackpacks.Features.SidePanel;

/// <summary>
/// Pure layout helpers for parking the side backpack panel to the right of inventory.
/// </summary>
public static class SidePanelLayoutMath
{
    public static float ContentWidth(int columns, float cell, float padX) =>
        columns * cell + padX;

    public static float ContentHeight(int rows, float cell, float padY) =>
        rows * cell + padY;

    /// <summary>
    /// Desired top-left of the panel in parent-local space.
    /// </summary>
    public static void TopLeftFromAnchors(
        float rightEdgeX,
        float topEdgeY,
        float gap,
        float offsetY,
        out float localLeft,
        out float localTop)
    {
        localLeft = rightEdgeX + gap;
        localTop = topEdgeY + offsetY;
    }

    /// <summary>
    /// Convert a top-left corner + size into a pivot position (parent-local).
    /// </summary>
    public static void PivotFromTopLeft(
        float localLeft,
        float localTop,
        float width,
        float height,
        float pivotX,
        float pivotY,
        out float pivotLocalX,
        out float pivotLocalY)
    {
        pivotLocalX = localLeft + width * pivotX;
        pivotLocalY = localTop - height * (1f - pivotY);
    }

    /// <summary>
    /// anchoredPosition = parentLocal - anchorReference (midpoint of anchorMin/Max in parent rect).
    /// </summary>
    public static void ParentLocalToAnchored(
        float parentLocalX,
        float parentLocalY,
        float parentXMin,
        float parentXMax,
        float parentYMin,
        float parentYMax,
        float anchorMinX,
        float anchorMaxX,
        float anchorMinY,
        float anchorMaxY,
        out float anchoredX,
        out float anchoredY)
    {
        var anchorRefX = Lerp(parentXMin, parentXMax, (anchorMinX + anchorMaxX) * 0.5f);
        var anchorRefY = Lerp(parentYMin, parentYMax, (anchorMinY + anchorMaxY) * 0.5f);
        anchoredX = parentLocalX - anchorRefX;
        anchoredY = parentLocalY - anchorRefY;
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
