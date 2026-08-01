using System;
using System.Windows;
using PCL.Core.App;
using PCL.Core.App.Configuration;
using PCL.Core.UI;

namespace PCL;

/// <summary>
/// UI 形态管理器：根据 <see cref="Config.Preference.Shape"/> 将圆角/密度/边框/阴影参数
/// 写入应用级资源字典，供所有控件以 DynamicResource 引用，实现全局形态切换。
/// </summary>
public static class ShapeManager
{
    private static bool _initialized;

    public static ResourceDictionary AppResources => Application.Current.Resources;

    /// <summary>
    /// 初始化：注册配置变更回调，应用当前形态。应在启动器主窗口创建后调用。
    /// </summary>
    public static void Init()
    {
        if (_initialized) return;
        _initialized = true;

        // 监听 Shape 配置组所有项的变更
        var shape = Config.Preference.Shape;
        ObserveItem(shape.CornerStyleConfig);
        ObserveItem(shape.DensityConfig);
        ObserveItem(shape.BorderStyleConfig);
        ObserveItem(shape.ShadowEnabledConfig);

        Apply();
    }

    private static void ObserveItem(ConfigItem item)
    {
        var observer = new ConfigObserver(ConfigEvent.Update, _ => Apply());
        item.Observe(observer);
    }

