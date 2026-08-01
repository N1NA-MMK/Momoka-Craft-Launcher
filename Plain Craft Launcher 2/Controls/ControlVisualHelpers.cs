using System.Windows;
using PCL.Core.App;

namespace PCL;

internal static class ControlVisualHelpers
{
    /// <summary>
    /// 是否启用了「减少动画」。读取用户配置，开启时弹簧/位移类动画应降级为短促淡入淡出。
    /// 对应 Apple 的 prefers-reduced-motion 语义。
    /// </summary>
    internal static bool IsReducedMotion => Config.System.ReducedMotion;

    internal static bool ShouldAnimate(FrameworkElement control, object? animationOverride = null)
    {
        // ReducedMotion 下：跳过弹簧/位移动画，仅保留极短的淡入淡出（由各控件自行判断 IsReducedMotion）
        return control.IsLoaded && ModAnimation.AniControlEnabled == 0 && !IsReducedMotion && !false.Equals(animationOverride);
    }

    internal static void AnimateColorOrSetResource(FrameworkElement target, DependencyProperty property,
        string resourceKey, int duration, string animationKey, bool shouldAnimate)
    {
        if (shouldAnimate)
        {
            // ReducedMotion 下用更短时长 + 线性，避免动效眩晕
            var time = IsReducedMotion ? 80 : duration;
            ModAnimation.AniEase ease = IsReducedMotion
                ? new ModAnimation.AniEaseLinear()
                : new ModAnimation.AniEaseApple(ModAnimation.AniEaseApple.AppleEaseStyle.EaseOut);
            ModAnimation.AniStart(ModAnimation.AaColor(target, property, resourceKey, time, ease: ease), animationKey);
        }
        else
        {
            ModAnimation.AniStop(animationKey);
            target.SetResourceReference(property, resourceKey);
        }
    }
}
