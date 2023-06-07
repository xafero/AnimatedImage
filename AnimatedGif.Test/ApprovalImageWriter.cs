using ApprovalTests.Core;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace AnimatedGif.Test
{
    public class ApprovalImageWriter : IApprovalWriter
    {
        public WriteableBitmap Data { get; set; }
        public string DirName { get; }
        public string FrameName { get; }

        public ApprovalImageWriter(WriteableBitmap image, string dirname, string framename)
        {
            Data = image ?? throw new ArgumentNullException(nameof(image));
            DirName = dirname;
            FrameName = framename;
        }

        public virtual string GetApprovalFilename(string basename)
        {
            var basepath = Path.GetDirectoryName(basename);
            return basepath is null ?
                        Path.Combine("Outputs", DirName, $"{DirName}#{FrameName}.approved.png") :
                        Path.Combine(basepath, "Outputs", DirName, $"{DirName}#{FrameName}.approved.png");
        }

        public virtual string GetReceivedFilename(string basename)
        {
            var basepath = Path.GetDirectoryName(basename);
            return basepath is null ?
                        Path.Combine("Outputs", DirName, $"{DirName}#{FrameName}.received.png") :
                        Path.Combine(basepath, "Outputs", DirName, $"{DirName}#{FrameName}.received.png");
        }

        public string WriteReceivedFile(string received)
        {
            var dir = Path.GetDirectoryName(received);
            if (dir is not null)
                Directory.CreateDirectory(dir);

            using var stream = new FileStream(received, FileMode.Create);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Data));
            encoder.Save(stream);

            return received;
        }
    }
}
