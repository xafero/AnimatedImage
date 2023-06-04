using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif.Formats.Gif;

namespace WpfAnimatedGif.Formats
{
    internal class GifRenderer : FrameRenderer
    {
        private GifFile _file;
        private int _frameIndex = -1;
        private WriteableBitmap _bitmap;
        private GifRendererFrame[] _frames;

        private byte[] _work;

        // variables for RestorePrevious
        private byte[] _restorePixels;
        private GifRendererFrame _previouns;

        // variables for RestoreBackground
        private FrameRenderFrame _background;
        private readonly FrameRenderFrame _fullFrame;

        public GifRenderer(GifFile file)
        {
            _file = file;

            var descriptor = file.Header.LogicalScreenDescriptor;
            Width = descriptor.Width;
            Height = descriptor.Height;

            _fullFrame = _background = new FrameRenderFrame(0, 0, Width, Height, TimeSpan.Zero, TimeSpan.Zero);

            _bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32, null);
            _restorePixels = new byte[Width * Height * 4];
            _work = new byte[Width * Height * 4];

            var frames = new List<GifRendererFrame>();
            var span = TimeSpan.Zero;
            foreach (var giffrm in file.Frames)
            {
                var frame = GifRendererFrame.Create(file, giffrm, span);
                span = frame.End;

                frames.Add(frame);
            }

            Duration = span;
            _frames = frames.ToArray();
        }

        public int Width { get; }

        public int Height { get; }

        public override int CurrentIndex => _frameIndex;

        public override int FrameCount => _file.Frames.Count;

        public override WriteableBitmap Current => _bitmap;

        public override TimeSpan Duration { get; }

        public override int RepeatCount => _file.RepeatCount;

        protected override FrameRenderFrame this[int idx] => _frames[idx];

        public override void ProcessFrame(int frameIndex)
        {
            if (_frameIndex == frameIndex)
                return;

            if (_frameIndex > frameIndex)
            {
                _frameIndex = -1;
                _previouns = null;
                _background = _fullFrame;
            }

            // restore

            if (_previouns != null)
            {
                _bitmap.WritePixels(_previouns.Bounds, _restorePixels, _previouns.Width * 4, 0);
                _previouns = null;
            }

            if (_background != null)
            {
                Clear(_bitmap, _background.X, _background.Y, _background.Width, _background.Height);
            }


            // render intermediate frames

            for (var fidx = _frameIndex + 1; fidx < frameIndex; ++fidx)
            {
                var prevFrame = _frames[fidx];

                if (prevFrame.DisposalMethod == FrameDisposalMethod.RestorePrevious)
                {
                    _background = null;
                    continue;
                }

                if (prevFrame.DisposalMethod == FrameDisposalMethod.RestoreBackground)
                {
                    // skips clear because the cleaning area is already cleared by previous frame.
                    if (_background != null && _background.IsInvolve(prevFrame))
                        continue;

                    Clear(_bitmap, prevFrame.X, prevFrame.Y, prevFrame.Width, prevFrame.Height);
                    if (_background is null || prevFrame.IsInvolve(_background))
                    {
                        _background = prevFrame;
                    }

                    continue;
                }

                prevFrame.Render(_bitmap, _work, null);
                _background = null;
            }


            // render current frame

            var curFrame = _frames[frameIndex];

            switch (curFrame.DisposalMethod)
            {
                case FrameDisposalMethod.RestorePrevious:
                    curFrame.Render(_bitmap, _work, _restorePixels);
                    _background = null;
                    _previouns = curFrame;
                    break;

                case FrameDisposalMethod.RestoreBackground:
                    curFrame.Render(_bitmap, _work, null);
                    _background = curFrame;
                    _previouns = null;
                    break;

                default:
                    curFrame.Render(_bitmap, _work, null);
                    _background = null;
                    _previouns = null;
                    break;
            }

            _frameIndex = frameIndex;
        }

        public override TimeSpan GetStartTime(int idx) => _frames[idx].Begin;

        public override FrameRenderer Clone() => new GifRenderer(_file);

        private static void Clear(WriteableBitmap bitmap, int x, int y, int width, int height)
        {
            var bounds = new Int32Rect(x, y, width, height);
            bitmap.WritePixels(
                        bounds,
                        new byte[width * height * 4],
                        width * 4,
                        0);
        }
    }

    internal class GifRendererFrame : FrameRenderFrame
    {
        public FrameDisposalMethod DisposalMethod { get; }

        private GifColor[] _colorTable;
        private GifImageData _data;
        private int _transparencyIndex;
        private byte[] _decompress;

        public GifRendererFrame(
                GifFile file, GifFrame frame,
                TimeSpan begin, TimeSpan end,
                FrameDisposalMethod method,
                int transparencyIndex)
            : base(frame.Descriptor.Left, frame.Descriptor.Top,
                   frame.Descriptor.Width, frame.Descriptor.Height,
                   begin, end)
        {
            _colorTable = frame.LocalColorTable
                       ?? file.GlobalColorTable
                       ?? throw new FormatException("ColorTable not found");
            _data = frame.ImageData;
            _transparencyIndex = transparencyIndex;

            DisposalMethod = method;
        }

        public void Render(WriteableBitmap bitmap, byte[] work, byte[] backup)
        {
            if (_decompress is null)
            {
                _decompress = _data.Decompress();

                for (var i = 0; i < _decompress.Length; ++i)
                {
                    if (_decompress[i] >= _colorTable.Length)
                        _decompress[i] = 0;
                }
            }

            bitmap.CopyPixels(Bounds, work, 4 * Width, 0);

            if (backup != null)
            {
                Array.Copy(work, backup, Width * Height * 4);
            }

            int j = 0;
            for (var i = 0; i < _decompress.Length; ++i)
            {
                var idx = _decompress[i];

                if (idx == _transparencyIndex)
                {
                    j += 4;
                    continue;
                }

                var color = _colorTable[idx];
                work[j++] = color.B;
                work[j++] = color.G;
                work[j++] = color.R;
                work[j++] = 255;
            }

            bitmap.WritePixels(Bounds, work, 4 * Width, 0);
        }

        public static GifRendererFrame Create(GifFile file, GifFrame frame, TimeSpan begin)
        {
            var gce = frame.Extensions
                           .OfType<GifGraphicControlExtension>()
                           .FirstOrDefault();

            TimeSpan end;
            FrameDisposalMethod method;
            int transparencyIndex;

            if (gce is null)
            {
                end = begin + TimeSpan.FromMilliseconds(100);
                method = FrameDisposalMethod.None;
                transparencyIndex = -1;
            }
            else
            {
                end = begin + TimeSpan.FromMilliseconds(gce.Delay == 0 ? 100 : gce.Delay);
                method = (FrameDisposalMethod)gce.DisposalMethod;
                transparencyIndex = gce.HasTransparency ? gce.TransparencyIndex : -1;
            }

            return new GifRendererFrame(file, frame, begin, end, method, transparencyIndex);
        }
    }
}
