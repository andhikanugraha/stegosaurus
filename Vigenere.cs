using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stegosaurus
{
    class Vigenere
    {
        static int pos = 0;
        static String key = "";
        static char[] kA;

        public static void ResetPos()
        {
            pos = 0;
        }

        public static void LoadKey(String k)
        {
            key = k;
            kA = k.ToCharArray();
        }

        public static Byte[] EncryptBuffer(Byte[] buffer)
        {
            if (kA == null)
                return null;

            int len = buffer.Length;
            Byte[] encrypted = new Byte[len];

            for (int i = 0; i < len; i++)
            {
                // Load the byte
                byte b = buffer[i];

                // Shift the byte
                byte shift = Convert.ToByte(kA[pos]);

                // The shifted byte
                int e = b ^ shift;

                // Convert to byte
                byte bb = Convert.ToByte(e);

                // Save to encrypted byte[]
                encrypted[i] = bb;

                // increment the pos.
                ++pos;
                if (pos >= kA.Length)
                    pos = 0;
            }

            return encrypted;
        }

        public static Byte[] DecryptBuffer(Byte[] buffer)
        {
            return EncryptBuffer(buffer);
        }
    }
}
