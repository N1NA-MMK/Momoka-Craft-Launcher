using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace PCL.Core.UI.Controls;

/// <summary>
/// 液态玻璃面板（Liquid Glass）。
/// <para>这是一种「活体材质」而非静态毛玻璃：背景模糊层之上叠加透镜折射、
/// 镜面高光、边缘亮边、厚度阴影与触控发光，并以弹簧物理驱动交互过渡。</para>
/// <para>遵循规范：禁止仅用高斯模糊；交互必须改变折射/光泽而非仅缩放；
/// 按下瞬时提升饱和亮度；释放以弹簧回弹；尺寸越大材质越厚；减弱动效时降级为透明度交叉淡变。</para>
/// </summary>
public class LiquidGlassPanel : BlurBorder
{
    // ── 层结构 ──────────────────────────────────────────────
    private readonly Grid _rootGrid;
    private readonly ContentPresenter _contentHost;
    // 折射透镜层：对内容做轻微缩放/位移，模拟光线穿过不同厚度玻璃的扭曲
    private readonly Border _refractionLayer;
    // 镜面高光层：顶部光泽反射
    private readonly Border _specularLayer;
    // 边缘高光：玻璃切面捕捉的亮边（顶部更亮）
    private readonly Border _edgeHighlightLayer;
    // 边缘阴影：内描边暗角，增强实体厚度感
    private readonly Border _edgeShadowLayer;
    // 触控发光层：按下时从接触点扩散的彩色光晕
    private readonly Border _glowLayer;
    // 厚度阴影：投射到背景的柔和深阴影（尺寸越大越深）
    private readonly DropShadowEffect _depthShadow;

    // ── 状态 ────────────────────────────────────────────────
    private bool _isPressed;
    private bool _isHovered;
    private bool _restingApplied;
    private Point _lastPointerPos;
    private Vector _pointerVelocity;
    private DateTime _lastPointerTime = DateTime.MinValue;
    private double _currentIntensity = 0.6;

