using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace PCL;

[ContentProperty("Inlines")]
public partial class MyButton
{
    public delegate void ClickEventHandler(object sender, MouseButtonEventArgs e); // 自定义事件

    public enum ColorState
    {
        Normal = 0,
        Highlight = 1,
        Red = 2
    }

    // 自定义事件
    // 颜色过渡：Apple 风格 ease-out，而非 Linear。进入快、退出稍慢。
    private const int animationColorIn = 180;
    private const int animationColorOut = 240;

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
        typeof(MyButton), new PropertyMetadata((sender, e) =>
        {
            if (sender is not null) ((MyButton)sender).LabText.Text = (string)e.NewValue;
        }));

    // 属性穿透
    public new static readonly DependencyProperty PaddingProperty = DependencyProperty.Register("Padding",
        typeof(Thickness), typeof(MyButton), new PropertyMetadata((sender, e) =>
        {
            if (sender is not null) ((MyButton)sender).PanFore.Padding = (Thickness)e.NewValue;
        }));
    
    private ColorState _ColorType = ColorState.Normal; // 配色方案

    // 鼠标点击判定（务必放在点击事件之后，以使得 Button_MouseUp 先于 Button_MouseLeave 执行）
    

    // 自定义属性
    public int Uuid = ModBase.GetUuid();

    public MyButton()
    {
        InitializeComponent();

        MouseEnter += RefreshColor;
        MouseLeave += RefreshColor;
        Loaded += RefreshColor;
        IsEnabledChanged += (_, _) => RefreshColor();
        MouseLeftButtonUp += Button_MouseUp;
        MouseLeftButtonDown += Button_MouseDown;
        MouseEnter += (_, _) => Button_MouseEnter();
        MouseLeftButtonUp += (_, _) => Button_MouseUp();
        MouseLeave += (_, _) => Button_MouseLeave();
    }

    public InlineCollection Inlines => LabText.Inlines;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    } // 显示文本

    public Thickness TextPadding
    {
        get => LabText.Padding;
        set => LabText.Padding = value;
    }

    public ColorState ColorType
    {
        get => _ColorType;
        set
        {
            _ColorType = value;
            RefreshColor();
        }
    }

    public new Thickness Padding
    {
        get => PanFore.Padding;
        set => PanFore.Padding = value;
    }

    public Transform RealRenderTransform
    {
        get => PanFore.RenderTransform;
        set => PanFore.RenderTransform = value;
    }

    // 声明
    public event ClickEventHandler? Click;

    private string GetBorderBrushResourceKey()
    {
        return ColorType switch
        {
            ColorState.Normal => IsMouseOver ? "ColorBrush3" : "ColorBrush1",
            ColorState.Highlight => IsMouseOver ? "ColorBrush3" : "ColorBrush2",
            ColorState.Red => IsMouseOver ? "ColorBrushRedLight" : "ColorBrushRedDark",
            _ => "ColorBrush1"
        };
    }

    private void StartBorderBrushAnimation(string resourceKey, int duration)
    {
        ModAnimation.AniStart(
            new[]
            {
                ModAnimation.AaColor(PanFore, BorderBrushProperty, resourceKey, duration,
                    ease: new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.EaseOut))
            }, "MyButton Color " + Uuid);
    }

    private void RefreshColor(object obj = null, object e = null)
    {
        try
        {
            if (ControlVisualHelpers.ShouldAnimate(this)) // 防止默认属性变更触发动画
            {
                if (IsEnabled)
                    StartBorderBrushAnimation(GetBorderBrushResourceKey(), IsMouseOver ? animationColorIn : animationColorOut);
                else
                    // 不可用（Gray 4）
                    ModAnimation.AniStart(
                        new[]
                        {
                            ModAnimation.AaColor(PanFore, BorderBrushProperty,
                                ThemeManager.colorGray4 - PanFore.BorderBrush, animationColorOut,
                                ease: new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.EaseOut))
                        }, "MyButton Color " + Uuid);
            }
            else
            {
                ModAnimation.AniStop("MyButton Color " + Uuid);
                if (IsEnabled)
                    PanFore.SetResourceReference(BorderBrushProperty, GetBorderBrushResourceKey());
                else
                    PanFore.BorderBrush = ThemeManager.colorGray4;
            }
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "刷新按钮颜色出错");
        }
    }

    // 实现自定义事件
    private bool isMouseDown = false;
    private void Button_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!isMouseDown)
            return;
        ModBase.Log("[Control] 按下按钮：" + Text);
        Click?.Invoke(sender, e);
        ModMain.RaiseCustomEvent(this);
    }

    private void Button_MouseDown(object sender, MouseButtonEventArgs e)
    {
        isMouseDown = true;
        Focus();
        ModAnimation.AniStart(
            new[]
            {
                // 按下：临界阻尼弹簧（无 overshoot），即时响应。Apple 按钮按下缩放 0.97。
                ModAnimation.AaScaleTransform(PanFore, 0.97d - ((ScaleTransform)PanFore.RenderTransform).ScaleX, 180,
                    ease: new ModAnimation.AniEaseAppleSpring(1.0d, 0.32d)),
                // 液态玻璃 Touch-to-Glow：按下瞬间饱和/亮度爆发，发光层显现
                ModAnimation.AaOpacity(GlowLayer, 0.7d - GlowLayer.Opacity, 140,
                    ease: new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.EaseOut))
            }, "MyButton Scale " + Uuid);
    }

    private void Button_MouseEnter()
    {
        ModAnimation.AniStart(
            ModAnimation.AaColor(PanFore, BackgroundProperty,
                _ColorType == ColorState.Red ? "ColorBrushRedBack" : "ColorBrush7", animationColorIn,
                ease: new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.EaseOut)),
            "MyButton Background " + Uuid);
    }

    private void Button_MouseUp()
    {
        if (!isMouseDown)
            return;
        isMouseDown = false;
        ModAnimation.AniStart(
            new[]
            {
                // 弹起：欠阻尼弹簧（damping 0.72），真实 overshoot 回弹。Apple 动量交互的标志质感。
                ModAnimation.AaScaleTransform(PanFore, 1.0d - ((ScaleTransform)PanFore.RenderTransform).ScaleX, 480, 10,
                    new ModAnimation.AniEaseAppleSpring(0.72d, 0.34d)),
                // 发光层弹簧式消退
                ModAnimation.AaOpacity(GlowLayer, 0.0d - GlowLayer.Opacity, 420,
                    ease: new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.Standard))
            }, "MyButton Scale " + Uuid);
    }

    private void Button_MouseLeave()
    {
        ModAnimation.AniStart(
            ModAnimation.AaColor(PanFore, BackgroundProperty, "ColorBrushHalfWhite", animationColorOut,
                ease: new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.EaseOut)),
            "MyButton Background " + Uuid);
        if (!isMouseDown)
            return;
        isMouseDown = false;
        ModAnimation.AniStart(
            new[]
            {
                // 离开：临界阻尼弹簧，平滑回正无 overshoot（非动量交互，不应弹）。
                ModAnimation.AaScaleTransform(PanFore, 1d - ((ScaleTransform)PanFore.RenderTransform).ScaleX, 420,
                    ease: new ModAnimation.AniEaseAppleSpring(1.0d, 0.36d)),
                ModAnimation.AaOpacity(GlowLayer, 0.0d - GlowLayer.Opacity, 360,
                    ease: new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.EaseOut))
            }, "MyButton Scale " + Uuid);
    }
}
