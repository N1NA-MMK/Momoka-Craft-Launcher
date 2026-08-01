namespace PCL.Core.App;

/// <summary>
/// 联机协议偏好
/// </summary>
public enum LinkProtocolPreference
{
    Tcp,
    Udp
}

/// <summary>
/// 主题模式（亮/暗/系统）
/// </summary>
public enum ColorMode
{
    Light = 0,
    Dark = 1,
    System = 2
}

/// <summary>
/// 配色主题
/// </summary>
public enum ColorTheme
{
    SkyBlue = 0,
    CatBlue = 1,
    DeathBlue = 2,
    HmclBlue = 3,
    /// <summary>自定义主题色，由 UiLauncherHue/Sat/Light/Delta 控制。</summary>
    Custom = 4
}

/// <summary>
/// 更新通道
/// </summary>
public enum UpdateChannel
{
    Release = 0,
    Beta = 1,
    Dev = 2
}

/// <summary>
/// 游戏窗口大小模式
/// </summary>
public enum GameWindowSizeMode
{
    Fullscreen = 0,
    Default = 1,
    Launcher = 2,
    Custom = 3,
    Maximized = 4
}

/// <summary>
/// 游戏进程优先级
/// </summary>
public enum GameProcessPriority
{
    AboveNormal = 0,
    Normal = 1,
    BelowNormal = 2,
    High = 3,
    RealTime = 4
}

/// <summary>
/// 游戏启动后启动器可见性
/// </summary>
public enum LauncherVisibility
{
    ExitImmediately = 0,
    ObsoleteCaseDoNotUse = 1,
    HideAndExit = 2,
    HideAndReopen = 3,
    MinimizeAndReopen = 4,
    DoNothing = 5
}

/// <summary>
/// JVM 优先 IP 栈类型
/// </summary>
public enum JvmPreferredIpStack
{
    PreferV4 = 0,
    Default = 1,
    PreferV6 = 2
}

/// <summary>
/// 联机中继行为
/// </summary>
public enum LinkRelayBehavior
{
    Default = 0,
    ForceRelay = 1
}

/// <summary>
/// 启动器更新行为
/// </summary>
public enum LauncherAutoUpdateBehavior
{
    DownloadAndInstall = 0,
    DownloadAndAnnounce = 1,
    AnnounceOnly = 2,
    Disable = 3
}

public enum LauncherTitleType
{
    None = 0,
    Default = 1,
    Text = 2,
    Image = 3
}

/// <summary>
/// 圆角风格
/// </summary>
public enum CornerStyle
{
    /// <summary>无圆角（锐利）</summary>
    Sharp = 0,
    /// <summary>小圆角</summary>
    Small = 1,
    /// <summary>中圆角（默认）</summary>
    Medium = 2,
    /// <summary>大圆角</summary>
    Large = 3,
    /// <summary>全圆角（胶囊）</summary>
    Pill = 4
}

/// <summary>
/// 界面密度
/// </summary>
public enum UiDensity
{
    /// <summary>紧凑</summary>
    Compact = 0,
    /// <summary>标准（默认）</summary>
    Standard = 1,
    /// <summary>宽松</summary>
    Comfortable = 2
}

/// <summary>
/// 边框风格
/// </summary>
public enum BorderStyle
{
    /// <summary>无边框（纯扁平）</summary>
    None = 0,
    /// <summary>细边框</summary>
    Thin = 1,
    /// <summary>标准边框（默认）</summary>
    Standard = 2
}
