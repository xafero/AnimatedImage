using System;

using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AnimatedImage.Formats;

namespace AnimatedImage.Wpf
{
    internal class RendererAnimation : ObjectAnimationBase
    {
        private int _currentIndex = 0;
        private FrameRenderer _renderer;

        public event Action? CurrentIndexUpdated;

        public int CurrentIndex => _currentIndex;
        public int Count => _renderer.FrameCount;

        public RendererAnimation(FrameRenderer renderer)
        {
            _renderer = renderer;
            Duration = renderer.Duration;
        }

        public WriteableBitmap CreateFirst()
        {
            _renderer.ProcessFrame(0);
            return Current;
        }

        protected override Freezable CreateInstanceCore()
            => new RendererAnimation(_renderer);

        protected override object GetCurrentValueCore(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            TimeSpan value = animationClock.CurrentTime.HasValue ? animationClock.CurrentTime.Value : TimeSpan.Zero;
            _renderer.ProcessFrame(value);

            if (_renderer.CurrentIndex != _currentIndex)
            {
                _currentIndex = _renderer.CurrentIndex;
                CurrentIndexUpdated?.Invoke();
            }

            return Current;
        }

        private WriteableBitmap Current => ((WriteableBitmapFace)_renderer.Current).Bitmap;

        public TimeSpan GetStartTime(int idx) => _renderer.GetStartTime(idx);
    }
}
