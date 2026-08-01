using System;
using System.Windows.Media;
using PCL.Core.App;
using PCL.Core.App.Configuration;
using PCL.Core.App.IoC;

namespace PCL.Core.UI.Theme;

/// <summary>
/// 配色模式更改事件。
/// </summary>
/// <param name="isDarkMode">当前是否为深色模式</param>
/// <param name="theme">当前配色主题</param>
public delegate void ColorModeChangedEvent(bool isDarkMode, ColorTheme theme);

/// <summary>
/// 配色主题更改事件。
/// </summary>
/// <param name="theme">当前配色主题</param>
public delegate void ColorThemeChangedEvent(ColorTheme theme);

[LifecycleScope("theme", "主题", false)]
[LifecycleService(LifecycleState.WindowCreating)]
public sealed partial class ThemeService
{
    [AnyConfigItem<ToneProfileConfig>("UiToneProfiles", ConfigSource.Local)]
    public static partial ToneProfileConfig ToneProfiles { get; set; }

    [RegisterConfigEvent]
    public static ConfigEventRegistry OnColorModeConfigChanged => new(
        scope: Config.Preference.Theme.ColorModeConfig,
        trigger: ConfigEvent.Update,
        handler: _ => RefreshColorMode()
    );

    [RegisterConfigEvent]
    public static ConfigEventRegistry OnColorThemeConfigChanged => new(
        scope: [Config.Preference.Theme.DarkColorConfig, Config.Preference.Theme.LightColorConfig],
        trigger: ConfigEvent.Update,
        handler: e =>
        {
            // ignore no change or non-current color theme change
            if (e.OldValue == e.Value) return;
            if (IsDarkMode) { if (e.Item == Config.Preference.Theme.LightColorConfig) return; }
            else { if (e.Item == Config.Preference.Theme.DarkColorConfig) return; }
            // trigger color refresh
            if (Lifecycle.CurrentState > LifecycleState.Loading)
            {
                Lifecycle.CurrentApplication.Dispatcher.BeginInvoke(() =>
                {
                    ApplyColorResources();
                    ColorThemeChanged?.Invoke(CurrentTheme);
                    _AprilFoolLogic();
                });
            }
        }
    );

    [LifecycleStart]
    private static void _Start()
    {
        IsDarkMode = _IsDarkMode();
        _LogStatus();
        _RefreshAll();
    }

    private static void _LogStatus()
    {
        Context.Debug($"当前状态: {(IsDarkMode ? "Dark" : "Light")}, {CurrentTheme}");
    }

    private static bool _IsDarkMode() => Config.Preference.Theme.ColorMode switch
    {
        ColorMode.Light => false,
        ColorMode.Dark => true,
        ColorMode.System => SystemThemeHelper.IsSystemInDarkMode(),
        _ => false
    };

    /// <summary>
    /// 当前是否为深色主题。
    /// </summary>
    public static bool IsDarkMode { get; private set; }

    /// <summary>
    /// 配色模式更改事件。
    /// </summary>
    public static event ColorModeChangedEvent? ColorModeChanged;

    /// <summary>
    /// 配色主题更改事件。
    /// </summary>
    public static event ColorThemeChangedEvent? ColorThemeChanged;

    private static void _RefreshAll()
    {
        ApplyGrayResources();
        ApplyColorResources();
        ApplyGlassResources();
        ColorModeChanged?.Invoke(IsDarkMode, CurrentTheme);
        _AprilFoolLogic();
    }

    private static void _AprilFoolLogic()
    {
        // for HMCL theme on April Fools' Day
        if (Basics.IsAprilFool) Config.Preference.WindowTitleTypeConfig.TriggerEvent(ConfigEvent.Changed, null);
        else if (CurrentTheme == ColorTheme.HmclBlue) CurrentTheme = ColorTheme.CatBlue;
    }

