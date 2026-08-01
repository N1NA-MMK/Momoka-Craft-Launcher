using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using PCL.Core.App;
using PCL.Core.App.Configuration;
using PCL.Core.IO.Net.Http;
using PCL.Core.UI.Theme;
using PCL.Core.Utils.Exts;
using PCL.Network;

using PCL.Core.App.Localization;
namespace PCL;

public class ModSetup
{
    public ModSetup()
    {
        // === Hide Group ===
        ConfigService.RegisterObserver(Config.Preference.Hide,
            new ConfigObserver(ConfigEvent.Changed, _ => PageSetupUI.HiddenRefresh()));

        // === Launch ===
        Config.Launch.MemoryAllocationModeConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => LaunchRamType((int)e.Value!)));
        States.Game.SelectedFolderConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => LaunchFolderSelect((string)(e.Value ?? ""))));
        States.Game.SelectedInstanceConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => LaunchInstanceSelect((string)(e.Value ?? ""))));

        // === Tool ===
        Config.Download.ThreadLimitConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => ToolDownloadThread((int)e.Value!)));
        Config.Download.SpeedLimitConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => ToolDownloadSpeed((int)e.Value!)));

        // === UI - Launcher ===
        Config.Preference.Theme.WindowOpacityConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiLauncherTransparent((int)e.Value!)));
        Config.Preference.Theme.ThemeSelectedConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiLauncherTheme((int)e.Value!)));
        Config.Preference.Background.BackgroundColorfulConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiBackgroundColorful((bool)e.Value!)));
        Config.Preference.LockWindowSizeConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiLockWindowSize((bool)e.Value!)));

        // UI - Video Background
        Config.Preference.Background.AutoPauseVideoConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiAutoPauseVideo((bool)e.Value!)));

        // UI - Background Image
        Config.Preference.Background.WallpaperOpacityConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiBackgroundOpacity((int)e.Value!)));
        Config.Preference.Background.WallpaperBlurRadiusConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiBackgroundBlur((int)e.Value!)));
        Config.Preference.Background.WallpaperSuitModeConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiBackgroundSuit((int)e.Value!)));

        // UI - Font
        Config.Preference.FontConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiFont((string)(e.Value ?? ""))));

        // UI - Homepage
        Config.Preference.Homepage.TypeConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiCustomType((int)e.Value!)));

        // UI - Blur
        Config.Preference.Blur.IsEnabledConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiBlur((bool)e.Value!)));
        Config.Preference.Blur.RadiusConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiBlurValue((int)e.Value!)));
        Config.Preference.Blur.SamplingRateConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiBlurSamplingRate((int)e.Value!)));
        Config.Preference.Blur.KernelTypeConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiBlurType((int)e.Value!)));
        Config.Preference.Blur.GlassTintConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiGlassTint((int)e.Value!)));
        Config.Preference.Blur.GlassShadowRadiusConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiGlassShadowRadius((int)e.Value!)));
        Config.Preference.Blur.GlassShadowOpacityConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiGlassShadowOpacity((int)e.Value!)));
        Config.Preference.Blur.GlassSpecularConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiGlassSpecular((int)e.Value!)));
        Config.Preference.Blur.GlassEdgeConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiGlassEdge((int)e.Value!)));

        // UI - Title Bar
        Config.Preference.WindowTitleTypeConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiLogoType((int)e.Value!)));
        Config.Preference.WindowTitleCustomTextConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiLogoText((string)(e.Value ?? ""))));
        Config.Preference.TopBarLeftAlignConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => UiLogoLeft((bool)e.Value!)));

        // === System ===
        Config.Debug.EnabledConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => SystemDebugMode((bool)e.Value!)));
        Config.Debug.AnimationSpeedConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => SystemDebugAnim((int)e.Value!)));
        Config.Network.HttpProxy.CustomAddressConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => SystemHttpProxy((string)(e.Value ?? ""))));
        Config.Network.HttpProxy.TypeConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => SystemHttpProxyType((int)e.Value!)));
        Config.Network.HttpProxy.CustomUsernameConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => SystemHttpProxyCustomUsername((string)(e.Value ?? ""))));
        Config.Network.HttpProxy.CustomPasswordConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => SystemHttpProxyCustomPassword((string)(e.Value ?? ""))));

        // === Version ===
        Config.Instance.MemorySolutionConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => VersionRamType((int)e.Value!)));
        Config.InstanceAuth.LoginRequirementSolutionConfig.Observe(new ConfigObserver(ConfigEvent.Changed,
            e => VersionServerLogin((int)e.Value!)));
    }

    /// <summary>
    ///     主动应用所有当前配置值。
    /// </summary>
    public static void ApplyAll()
    {
        // Launch
        LaunchRamType(Config.Launch.MemoryAllocationMode);

        // Tool
        ToolDownloadThread(Config.Download.ThreadLimit);
        ToolDownloadSpeed(Config.Download.SpeedLimit);

        // UI - Launcher
        UiLauncherTransparent(Config.Preference.Theme.WindowOpacity);
        UiLauncherTheme(Config.Preference.Theme.ThemeSelected);
        UiBackgroundColorful(Config.Preference.Background.BackgroundColorful);
        UiLockWindowSize(Config.Preference.LockWindowSize);

        // UI - Video Background
        UiAutoPauseVideo(Config.Preference.Background.AutoPauseVideo);

        // UI - Background Image
        UiBackgroundOpacity(Config.Preference.Background.WallpaperOpacity);
        UiBackgroundBlur(Config.Preference.Background.WallpaperBlurRadius);
        UiBackgroundSuit(Config.Preference.Background.WallpaperSuitMode);

        // UI - Font
        UiFont(Config.Preference.Font);

        // UI - Homepage
        UiCustomType(Config.Preference.Homepage.Type);

        // UI - Blur
        if (Config.Preference.Blur.IsEnabled)
        {
            UiBlurValue(Config.Preference.Blur.Radius);
            UiBlurSamplingRate(Config.Preference.Blur.SamplingRate);
            UiBlurType(Config.Preference.Blur.KernelType);
        }
        else
        {
            UiBlurValue(0);
        }

        UiBlur(Config.Preference.Blur.IsEnabled);

        // UI - Glass
        UiGlassTint(Config.Preference.Blur.GlassTint);
        UiGlassShadowRadius(Config.Preference.Blur.GlassShadowRadius);
        UiGlassShadowOpacity(Config.Preference.Blur.GlassShadowOpacity);
        UiGlassSpecular(Config.Preference.Blur.GlassSpecular);
        UiGlassEdge(Config.Preference.Blur.GlassEdge);

        // UI - Title Bar
        UiLogoType((int)Config.Preference.WindowTitleType);
        UiLogoText(Config.Preference.WindowTitleCustomText);
        UiLogoLeft(Config.Preference.TopBarLeftAlign);

        // UI - Hide
        PageSetupUI.HiddenRefresh();

        // System
        SystemDebugMode(Config.Debug.Enabled);
        SystemDebugAnim(Config.Debug.AnimationSpeed);
        SystemHttpProxy(Config.Network.HttpProxy.CustomAddress);
        SystemHttpProxyType(Config.Network.HttpProxy.Type);
        SystemHttpProxyCustomUsername(Config.Network.HttpProxy.CustomUsername);
        SystemHttpProxyCustomPassword(Config.Network.HttpProxy.CustomPassword);
    }

    #region Launch

    // 切换选择
    public static void LaunchInstanceSelect(string value)
    {
        ModBase.Log("[Setup] 当前选择的 Minecraft 版本：" + value);
        ModBase.WriteIni(ModFolder.mcFolderSelected + "PCL.ini", "Version", value);
    }

    public static void LaunchFolderSelect(string value)
    {
        ModBase.Log("[Setup] 当前选择的 Minecraft 文件夹：" + value.Replace("$", ModBase.exePath));
        ModFolder.mcFolderSelected = value.Replace("$", ModBase.exePath);
    }

    // 游戏内存
    public static void LaunchRamType(int type)
    {
        if (ModMain.frmSetupLaunch is null)
            return;
        ModMain.frmSetupLaunch.RamType(type);
    }

    #endregion

    #region Tool

    public static void ToolDownloadThread(int value)
    {
        ModNet.NetTaskThreadLimit = value + 1;
    }

    public static void ToolDownloadSpeed(int value)
    {
        ModNet.NetTaskSpeedLimitHigh = value switch
        {
            <= 14 => (long)Math.Round((value + 1) * 0.1d * 1024d * 1024d),
            <= 31 => (long)Math.Round((value - 11) * 0.5d * 1024d * 1024d),
            <= 41 => (value - 21) * 1024 * 1024L,
            _ => -1
        };
    }

    #endregion

    #region UI

    // 启动器
    public static void UiLauncherTransparent(int value)
    {
        ModMain.frmMain.Opacity = value / 1000d + 0.4d;
    }

    public static void UiLauncherTheme(int value)
    {
        ThemeManager.ThemeRefresh(value);
    }

    public static void UiBackgroundColorful(bool value)
    {
        ThemeManager.ThemeRefresh();
    }

    public static void UiLockWindowSize(bool value)
    {
        if (value)
            ModMain.frmMain.RemoveResizer();
        else
            ModMain.frmMain.AddResizer();
    }

    // 视频背景
    public static void UiAutoPauseVideo(bool value)
    {
        if (!value)
        {
            ModVideoBack.ForcePlay = true;
            ModVideoBack.VideoPlay();
        }
        else
        {
            ModVideoBack.ForcePlay = false;
            if (ModVideoBack.IsGaming)
                ModVideoBack.VideoPause();
        }
    }

    // 背景图片
    public static void UiBackgroundOpacity(int value)
    {
        ModMain.frmMain.ImgBack.Opacity = value / 1000d;
    }

    public static void UiBackgroundBlur(int value)
    {
        ModMain.frmMain.ImgBack.Effect = value == 0 ? null : new BlurEffect { Radius = value + 1 };
        ModMain.frmMain.ImgBack.Margin = new Thickness(-(value + 1) / 1.8d);
    }

    public static void UiBackgroundSuit(int value)
    {
        if (ModMain.frmMain.ImgBack.Background is null)
            return;
        var width = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Width;
        var height = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Height;
        if (value == 0)
        {
            // 智能：当图片较小时平铺，较大时适应
            if (width < ModMain.frmMain.PanMain.ActualWidth / 2d && height < ModMain.frmMain.PanMain.ActualHeight / 2d)
                value = 4; // 平铺
            else
                value = 2; // 适应
        }

        ((ImageBrush)ModMain.frmMain.ImgBack.Background).TileMode = TileMode.None;
        ((ImageBrush)ModMain.frmMain.ImgBack.Background).Viewport = new Rect(0d, 0d, 1d, 1d);
        ((ImageBrush)ModMain.frmMain.ImgBack.Background).ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
        switch (value)
        {
            case 1: // 居中
            {
                ModMain.frmMain.ImgBack.HorizontalAlignment = HorizontalAlignment.Center;
                ModMain.frmMain.ImgBack.VerticalAlignment = VerticalAlignment.Center;
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).Stretch = Stretch.None;
                ModMain.frmMain.ImgBack.Width = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Width;
                ModMain.frmMain.ImgBack.Height = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Height;
                break;
            }
            case 2: // 适应
            {
                ModMain.frmMain.ImgBack.HorizontalAlignment = HorizontalAlignment.Stretch;
                ModMain.frmMain.ImgBack.VerticalAlignment = VerticalAlignment.Stretch;
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).Stretch = Stretch.UniformToFill;
                ModMain.frmMain.ImgBack.Width = double.NaN;
                ModMain.frmMain.ImgBack.Height = double.NaN;
                break;
            }
            case 3: // 拉伸
            {
                ModMain.frmMain.ImgBack.HorizontalAlignment = HorizontalAlignment.Stretch;
                ModMain.frmMain.ImgBack.VerticalAlignment = VerticalAlignment.Stretch;
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).Stretch = Stretch.Fill;
                ModMain.frmMain.ImgBack.Width = double.NaN;
                ModMain.frmMain.ImgBack.Height = double.NaN;
                break;
            }
            case 4: // 平铺
            {
                ModMain.frmMain.ImgBack.HorizontalAlignment = HorizontalAlignment.Stretch;
                ModMain.frmMain.ImgBack.VerticalAlignment = VerticalAlignment.Stretch;
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).Stretch = Stretch.None;
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).TileMode = TileMode.Tile;
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).Viewport = new Rect(0d, 0d,
                    ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Width,
                    ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Height);
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).ViewportUnits = BrushMappingMode.Absolute;
                ModMain.frmMain.ImgBack.Width = double.NaN;
                ModMain.frmMain.ImgBack.Height = double.NaN;
                break;
            }
            case 5: // 左上
            {
                ModMain.frmMain.ImgBack.HorizontalAlignment = HorizontalAlignment.Left;
                ModMain.frmMain.ImgBack.VerticalAlignment = VerticalAlignment.Top;
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).Stretch = Stretch.None;
                ModMain.frmMain.ImgBack.Width = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Width;
                ModMain.frmMain.ImgBack.Height = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Height;
                break;
            }
            case 6: // 右上
            {
                ModMain.frmMain.ImgBack.HorizontalAlignment = HorizontalAlignment.Right;
                ModMain.frmMain.ImgBack.VerticalAlignment = VerticalAlignment.Top;
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).Stretch = Stretch.None;
                ModMain.frmMain.ImgBack.Width = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Width;
                ModMain.frmMain.ImgBack.Height = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Height;
                break;
            }
            case 7: // 左下
            {
                ModMain.frmMain.ImgBack.HorizontalAlignment = HorizontalAlignment.Left;
                ModMain.frmMain.ImgBack.VerticalAlignment = VerticalAlignment.Bottom;
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).Stretch = Stretch.None;
                ModMain.frmMain.ImgBack.Width = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Width;
                ModMain.frmMain.ImgBack.Height = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Height;
                break;
            }
            case 8: // 右下
            {
                ModMain.frmMain.ImgBack.HorizontalAlignment = HorizontalAlignment.Right;
                ModMain.frmMain.ImgBack.VerticalAlignment = VerticalAlignment.Bottom;
                ((ImageBrush)ModMain.frmMain.ImgBack.Background).Stretch = Stretch.None;
                ModMain.frmMain.ImgBack.Width = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Width;
                ModMain.frmMain.ImgBack.Height = ((ImageBrush)ModMain.frmMain.ImgBack.Background).ImageSource.Height;
                break;
            }
        }
    }

    // 字体
    public static void UiFont(string value)
    {
        try
        {
            ModBase.SetLaunchFont(value);
        }
        catch (Exception ex)
        {
            ModBase.Log(
                ex,
                "字体加载失败",
                ModBase.LogLevel.Hint,
                userSummary: Lang.Text("Setup.Error.OperationFailed"));
        }
    }

    // 主页
    public static void UiCustomType(int value)
    {
        if (ModMain.frmSetupUI is null)
            return;
        switch (value)
        {
            case 0: // 无
            {
                ModMain.frmSetupUI.PanCustomPreset.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.PanCustomLocal.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.PanCustomNet.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.HintCustom.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.HintCustomWarn.Visibility = Visibility.Collapsed;
                break;
            }
            case 1: // 本地
            {
                ModMain.frmSetupUI.PanCustomPreset.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.PanCustomLocal.Visibility = Visibility.Visible;
                ModMain.frmSetupUI.PanCustomNet.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.HintCustom.Visibility = Visibility.Visible;
                ModMain.frmSetupUI.HintCustomWarn.Visibility =
                    States.Hint.UntrustedHomepage ? Visibility.Collapsed : Visibility.Visible;
                ModMain.frmSetupUI.HintCustom.Text =
                    Lang.Text("Setup.Ui.Homepage.LocalFile.Hint");
                CustomEventService.SetEventType(ModMain.frmSetupUI.HintCustom, EventType.None);
                break;
            }
            case 2: // 联网
            {
                ModMain.frmSetupUI.PanCustomPreset.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.PanCustomLocal.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.PanCustomNet.Visibility = Visibility.Visible;
                ModMain.frmSetupUI.HintCustom.Visibility = Visibility.Visible;
                ModMain.frmSetupUI.HintCustomWarn.Visibility =
                    States.Hint.UntrustedHomepage ? Visibility.Collapsed : Visibility.Visible;
                ModMain.frmSetupUI.HintCustom.Text =
                    Lang.Text("Setup.Ui.Homepage.NetUpdate.Hint");
                CustomEventService.SetEventType(ModMain.frmSetupUI.HintCustom, EventType.OpenUrl);
                CustomEventService.SetEventData(ModMain.frmSetupUI.HintCustom,
                    "https://github.com/N1NA-MMK/Momoka-Craft-Launcher/discussions");
                break;
            }
            case 3: // 预设
            {
                ModMain.frmSetupUI.PanCustomPreset.Visibility = Visibility.Visible;
                ModMain.frmSetupUI.PanCustomLocal.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.PanCustomNet.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.HintCustom.Visibility = Visibility.Collapsed;
                ModMain.frmSetupUI.HintCustomWarn.Visibility = Visibility.Collapsed;
                break;
            }
        }

        ModMain.frmSetupUI.CardCustom.TriggerForceResize();
    }

    // 高级材质
    public static void UiBlur(bool value)
    {
        if (ModMain.frmSetupUI is null)
            return;

        ModMain.frmSetupUI.PanBlurValue.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        ModMain.frmSetupUI.PanGlassAdvanced.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        UiBlurValue(value ? Config.Preference.Blur.Radius : 0);
    }

    public static void UiBlurValue(int value)
    {
        System.Windows.Application.Current.Resources["BlurRadius"] = value * 1.0d;
    }

    public static void UiBlurSamplingRate(int value)
    {
        System.Windows.Application.Current.Resources["BlurSamplingRate"] = value * 0.01d;
    }

    public static void UiBlurType(int value)
    {
        System.Windows.Application.Current.Resources["BlurType"] = (KernelType)value;
    }

    // 液态玻璃材质
    public static void UiGlassTint(int value)
    {
        var opacity = Math.Clamp(value, 0, 100) / 100.0;
        var res = System.Windows.Application.Current.Resources;
        var isDark = ThemeService.IsDarkMode;
        // 基色：深色模式白色，浅色模式蓝灰色调
        var (br, bg, bb) = isDark ? ((byte)0xFF, (byte)0xFF, (byte)0xFF) : ((byte)0xE8, (byte)0xED, (byte)0xF5);
        var (ar, ag, ab) = isDark ? ((byte)0xF2, (byte)0xF6, (byte)0xFB) : ((byte)0xDD, (byte)0xE3, (byte)0xF0);
        var (dr, dg, db) = isDark ? ((byte)0xFB, (byte)0xFB, (byte)0xFB) : ((byte)0xE8, (byte)0xED, (byte)0xF5);
        res["LiquidGlassBackground"] = new SolidColorBrush(Color.FromArgb((byte)(opacity * 255), br, bg, bb));
        res["LiquidGlassBackgroundAlt"] = new SolidColorBrush(Color.FromArgb((byte)(opacity * 0.84 * 255), ar, ag, ab));
        res["LiquidGlassBackgroundDeep"] = new SolidColorBrush(Color.FromArgb((byte)(opacity * 1.08 * 255), dr, dg, db));
    }

    public static void UiGlassShadowRadius(int value)
    {
        System.Windows.Application.Current.Resources["LiquidGlassShadowRadius"] = (double)Math.Clamp(value, 0, 40);
    }

    public static void UiGlassShadowOpacity(int value)
    {
        System.Windows.Application.Current.Resources["LiquidGlassShadowOpacity"] = Math.Clamp(value, 0, 100) / 100.0;
    }

    public static void UiGlassSpecular(int value)
    {
        var res = System.Windows.Application.Current.Resources;
        var opacity = Math.Clamp(value, 0, 100) / 100.0;
        var isDark = ThemeService.IsDarkMode;
        // 镜面高光基色：深色模式白色，浅色模式浅蓝白
        var (cr, cg, cb) = isDark ? ((byte)0xFF, (byte)0xFF, (byte)0xFF) : ((byte)0xF0, (byte)0xF4, (byte)0xFA);
        var stops = new (double Offset, double Alpha)[]
        {
            (0.0, 0x66), (0.15, 0x22), (0.35, 0x00), (0.65, 0x00), (0.85, 0x10), (1.0, 0x33)
        };
        var newBrush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
        foreach (var (offset, alpha) in stops)
            newBrush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(alpha * opacity), cr, cg, cb), offset));
        res["LiquidGlassSpecularBrush"] = newBrush;
    }

    public static void UiGlassEdge(int value)
    {
        var res = System.Windows.Application.Current.Resources;
        var opacity = Math.Clamp(value, 0, 100) / 100.0;
        var isDark = ThemeService.IsDarkMode;
        // 边缘高光基色：深色模式白色，浅色模式深蓝灰色
        var (er, eg, eb) = isDark ? ((byte)0xFF, (byte)0xFF, (byte)0xFF) : ((byte)0x66, (byte)0x6C, (byte)0x7A);
        var (sr, sg, sb) = isDark ? ((byte)0xFF, (byte)0xFF, (byte)0xFF) : ((byte)0x50, (byte)0x5A, (byte)0x6E);
        res["LiquidGlassEdgeHighlight"] = new SolidColorBrush(Color.FromArgb((byte)(0x88 * opacity), er, eg, eb));
        res["LiquidGlassEdgeHighlightStrong"] = new SolidColorBrush(Color.FromArgb((byte)(0xCC * opacity), sr, sg, sb));
    }

    // 顶部栏
    public static void UiLogoType(int value)
    {
        if (ThemeService.CurrentTheme == ColorTheme.HmclBlue) value = 4;
        switch (value)
        {
            case 0: // 无
            {
                ModMain.frmMain.ShapeTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.BtnTitleHelp.Visibility = Visibility.Collapsed;
                ModMain.frmMain.ShapeHMCLTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.LabTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.ImageTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.CELogo.Visibility = Visibility.Collapsed;
                if (ModMain.frmSetupUI is not null)
                {
                    ModMain.frmSetupUI.CheckLogoLeft.Visibility = Visibility.Visible;
                    ModMain.frmSetupUI.PanLogoText.Visibility = Visibility.Collapsed;
                    ModMain.frmSetupUI.PanLogoChange.Visibility = Visibility.Collapsed;
                }

                break;
            }
            case 1: // 默认
            {
                ModMain.frmMain.ShapeTitleLogo.Visibility = Visibility.Visible;
                ModMain.frmMain.BtnTitleHelp.Visibility = Visibility.Collapsed;
                ModMain.frmMain.ShapeHMCLTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.LabTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.ImageTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.CELogo.Visibility = Visibility.Visible;
                if (ModMain.frmSetupUI is not null)
                {
                    ModMain.frmSetupUI.CheckLogoLeft.Visibility = Visibility.Collapsed;
                    ModMain.frmSetupUI.PanLogoText.Visibility = Visibility.Collapsed;
                    ModMain.frmSetupUI.PanLogoChange.Visibility = Visibility.Collapsed;
                }

                break;
            }
            case 2: // 文本
            {
                ModMain.frmMain.ShapeTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.BtnTitleHelp.Visibility = Visibility.Collapsed;
                ModMain.frmMain.ShapeHMCLTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.LabTitleLogo.Visibility = Visibility.Visible;
                ModMain.frmMain.ImageTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.CELogo.Visibility = Visibility.Visible;
                if (ModMain.frmSetupUI is not null)
                {
                    ModMain.frmSetupUI.CheckLogoLeft.Visibility = Visibility.Collapsed;
                    ModMain.frmSetupUI.PanLogoText.Visibility = Visibility.Visible;
                    ModMain.frmSetupUI.PanLogoChange.Visibility = Visibility.Collapsed;
                }

                _ = Config.Preference.WindowTitleCustomText;
                break;
            }
            case 3: // 图片
            {
                ModMain.frmMain.ShapeTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.BtnTitleHelp.Visibility = Visibility.Collapsed;
                ModMain.frmMain.ShapeHMCLTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.LabTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.ImageTitleLogo.Visibility = Visibility.Visible;
                ModMain.frmMain.CELogo.Visibility = Visibility.Visible;
                if (ModMain.frmSetupUI is not null)
                {
                    ModMain.frmSetupUI.CheckLogoLeft.Visibility = Visibility.Collapsed;
                    ModMain.frmSetupUI.PanLogoText.Visibility = Visibility.Collapsed;
                    ModMain.frmSetupUI.PanLogoChange.Visibility = Visibility.Visible;
                }

                try
                {
                    ModMain.frmMain.ImageTitleLogo.Source = ModBase.exePath + @"PCL\Logo.png";
                }
                catch (Exception ex)
                {
                    ModMain.frmMain.ImageTitleLogo.Source = null;
                    ModBase.Log(
                        ex,
                        "显示标题栏图片失败",
                        ModBase.LogLevel.Msgbox,
                        userSummary: Lang.Text("Setup.Error.OperationFailed"));
                }

                break;
            }
            case 4: //HMCL (愚人节)
                ModMain.frmMain.ShapeTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.ShapeHMCLTitleLogo.Visibility = Visibility.Visible;
                ModMain.frmMain.LabTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.ImageTitleLogo.Visibility = Visibility.Collapsed;
                ModMain.frmMain.BtnTitleHelp.Visibility = Visibility.Visible;
                if (ModMain.frmSetupUI is not null) 
                {
                    ModMain.frmSetupUI.CheckLogoLeft.Visibility = Visibility.Collapsed;
                    ModMain.frmSetupUI.PanLogoText.Visibility = Visibility.Collapsed;
                    ModMain.frmSetupUI.PanLogoChange.Visibility = Visibility.Collapsed;
                }

                break;
        }

        _ = Config.Preference.TopBarLeftAlign;
        if (ModMain.frmSetupUI is not null)
            ModMain.frmSetupUI.CardLogo.TriggerForceResize();
    }

    public static void UiLogoText(string value)
    {
        ModMain.frmMain.LabTitleLogo.Text = value;
    }

    public static void UiLogoLeft(bool value)
    {
        // 重构后：logo 位于固定宽度的左侧主导航栏，不再需要根据标题栏剩余空间动态调整列宽。
        // 保留方法以兼容 ApplyAll 调用，安全跳过不存在的 ColumnDefinitions。
        var cols = ModMain.frmMain.PanTitleMain.ColumnDefinitions;
        if (cols.Count == 0)
            return;
        cols[0].Width = new GridLength(
            value && Config.Preference.WindowTitleType == LauncherTitleType.None ? 0 : 1,
            GridUnitType.Star);
    }

    #endregion

    #region System

    // 调试选项
    public static void SystemDebugMode(bool value)
    {
        ModBase.modeDebug = value;
    }

    public static void SystemDebugAnim(int value)
    {
        ModAnimation.aniSpeed = value >= 30
            ? 200d
            : ModBase.MathClamp(value * 0.1d + 0.1d, 0.1d, 3d);
    }

    public static void SystemHttpProxy(string value)
    {
        if (value.IsNullOrWhiteSpace()) return;
        try
        {
            HttpProxyManager.Instance.CustomProxyAddress = new Uri(value);
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "HTTP 代理应用出错");
        }
    }

    public static void SystemHttpProxyType(int value)
    {
        var mode = (HttpProxyManager.ProxyMode)value;
        HttpProxyManager.Instance.Mode = Enum.IsDefined(mode)
            ? mode
            : HttpProxyManager.Instance.Mode;
    }

    public static void SystemHttpProxyCustomUsername(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var password = Config.Network.HttpProxy.CustomPassword;
            HttpProxyManager.Instance.Credentials = new NetworkCredential(value, password);
        }
        else
        {
            HttpProxyManager.Instance.Credentials = null;
        }
    }

    public static void SystemHttpProxyCustomPassword(string value)
    {
        var username = Config.Network.HttpProxy.CustomUsername;
        HttpProxyManager.Instance.Credentials = !string.IsNullOrEmpty(username)
            ? new NetworkCredential(username, value)
            : null;
    }

    #endregion

    #region Version

    // 游戏内存
    public static void VersionRamType(int type)
    {
        if (ModMain.frmInstanceSetup is null)
            return;
        ModMain.frmInstanceSetup.RamType(type);
    }

    // 服务器
    public static void VersionServerLogin(int type)
    {
        if (ModMain.frmInstanceSetup is null)
            return;
        // 为第三方登录清空缓存以更新描述
        ModBase.WriteIni(ModFolder.mcFolderSelected + "PCL.ini", "InstanceCache", "");
        if (PageInstanceLeft.McInstance is null)
            return;
        PageInstanceLeft.McInstance = new McInstance(PageInstanceLeft.McInstance.Name).Load();
        ModLoader.LoaderFolderRun(ModInstanceList.mcInstanceListLoader, ModFolder.mcFolderSelected,
            ModLoader.LoaderFolderRunType.ForceRun, 1, @"versions\");
    }

    #endregion
}
