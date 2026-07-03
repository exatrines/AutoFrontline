using AutoFrontline.Services;

namespace AutoFrontline.UI;

internal static class MountPicker
{
    private static string searchFilter = string.Empty;

    public static void Draw(float width) =>
        MirageUi.SearchCombo(
            "Mount##AflMountCombo",
            MountCatalog.GetDisplayName(C.MountSelectionId),
            width,
            ref searchFilter,
            "##AflMountSearch"u8,
            "Search...",
            OnPopupOpened,
            DrawOptions);

    private static void OnPopupOpened()
    {
        searchFilter = string.Empty;
        MountCatalog.InvalidateCache();
    }

    private static void DrawOptions()
    {
        foreach (var option in MountCatalog.GetOptions())
        {
            if (!MirageUi.MatchesFilter(option.DisplayName, option.DisplayName, searchFilter))
                continue;

            var selected = option.SelectionId == C.MountSelectionId;
            if (ImGui.Selectable(option.DisplayName, selected))
                C.MountSelectionId = option.SelectionId;

            if (ImGui.IsWindowAppearing() && selected)
                ImGui.SetScrollHereY();
        }
    }
}
