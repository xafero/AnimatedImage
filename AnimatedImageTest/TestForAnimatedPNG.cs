using ApprovalTests.Reporters;
using ApprovalTests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AnimatedImage.Formats.Png;
using AnimatedImage.Formats;
using AnimatedImage.Wpf;
using System.Windows.Media.Imaging;
using AnimatedImage;

namespace AnimatedImageTest
{
    public class TestForAnimatedPNG
    {
        [Test]
        [TestCase("BouncingBeachBall.png")]
        [TestCase("GrayscaleBouncingBeachBall.png")]
        [TestCase("GrayscaleMatteBouncingBeachBall.png")]
        [TestCase("PaletteBouncingBeachBall.png")]
        [TestCase("NonAlphaBouncingBeachBall.png")]
        public void Sequence(string filename)
        {
            var imageStream = Open(filename);
            var pngfile = new ApngFile(imageStream);
            var renderer = new PngRenderer(pngfile, new WriteableBitmapFaceFactory());

            for (int i = 0; i < renderer.FrameCount; ++i)
            {
                renderer.ProcessFrame(i);

                var dirname = Path.GetFileNameWithoutExtension(filename);
                var framename = i.ToString("D2");

                Approvals.Verify(
                    new ApprovalImageWriter(ToBitmap(renderer.Current), dirname, framename),
                    Approvals.GetDefaultNamer(),
                    new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
            }

        }

        [Test]
        [TestCase("BouncingBeachBall.png")]
        public void Jump(string filename)
        {
            var imageStream = Open(filename);
            var pngfile = new ApngFile(imageStream);
            var renderer = new PngRenderer(pngfile, new WriteableBitmapFaceFactory());

            var indics = new List<int>();

            foreach (var step in Enumerable.Range(1, renderer.FrameCount))
            {
                indics.Add(0);

                for (int start = 1; start < renderer.FrameCount; ++start)
                    for (int idx = start; idx < renderer.FrameCount; idx += step)
                        indics.Add(idx);
            }

            foreach (int i in indics)
            {
                renderer.ProcessFrame(i);

                var dirname = Path.GetFileNameWithoutExtension(filename);
                var framename = i.ToString("D2");

                Approvals.Verify(
                    new ApprovalImageWriter(ToBitmap(renderer.Current), dirname, framename),
                    Approvals.GetDefaultNamer(),
                    new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
            }
        }

        private static WriteableBitmap ToBitmap(IBitmapFace face) => ((WriteableBitmapFace)face).Bitmap;

        public static Stream Open(string imagefilename)
        {
            var path = $"AnimatedImageTest.Inputs.{imagefilename}";

            return Assembly.GetCallingAssembly().GetManifestResourceStream(path)
                   ?? throw new ArgumentException($"image not found: '{imagefilename}'");
        }
    }
}
