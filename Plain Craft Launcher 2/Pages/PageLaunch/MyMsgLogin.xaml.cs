using System.Windows.Controls;
using System.Windows.Input;
using PCL.Core.App;
using PCL.Core.App.Localization;
using PCL.Core.UI.Controls;
using PCL.Core.Utils;
using PCL.Core.IO.Net.Http;
using System.Text.Json.Serialization;

namespace PCL;

public partial class MyMsgLogin
{
    private readonly JsonObject data;
    private string deviceCode; // 用于轮询的设备代码
    private string oAuthUrl = ""; // OAuth 轮询验证地址
    private string userCode; // 需要用户在网页上输入的设备代码
    private string website; // 验证网页的网址
    private Task? workingThread;

    public MyMsgLogin()
    {
        InitializeComponent();
        // Handles
        Loaded += Load;
        Btn1.Click += Btn1_Click;
        Btn3.Click += Btn3_Click;
        PanBorder.MouseLeftButtonDown += Drag;
        LabTitle.MouseLeftButtonDown += Drag;
    }

    private void Finished(object result)
    {
        if (myConverter.IsExited)
            return;
        myConverter.IsExited = true;
        myConverter.Result = result;
        ModBase.RunInUi(Close);
        Thread.Sleep(200);
        ModMain.frmMain.ShowWindowToTop();
    }

    private void Init()
    {
        userCode = (string)data["user_code"];
        deviceCode = (string)data["device_code"];
        if (data["verification_uri_complete"] is not null)
        {
            website = (string)data["verification_uri_complete"];
            LabCaption.Text = Lang.Text("Launch.Account.LoginDialog.MicrosoftInstructions.WithAutoFill", userCode, website);
        }
        else
        {
            website = (string)data["verification_uri"];
            LabCaption.Text = Lang.Text("Launch.Account.LoginDialog.MicrosoftInstructions", userCode, website);
        }

        // 设置 UI
        LabTitle.Text = Lang.Text("Launch.Account.LoginDialog.MinecraftLogin");
        CustomEventService.SetEventData(Btn1, website);
        CustomEventService.SetEventData(Btn2, userCode);
        // 启动工作线程
        workingThread = WorkThreadAsync();
    }

    private record ErrorBody(
        [property: JsonPropertyName("error")] string Error,
        [property: JsonPropertyName("error_description")] string Desc);

    private async Task WorkThreadAsync()
    {
        await Task.Delay(2000).ConfigureAwait(false);
        if (myConverter.IsExited)
            return;
        ModBase.OpenWebsite(website);
        ModBase.ClipboardSet(userCode);
        var delayTime = (data["interval"].ToObject<int>() - 1) * 1000;
        // 轮询
        var unknownFailureCount = 0;
        while (!myConverter.IsExited)
        {
            try
            {
                var bodyData = $"grant_type=urn:ietf:params:oauth:grant-type:device_code&client_id={Secrets.MSOAuthClientId}&device_code={deviceCode}&scope=XboxLive.signin%20offline_access";
                using var result = await HttpRequest
                    .Create("https://login.microsoftonline.com/consumers/oauth2/v2.0/token")
                    .WithFormContent(bodyData)
                    .SendAsync(enableLogging: false)
                    .ConfigureAwait(false);
                // 获取响应体（用于错误关键词匹配，与原版 PCL 行为一致）
                var body = await result.AsStringAsync().ConfigureAwait(false);
                if (!result.IsSuccess)
                {
                    // 修改错误列表时，同时修改 ModLaunch.MsLoginStep1Refresh 中的对应代码
                    if (body.Contains("authorization_declined"))
                    {
                        Finished(new Exception("$你拒绝了 PCL 申请的权限……"));
                        return;
                    }
                    if (body.Contains("expired_token"))
                    {
                        Finished(new Exception("$登录用时太长啦，重新试试吧！"));
                        return;
                    }
                    if (body.Contains("Account security interrupt"))
                    {
                        Finished(new Exception("$该账号由于安全问题无法登陆，请前往微软账户页获取更多信息。"));
                        return;
                    }
                    if (body.Contains("service abuse"))
                    {
                        Finished(new Exception("$非常抱歉，该账号已被微软封禁，无法登录。"));
                        return;
                    }
                    // AADSTS70000：可能不能判 "invalid_grant"，见 #269；触发密码登录引导流程
                    if (body.Contains("AADSTS70000"))
                    {
                        Finished(new ModBase.RestartException());
                        return;
                    }
                    if (body.Contains("authorization_pending"))
                    {
                        await Task.Delay(delayTime).ConfigureAwait(false);
                        continue;
                    }
                    throw new Exception(body);
                }
                // 获取结果
                var resultJson = (JsonObject)ModBase.GetJson(body);
                ModProfile.ProfileLog($"令牌过期时间：{resultJson["expires_in"]} 秒");
                HintService.Hint(Lang.Text("Launch.Account.LoginDialog.Success"), HintType.Success);
                Finished(new[] { resultJson["access_token"].ToString(), resultJson["refresh_token"].ToString() });
                return;
            }
            catch (Exception ex)
            {
                if (unknownFailureCount <= 2)
                {
                    unknownFailureCount += 1;
                    ModBase.Log(ex, $"正版验证轮询第 {unknownFailureCount} 次失败");
                    await Task.Delay(2000).ConfigureAwait(false);
                }
                else
                {
                    Finished(new Exception(Lang.Text("Launch.Account.LoginDialog.PollingFailed"), ex));
                    return;
                }
            }
        }
    }


