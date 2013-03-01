using System;
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
            var videoHandler = new Video(SourceVideoFileName);
        }
    }
}