    /// <summary>
    /// 刷新配色模式，若检测到当前配色模式有实际更改，则会触发主题刷新。
    /// </summary>
    public static void RefreshColorMode()
    {
        var isDarkMode = _IsDarkMode();
        if (IsDarkMode == isDarkMode) return;
        Context.Info("正在更改配色模式");
        IsDarkMode = isDarkMode;
        _LogStatus();
        if (Lifecycle.CurrentState > LifecycleState.Loading)
        {
            Lifecycle.CurrentApplication.Dispatcher.BeginInvoke(_RefreshAll);
        }
    }

    /// <summary>
    /// 当前使用的色彩属性。
    /// </summary>
    public static ToneProfile CurrentTone => IsDarkMode ? ToneProfiles.Dark : ToneProfiles.Light;

    /// <summary>
    /// 当前使用的主题色。
    /// </summary>
    public static ColorTheme CurrentTheme
    {
        get
        {
            var theme = Config.Preference.Theme;
            return IsDarkMode ? theme.DarkColor : theme.LightColor;
        }
        set
        {
            var theme = Config.Preference.Theme;
            var config = IsDarkMode ? theme.DarkColorConfig : theme.LightColorConfig;
            config.SetValue(value);
        }
    }

    /// <summary>
    /// 获取当前色彩主题对应的各种参数。
    /// </summary>
    public static (int Hue, double LightAdjust, double ChromaAdjust) GetCurrentThemeArgs()
    {
        var theme = CurrentTheme;
        // 自定义主题：从 UiLauncherHue/Sat/Light 配置计算色相与明度/彩度调整量
        if (theme == ColorTheme.Custom)
        {
            var t = Config.Preference.Theme;
            var hue = ((t.WindowHue % 360) + 360) % 360;
            // 明度配置 0-100（默认 48），映射为 -0.5~0.5 的调整量
            var lightAdjust = (t.WindowLight - 50) / 100.0;
            // 饱和度配置 0-100（默认 75），映射为 -0.5~0.5 的调整量
            var chromaAdjust = (t.WindowSat - 50) / 100.0;
            return (hue, lightAdjust, chromaAdjust);
        }
        return theme switch
        {
            ColorTheme.SkyBlue => (235, 0.36, 0.2),
            ColorTheme.CatBlue => (255, 0, -0.2),
            ColorTheme.DeathBlue => (268, -0.05, -0.1),
            ColorTheme.HmclBlue => (275, -0.03, -0.35),
            // 粉蓝渐变：以粉色为主色相（340），其余参数中性
            ColorTheme.PinkBlue => (340, 0.05, 0.0),
#if DEBUG
            _ => ((int)theme, 0, 0)
#else
            _ => throw new IndexOutOfRangeException($"Invalid theme index: {(int)theme}")
#endif
        };
    }

    private static double _AdjustLinear(double value, double adjustment)
    {
        if (adjustment == 0) return value;
        // 确保输入在合理范围内
        value = Math.Clamp(value, 0.0, 1.0);
        adjustment = Math.Clamp(adjustment, -1.0, 1.0);
        // 非对称线性插值
        return adjustment switch
        {
            > 0 => value + (1.0 - value) * adjustment,
            _ => value + value * adjustment
        };
    }