    #region 弹窗

    private readonly ModMain.MyMsgBoxConverter myConverter;
    private readonly int uuid = ModBase.GetUuid();

    public MyMsgLogin(ModMain.MyMsgBoxConverter converter)
    {
        try
        {
            InitializeComponent();
            Btn1.Name += ModBase.GetUuid();
            Btn2.Name += ModBase.GetUuid();
            Btn3.Name += ModBase.GetUuid();
            myConverter = converter;
            ShapeLine.StrokeThickness = ModBase.GetWPFSize(1d);
            data = (JsonObject)converter.Content;
            oAuthUrl = converter.AuthUrl?.ToString() ?? "";
            Init();
        }
        catch (Exception ex)
        {
            ModBase.Log(
                ex,
                Lang.Text("Launch.Account.LoginDialog.Error.Init"),
                ModBase.LogLevel.Hint,
                userSummary: Lang.Text("Launch.Account.LoginDialog.Error.Init"));
        }

        Loaded += Load;
    }

    private void Load(object sender, EventArgs e)
    {
        try
        {
            // 动画
            Opacity = 0d;
            ModAnimation.AniStart(
                ModAnimation.AaColor(ModMain.frmMain.PanMsgBackground, BlurBorder.BackgroundProperty,
                    (myConverter.IsWarn
                        ? new ModBase.MyColor(140d, 80d, 0d, 0d)
                        : new ModBase.MyColor(90d, 0d, 0d, 0d)) - ModMain.frmMain.PanMsgBackground.Background, 200),
                "PanMsgBackground Background");
            ModAnimation.AniStart(
                new[]
                {
                    ModAnimation.AaOpacity(this, 1d, 120, 60),
                    ModAnimation.AaDouble(i => TransformPos.Y += (double)i,
                        -TransformPos.Y, 300, 60, new ModAnimation.AniEaseOutBack(ModAnimation.AniEasePower.Weak)),
                    ModAnimation.AaDouble(i => TransformRotate.Angle += (double)i,
                        -TransformRotate.Angle, 300, 60,
                        new ModAnimation.AniEaseOutFluent(ModAnimation.AniEasePower.Weak))
                }, "MyMsgBox " + uuid);
            // 记录日志
            ModBase.Log($"[Control] 正版验证弹窗：{LabTitle.Text}\r\n{LabCaption.Text}");
        }
        catch (Exception ex)
        {
            ModBase.Log(
                ex,
                Lang.Text("Launch.Account.LoginDialog.Error.Load"),
                ModBase.LogLevel.Hint,
                userSummary: Lang.Text("Launch.Account.LoginDialog.Error.Load"));
        }
    }

    private void Close()
    {
        // 动画
        ModAnimation.AniStart(new[]
        {
            ModAnimation.AaCode(() =>
            {
                if (!ModMain.WaitingMyMsgBox.Any())
                    ModAnimation.AniStart(ModAnimation.AaColor(ModMain.frmMain.PanMsgBackground,
                        BlurBorder.BackgroundProperty,
                        new ModBase.MyColor(0d, 0d, 0d, 0d) - ModMain.frmMain.PanMsgBackground.Background, 200,
                        ease: new ModAnimation.AniEaseOutFluent(ModAnimation.AniEasePower.Weak)));
            }, 30),
            ModAnimation.AaOpacity(this, -Opacity, 80, 20),
            ModAnimation.AaDouble(i => TransformPos.Y += (double)i, 20d - TransformPos.Y,
                150, 0, new ModAnimation.AniEaseOutFluent()),
            ModAnimation.AaDouble(i => TransformRotate.Angle += (double)i,
                6d - TransformRotate.Angle, 150, 0, new ModAnimation.AniEaseInFluent(ModAnimation.AniEasePower.Weak)),
            ModAnimation.AaCode(() => ((Grid)Parent).Children.Remove(this), after: true)
        }, "MyMsgBox " + uuid);
    }

    // 实现回车和 Esc 的接口（#4857）
    public void Btn1_Click(object sender, MouseButtonEventArgs e)
    {
    }

    public void Btn3_Click(object sender, MouseButtonEventArgs e)
    {
        Finished(new ThreadInterruptedException());
    }

    private void Drag(object sender, MouseButtonEventArgs e)
    {
        // On Error Resume Next
        if (e.GetPosition(ShapeLine).Y <= 2d)
            ModMain.frmMain.DragMove();
    }

    #endregion
}