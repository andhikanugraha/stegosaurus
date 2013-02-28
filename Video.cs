using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using AForge.Video;
using AForge.Video.VFW;

namespace Stegosaurus
{
    class Video
    {
        public int Length { get; private set; }
        public String Extension { get; private set; }
        public int BytePerFrame { get; private set; }
        public int LSB { get; private set; }

        private AVIReader reader;
        private AVIWriter writer;

        public Video(String input, String output, int length, int extension, int bytePerFrame, int LSB)
        {
            reader = new AVIReader();
            writer = new AVIWriter();

            reader.Open(input);

            Bitmap bitmap = reader.GetNextFrame();
        }

        public void InsertToFrame(byte[] insertedByte)
        {
            Bitmap bitmap = reader.GetNextFrame();
        }

        public byte[] GetByteFromFrame()
        {
            return null;
        }
    }
}