    private static CatColorResource[] _CalculateGrays(ToneProfile tone) => [
        LabColor.FromLch(tone.L1).ToCatColor("Gray1"),
        LabColor.FromLch(tone.L2).ToCatColor("Gray2"),
        LabColor.FromLch(tone.L3).ToCatColor("Gray3"),
        LabColor.FromLch(tone.L4).ToCatColor("Gray4"),
        LabColor.FromLch(tone.L5).ToCatColor("Gray5"),
        LabColor.FromLch(tone.L6).ToCatColor("Gray6"),
        LabColor.FromLch(tone.L7).ToCatColor("Gray7"),
        LabColor.FromLch(tone.L8).ToCatColor("Gray8"),
        LabColor.FromLch(tone.LWhite, alpha:tone.AHalfWhite).ToCatColor("HalfWhite", false),
        LabColor.FromLch(tone.LWhite, alpha:tone.ASemiWhite).ToCatColor("SemiWhite", false),
        LabColor.FromLch(tone.LWhite).ToCatColor("White", false),
        LabColor.FromLch(tone.LWhite, alpha:tone.ATransparent).ToCatColor("Transparent", false),
        LabColor.FromLch(tone.LBackground, alpha:tone.ABackground).ToCatColor("TransparentBackground", false),
        LabColor.FromLch(tone.LBackground).ToCatColor("Background", false),
        LabColor.FromLch(tone.LBackground, alpha:tone.AToolTip).ToCatColor("ToolTip", false),
        LabColor.FromLch(tone.L7, 0.25, 30, tone.AHalfTransparent).ToCatColor("RedBack", false),
        LabColor.FromLch(tone.LForeground).ToCatColor("Memory", false),
    ];

    private static CatColorResource[] _CalculateColors(ToneProfile tone, (int hue, double lightAdj, double chromaAdj) args) =>
        _CalculateColors(tone, args, CurrentTheme);

    private static CatColorResource[] _CalculateColors(ToneProfile tone, (int hue, double lightAdj, double chromaAdj) args, ColorTheme theme)
    {
        // 粉蓝渐变主题：深色等级用蓝色(66CCFF, LCh色相246)，浅色等级用粉色(LCh色相340)，分段避免经过紫色色相区
        var isPinkBlue = theme == ColorTheme.PinkBlue;
        double HueForLevel(int level) => isPinkBlue
            ? level <= 4 ? 246.0                          // L1-L4: 66CCFF 蓝
                         : 340.0                          // L5-L8: 粉
            : args.hue;
        return [
            LabColor.FromLch(_AdjustLinear(tone.L1, args.lightAdj * 0.1), _AdjustLinear(tone.C1, args.chromaAdj * 0.25), HueForLevel(1)).ToCatColor("1"),
            LabColor.FromLch(_AdjustLinear(tone.L2, args.lightAdj), _AdjustLinear(tone.C2, args.chromaAdj), HueForLevel(2)).ToCatColor("2"),
            LabColor.FromLch(_AdjustLinear(tone.L3, args.lightAdj), _AdjustLinear(tone.C3, args.chromaAdj), HueForLevel(3)).ToCatColor("3"),
            LabColor.FromLch(_AdjustLinear(tone.L4, args.lightAdj), _AdjustLinear(tone.C4, args.chromaAdj), HueForLevel(4)).ToCatColor("4"),
            LabColor.FromLch(_AdjustLinear(tone.L5, args.lightAdj), _AdjustLinear(tone.C5, args.chromaAdj), HueForLevel(5)).ToCatColor("5"),
            LabColor.FromLch(_AdjustLinear(tone.L6, args.lightAdj), _AdjustLinear(tone.C6, args.chromaAdj), HueForLevel(6)).ToCatColor("6"),
            LabColor.FromLch(_AdjustLinear(tone.L7, args.lightAdj), _AdjustLinear(tone.C7, args.chromaAdj), HueForLevel(7)).ToCatColor("7"),
            LabColor.FromLch(_AdjustLinear(tone.L8, args.lightAdj), _AdjustLinear(tone.C8, args.chromaAdj), HueForLevel(8)).ToCatColor("8"),
            LabColor.FromLch(_AdjustLinear(tone.L8, args.lightAdj), _AdjustLinear(tone.C8, args.chromaAdj), HueForLevel(8), tone.ASemiTransparent).ToCatColor("SemiTransparent", false),
            LabColor.FromLch(_AdjustLinear(tone.L5, args.lightAdj), _AdjustLinear(tone.C5, args.chromaAdj), HueForLevel(5)).ToCatColor("Bg0"),
            LabColor.FromLch(_AdjustLinear(tone.L7, args.lightAdj), _AdjustLinear(tone.C7, args.chromaAdj), HueForLevel(7), tone.ASemiWhite).ToCatColor("Bg1"),
        ];
    }

