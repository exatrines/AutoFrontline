using System.Linq;
using AutoFrontline.Services;

namespace AutoFrontline.UI;

internal static class MountPicker
{
    private static string searchFilter = string.Empty;

    public static void Draw()
    {
        var options = MountCatalog.GetOptions();
        var names = options.Select(option => option.DisplayName).ToArray();
        var selected = MountCatalog.GetDisplayName(C.MountSelectionId);

        if (!MirageUi.SearchableDropdown(
                I18n.Get("settings.mount"),
                ref selected,
                names,
                ref searchFilter,
                allowClear: false,
                searchHint: I18n.Get("settings.mount.search"),
                id: "mount"))
            return;

        foreach (var option in options)
        {
            if (option.DisplayName != selected)
                continue;

            C.MountSelectionId = option.SelectionId;
            return;
        }
    }
}
