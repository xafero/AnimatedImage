using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif.Formats.Png;
using WpfAnimatedGif.Formats.Png.Chunks;
using WpfAnimatedGif.Formats.Png.Types;

namespace WpfAnimatedGif.Formats
{
    internal class PngRenderer : FrameRenderer
    {
        private ApngFile _file;
        private int _frameIndex = -1;
        private readonly WriteableBitmap _bitmap;
        private readonly PngRendererFrame[] _frames;

        private readonly byte[] _work;

        // variables for RestorePrevious
        private readonly byte[] _restorePixels;
        private PngRendererFrame _previouns;

        // variables for RestoreBackground
        private FrameRenderFrame _background;

        public PngRenderer(ApngFile file)
        {
            _file = file;
            Width = file.IHDRChunk.Width;
            Height = file.IHDRChunk.Height;
            _bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32, null);
            _work = new byte[Width * Height * 4];

            var frames = new List<PngRendererFrame>();
            var span = TimeSpan.Zero;
            foreach (var pngfrm in file.Frames)
            {
                var frame = CreateFrame(file, pngfrm, span);
                span = frame.End;

                frames.Add(frame);
            }


            if (frames.Count == 0)
            {
                _frames = new[] { CreateFrame(file, file.DefaultImage, span) };
                Duration = _frames[0].End;
            }
            else
            {
                _frames = frames.ToArray();
                Duration = span;
            }

            static PngRendererFrame CreateFrame(ApngFile file, ApngFrame pngfrm, TimeSpan span)
            {
                return file.IHDRChunk.ColorType switch
                {
                    ColorType.Glayscale => new GrayscaleFrame(file, pngfrm, span),
                    ColorType.GlayscaleAlpha => new GrayscaleFrame(file, pngfrm, span),
                    ColorType.Color => new ColorFrame(file, pngfrm, span),
                    ColorType.ColorAlpha => new ColorFrame(file, pngfrm, span),
                    ColorType.IndexedColor => new IndexedFrame(file, pngfrm, span)
                };
            }
        }

        public int Width { get; }

        public int Height { get; }

        public override int CurrentIndex => _frameIndex;

        public override int FrameCount => _frames.Length;

        public override WriteableBitmap Current => _bitmap;

        public override int RepeatCount { get; }

        protected override FrameRenderFrame this[int idx] => _frames[idx];

        public override TimeSpan Duration { get; }

        public override FrameRenderer Clone()
        {
            return new PngRenderer(_file);
        }

        public override TimeSpan GetStartTime(int idx) => _frames[idx].Begin;

