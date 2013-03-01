using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
        public static double PNSR { get; set; }
        public static int LsbMode { get; set; }

        // TODO
        public static int BytePerFrame = 1024;

        // Precondition: all relevant properties have proper values.
        public static void EncryptAndSave()
        {
            FileStream messageInput = File.OpenRead(SourceMessageFileName);

            int len = Convert.ToInt32(messageInput.Length);
            int it = 0;
            byte[] bytes;

            Video video = new Video(SourceVideoFileName, OutputVideoFileName, len, ".pdf", LsbMode, Key);
            video.ResetWriteByte();

            Console.WriteLine(len);
            
            while ((len - it) > BytePerFrame)
            {
                bytes = new byte[BytePerFrame];
                for (int i = 0; i < BytePerFrame; i++)
                {
                    bytes[i] = (byte) messageInput.ReadByte();
                }

                // encrypt bytes

                video.InsertToFrame(bytes);
                it += BytePerFrame;
                Console.WriteLine("it={0}",it);
            }

            bytes = new byte[len - it];
            for (int i = 0; i < len - it; i++)
            {
                bytes[i] = (byte)messageInput.ReadByte();
            }
            // Vigenere
            bytes = Vigenere.EncryptBuffer(bytes);
            video.InsertToFrame(bytes);

            PNSR = video.PNSR;

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

                bytes = Vigenere.DecryptBuffer(bytes);
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
