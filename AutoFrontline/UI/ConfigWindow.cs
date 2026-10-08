using ConfigWindowBase = ECommons.SimpleGui.ConfigWindow;
using Dalamud.Interface.Utility.Raii;
using MirageUI.Theme;

namespace AutoFrontline.UI;

public sealed class ConfigWindow : ConfigWindowBase
{
    private ImRaii.ColorDisposable _themeScope;

    public ConfigWindow()
    {
        WindowName = "Auto Frontline###autofrontline";
        MirageWindowDefaults.ApplyTo(this);
    }

    public override void PreDraw()
    {
        WindowName = "Auto Frontline###autofrontline";
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);

        MirageTheme.EnsureDefaultsCaptured();
        _themeScope = MirageTheme.PushCustom(MirageTheme.ResolveAppliedColors());
    }

    public override void PostDraw()
    {
        MirageTheme.Pop(_themeScope);
        _themeScope = default;
        ImGui.PopStyleVar();
    }

    public override void Draw() => GenericHelpers.Safe(ConfigWindowContent.Draw);
}
