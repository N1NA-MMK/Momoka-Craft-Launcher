using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using PCL.Core.App.Localization;
namespace PCL;

public partial class MySlider
{
    public delegate void ChangeEventHandler(object sender, bool user);

    public delegate void PreviewChangeEventHandler(object sender, ModBase.RouteEventArgs e);

    // 自定义属性

    private int _Value;
    private bool changeByKey;

    // 拖动

    public Delegate getHintText;

    // 拖动中标志：拖动时直接 1:1 跟手（无缓动），仅程序化/按键改变时走 Apple 缓动
    private bool isDragging;

    // 基础

    public int Uuid = ModBase.GetUuid();

    public MySlider()
    {
        InitializeComponent();
        SizeChanged += RefreshWidth;
        MouseLeftButtonDown += DragStart;
        IsEnabledChanged += (_, _) => RefreshColor();
        MouseEnter += (_, _) => RefreshColor();
        MouseLeave += (_, _) => RefreshColor();
        MouseEnter += (_, _) => MySlider_MouseEnter();
        KeyDown += MySlider_KeyDown;
    }

    public int MaxValue
    {
        get => field;
        set
        {
            if (value == field)
                return;
            field = value;
            RefreshWidth(null, null);
        }
    } = 100;

    public int Value
    {
        get => _Value;
        set
        {
            try
            {
                value = (int)Math.Round(ModBase.MathClamp(value, 0d, MaxValue));
                if (_Value == value)
                    return;

                // 触发 Preview 事件，修改新值
                var oldValue = _Value;
                _Value = value;
                if (ModAnimation.AniControlEnabled == 0)
                {
                    var e = new ModBase.RouteEventArgs();
                    PreviewChange?.Invoke(this, e);
                    if (e.handled)
                    {
                        _Value = oldValue;
                        DragStop();
                        return;
                    }
                }

                if (IsLoaded && ModAnimation.AniControlEnabled == 0)
                {
                    if (ActualWidth < ShapeDot.Width)
                        return;
                    var newWidth = _Value / (double)MaxValue * (ActualWidth - ShapeDot.Width);
                    // Apple Direct Manipulation：拖动中 1:1 跟手，无缓动；仅程序化/按键改变时走 Apple 缓动。
                    if (isDragging)
                    {
                        ModAnimation.AniStop("MySlider Progress " + Uuid);
                        LineFore.Width = Math.Max(0d, newWidth + (newWidth < 0.5d ? 0d : 0.5d));
                        LineBack.Width = Math.Max(0d,
                            ActualWidth - ShapeDot.Width - newWidth + (ActualWidth - ShapeDot.Width - newWidth < 0.5d ? 0d : 0.5d));
                        ModBase.SetLeft(ShapeDot, newWidth);
                    }
                    else
                    {
                        var deltaProcess =
                            Math.Abs(LineFore.Width / (ActualWidth - ShapeDot.Width) - _Value / (double)MaxValue);
                        var time = (1d - Math.Pow(1d - deltaProcess, 3d)) * 300d + (changeByKey ? 100 : 0);
                        var appleEase = time > 50d
                            ? new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.Standard)
                            : (ModAnimation.AniEase)new ModAnimation.AniEaseLinear();
                        ModAnimation.AniStart(
                            new[]
                            {
                                ModAnimation.AaWidth(LineFore,
                                    Math.Max(0d, newWidth + (newWidth < 0.5d ? 0d : 0.5d)) - LineFore.Width,
                                    (int)Math.Round(time),
                                    ease: appleEase),
                                ModAnimation.AaWidth(LineBack,
                                    Math.Max(0d,
                                        ActualWidth - ShapeDot.Width - newWidth +
                                        (ActualWidth - ShapeDot.Width - newWidth < 0.5d ? 0d : 0.5d)) - LineBack.Width,
                                    (int)Math.Round(time),
                                    ease: appleEase),
                                ModAnimation.AaX(ShapeDot, newWidth - ShapeDot.Margin.Left, (int)Math.Round(time),
                                    ease: appleEase)
                            }, "MySlider Progress " + Uuid);
                    }
                }
                else
                {
                    RefreshWidth(null, null);
                }

                if (ModAnimation.AniControlEnabled == 0)
                    Change?.Invoke(this, false);
            }

            catch (Exception ex)
            {
                ModBase.Log(
                    ex,
                    "滑动条进度改变出错",
                    ModBase.LogLevel.Hint,
                    userSummary: Lang.Text("Application.Control.Error.OperationFailed"));
            }
        }
    }

    // 按键改变

    public uint ValueByKey { get; set; } = 1U;
    public event ChangeEventHandler? Change;
    public event PreviewChangeEventHandler? PreviewChange;

    private void RefreshWidth(object sender, SizeChangedEventArgs? e)
    {
        if (e is not null)
            PanMain.Width = e.NewSize.Width;
        ModAnimation.AniStop("MySlider Progress " + Uuid);
        var newWidth = _Value / (double)MaxValue * (ActualWidth - ShapeDot.Width);
        LineFore.Width = Math.Max(0d, newWidth + (newWidth < 0.5d ? 0d : 0.5d));
        LineBack.Width = Math.Max(0d,
            ActualWidth - ShapeDot.Width - newWidth + (ActualWidth - ShapeDot.Width - newWidth < 0.5d ? 0d : 0.5d));
        ModBase.SetLeft(ShapeDot, newWidth);
    }

    private void DragStart(object sender, MouseButtonEventArgs e)
    {
        CaptureMouse();
        MouseMove += OnDragMouseMove;
        e.Handled = true; // 防止 ScrollViewer 失焦问题
        ModMain.dragControl = this;
        RefreshColor();
        ModMain.frmMain.DragDoing();
        isDragging = true;
        // 拖动开始：圆点放大，Apple 弹簧（欠阻尼，带轻微 overshoot 的动量反馈）
        ModAnimation.AniStart(
            ModAnimation.AaScaleTransform(ShapeDot, 1.3d - ((ScaleTransform)ShapeDot.RenderTransform).ScaleX, 220,
                ease: new ModAnimation.AniEaseAppleSpring(0.8d, 0.3d)), "MySlider Scale " + Uuid);
        RefreshPopup();
        ModAnimation.AniStop("MySlider KeyPopup " + Uuid);
    }

    public void DragDoing()
    {
        var percent =
            ModBase.MathClamp((Mouse.GetPosition(PanMain).X - ShapeDot.Width / 2d) / (ActualWidth - ShapeDot.Width), 0d,
                1d);
        var newValue = (int)Math.Round(percent * MaxValue);
        if (newValue != Value) Value = newValue;
        RefreshPopup();
    }
    
    private void OnDragMouseMove(object sender, MouseEventArgs e)
    {
        // 防御性检查：如果鼠标左键已释放但 DragStop 尚未被时钟触发，立即清理
        if (Mouse.LeftButton != MouseButtonState.Pressed)
        {
            DragStop();
            return;
        }
        DragDoing();
    }
    
    public void DragStop()
    {
        isDragging = false;
        MouseMove -= OnDragMouseMove;
        if (IsMouseCaptured) ReleaseMouseCapture();
        RefreshColor();
        // 拖动结束：圆点回缩，临界阻尼弹簧（平滑无 overshoot）
        ModAnimation.AniStart(
            ModAnimation.AaScaleTransform(ShapeDot, 1d - ((ScaleTransform)ShapeDot.RenderTransform).ScaleX, 280,
                ease: new ModAnimation.AniEaseAppleSpring(1.0d, 0.34d)), "MySlider Scale " + Uuid);
        Popup.IsOpen = false;
    }

    public void RefreshPopup()
    {
        if (getHintText is null)
            return;
        Popup.IsOpen = true;
        TextHint.Text = getHintText.DynamicInvoke(Value)?.ToString() ?? "";
        var typeface = new Typeface(TextHint.FontFamily, TextHint.FontStyle, TextHint.FontWeight, TextHint.FontStretch);
        var formattedText = new FormattedText(TextHint.Text, Thread.CurrentThread.CurrentCulture,
            TextHint.FlowDirection, typeface, TextHint.FontSize, TextHint.Foreground, ModBase.dpi);
        TextHint.Width = formattedText.Width; // 使用手动测量的宽度修复 #1057
    }

    // 指向动画

    private void RefreshColor()
    {
        try
        {
            // 判断当前颜色
            string foregroundName;
            string dotFillName;
            int animationTime;
            if (IsEnabled)
            {
                if (ModMain.dragControl is not null && ModMain.dragControl.Equals(this) || IsMouseOver)
                {
                    foregroundName = "ColorBrush3";
                    dotFillName = "ColorBrush3";
                    animationTime = 40;
                }
                else
                {
                    foregroundName = "ColorBrushBg0";
                    dotFillName = "ColorBrushBg0";
                    animationTime = 100;
                }
            }
            else
            {
                foregroundName = "ColorBrushGray5";
                dotFillName = "ColorBrushGray5";
                animationTime = 200;
            }

            // 触发颜色动画
            if (IsLoaded && ModAnimation.AniControlEnabled == 0) // 防止默认属性变更触发动画
            {
                // 有动画
                ModAnimation.AniStart(
                    new[]
                    {
                        ModAnimation.AaColor(this, BorderBrushProperty, foregroundName, animationTime),
                        ModAnimation.AaColor(ShapeDot, Shape.FillProperty, dotFillName, animationTime)
                    }, "MySlider Color " + Uuid);
            }
            else
            {
                // 无动画
                ModAnimation.AniStop("MySlider Color " + Uuid);
                SetResourceReference(BorderBrushProperty, foregroundName);
                ShapeDot.SetResourceReference(Shape.FillProperty, dotFillName);
            }
        }

        catch (Exception ex)
        {
            ModBase.Log(ex, "滑动条颜色改变出错");
        }
    }

    private void MySlider_MouseEnter()
    {
        Focus(); // 确保按键能改变值
    }

    private void MySlider_KeyDown(object sender, KeyEventArgs e)
    {
        // 拒绝一边拖动一边用按键改变
        if (ReferenceEquals(this, ModMain.dragControl))
            return;
        // 改变值
        if (e.Key == Key.Left)
        {
            changeByKey = true;
            Value = (int)(Value - ValueByKey);
            changeByKey = false;
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            changeByKey = true;
            Value = (int)(Value + ValueByKey);
            changeByKey = false;
            e.Handled = true;
        }
        else
        {
            return;
        }

        // 更新 Popup
        if (getHintText is not null)
        {
            RefreshPopup();
            ModAnimation.AniStop("MySlider KeyPopup " + Uuid);
            ModAnimation.AniStart(
                ModAnimation.AaCode(() => Popup.IsOpen = false, (int)Math.Round(700d * ModAnimation.aniSpeed)),
                "MySlider KeyPopup " + Uuid);
        }
    }
}