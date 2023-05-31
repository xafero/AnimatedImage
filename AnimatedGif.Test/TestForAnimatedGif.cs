using System.IO;
using System.Reflection;
using System;
using ApprovalTests.Reporters;
using NUnit.Framework;
using ApprovalTests;
using WpfAnimatedGif.Formats;
using WpfAnimatedGif.Formats.Gif;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace AnimatedGif.Test
{
    [UseReporter(typeof(DiffReporter))]
    public class TestForAnimatedGif
    {
        [Test]
        [TestCase("earth.gif")]
        [TestCase("bomb.gif")]
        [TestCase("monster.gif")]
        [TestCase("working.gif")]
        [TestCase("step_all_background.gif")]
        [TestCase("step_all_none.gif")]
        [TestCase("step_all_previous.gif")]
        [TestCase("step_firstnone_laterback.gif")]
        [TestCase("step_firstnone_laterprev.gif")]
        [TestCase("step_jagging_back_prev.gif")]
        public void Sequence(string filename)
        {
            var imageStream = Open(filename);
            var giffile = GifFile.ReadGifFile(imageStream, false);
            var renderer = new GifRenderer(giffile);

            for (int i = 0; i < renderer.FrameCount; ++i)
            {
                renderer.ProcessFrame(i);

                var dirname = Path.GetFileNameWithoutExtension(filename);
                var framename = i.ToString("D2");

                Approvals.Verify(
                    new ApprovalImageWriter(renderer.Current, dirname, framename),
                    Approvals.GetDefaultNamer(),
                    new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
            }

        }

        [Test]
        [TestCase("earth.gif")]
        [TestCase("bomb.gif")]
        [TestCase("monster.gif")]
        [TestCase("working.gif")]
        [TestCase("step_all_background.gif")]
        [TestCase("step_all_none.gif")]
        [TestCase("step_all_previous.gif")]
        [TestCase("step_firstnone_laterback.gif")]
        [TestCase("step_firstnone_laterprev.gif")]
        [TestCase("step_jagging_back_prev.gif")]
        public void Jump(string filename)
        {
            var imageStream = Open(filename);
            var giffile = GifFile.ReadGifFile(imageStream, false);
            var renderer = new GifRenderer(giffile);

            var indics = new List<int>();


            foreach (var step in Enumerable.Range(1, renderer.FrameCount))
            {
                indics.Add(0);

                for (int start = 1; start < renderer.FrameCount; ++start)
                    for (int idx = start; idx < renderer.FrameCount; idx += step)
                        indics.Add(idx);
            }

            var prev = -1;
            foreach (int i in indics)
            {
                renderer.ProcessFrame(i);

                var dirname = Path.GetFileNameWithoutExtension(filename);
                var framename = i.ToString("D2");

                Debug.Print($"{prev} => {i}");

                prev = i;
                Approvals.Verify(
                    new ApprovalImageWriter(renderer.Current, dirname, framename),
                    Approvals.GetDefaultNamer(),
                    new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
            }
        }



        public static Stream Open(string imagefilename)
        {
            var path = $"AnimatedGif.Test.Inputs.{imagefilename}";

            return Assembly.GetCallingAssembly().GetManifestResourceStream(path)
                   ?? throw new ArgumentException($"image not found: '{imagefilename}'");
        }
    }
}