    /// <summary>
    /// 根据当前配置计算并写入所有形态资源。
    /// </summary>
    public static void Apply()
    {
        RunInUi(() =>
        {
            var shape = Config.Preference.Shape;
            var corner = shape.CornerStyle;
            var density = shape.Density;
            var border = shape.BorderStyle;
            var shadowOn = shape.ShadowEnabled;

            // ── 圆角 ──────────────────────────────────────
            double rButton, rCard, rInput, rHint, rSmall;
            switch (corner)
            {
                case CornerStyle.Sharp:
                    rButton = 0; rCard = 0; rInput = 0; rHint = 0; rSmall = 0;
                    break;
                case CornerStyle.Small:
                    rButton = 4; rCard = 6; rInput = 4; rHint = 6; rSmall = 2;
                    break;
                case CornerStyle.Large:
                    rButton = 10; rCard = 14; rInput = 10; rHint = 12; rSmall = 8;
                    break;
                case CornerStyle.Pill:
                    rButton = 100; rCard = 18; rInput = 100; rHint = 14; rSmall = 100;
                    break;
                case CornerStyle.Medium:
                default:
                    rButton = 6; rCard = 10; rInput = 6; rHint = 8; rSmall = 4;
                    break;
            }

            SetDouble("ShapeCornerRadiusButton", rButton);
            SetDouble("ShapeCornerRadiusCard", rCard);
            SetDouble("ShapeCornerRadiusInput", rInput);
            SetDouble("ShapeCornerRadiusHint", rHint);
            SetDouble("ShapeCornerRadiusSmall", rSmall);
            SetDouble("ShapeCornerRadiusPill", 100);

            SetCornerRadius("ShapeCornerButton", rButton);
            SetCornerRadius("ShapeCornerCard", rCard);
            SetCornerRadius("ShapeCornerInput", rInput);
            SetCornerRadius("ShapeCornerHint", rHint);
            SetCornerRadius("ShapeCornerSmall", rSmall);

            // ── 边框 ──────────────────────────────────────
            Thickness borderThick;
            switch (border)
            {
                case BorderStyle.None:
                    borderThick = new Thickness(0);
                    break;
                case BorderStyle.Thin:
                    borderThick = new Thickness(0.75);
                    break;
                case BorderStyle.Standard:
                default:
                    borderThick = new Thickness(1);
                    break;
            }
            AppResources["ShapeBorderThickness"] = borderThick;
            AppResources["ShapeBorderThicknessThin"] = new Thickness(borderThick.Left * 0.75);
            AppResources["ShapeBorderThicknessNone"] = new Thickness(0);

            // ── 阴影 ──────────────────────────────────────
            double shadowRadius, shadowOpacity, shadowDepth;
            if (shadowOn)
            {
                shadowRadius = 12;
                shadowOpacity = 0.10;
                shadowDepth = 4;
            }
            else
            {
                shadowRadius = 0;
                shadowOpacity = 0;
                shadowDepth = 0;
            }
            SetDouble("ShapeShadowRadius", shadowRadius);
            SetDouble("ShapeShadowOpacity", shadowOpacity);
            SetDouble("ShapeShadowDepth", shadowDepth);

            // ── 密度 ──────────────────────────────────────
            double padButtonH, padButtonV, controlHeight, controlHeightLarge, spacing;
            Thickness padCard, padInput;
            double fontSizeBody, fontSizeCaption;
            double fontSizeLargeTitle, fontSizeTitle, fontSizeHeader, fontSizeSubhead;
            double lineHeightBody, lineHeightTitle, lineHeightLargeTitle;
            Thickness pagePad, cardMargin, cardContentPad;
            switch (density)
            {
                case UiDensity.Compact:
                    padButtonH = 12; padButtonV = 5;
                    controlHeight = 28; controlHeightLarge = 36;
                    spacing = 3;
                    padCard = new Thickness(12, 8, 12, 12);
                    padInput = new Thickness(8, 4, 8, 4);
                    fontSizeBody = 12.5; fontSizeCaption = 11.5;
                    fontSizeLargeTitle = 23; fontSizeTitle = 16; fontSizeHeader = 14; fontSizeSubhead = 13;
                    lineHeightBody = 17; lineHeightTitle = 21; lineHeightLargeTitle = 27;
                    pagePad = new Thickness(18, 16, 18, 8);
                    cardMargin = new Thickness(0, 0, 0, 11);
                    cardContentPad = new Thickness(18, 34, 16, 12);
                    break;
                case UiDensity.Comfortable:
                    padButtonH = 20; padButtonV = 11;
                    controlHeight = 38; controlHeightLarge = 46;
                    spacing = 6;
                    padCard = new Thickness(20, 16, 20, 20);
                    padInput = new Thickness(12, 9, 12, 9);
                    fontSizeBody = 14; fontSizeCaption = 13;
                    fontSizeLargeTitle = 30; fontSizeTitle = 18; fontSizeHeader = 16; fontSizeSubhead = 15;
                    lineHeightBody = 21; lineHeightTitle = 24; lineHeightLargeTitle = 34;
                    pagePad = new Thickness(32, 30, 32, 14);
                    cardMargin = new Thickness(0, 0, 0, 20);
                    cardContentPad = new Thickness(30, 42, 28, 18);
                    break;
                case UiDensity.Standard:
                default:
                    padButtonH = 16; padButtonV = 8;
                    controlHeight = 32; controlHeightLarge = 40;
                    spacing = 4;
                    padCard = new Thickness(16, 12, 16, 16);
                    padInput = new Thickness(10, 6, 10, 6);
                    fontSizeBody = 13; fontSizeCaption = 12;
                    fontSizeLargeTitle = 26; fontSizeTitle = 17; fontSizeHeader = 15; fontSizeSubhead = 14;
                    lineHeightBody = 19; lineHeightTitle = 22; lineHeightLargeTitle = 30;
                    pagePad = new Thickness(25, 25, 25, 10);
                    cardMargin = new Thickness(0, 0, 0, 15);
                    cardContentPad = new Thickness(25, 38, 23, 15);
                    break;
            }
            SetDouble("ShapePaddingButtonH", padButtonH);
            SetDouble("ShapePaddingButtonV", padButtonV);
            AppResources["ShapePaddingCard"] = padCard;
            AppResources["ShapePaddingInput"] = padInput;
            SetDouble("ShapeFontSizeBody", fontSizeBody);
            SetDouble("ShapeFontSizeCaption", fontSizeCaption);
            SetDouble("ShapeFontSizeLargeTitle", fontSizeLargeTitle);
            SetDouble("ShapeFontSizeTitle", fontSizeTitle);
            SetDouble("ShapeFontSizeHeader", fontSizeHeader);
            SetDouble("ShapeFontSizeSubhead", fontSizeSubhead);
            SetDouble("ShapeLineHeightBody", lineHeightBody);
            SetDouble("ShapeLineHeightTitle", lineHeightTitle);
            SetDouble("ShapeLineHeightLargeTitle", lineHeightLargeTitle);
            SetDouble("ShapeSpacingUnit", spacing);
            SetDouble("ShapeControlHeight", controlHeight);
            SetDouble("ShapeControlHeightLarge", controlHeightLarge);
            AppResources["ShapePagePadding"] = pagePad;
            AppResources["ShapeCardMargin"] = cardMargin;
            AppResources["ShapeCardContentPadding"] = cardContentPad;

            // 通知已加载的控件刷新（通过无效化资源引用）
            ShapeChanged?.Invoke();
        });
    }

    /// <summary>
    /// 形态变更事件，控件可订阅以重读参数。
    /// </summary>
    public static event Action? ShapeChanged;

    private static void SetCornerRadius(string key, double uniform)
    {
        AppResources[key] = new CornerRadius(uniform);
    }

    private static void SetDouble(string key, double value)
    {
        AppResources[key] = value;
    }

    private static void RunInUi(Action action)
    {
        if (Application.Current?.Dispatcher?.CheckAccess() != false)
            action();
        else
            Application.Current.Dispatcher.Invoke(action);
    }
}
