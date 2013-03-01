using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stegosaurus
{
    // Main application logic class. A singleton.
    public class Engine
    {
        public static string SourceMessageFileName { get; set; }
        public static string SourceVideoFileName { get; set; }
        public static string OutputMessageFileName { get; set; }
        public static string OutputVideoFileName { get; set; }
        public static string Key { get; set; }
        public static int LsbMode { get; set; }

        // TODO
        public static int BytePerFrame = 4096;

        // Precondition: all relevant properties have proper values.
        public static void EncryptAndSave()
        {
            FileStream messageInput = File.OpenRead(SourceMessageFileName);

            int len = Convert.ToInt32(messageInput.Length);
            int it = 0;
            byte[] bytes;

            Video video = new Video(SourceVideoFileName, OutputVideoFileName, len, ".pdf", LsbMode, Key);
            video.ResetWriteByte();
            
            while ((len - it) > BytePerFrame)
            {
                bytes = new byte[BytePerFrame];
                messageInput.Read(bytes, it, BytePerFrame);

                // encrypt bytes

                video.InsertToFrame(bytes);
                it += BytePerFrame;
            }

            bytes = new byte[len - it];
            messageInput.Read(bytes, it, len - it);
            video.InsertToFrame(bytes);

            video.CloseWriter();
            messageInput.Close();
        }

        // Precondition: all relevant properties have proper values.
        public static void DecryptAndSave()
        {
            FileStream messageOutput = File.OpenWrite(OutputMessageFileName);
            Video video = new Video(SourceVideoFileName, Key);

            int len = video.Length;
            byte[] bytes;

            video.ResetReadByte();

            while ((bytes = video.GetByteFromNextFrame()) != null)
            {
                // decrypt bytes

                for (int i = 0; i < bytes.Length; i++)
                {
                    messageOutput.WriteByte(bytes[i]);
                }
            }

            video.CloseReader();
            messageOutput.Close();
        }
    }
}
