using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PCL.Core.UI.Controls;

namespace PCL;

public class MyCard : AnimatedBackgroundGrid
{
    // 动画
    private const double dropShadowIdleOpacity = 0.10d;
    private const double dropShadowHoverOpacity = 0.32d;
    // 悬停 lift：Apple 卡片悬停的轻微上浮 + 阴影扩散
    private const double hoverLiftY = -2.5d;
    private const double shadowIdleRadiusFactor = 1.0d;
    private const double shadowHoverRadiusFactor = 1.45d;
    private readonly TranslateTransform liftTransform;

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(MyCard), new PropertyMetadata(""));

    private readonly BlurBorder mainBorder;

    // 控件
    private readonly Grid mainGrid;
    private readonly Border specularLayer;
    private readonly Border edgeLayer;
    private TextBlock _MainTextBlock;
    private bool isLoad;

    // UI 建立
    public MyCard() : base(BlurBorder.BackgroundProperty)
    {
        var corner = ReadShapeDouble("ShapeCornerRadiusCard", 10d);
        var shadowR = ReadShapeDouble("ShapeShadowRadius", 12d);
        var shadowOn = shadowR > 0;
        MainChrome = new MyDropShadow
        {
            Margin = new Thickness(-6, -6, -6, -8), ShadowRadius = shadowR,
            Opacity = shadowOn ? dropShadowIdleOpacity : 0d, CornerRadius = new CornerRadius(corner)
        };
        MainChrome.SetResourceReference(MyDropShadow.ColorProperty, "ColorObject1");
        Children.Insert(0, MainChrome);
        mainBorder = new BlurBorder { CornerRadius = new CornerRadius(corner), IsHitTestVisible = false };
        Children.Insert(1, mainBorder);
        mainGrid = new Grid();
        Children.Add(mainGrid);
        // 液态玻璃层：边缘高光 + 镜面高光（静止态低强度，悬停/按下动态增强，Init 末尾追加到 Children 顶层）
        specularLayer = new Border
        {
            CornerRadius = new CornerRadius(corner),
            IsHitTestVisible = false,
            Opacity = 0.0
        };
        specularLayer.SetResourceReference(Border.BackgroundProperty, "LiquidGlassSpecularBrush");
        edgeLayer = new Border
        {
            CornerRadius = new CornerRadius(corner),
            BorderThickness = new Thickness(1d),
            IsHitTestVisible = false,
            Opacity = 0.0
        };
        edgeLayer.SetResourceReference(Border.BorderBrushProperty, "LiquidGlassEdgeHighlightStrong");
        // 设置背景色
        SetResourceReference(BackgroundBrushProperty, "ColorBrushTransparentBackground");
        // 悬停 lift 变换：整卡轻微上浮（仅渲染，不影响布局）
        liftTransform = new TranslateTransform { Y = 0d };
        RenderTransform = liftTransform;
        RenderTransformOrigin = new Point(0.5d, 0.5d);
        Loaded += (_, _) => Init();
        MouseEnter += MyCard_MouseEnter;
        MouseLeave += MyCard_MouseLeave;
        SizeChanged += MySizeChanged;
        MouseLeftButtonDown += MyCard_MouseLeftButtonDown;
        MouseLeftButtonUp += MyCard_MouseLeftButtonUp;
        MouseLeave += MyCard_MouseLeave_Swap;
        // 订阅形态变更，动态更新圆角/阴影
        ShapeManager.ShapeChanged += ApplyShape;
        Unloaded += (_, _) => ShapeManager.ShapeChanged -= ApplyShape;
    }

    private static double ReadShapeDouble(string key, double fallback)
    {
        try
        {
            if (Application.Current?.Resources[key] is double d) return d;
        }
        catch { }
        return fallback;
    }

    private void ApplyShape()
    {
        var corner = ReadShapeDouble("ShapeCornerRadiusCard", 10d);
        var cr = new CornerRadius(corner);
        MainChrome.CornerRadius = cr;
        mainBorder.CornerRadius = cr;
        specularLayer.CornerRadius = cr;
        edgeLayer.CornerRadius = cr;
        var shadowR = ReadShapeDouble("ShapeShadowRadius", 12d);
        MainChrome.ShadowRadius = shadowR;
        MainChrome.Opacity = shadowR > 0 ? dropShadowIdleOpacity : 0d;
    }

    public MyDropShadow MainChrome { get; }

    public UIElement BorderChild
    {
        get => mainBorder.Child;
        set => mainBorder.Child = value;
    }

    public TextBlock MainTextBlock
    {
        get
        {
            Init(); // 当父级触发 Loaded 时，本卡片可能尚未触发 Loaded（该事件从父级向子级调用），因此这会是 null。手动触发以确保控件已加载。
            return _MainTextBlock;
        }
        set => _MainTextBlock = value;
    }

    public Path MainSwap
    {
        get
        {
            Init();
            return field;
        }
        set => field = value;
    }

    // 属性
    public InlineCollection Inlines => MainTextBlock.Inlines;

    public CornerRadius CornerRadius
    {
        get => MainChrome.CornerRadius;
        set
        {
            MainChrome.CornerRadius = value;
            mainBorder.CornerRadius = value;
            specularLayer.CornerRadius = value;
            edgeLayer.CornerRadius = value;
        }
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set
        {
            SetValue(TitleProperty, value);
            if (_MainTextBlock is not null)
                MainTextBlock.Text = value;
        }
    }

    protected override SolidColorBrush AnimatableBrush
    {
        get => (SolidColorBrush)mainBorder.Background;
        set => mainBorder.Background = value;
    }

    protected override FrameworkElement AnimatableElement => mainBorder;
    public bool HasMouseAnimation { get; set; } = true;

    private void Init()
    {
        if (isLoad)
            return;
        isLoad = true;
        // AddHandler ThemeChanged, AddressOf _BackgroundBrushChanged '已在依赖属性中实现
        // 初次加载限定
        if (MainTextBlock is null)
        {
            MainTextBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(15d, 12d, 0d, 0d), FontWeight = FontWeights.SemiBold,
                IsHitTestVisible = false
            };
            MainTextBlock.SetResourceReference(TextBlock.FontSizeProperty, "ShapeFontSizeTitle");
            MainTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrush1");
            MainTextBlock.SetBinding(TextBlock.TextProperty,
                new Binding("Title") { Source = this, Mode = BindingMode.OneWay });
            mainGrid.Children.Add(MainTextBlock);
        }

        if (CanSwap || SwapControl is not null)
        {
            if (SwapControl is null && Children.Count > 3)
                SwapControl = Children[3];
            MainSwap = new Path
            {
                HorizontalAlignment = HorizontalAlignment.Right, Stretch = Stretch.Uniform, Height = 6d, Width = 10d,
                VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0d, 17d, 16d, 0d),
                Data =
                    (Geometry)new GeometryConverter().ConvertFromString("M2,4 l-2,2 10,10 10,-10 -2,-2 -8,8 -8,-8 z"),
                RenderTransform = new RotateTransform(180d), RenderTransformOrigin = new Point(0.5d, 0.5d)
            };
            MainSwap.SetResourceReference(Shape.FillProperty, "ColorBrush1");
            mainGrid.Children.Add(MainSwap);
        }

        // 改变默认的折叠
        if (IsSwapped && SwapControl is not null)
        {
            MainSwap.RenderTransform = new RotateTransform(SwapLogoRight ? 270 : 0);
            SwapControl.Visibility = Visibility.Collapsed;
            // 取消由于高度变化被迫触发的高度动画
            var rawUseAnimation = UseAnimation;
            UseAnimation = false;
            Height = SwapedHeight;
            ModAnimation.AniStop("MyCard Height " + uuid);
            isHeightAnimating = false;
            ModBase.RunInUi(() => UseAnimation = rawUseAnimation, true);
        }

        // 追加液态玻璃高光层与边缘高光层到 Children 末尾（覆盖在所有内容之上）
        // 此时 SwapControl 已从 Children[3] 解析完毕，追加不会影响其索引
        if (!Children.Contains(specularLayer))
            Children.Add(specularLayer);
        if (!Children.Contains(edgeLayer))
            Children.Add(edgeLayer);
        // 初始静止态：边缘高光低强度显现，镜面高光关闭
        edgeLayer.Opacity = 0.25d;
        specularLayer.Opacity = 0.0d;
    }

    public void StackInstall()
    {
        var argstack = (StackPanel)SwapControl;
        StackInstall(ref argstack, InstallMethod);
        SwapControl = argstack;
        TriggerForceResize();
    }

    public static void StackInstall(ref StackPanel stack, Action<StackPanel> installMethod)
    {
        if (stack.Tag is null)
            return;
        try
        {
            installMethod(stack);
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "[MyCard] InstallMethod 调用失败");
        }

        stack.Children.Add(new FrameworkElement { Height = 18d }); // 下边距，同时适应折叠
        stack.Tag = null;
    }

    private void MyCard_MouseEnter(object sender, MouseEventArgs e)
    {
        if (!HasMouseAnimation)
            return;
        var baseShadowR = ReadShapeDouble("ShapeShadowRadius", 12d);
        var hoverShadowR = baseShadowR * shadowHoverRadiusFactor;
        var appleEase = new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.EaseOut);
        var appleStandard = new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.Standard);
        var aniList = new List<ModAnimation.AniData>();
        if (MainTextBlock is not null)
            aniList.Add(ModAnimation.AaColor(MainTextBlock, TextBlock.ForegroundProperty, "ColorBrush2", 180, ease: appleEase));
        if (MainSwap is not null)
            aniList.Add(ModAnimation.AaColor(MainSwap, Shape.FillProperty, "ColorBrush2", 180, ease: appleEase));
        aniList.AddRange(new[]
        {
            ModAnimation.AaColor(MainChrome, MyDropShadow.ColorProperty, "ColorObject4", 180, ease: appleEase),
            ModAnimation.AaOpacity(MainChrome, dropShadowHoverOpacity - MainChrome.Opacity, 220, ease: appleStandard),
            // 阴影扩散：blur radius 增大，模拟物体上浮后投影变散（Apple 悬浮感的核心）
            ModAnimation.AaDouble(MainChrome, MyDropShadow.ShadowRadiusProperty, hoverShadowR - MainChrome.ShadowRadius, 260, ease: appleStandard),
            // 整卡轻微上浮（Apple 卡片悬停的标志性微动）
            ModAnimation.AaDouble(liftTransform, TranslateTransform.YProperty, hoverLiftY - liftTransform.Y, 260, ease: new ModAnimation.AniEaseAppleSpring(1.0d, 0.4d)),
            // 高光层变亮，模拟光线聚焦
            ModAnimation.AaOpacity(specularLayer, 1.0d - specularLayer.Opacity, 240, ease: appleStandard),
            // 边缘高光增强
            ModAnimation.AaOpacity(edgeLayer, 1.0d - edgeLayer.Opacity, 240, ease: appleStandard)
        });
        if (!IsAnimating)
            ModAnimation.AniStart(aniList, "MyCard Mouse " + uuid);
    }

    private void MyCard_MouseLeave(object sender, MouseEventArgs e)
    {
        if (!HasMouseAnimation)
            return;
        var baseShadowR = ReadShapeDouble("ShapeShadowRadius", 12d);
        var idleShadowR = baseShadowR * shadowIdleRadiusFactor;
        var appleEase = new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.EaseOut);
        var aniList = new List<ModAnimation.AniData>();
        if (MainTextBlock is not null)
            aniList.Add(ModAnimation.AaColor(MainTextBlock, TextBlock.ForegroundProperty, "ColorBrush1", 200, ease: appleEase));
        if (MainSwap is not null)
            aniList.Add(ModAnimation.AaColor(MainSwap, Shape.FillProperty, "ColorBrush1", 200, ease: appleEase));
        aniList.AddRange(new[]
        {
            ModAnimation.AaColor(MainChrome, MyDropShadow.ColorProperty, "ColorObject1", 200, ease: appleEase),
            ModAnimation.AaOpacity(MainChrome, dropShadowIdleOpacity - MainChrome.Opacity, 300, ease: appleEase),
            // 阴影收缩回静止
            ModAnimation.AaDouble(MainChrome, MyDropShadow.ShadowRadiusProperty, idleShadowR - MainChrome.ShadowRadius, 320, ease: appleEase),
            // 上浮回落
            ModAnimation.AaDouble(liftTransform, TranslateTransform.YProperty, 0d - liftTransform.Y, 360, ease: new ModAnimation.AniEaseAppleSpring(1.0d, 0.42d)),
            // 高光层回落
            ModAnimation.AaOpacity(specularLayer, 0.0d - specularLayer.Opacity, 360, ease: appleEase),
            // 边缘高光回落
            ModAnimation.AaOpacity(edgeLayer, 0.25d - edgeLayer.Opacity, 360, ease: appleEase)
        });
        if (!IsAnimating)
            ModAnimation.AniStart(aniList, "MyCard Mouse " + uuid);
    }

    #region 高度改变动画

    /// <summary>
    ///     是否启用高度改变动画。
    /// </summary>
    public bool UseAnimation { get; set; } = true;

    private bool isHeightAnimating;
    private double actualUsedHeight; // 回滚实际高度（例如 NaN）

    private void MySizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!UseAnimation)
            return;
        var deltaHeight = (IsSwapped ? SwapedHeight : e.NewSize.Height) - e.PreviousSize.Height;
        // 卡片的进入时动画已被页面通用切换动画替代
        if (e.PreviousSize.Height == 0d || isHeightAnimating || Math.Abs(deltaHeight) < 1d || ActualHeight == 0d)
            return;
        StartHeightAnimation(deltaHeight, e.PreviousSize.Height, false);
    }

    /// <summary>
    ///     启动卡片高度变化的动画效果
    ///     根据变化距离的大小采用不同的动画策略：短距离使用简单缓动，长距离使用分段动画
    /// </summary>
    /// <param name="delta">高度变化量</param>
    /// <param name="previousHeight">之前的高度</param>
    /// <param name="isLoadAnimation">是否为加载动画</param>
    private void StartHeightAnimation(double delta, double previousHeight, bool isLoadAnimation)
    {
        if (isHeightAnimating || ModMain.frmMain is null)
            return; // 避免 XAML 设计器出错

        var animList = new List<ModAnimation.AniData>();
        var absDelta = Math.Abs(delta);

        if (absDelta <= 800d)
        {
            // 短距离，直接使用 150ms 的缓动动画
            animList.Add(ModAnimation.AaHeight(this, delta, 150,
                ease: new ModAnimation.AniEaseOutFluent(ModAnimation.AniEasePower.ExtraStrong)));
        }
        else
        {
            var easeLength = default(int);
            int easeTime;
            int initSpeed; // 到达缓动区前的初速度
            if (delta < 0d && absDelta - easeLength > 5000d * 0.1d)
            {
                // 收回距离过长 (>0.1s)，强制以 100ms 完成匀速段，然后让减速段更长
                easeLength = 200;
                easeTime = 150;
                initSpeed = (int)Math.Round((absDelta - easeLength) / 0.1d);
            }
            else if (delta > 0d && absDelta - easeLength > 5000d * 0.6d)
            {
                // 展开距离过长 (>0.6s)，以 5000 速度展示 300ms 匀速段，剩下的距离全部归入减速段
                initSpeed = 5000;
                easeLength = (int)Math.Round(absDelta - initSpeed * 0.3d);
                easeTime = 400;
            }
            else
            {
                // 中程，匀速地快速展开（或收回）
                easeLength = 150;
                easeTime = 200;
                initSpeed = 4000;
            }

            // 匀速段
            animList.Add(ModAnimation.AaHeight(this, (absDelta - easeLength) * Math.Sign(delta),
                (int)Math.Round((absDelta - easeLength) / initSpeed * 1000d)));
            // 减速段
            animList.Add(ModAnimation.AaHeight(this, easeLength * Math.Sign(delta), easeTime,
                ease: new ModAnimation.AniEaseOutFluentWithInitial(initSpeed, easeTime / 1000d, easeLength),
                after: true));
        }

        animList.Add(ModAnimation.AaCode(() =>
        {
            isHeightAnimating = false;
            Height = actualUsedHeight;
            if (IsSwapped && SwapControl is not null)
                SwapControl.Visibility = Visibility.Collapsed;
        }, after: true));
        ModAnimation.AniStart(animList, "MyCard Height " + uuid);
        isHeightAnimating = true;
        actualUsedHeight = IsSwapped ? SwapedHeight : Height;
        Height = previousHeight;
    }

    /// <summary>
    ///     通知 MyCard，控件内容已改变，需要中断动画并瞬间更新高度。
    /// </summary>
    public void TriggerForceResize()
    {
        Height = IsSwapped ? SwapedHeight : double.NaN;
        ModAnimation.AniStop("MyCard Height " + uuid);
        isHeightAnimating = false;
    }

    #endregion

    #region 折叠

    // 若设置了 CanSwap，或 SwapControl 不为空，则判定为会进行折叠
    // 这是因为不能直接在 XAML 中设置 SwapControl
    public UIElement SwapControl;
    public bool CanSwap { get; set; } = false;

    /// <summary>
    ///     数据转为列表项的转换方法
    /// </summary>
    /// <returns></returns>
    public Action<StackPanel> InstallMethod { get; set; }

    /// <summary>
    ///     是否已被折叠。
    /// </summary>
    public bool IsSwapped
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field = value;
            if (SwapControl is null)
                return;

            // 当卡片展开时，如果SwapControl是StackPanel类型，则执行安装方法
            // 这通常用于动态添加内容到折叠卡片中
            if (!IsSwapped && SwapControl is StackPanel)
            {
                var argstack = (StackPanel)SwapControl;
                StackInstall(ref argstack, InstallMethod);
                SwapControl = argstack;
            }

            // 若尚未加载，会在 Loaded 事件中触发无动画的折叠，不需要在这里进行
            if (!IsLoaded)
                return;

            // 更新控件的可见性和高度
            SwapControl.Visibility = Visibility.Visible;
            TriggerForceResize();

            // 根据折叠状态旋转箭头图标
            // 折叠时箭头指向右侧或向上（根据SwapLogoRight设置），展开时指向下方
            ModAnimation.AniStart(
                ModAnimation.AaRotateTransform(MainSwap,
                    (field ? SwapLogoRight ? 270 : 0 : 180) - ((RotateTransform)MainSwap.RenderTransform).Angle,
                    250, ease: new ModAnimation.AniEaseOutFluent(ModAnimation.AniEasePower.ExtraStrong)),
                "MyCard Swap " + uuid, true);
        }
    }

    /// <summary>
    ///     是否已被折叠。(已过时，请使用 IsSwapped)
    /// </summary>
    [Obsolete("请使用 IsSwapped 属性，IsSwaped 存在拼写错误")]
    public bool IsSwaped
    {
        get => IsSwapped;
        set => IsSwapped = value;
    }

    public bool SwapLogoRight { get; set; } = false;
    private bool isSwapMouseDown = false; //用于触发卡片展开/折叠的 MouseDown
    private bool isCustomMouseDown = false; //用于触发自定义事件的 MouseDown
    public event PreviewSwapEventHandler? PreviewSwap;

    public delegate void PreviewSwapEventHandler(object sender, ModBase.RouteEventArgs e);

    public event SwapEventHandler? Swap;

    public delegate void SwapEventHandler(object sender, ModBase.RouteEventArgs e);

    public const int SwapedHeight = 40;

    private void MyCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        double pos = Mouse.GetPosition(this).Y;
        if (!IsSwapped && (pos > (IsSwapped ? SwapedHeight : SwapedHeight - 6) || (pos == 0 && !IsMouseDirectlyOver)))
            return;
        isCustomMouseDown = true;
        if (!IsSwapped && (SwapControl is null || pos > (IsSwapped ? SwapedHeight : SwapedHeight - 6) || (pos == 0 && !IsMouseDirectlyOver)))
            return;
        isSwapMouseDown = true;
    }

    private void MyCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!isCustomMouseDown) return;
        isCustomMouseDown = false;
        ModMain.RaiseCustomEvent(this);

        if (!isSwapMouseDown) return;
        isSwapMouseDown = false;

        double pos = Mouse.GetPosition(this).Y;
        if (!IsSwapped && (SwapControl is null || pos > (IsSwapped ? SwapedHeight : SwapedHeight - 6) || (pos == 0 && !IsMouseDirectlyOver)))
            return; // 检测点击位置；或已经不在可视树上的误判

        var e2 = new ModBase.RouteEventArgs(true);
        PreviewSwap?.Invoke(this, e2);
        if (e2.handled)
        {
            isSwapMouseDown = false;
            return;
        }

        IsSwapped = !IsSwapped;
        ModBase.Log("[Control] " + (IsSwapped ? "折叠卡片" : "展开卡片") + (Title is null ? "" : "：" + Title));
        Swap?.Invoke(this, e2);
    }

    private void MyCard_MouseLeave_Swap(object sender, MouseEventArgs e)
    {
        isSwapMouseDown = false;
    }

    #endregion
}

public static partial class ModAnimation
{
    public static void AniDispose(MyCard control, bool removeFromChildren, ParameterizedThreadStart callBack = null)
    {
        if (control.IsHitTestVisible)
        {
            control.IsHitTestVisible = false;
            AniStart(new[]
            {
                AaScaleTransform(control, -0.08d, 200, ease: new AniEaseInFluent()),
                AaOpacity(control, -1, 200, ease: new AniEaseOutFluent()),
                AaHeight(control, -control.ActualHeight, 150, 100, new AniEaseOutFluent()),
                AaCode(() =>
                {
                    if (removeFromChildren)
                    {
                        if (control.Parent is null)
                            return;
                        ((Panel)control.Parent).Children.Remove(control);
                    }
                    else
                    {
                        control.Visibility = Visibility.Collapsed;
                    }

                    if (callBack is not null)
                        callBack(control);
                }, after: true)
            }, "MyCard Dispose " + control.uuid);
        }
        else
        {
            if (removeFromChildren)
            {
                if (control.Parent is null)
                    return;
                ((Panel)control.Parent).Children.Remove(control);
            }
            else
            {
                control.Visibility = Visibility.Collapsed;
            }

            if (callBack is not null)
                callBack(control);
        }
    }
}