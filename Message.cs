using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Stegosaurus
{
    class Message
    {
        public String fileName;
        public FileStream stream;

        // Returns true if success
        public bool OpenFileRead(String fn)
        {
            try
            {
                stream = File.OpenRead(fn);
            }
            catch (IOException e)
            {
                Debug.WriteLine(e);
                return false;
            }

            return true;
        }

        public bool OpenFileWrite(String fn)
        {
            try
            {
                stream = File.OpenWrite(fn);
            }
            catch (IOException e)
            {
                Debug.WriteLine(e);
                return false;
            }

            return true;
        }

        public byte[] ReadEncryptedBytes(Int32 offset, Int32 count)
        {
            byte[] originalBytes = new byte[count];
            stream.Read(originalBytes, offset, count);

            byte[] encryptedBytes = Vigenere.EncryptBuffer(originalBytes);

            return encryptedBytes;
        }
    }
}
