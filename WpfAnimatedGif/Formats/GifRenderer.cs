using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using WpfAnimatedGif.Formats.Gif;

namespace WpfAnimatedGif.Formats
{
    internal class GifRenderer : FrameRenderer
    {
        private GifFile _file;
        private int _frameIndex = -1;
        private WriteableBitmap _bitmap;
        private Frame[] _frames;

        private byte[] _work;

        // variables for RestorePrevious
        private byte[] _restorePixels;
        private Frame _previouns;

        // variables for RestoreBackground
        private Frame _background;


        public GifRenderer(GifFile file)
        {
            _file = file;

            var descriptor = file.Header.LogicalScreenDescriptor;
            Width = descriptor.Width;
            Height = descriptor.Height;

            _bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32, null);
            _restorePixels = new byte[Width * Height * 4];
            _work = new byte[Width * Height * 4];

            var frames = new List<Frame>();
            var span = TimeSpan.Zero;
            foreach (var giffrm in file.Frames)
            {
                var frame = new Frame(file, giffrm, span);
                span = frame.End;

                frames.Add(frame);
            }

            Duration = span;
            _frames = frames.ToArray();
        }

        public int Width { get; }

        public int Height { get; }

        public override int CurrentIndex => _frameIndex;

        public override int Count => _file.Frames.Count;

        public override WriteableBitmap Current => _bitmap;

        public override TimeSpan Duration { get; }

        public override int RepeatCount => _file.RepeatCount;

        public override void ProcessFrame(TimeSpan timespan)
        {
            while (timespan > Duration)
            {
                timespan -= Duration;
            }

            for (var frameIdx = 0; frameIdx < _frames.Length; ++frameIdx)
            {
                var frame = _frames[frameIdx];
                if (frame.Begin <= timespan && timespan < frame.End)
                {
                    ProcessFrame(frameIdx);
                    return;
                }
            }
        }

        public override void ProcessFrame(int frameIndex)
        {
            if (_frameIndex == frameIndex)
                return;

            if (_frameIndex > frameIndex)
            {
                Clear(_bitmap, 0, 0, Width, Height);
                _frameIndex = -1;
                _previouns = null;
                _background = null;
            }

            // restore

            if (_previouns != null)
            {
                var rect = new Int32Rect(_previouns.X, _previouns.Y, _previouns.Width, _previouns.Height);

                _bitmap.WritePixels(rect, _restorePixels, _previouns.Width * 4, 0);
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

        internal class Frame
        {
            public TimeSpan Begin { get; }
            public TimeSpan End { get; }
            public FrameDisposalMethod DisposalMethod { get; }
            public int X => _bounds.X;
            public int Y => _bounds.Y;
            public int Width => _bounds.Width;
            public int Height => _bounds.Height;

            private Int32Rect _bounds;
            private GifColor[] _colorTable;
            private GifImageData _data;
            private int _transparencyIndex;
            private byte[] _decompress;


            public Frame(GifFile file, GifFrame frame, TimeSpan begin)
            {
                Begin = begin;

                _bounds = new Int32Rect(
                            frame.Descriptor.Left,
                            frame.Descriptor.Top,
                            frame.Descriptor.Width,
                            frame.Descriptor.Height);

                _colorTable = frame.Descriptor.HasLocalColorTable ?
                                        frame.LocalColorTable :
                                        file.GlobalColorTable;
                _data = frame.ImageData;


                var gce = frame.Extensions
                               .OfType<GifGraphicControlExtension>()
                               .FirstOrDefault();

                if (gce is null)
                {
                    End = begin + TimeSpan.FromMilliseconds(100);
                    DisposalMethod = FrameDisposalMethod.None;
                    _transparencyIndex = -1;
                }
                else
                {
                    End = begin + TimeSpan.FromMilliseconds(gce.Delay == 0 ? 100 : gce.Delay);
                    DisposalMethod = (FrameDisposalMethod)gce.DisposalMethod;
                    _transparencyIndex = gce.HasTransparency ? gce.TransparencyIndex : -1;
                }
            }

            public bool IsInvolve(Frame frame)
            {
                return X <= frame.X
                    && Y <= frame.Y
                    && (frame.X + frame.Width) <= (X + Width)
                    && (frame.Y + frame.Height) <= (Y + Height);
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

                bitmap.CopyPixels(_bounds, work, 4 * _bounds.Width, 0);

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

                bitmap.WritePixels(_bounds, work, 4 * _bounds.Width, 0);
            }
        }
    }
}