        public override void ProcessFrame(int frameIndex)
        {
            if (_frameIndex == frameIndex)
                return;

            // increment
            for (; ; )
            {
                var frm = _frames[frameIndex];
                if (frm.Begin == frm.End && frameIndex + 1 < _frames.Length)
                {
                    ++frameIndex;
                    continue;
                }
                break;
            }

            if (_frameIndex > frameIndex)
            {
                Clear(_bitmap, 0, 0, Width, Height);
                _frameIndex = 0;
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

            for (var fidx = Math.Max(_frameIndex, 0); fidx < frameIndex; ++fidx)
            {
                var prevFrame = _frames[fidx];

                if (prevFrame.DisposalMethod == DisposeOps.APNGDisposeOpPrevious)
                    continue;

                if (prevFrame.DisposalMethod == DisposeOps.APNGDisposeOpBackground)
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
            }


            // render current frame
            var curFrame = _frames[frameIndex];

            switch (curFrame.DisposalMethod)
            {
                case DisposeOps.APNGDisposeOpPrevious:
                    curFrame.Render(_bitmap, _work, _restorePixels);
                    _background = null;
                    _previouns = curFrame;
                    break;

                case DisposeOps.APNGDisposeOpBackground:
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

    internal abstract class PngRendererFrame : FrameRenderFrame
    {
        public DisposeOps DisposalMethod { get; }
        public BlendOps BlendMethod { get; }

        protected PngRendererFrame(ApngFile file, Png.ApngFrame frame, TimeSpan begin) :
            base(
                (int)Nvl(frame.fcTLChunk?.XOffset, 0u),
                (int)Nvl(frame.fcTLChunk?.YOffset, 0u),
                (int)Nvl(frame.fcTLChunk?.Width, (uint)file.IHDRChunk.Width),
                (int)Nvl(frame.fcTLChunk?.Height, (uint)file.IHDRChunk.Height),
                begin,
                begin + Nvl(frame.fcTLChunk?.ComputeDelay(), TimeSpan.FromMilliseconds(100)))
        {
            if (frame.fcTLChunk is null)
            {
                DisposalMethod = DisposeOps.APNGDisposeOpNone;
                BlendMethod = BlendOps.APNGBlendOpSource;
            }
            else
            {
                DisposalMethod = frame.fcTLChunk.DisposeOp;
                BlendMethod = frame.fcTLChunk.BlendOp;
            }
        }

        public abstract void Render(WriteableBitmap bitmap, byte[] work, byte[] backup);


        private static T Nvl<T>(T? v1, T v2) where T : struct => v1.HasValue ? v1.Value : v2;
    }

    internal class GrayscaleFrame : PngRendererFrame
    {
        private static readonly byte[] s_scle1 = { 0, 255 };
        private static readonly byte[] s_scle2 = Enumerable.Range(0, 4).Select(i => (byte)(i * 255 / 3)).ToArray();
        private static readonly byte[] s_scle4 = Enumerable.Range(0, 16).Select(i => (byte)(i * 255 / 15)).ToArray();
        private static readonly byte[] s_scle8 = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

        private readonly IDATStream _data;
        private readonly bool _hasAlpha;
        private readonly byte[] _alphaLevel;
        private readonly byte[] _line;
        private readonly byte[] _scale;

        public GrayscaleFrame(ApngFile file, Png.ApngFrame frame, TimeSpan begin) : base(file, frame, begin)
        {
            _data = new IDATStream(file, frame);

            if (file.tRNSChunk is null)
            {
                _alphaLevel = new byte[256];
                for (var i = 0; i < _alphaLevel.Length; ++i)
                    _alphaLevel[i] = 255;
            }
            else
            {
                var trns = (tRNSGrayscaleChunk)file.tRNSChunk;
                _alphaLevel = trns.AlphaForEachGrayLevel.Select(s => (byte)(s >> 8)).ToArray();
            }

            _line = new byte[_data.Stride];
            _hasAlpha = file.IHDRChunk.ColorType == ColorType.GlayscaleAlpha;

            _scale = file.IHDRChunk.BitDepth switch
            {
                1 => s_scle1,
                2 => s_scle2,
                4 => s_scle4,
                8 => s_scle8,
                16 => s_scle8,
                _ => throw new ArgumentException()
            };
        }

        public override void Render(WriteableBitmap bitmap, byte[] work, byte[] backup)
        {
            bitmap.CopyPixels(Bounds, work, 4 * Bounds.Width, 0);

            if (backup != null)
            {
                Array.Copy(work, backup, Width * Height * 4);
            }

            int workIdx = 0;
            for (var i = 0; i < Height; ++i)
            {
                _data.DecompressLine(_line, 0, _line.Length);

                int lineIdx = 0;
                int workEdIdx = workIdx + Width * 4;
                while (workIdx < workEdIdx)
                {
                    var val = _line[lineIdx++];
                    var alpha = _hasAlpha ? _line[lineIdx++] : _alphaLevel[val];

                    var scl = _scale[val];

                    if (BlendMethod == BlendOps.APNGBlendOpSource)
                    {
                        work[workIdx++] = scl;
                        work[workIdx++] = scl;
                        work[workIdx++] = scl;
                        work[workIdx++] = alpha;
                    }
                    else if (BlendMethod == BlendOps.APNGBlendOpOver)
                    {

                        if (alpha == 0)
                        {
                            workIdx += 4;
                            continue;
                        }
                        else if (alpha == 0xFF)
                        {
                            work[workIdx++] = scl;
                            work[workIdx++] = scl;
                            work[workIdx++] = scl;
                            work[workIdx++] = alpha;
                        }
                        else
                        {
                            var overVal = ComputeColorScale(alpha, scl, work[workIdx]); ;
                            work[workIdx++] = overVal;
                            work[workIdx++] = overVal;
                            work[workIdx++] = overVal;
                            work[workIdx] = ComputeAlphaScale(alpha, work[workIdx]);
                            ++workIdx;
                        }
                    }
                }
            }

            bitmap.WritePixels(Bounds, work, 4 * Bounds.Width, 0);

            _data.Reset();
        }

        static byte ComputeColorScale(byte sa, byte sv, byte dv)
        {
            var val = sa * sv + (255 - sa) * dv;
            val = (val * 2 + 255) / 255 / 2;
            return (byte)val;
        }

        static byte ComputeAlphaScale(byte sa, byte dv)
        {
            // work[workIdx] = (byte)(alpha + work[workIdx] * (255 - alpha) / 255);
            var val = ((255 - sa) * dv * 2 + 255) / 255 / 2;
            return (byte)(sa + val);
        }
    }

    internal class ColorFrame : PngRendererFrame
    {
        private readonly IDATStream _data;
        private readonly bool _hasAlpha;
        private readonly HashSet<PngColor> _transparencyColor;
        private readonly byte[] _line;

        public ColorFrame(ApngFile file, Png.ApngFrame frame, TimeSpan begin) : base(file, frame, begin)
        {
            _data = new IDATStream(file, frame);


            if (file.tRNSChunk is null)
            {
                _transparencyColor = new HashSet<PngColor>();
            }
            else
            {
                var trns = (tRNSColorChunk)file.tRNSChunk;
                _transparencyColor = new HashSet<PngColor>(trns.TransparencyColors);
            }

            _line = new byte[_data.Stride];
            _hasAlpha = file.IHDRChunk.ColorType == ColorType.ColorAlpha;
        }

        public override void Render(WriteableBitmap bitmap, byte[] work, byte[] backup)
        {
            bitmap.CopyPixels(Bounds, work, 4 * Bounds.Width, 0);

            if (backup != null)
            {
                Array.Copy(work, backup, Width * Height * 4);
            }

            int workIdx = 0;
            for (var i = 0; i < Height; ++i)
            {
                _data.DecompressLine(_line, 0, _line.Length);

                int lineIdx = 0;
                int workEdIdx = workIdx + Width * 4;
                while (workIdx < workEdIdx)
                {
                    var r = _line[lineIdx++];
                    var g = _line[lineIdx++];
                    var b = _line[lineIdx++];
                    var alpha =
                        _hasAlpha ? _line[lineIdx++] :
                        _transparencyColor.Contains(new PngColor(r, g, b)) ? (byte)0 :
                        (byte)255;

                    if (BlendMethod == BlendOps.APNGBlendOpSource)
                    {
                        work[workIdx++] = b;
                        work[workIdx++] = g;
                        work[workIdx++] = r;
                        work[workIdx++] = alpha;
                    }
                    else if (BlendMethod == BlendOps.APNGBlendOpOver)
                    {
                        if (alpha == 0)
                        {
                            workIdx += 4;
                        }
                        else if (alpha == 0xFF)
                        {
                            work[workIdx++] = b;
                            work[workIdx++] = g;
                            work[workIdx++] = r;
                            work[workIdx++] = alpha;
                        }
                        else
                        {
                            work[workIdx] = ComputeColorScale(alpha, b, work[workIdx]); ++workIdx;
                            work[workIdx] = ComputeColorScale(alpha, g, work[workIdx]); ++workIdx;
                            work[workIdx] = ComputeColorScale(alpha, r, work[workIdx]); ++workIdx;
                            work[workIdx] = ComputeAlphaScale(alpha, work[workIdx]);
                            ++workIdx;
                        }
                    }
                }
            }

            bitmap.WritePixels(Bounds, work, 4 * Bounds.Width, 0);

            _data.Reset();
        }

        static byte ComputeColorScale(byte sa, byte sv, byte dv)
        {
            var val = sa * sv + (255 - sa) * dv;
            val = (val * 2 + 255) / 255 / 2;
            return (byte)val;
        }

        static byte ComputeAlphaScale(byte sa, byte dv)
        {
            // work[workIdx] = (byte)(alpha + work[workIdx] * (255 - alpha) / 255);
            var val = ((255 - sa) * dv * 2 + 255) / 255 / 2;
            return (byte)(sa + val);
        }
    }

    internal class IndexedFrame : PngRendererFrame
    {
        private IDATStream _data;
        private PngColor[] _palette;
        private byte[] _transparency;

        private byte[] _decompress;

        public IndexedFrame(ApngFile file, Png.ApngFrame frame, TimeSpan begin) : base(file, frame, begin)
        {
            _data = new IDATStream(file, frame);

            _palette = file.PLTEChunk.Colors;

            if (file.tRNSChunk is null)
            {
                _transparency = new byte[0];
            }
            else
            {
                var trns = (tRNSIndexChunk)file.tRNSChunk;
                _transparency = trns.AlphaForEachIndex;
            }

            if (_transparency.Length < _palette.Length)
            {
                _transparency = _transparency.Concat(Enumerable.Repeat((byte)0xFF, _palette.Length - _transparency.Length))
                                             .ToArray();
            }
        }

        public override void Render(WriteableBitmap bitmap, byte[] work, byte[] backup)
        {
            if (_decompress is null)
            {
                _decompress = new byte[_data.Stride * Height];

                var i = 0;
                while (i < _decompress.Length)
                {
                    _data.DecompressLine(_decompress, i, _data.Stride);
                    i += _data.Stride;
                }
            }

            bitmap.CopyPixels(Bounds, work, 4 * Bounds.Width, 0);
            if (backup != null)
            {
                Array.Copy(work, backup, Width * Height * 4);
            }

            if (BlendMethod == BlendOps.APNGBlendOpSource)
            {
                int j = 0;
                for (var i = 0; i < _decompress.Length; ++i)
                {
                    var idx = _decompress[i];
                    var color = _palette[idx];
                    work[j++] = color.B;
                    work[j++] = color.G;
                    work[j++] = color.R;
                    work[j++] = _transparency[idx];
                }
            }
            else if (BlendMethod == BlendOps.APNGBlendOpOver)
            {
                int j = 0;
                for (var i = 0; i < _decompress.Length; ++i)
                {
                    var idx = _decompress[i];
                    var alpha = _transparency[idx];

                    if (alpha == 0)
                    {
                        j += 4;
                        continue;
                    }

                    var color = _palette[idx];

                    if (alpha == 0xFF)
                    {
                        work[j++] = color.B;
                        work[j++] = color.G;
                        work[j++] = color.R;
                        work[j++] = alpha;
                    }
                    else
                    {
                        work[j] = (byte)((alpha * color.B + (1 - alpha) * work[j]) >> 8); ++j;
                        work[j] = (byte)((alpha * color.G + (1 - alpha) * work[j]) >> 8); ++j;
                        work[j] = (byte)((alpha * color.R + (1 - alpha) * work[j]) >> 8); ++j;
                        work[j] = alpha;
                        ++j;
                    }
                }
            }

            bitmap.WritePixels(Bounds, work, 4 * Bounds.Width, 0);
        }
    }
}
