using Avalonia.Animation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Linq;

namespace AnimatedImage.Avalonia
{
    internal class AnimationStyle : Style, IDisposable
    {
        private static readonly AttachedProperty<int> FrameIndexProperty =
            AvaloniaProperty.RegisterAttached<AnimationStyle, Image, int>("FrameIndex");

        private Image _image;
        private Animation? _animation;
        private readonly FrameRenderer _renderer;
        private Observable<IImage>? _source;
        private IDisposable? _disposable1;
        private IDisposable? _disposable2;
        private int _oldIndex = -1;

        private AnimationStyle(Image image, FrameRenderer renderer) : base(x => x.OfType<Image>())
        {
            _image = image;
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

            _image.Styles.Remove(this);
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

        public static void Setup(Image image, double speedRatio, FrameRenderer renderer)
        {
            var animeStyle = new AnimationStyle(image, renderer);
            animeStyle.SetSpeedRatio(speedRatio);
            image.Styles.Add(animeStyle);
        }
    }
}
