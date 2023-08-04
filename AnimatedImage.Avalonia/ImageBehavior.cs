using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;
using System.Security.Cryptography.X509Certificates;
using Avalonia.Threading;
using System.Diagnostics;
using System.Threading;
using static System.Collections.Specialized.BitVector32;

namespace AnimatedImage.Avalonia
{
    public class ImageBehavior
    {
        public static readonly AttachedProperty<Uri> AnimatedSourceProperty =
            AvaloniaProperty.RegisterAttached<ImageBehavior, Image, Uri>("AnimatedSource");

        public static readonly AttachedProperty<double> SpeedRatioProperty =
            AvaloniaProperty.RegisterAttached<ImageBehavior, Image, double>("SpeedRatio", 1d);

        private static readonly AttachedProperty<int> FrameIndexProperty =
            AvaloniaProperty.RegisterAttached<ImageBehavior, Image, int>("FrameIndex");

        static ImageBehavior()
        {
            AnimatedSourceProperty.Changed.Subscribe(Observer.Create<Uri>(HandleAnimatedSourceChanged));
            SpeedRatioProperty.Changed.Subscribe(Observer.Create<double>(HandleSpeedRationChanged));
        }

        private static void HandleAnimatedSourceChanged(AvaloniaObject element, Uri? animatedSource)
        {
            if (element is Image image)
            {
                foreach (var oldStyle in image.Styles.OfType<AnimationStyle>().ToArray())
                {
                    image.Styles.Remove(oldStyle);
                    oldStyle.Dispose();
                }

                if (animatedSource is not null
                 && FrameRendererCreator.TryCreate(animatedSource, out var renderer))
                {
                    var animeStyle = new AnimationStyle(image, renderer);
                    animeStyle.SetSpeedRatio(GetSpeedRatio(image));
                    image.Styles.Add(animeStyle);
                }
            }
        }
        private static void HandleSpeedRationChanged(AvaloniaObject element, double speedRatio)
        {
            if (element is Image image)
            {
                foreach (var oldStyle in image.Styles.OfType<AnimationStyle>().ToArray())
                {
                    oldStyle.SetSpeedRatio(speedRatio);
                }
            }
        }

        public static void SetAnimatedSource(AvaloniaObject obj, Uri uri)
            => ((Image)obj).SetValue(AnimatedSourceProperty, uri);

        public static void SetSpeedRatio(AvaloniaObject obj, double ratio)
            => ((Image)obj).SetValue(SpeedRatioProperty, ratio);

        public static Uri GetAnimatedSource(AvaloniaObject obj)
            => obj.GetValue(AnimatedSourceProperty);

        public static double GetSpeedRatio(AvaloniaObject obj)
            => obj.GetValue(SpeedRatioProperty);

        class AnimationStyle : Style, IDisposable
        {
            private Animation? _animation;
            private readonly FrameRenderer _renderer;
            private Observable<IImage>? _source;
            private IDisposable? _disposable1;
            private IDisposable? _disposable2;
            private int _oldIndex = -1;

            public AnimationStyle(Image image, FrameRenderer renderer) : base(x => x.OfType<Image>())
            {
                _renderer = renderer;
                _source = new Observable<IImage>();

                _animation = new Animation()
                {
                    Duration = renderer.Duration,
                    IterationCount = renderer.RepeatCount == 0 ?
                                        new IterationCount(0ul, IterationType.Infinite) :
                                        new IterationCount((ulong)renderer.RepeatCount)
                };

                for (var i = 0; i < renderer.FrameCount; ++i)
                {
                    var keyframe = new KeyFrame() { KeyTime = renderer[i].Begin };
                    keyframe.Setters.Add(new Setter(FrameIndexProperty, i));

                    _animation.Children.Add(keyframe);
                }

                var lastKeyframe = new KeyFrame() { KeyTime = renderer.Duration };
                lastKeyframe.Setters.Add(new Setter(FrameIndexProperty, renderer.FrameCount - 1));
                _animation.Children.Add(lastKeyframe);

                Animations.Add(_animation);

                _disposable2 = image.Bind(Image.SourceProperty, _source);

                var observer = image.GetObservable(FrameIndexProperty);
                _disposable1 = observer.Subscribe(new Observer<int>(HandleFrame));
            }

            public void SetSpeedRatio(double ratio)
            {
                if (_animation is not null)
                    _animation.SpeedRatio = ratio;
            }

            public void Dispose()
            {
                if (_animation is not null)
                {
                    _animation.Children.Clear();
                    _animation.IterationCount = new IterationCount(0);

                    _animation = null;
                }

                if (_disposable1 is not null)
                {
                    _disposable1.Dispose();
                    _disposable1 = null;
                }

                if (_disposable2 is not null)
                {
                    _disposable2.Dispose();
                    _disposable2 = null;
                }

                if (_source is not null)
                {
                    _source.Dispose();
                    _source = null;
                }
            }

            private void HandleFrame(int frameIndex)
            {
                if (_source is null)
                    return;

                if (frameIndex >= _renderer.FrameCount)
                    _renderer.ProcessFrame(frameIndex % _renderer.FrameCount);
                else
                    _renderer.ProcessFrame(frameIndex);

                var face = (WriteableBitmapFace)_renderer.Current;

                if (_renderer.CurrentIndex != _oldIndex)
                {
                    _oldIndex = _renderer.CurrentIndex;
                    _source.OnNext(face.Bitmap);
                }
                else _source.OnNext(face.Current);
            }
        }

        class Observable<T> : IObservable<T>, IDisposable
        {
            private List<IObserver<T>> _observers = new();

            public IDisposable Subscribe(IObserver<T> observer)
            {
                _observers.Add(observer);
                return new Disposable(() => _observers.Remove(observer));
            }

            public void OnNext(T value)
            {
                foreach (var observer in _observers)
                {
                    observer.OnNext(value);
                }
            }

            public void Dispose()
            {
                _observers.Clear();
            }
        }

        class Disposable : IDisposable
        {
            private Action? _action;

            public Disposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                if (_action is not null)
                {
                    _action();
                    _action = null;
                }
            }
        }


        class Observer<T> : IObserver<T>
        {
            private Action<T> _onnext;

            public Observer(Action<T> onnext)
            {
                _onnext = onnext;
            }

            public void OnCompleted() { }

            public void OnError(Exception error) { }

            public void OnNext(T value) => _onnext(value);
        }

        static class Observer
        {
            public static Observer<AvaloniaPropertyChangedEventArgs<T>> Create<T>(Action<AvaloniaObject, T> onnext)
                => new Observer<AvaloniaPropertyChangedEventArgs<T>>(e => onnext(e.Sender, e.NewValue.Value));
        }
    }
}
