using System;
using System.Collections.Generic;
using System.Linq;
using AnimatedImage.Formats.Gif;

namespace AnimatedImage.Formats
{
    internal class GifRenderer : FrameRenderer
    {
        private IBitmapFaceFactory _factory;
        private int _frameIndex = -1;
        private readonly IBitmapFace _bitmap;
        private readonly GifRendererFrame[] _frames;

        private readonly byte[] _work;

        // variables for RestorePrevious
        private readonly byte[] _restorePixels;
        private GifRendererFrame? _previouns;

        // variables for RestoreBackground
        private FrameRenderFrame? _background;
        private readonly FrameRenderFrame _fullFrame;

        public GifRenderer(GifFile file, IBitmapFaceFactory factory)
        {
            var descriptor = file.Header.LogicalScreenDescriptor;
            Width = descriptor.Width;
            Height = descriptor.Height;

            _factory = factory;
            _fullFrame = _background = new FrameRenderFrame(0, 0, Width, Height, TimeSpan.Zero, TimeSpan.Zero);

            _bitmap = _factory.Create(Width, Height);
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

            _frames = frames.ToArray();

            Duration = span;
            FrameCount = file.Frames.Count;
            RepeatCount = file.RepeatCount;
        }

        private GifRenderer(GifRenderer renderer)
        {
            Width = renderer.Width;
            Height = renderer.Height;

            _factory = renderer._factory;
            _fullFrame = _background = new FrameRenderFrame(0, 0, Width, Height, TimeSpan.Zero, TimeSpan.Zero);
            _bitmap = _factory.Create(Width, Height);
            _restorePixels = new byte[Width * Height * 4];
            _work = new byte[Width * Height * 4];
            _frames = renderer._frames.ToArray();

            Duration = renderer.Duration;
            FrameCount = renderer.FrameCount;
            RepeatCount = renderer.RepeatCount;
        }

        public int Width { get; }

        public int Height { get; }

        public override int CurrentIndex => _frameIndex;

        public override int FrameCount { get; }

        public override IBitmapFace Current => _bitmap;

        public override TimeSpan Duration { get; }

        public override int RepeatCount { get; }

        public override FrameRenderFrame this[int idx] => _frames[idx];

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
                _bitmap.WriteBGRA(_restorePixels, _previouns.X, _previouns.Y, _previouns.Width, _previouns.Height);
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

        public override FrameRenderer Clone() => new GifRenderer(this);

        private static void Clear(IBitmapFace bitmap, int x, int y, int width, int height)
        {
            bitmap.WriteBGRA(new byte[width * height * 4], x, y, width, height);
        }
    }

    internal class GifRendererFrame : FrameRenderFrame
    {
        public FrameDisposalMethod DisposalMethod { get; }

        private readonly GifColor[] _colorTable;
        private readonly GifImageData _data;
        private readonly int _transparencyIndex;
        private byte[]? _decompress;
        private readonly bool _interlace;

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
            _interlace = frame.Descriptor.Interlace;

            DisposalMethod = method;
        }

        public void Render(IBitmapFace bitmap, byte[] work, byte[]? backup)
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

            bitmap.ReadBGRA(work, X, Y, Width, Height);

            if (backup != null)
            {
                Array.Copy(work, backup, Width * Height * 4);
            }

            if (_interlace)
            {
                int i = 0;
                i += RenderInterlace(work, i, 0, 8);
                i += RenderInterlace(work, i, 4, 8);
                i += RenderInterlace(work, i, 2, 4);
                i += RenderInterlace(work, i, 1, 2);
            }
            else
            {
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
            }

            bitmap.WriteBGRA(work, X, Y, Width, Height);
        }

        private int RenderInterlace(byte[] work, int startLine, int start, int stepLine)
        {
            if (_decompress == null) throw new InvalidOperationException();

            int i = 0;
            for (int y = start; y < Height; y += stepLine)
            {
                for (int x = 0; x < Width; x++)
                {
                    var pos = y * Width + x;

                    var idx = _decompress[startLine + i];
                    i++;

                    if (idx == _transparencyIndex)
                    {
                        continue;
                    }

                    var color = _colorTable[idx];
                    work[4 * pos + 0] = color.B;
                    work[4 * pos + 1] = color.G;
                    work[4 * pos + 2] = color.R;
                    work[4 * pos + 3] = 255;
                }
            }
            return i;
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
