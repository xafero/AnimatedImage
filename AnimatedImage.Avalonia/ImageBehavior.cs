using Avalonia;
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AnimatedImage.Avalonia
{
    public class ImageBehavior
    {
        public static readonly AttachedProperty<Uri> AnimatedSourceProperty =
            AvaloniaProperty.RegisterAttached<ImageBehavior, Image, Uri>("AnimatedSource");

        public static readonly AttachedProperty<double> SpeedRatioProperty =
            AvaloniaProperty.RegisterAttached<ImageBehavior, Image, double>("SpeedRatio", 1d);

        public static readonly AttachedProperty<RepeatBehavior> RepeatBehaviorProperty =
            AvaloniaProperty.RegisterAttached<ImageBehavior, Image, RepeatBehavior>("RepeatBehavior", RepeatBehavior.Default);


        static ImageBehavior()
        {
            AnimatedSourceProperty.Changed.Subscribe(Observer.Create<Uri>(HandleAnimatedSourceChanged));
            SpeedRatioProperty.Changed.Subscribe(Observer.Create<double>(HandleSpeedRatioChanged));
            RepeatBehaviorProperty.Changed.Subscribe(Observer.Create<RepeatBehavior>(HandleRepeatBehavior));
        }

        private static void HandleAnimatedSourceChanged(AvaloniaObject element, Uri? animatedSource)
        {
            if (element is Image image)
            {
                foreach (var oldStyle in image.Styles.OfType<AnimationStyle>().ToArray())
                {
                    oldStyle.Dispose();
                }

                if (animatedSource is not null
                 && FrameRendererCreator.TryCreate(animatedSource, out var renderer))
                {
                    var behavior = GetRepeatBehavior(image);
                    var speedRatio = GetSpeedRatio(image);
                    AnimationStyle.Setup(image, speedRatio, behavior, renderer);
                }
            }
        }
        private static void HandleSpeedRatioChanged(AvaloniaObject element, double speedRatio)
        {
            if (element is Image image)
            {
                foreach (var oldStyle in image.Styles.OfType<AnimationStyle>().ToArray())
                {
                    oldStyle.SetSpeedRatio(speedRatio);
                }
            }
        }
        private static void HandleRepeatBehavior(AvaloniaObject element, RepeatBehavior behavior)
        {
            if (element is Image image)
            {
                foreach (var oldStyle in image.Styles.OfType<AnimationStyle>().ToArray())
                {
                    oldStyle.SetRepeatBehavior(behavior);
                }
            }
        }

        public static void SetAnimatedSource(AvaloniaObject obj, Uri uri)
            => ((Image)obj).SetValue(AnimatedSourceProperty, uri);

        public static void SetSpeedRatio(AvaloniaObject obj, double ratio)
            => ((Image)obj).SetValue(SpeedRatioProperty, ratio);

        public static void SetRepeatBehavior(AvaloniaObject obj, RepeatBehavior behavior)
            => ((Image)obj).SetValue(RepeatBehaviorProperty, behavior);

        public static Uri GetAnimatedSource(AvaloniaObject obj)
            => obj.GetValue(AnimatedSourceProperty);

        public static double GetSpeedRatio(AvaloniaObject obj)
            => obj.GetValue(SpeedRatioProperty);

        public static RepeatBehavior GetRepeatBehavior(AvaloniaObject obj)
            => obj.GetValue(RepeatBehaviorProperty);
    }
}