    private static CatColorResource[] LightGrayCache { get => field ??= _CalculateGrays(ToneProfiles.Light); set; } = null!;

    private static CatColorResource[] DarkGrayCache { get => field ??= _CalculateGrays(ToneProfiles.Dark); set; } = null!;

    /// <summary>
    /// 清除灰度配色的计算缓存。
    /// </summary>
    public static void InvalidateGrayCache()
    {
        LightGrayCache = null!;
        DarkGrayCache = null!;
    }

    /// <summary>
    /// 应用灰度配色到 WPF 资源字典。
    /// </summary>
    public static void ApplyGrayResources()
    {
        var cache = IsDarkMode ? DarkGrayCache : LightGrayCache;
        foreach (var c in cache) c.Apply();
    }

    /// <summary>
    /// 应用彩色配色到 WPF 资源字典。
    /// </summary>
    public static void ApplyColorResources()
    {
        var colors = _CalculateColors(CurrentTone, GetCurrentThemeArgs());
        foreach (var c in colors) c.Apply();
    }

    /// <summary>
    /// 根据深浅色模式应用液态玻璃资源。
    /// 浅色模式下使用带蓝灰色调的材质以增强对比度，深色模式保持白色高光。
    /// </summary>
    public static void ApplyGlassResources()
    {
        var app = System.Windows.Application.Current;
        if (app is null) return;
        var res = app.Resources;

        if (IsDarkMode)
        {
            // 深色模式：白色高光在深色背景下显眼
            res["LiquidGlassBackground"] = new SolidColorBrush(Color.FromArgb(0xC8, 0xFF, 0xFF, 0xFF));
            res["LiquidGlassBackgroundAlt"] = new SolidColorBrush(Color.FromArgb(0xA8, 0xF2, 0xF6, 0xFB));
            res["LiquidGlassBackgroundDeep"] = new SolidColorBrush(Color.FromArgb(0xD8, 0xFB, 0xFB, 0xFB));
            res["LiquidGlassEdgeHighlight"] = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
            res["LiquidGlassEdgeHighlightStrong"] = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF));
            res["LiquidGlassBorder"] = new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
            res["LiquidGlassBorderStrong"] = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF));
            res["LiquidGlassTitleBarBackground"] = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
            res["LiquidGlassTitleBarEdge"] = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));
        }
        else
        {
            // 浅色模式：蓝灰色调材质，在浅色背景下形成对比
            res["LiquidGlassBackground"] = new SolidColorBrush(Color.FromArgb(0xB8, 0xE8, 0xED, 0xF5));
            res["LiquidGlassBackgroundAlt"] = new SolidColorBrush(Color.FromArgb(0xB0, 0xDD, 0xE3, 0xF0));
            res["LiquidGlassBackgroundDeep"] = new SolidColorBrush(Color.FromArgb(0xC8, 0xE8, 0xED, 0xF5));
            res["LiquidGlassEdgeHighlight"] = new SolidColorBrush(Color.FromArgb(0x88, 0x66, 0x6C, 0x7A));
            res["LiquidGlassEdgeHighlightStrong"] = new SolidColorBrush(Color.FromArgb(0xAA, 0x50, 0x5A, 0x6E));
            res["LiquidGlassBorder"] = new SolidColorBrush(Color.FromArgb(0x60, 0x50, 0x5A, 0x6E));
            res["LiquidGlassBorderStrong"] = new SolidColorBrush(Color.FromArgb(0x90, 0x50, 0x5A, 0x6E));
            res["LiquidGlassTitleBarBackground"] = new SolidColorBrush(Color.FromArgb(0x99, 0xE8, 0xED, 0xF5));
            res["LiquidGlassTitleBarEdge"] = new SolidColorBrush(Color.FromArgb(0xAA, 0x66, 0x6C, 0x7A));
        }
    }
}