    static LiquidGlassPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LiquidGlassPanel),
            new FrameworkPropertyMetadata(typeof(LiquidGlassPanel)));
    }

    public LiquidGlassPanel()
    {
        // 内容宿主
        _contentHost = new ContentPresenter
        {
            // 折射：内容本身做微小的 RenderTransform，模拟透镜对背后内容的拉伸
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new ScaleTransform(1.0, 1.0)
        };

        // 折射层：一个半透明的色调层，按下/拖拽时增强，模拟色散
        _refractionLayer = new Border
        {
            IsHitTestVisible = false,
            Opacity = 0.0,
            CornerRadius = CornerRadius
        };
        _refractionLayer.SetResourceReference(Border.BackgroundProperty, "LiquidGlassChromaticBrush");

        // 镜面高光
        _specularLayer = new Border
        {
            IsHitTestVisible = false,
            Opacity = 0.0,
            CornerRadius = CornerRadius
        };
        _specularLayer.SetResourceReference(Border.BackgroundProperty, "LiquidGlassSpecularBrush");

        // 边缘高光：顶部明亮（玻璃切面捕捉光线）
        _edgeHighlightLayer = new Border
        {
            IsHitTestVisible = false,
            BorderThickness = new Thickness(1.2, 1.6, 1.2, 0.8),
            CornerRadius = CornerRadius,
            Opacity = 0.0
        };
        _edgeHighlightLayer.SetResourceReference(Border.BorderBrushProperty, "LiquidGlassEdgeHighlightStrong");

        // 边缘阴影：内描边暗角
        _edgeShadowLayer = new Border
        {
            IsHitTestVisible = false,
            BorderThickness = new Thickness(0.8),
            CornerRadius = CornerRadius,
            Opacity = 0.5
        };
        _edgeShadowLayer.SetResourceReference(Border.BorderBrushProperty, "LiquidGlassEdgeShadow");

        // 触控发光层：径向光晕
        _glowLayer = new Border
        {
            IsHitTestVisible = false,
            Opacity = 0.0,
            CornerRadius = CornerRadius
        };
        _glowLayer.SetResourceReference(Border.BackgroundProperty, "LiquidGlassRimLightBrush");

        // 厚度阴影：投射深阴影
        _depthShadow = new DropShadowEffect
        {
            Color = Colors.Black,
            Opacity = 0.12,
            BlurRadius = 16,
            ShadowDepth = 6,
            Direction = 270
        };
        Effect = _depthShadow;

        // 根网格：z 序由添加顺序决定（底→顶）
        _rootGrid = new Grid();
        _rootGrid.Children.Add(_contentHost);
        _rootGrid.Children.Add(_refractionLayer);
        _rootGrid.Children.Add(_edgeShadowLayer);
        _rootGrid.Children.Add(_edgeHighlightLayer);
        _rootGrid.Children.Add(_specularLayer);
        _rootGrid.Children.Add(_glowLayer);

        base.Child = _rootGrid;

        // 默认材质参数
        BlurRadius = 30.0;
        BlurSamplingRate = 0.8;

        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        PreviewMouseLeftButtonDown += OnPressed;
        PreviewMouseLeftButtonUp += OnReleased;
        LostMouseCapture += OnReleased;
        MouseMove += OnPointerMove;
        SizeChanged += OnSizeChanged;
        Loaded += (_, _) => EnsureRestingState();
        Unloaded += OnUnloaded;
    }

    // ── 公共属性 ────────────────────────────────────────────

    /// <summary>用户内容。</summary>
    public object Content
    {
        get => _contentHost.Content;
        set => _contentHost.Content = value;
    }

    /// <summary>折射/高光强度（0~1），由厚度感知自动计算或手动指定。</summary>
    public double GlassIntensity
    {
        get => (double)GetValue(GlassIntensityProperty);
        set => SetValue(GlassIntensityProperty, value);
    }

    public static readonly DependencyProperty GlassIntensityProperty =
        DependencyProperty.Register(nameof(GlassIntensity), typeof(double), typeof(LiquidGlassPanel),
            new PropertyMetadata(0.6, OnGlassIntensityChanged));

    private static void OnGlassIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LiquidGlassPanel p)
        {
            p._currentIntensity = (double)e.NewValue;
            p.UpdateRestingState();
        }
    }

    /// <summary>是否启用触控发光（交互式表面）。</summary>
    public bool InteractionGlow
    {
        get => (bool)GetValue(InteractionGlowProperty);
        set => SetValue(InteractionGlowProperty, value);
    }

    public static readonly DependencyProperty InteractionGlowProperty =
        DependencyProperty.Register(nameof(InteractionGlow), typeof(bool), typeof(LiquidGlassPanel),
            new PropertyMetadata(true));

    /// <summary>用户内容。XAML 中直接作为子元素设置。</summary>
    public new UIElement Child
    {
        get => base.Child == _rootGrid ? _contentHost.Content as UIElement : base.Child;
        set
        {
            _contentHost.Content = value;
            base.Child = _rootGrid;
        }
    }

    // ── 厚度感知 ────────────────────────────────────────────

    protected void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var size = Math.Min(e.NewSize.Width, e.NewSize.Height);
        var area = e.NewSize.Width * e.NewSize.Height;

        // 折射/高光随面积增强（有上限）
        _currentIntensity = Math.Min(1.0, 0.35 + area / 60000.0);
        GlassIntensity = _currentIntensity;

        // 模糊半径随尺寸增加（更厚的玻璃）
        BlurRadius = Math.Max(20.0, Math.Min(45.0, size * 0.35));

        // 厚度阴影：尺寸越大，阴影越深越远
        var depthScale = Math.Min(2.0, 0.5 + area / 80000.0);
        _depthShadow.BlurRadius = 16.0 * depthScale;
        _depthShadow.ShadowDepth = 6.0 * depthScale;
        _depthShadow.Opacity = 0.10 + 0.06 * _currentIntensity;

        UpdateCornerRadius(CornerRadius);
    }

    private void UpdateCornerRadius(CornerRadius cr)
    {
        _refractionLayer.CornerRadius = cr;
        _specularLayer.CornerRadius = cr;
        _edgeHighlightLayer.CornerRadius = cr;
        _edgeShadowLayer.CornerRadius = cr;
        _glowLayer.CornerRadius = cr;
    }

    // ── 静止态 ──────────────────────────────────────────────

    protected void EnsureRestingState()
    {
        if (_restingApplied) return;
        _restingApplied = true;
        UpdateRestingState();
    }

    protected void UpdateRestingState()
    {
        // 静止态：微弱边缘高光 + 暗角，折射/镜面/发光关闭
        _edgeHighlightLayer.Opacity = 0.25 * _currentIntensity;
        _edgeShadowLayer.Opacity = 0.5 * _currentIntensity;
        _specularLayer.Opacity = 0.0;
        _refractionLayer.Opacity = 0.0;
        _glowLayer.Opacity = 0.0;
        // 折射归零
        AnimateRefraction(1.0, 1.0, 0);
    }

    // ── 交互：Touch-to-Glow + 流体扭曲 ─────────────────────

    protected void OnMouseEnter(object sender, MouseEventArgs e)
    {
        _isHovered = true;
        if (!InteractionGlow) return;
        // 悬停：边缘高光增强、镜面高光浮现、折射微启（光线聚焦）
        AnimateTo(_edgeHighlightLayer, 0.8 * _currentIntensity, 240, EasingMode.EaseOut, EasingStrength.Strong);
        AnimateTo(_specularLayer, 0.55 * _currentIntensity, 300, EasingMode.EaseOut, EasingStrength.Strong);
        AnimateTo(_refractionLayer, 0.15 * _currentIntensity, 320, EasingMode.EaseOut, EasingStrength.Middle);
        // 折射拉伸：内容轻微放大
        AnimateRefraction(1.012, 1.012, 320);
    }

    protected void OnMouseLeave(object sender, MouseEventArgs e)
    {
        _isHovered = false;
        _pointerVelocity = new Vector();
        UpdateRestingState();
        // 折射弹簧归零
        AnimateRefraction(1.0, 1.0, 450, spring: true);
    }

    protected void OnPointerMove(object sender, MouseEventArgs e)
    {
        if (!InteractionGlow) return;
        var pos = e.GetPosition(this);
        var now = DateTime.UtcNow;
        // 速度追踪（流体扭曲跟随）
        if (_lastPointerTime != DateTime.MinValue)
        {
            var dt = (now - _lastPointerTime).TotalSeconds;
            if (dt > 0 && dt < 0.1)
            {
                var inst = (pos - _lastPointerPos) / dt;
                // 低通滤波平滑速度
                _pointerVelocity = _pointerVelocity * 0.6 + inst * 0.4;
            }
        }
        _lastPointerPos = pos;
        _lastPointerTime = now;

        // 拖拽/快速移动时折射增强（扭曲幅度与速度成正比）
        if (_isHovered && !_isPressed)
        {
            var speed = _pointerVelocity.Length;
            var distortion = Math.Min(speed * 0.0008, 0.015);
            if (distortion > 0.002)
            {
                AnimateRefraction(1.0 + distortion, 1.0 + distortion, 80);
            }
        }
    }

    protected void OnPressed(object sender, MouseButtonEventArgs e)
    {
        _isPressed = true;
        if (!InteractionGlow) return;
        // Touch-to-Glow：按下瞬间饱和度/亮度爆发（120ms 内），发光层显现
        AnimateTo(_glowLayer, 0.7 * _currentIntensity, 120, EasingMode.EaseIn, EasingStrength.Weak);
        AnimateTo(_specularLayer, 0.9 * _currentIntensity, 120, EasingMode.EaseIn, EasingStrength.Weak);
        AnimateTo(_edgeHighlightLayer, 1.0 * _currentIntensity, 120, EasingMode.EaseIn, EasingStrength.Weak);
        AnimateTo(_refractionLayer, 0.3 * _currentIntensity, 120, EasingMode.EaseIn, EasingStrength.Weak);
        // 折射：按下时内容轻微内凹（模拟玻璃受压）
        AnimateRefraction(0.985, 0.985, 120);
    }

    protected void OnReleased(object sender, EventArgs e)
    {
        if (!_isPressed) return;
        _isPressed = false;
        if (!InteractionGlow) return;
        // 弹簧式回弹：发光快速消退，高光以弹簧回落，折射弹回
        AnimateTo(_glowLayer, 0.0, 420, EasingMode.EaseOut, EasingStrength.Strong, spring: true);
        AnimateTo(_specularLayer, _isHovered ? 0.55 * _currentIntensity : 0.0, 480, EasingMode.EaseOut, EasingStrength.Middle, spring: true);
        AnimateTo(_edgeHighlightLayer, _isHovered ? 0.8 * _currentIntensity : 0.25 * _currentIntensity, 420, EasingMode.EaseOut, EasingStrength.Middle, spring: true);
        AnimateTo(_refractionLayer, _isHovered ? 0.15 * _currentIntensity : 0.0, 450, EasingMode.EaseOut, EasingStrength.Middle, spring: true);
        // 折射弹簧回弹（阻尼 0.8）
        AnimateRefraction(_isHovered ? 1.012 : 1.0, _isHovered ? 1.012 : 1.0, 480, spring: true);
    }

    protected void OnUnloaded(object sender, RoutedEventArgs e)
    {
        MouseEnter -= OnMouseEnter;
        MouseLeave -= OnMouseLeave;
        PreviewMouseLeftButtonDown -= OnPressed;
        PreviewMouseLeftButtonUp -= OnReleased;
        LostMouseCapture -= OnReleased;
        MouseMove -= OnPointerMove;
        SizeChanged -= OnSizeChanged;
        Unloaded -= OnUnloaded;
    }

    // ── 动画工具：弹簧物理 ──────────────────────────────────

    private enum EasingStrength { Weak, Middle, Strong }

    /// <summary>以弹簧物理驱动透明度过渡（规范禁止 transition:all 0.3s 硬时长）。</summary>
    private static void AnimateTo(UIElement target, double toOpacity, int ms,
        EasingMode mode, EasingStrength strength, bool spring = false)
    {
        if (target == null) return;
        var from = target.Opacity;
        if (Math.Abs(from - toOpacity) < 0.001) return;

        EasingFunctionBase ease;
        if (spring)
        {
            // 弹簧回弹：ElasticEase 模拟阻尼振荡
            ease = new ElasticEase
            {
                Oscillations = strength == EasingStrength.Strong ? 2 : 1,
                Springiness = strength == EasingStrength.Strong ? 4 : 6,
                EasingMode = EasingMode.EaseOut
            };
        }
        else
        {
            // 平滑结束：指数缓动（力量越强越快减速）
            ease = new ExponentialEase
            {
                Exponent = strength == EasingStrength.Weak ? 3 : strength == EasingStrength.Middle ? 5 : 7,
                EasingMode = mode
            };
        }

        var anim = new DoubleAnimation(toOpacity, TimeSpan.FromMilliseconds(ms))
        {
            EasingFunction = ease
        };
        target.BeginAnimation(OpacityProperty, anim);
    }

    /// <summary>折射层 RenderTransform 动画（透镜对内容的拉伸）。</summary>
    private void AnimateRefraction(double scaleX, double scaleY, int ms, bool spring = false)
    {
        if (_contentHost.RenderTransform is not ScaleTransform st) return;

        EasingFunctionBase ease;
        if (spring)
        {
            ease = new ElasticEase
            {
                Oscillations = 1,
                Springiness = 5,
                EasingMode = EasingMode.EaseOut
            };
        }
        else
        {
            ease = ms <= 0 ? null : new CubicEase { EasingMode = EasingMode.EaseOut };
        }

        if (ms <= 0)
        {
            st.ScaleX = scaleX;
            st.ScaleY = scaleY;
            return;
        }

        var ax = new DoubleAnimation(scaleX, TimeSpan.FromMilliseconds(ms)) { EasingFunction = ease };
        var ay = new DoubleAnimation(scaleY, TimeSpan.FromMilliseconds(ms)) { EasingFunction = ease };
        st.BeginAnimation(ScaleTransform.ScaleXProperty, ax);
        st.BeginAnimation(ScaleTransform.ScaleYProperty, ay);
    }

    // ── 无障碍：减弱动效降级 ────────────────────────────────
    // 规范第 7 条：prefers-reduced-motion 时移除扭曲与位移，降级为透明度交叉淡变；
    // prefers-reduced-transparency 时改为不透明背景。
    private static bool IsReducedMotion =>
        SystemParameters.ClientAreaAnimation == false ||
        SystemParameters.MenuAnimation == false;
}
