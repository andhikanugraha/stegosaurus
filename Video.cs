using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
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
        public int Password { get; private set; }

        private AVIReader reader;
        private AVIWriter writer;
        private bool[] RandUsed;
        private int CurrentLength;
        private int RandCounter;
        private static String HeaderFile = "header.txt";

        public Video(String input, String password)
        {
            Password = ToPasswordSeed(password);

            reader = new AVIReader();

            reader.Open(input);
            ResetReadByte();

            ExtractHeader();
        }

        public Video(String input, String output, int length, String extension, int bytePerFrame, int LSB, String password)
        {
            Length = length;
            Extension = extension;
            BytePerFrame = bytePerFrame;
            this.LSB = LSB;
            Password = ToPasswordSeed(password);

            reader = new AVIReader();
            writer = new AVIWriter();

            reader.Open(input);
            writer.Open(output, reader.Width, reader.Height);
            ResetWriteByte();

            HideHeader();
        }

        public void InsertToFrame(byte[] insertedByte)
        {
            Bitmap bitmap = reader.GetNextFrame();
            ResetRandIndex();
            int pos = RandIndex();
            for (int i = 0; i < (insertedByte.Length); i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int x = pos / bitmap.Size.Width;
                    int y = pos % bitmap.Size.Width;

                    byte r = bitmap.GetPixel(x, y).R;
                    byte g = bitmap.GetPixel(x, y).G;
                    byte b = bitmap.GetPixel(x, y).B;

                    if (LSB == 1)
                    {
                        int byteAtPos = (insertedByte[i] / (1 << (j * 3))) % 2;
                        r = (byte)ToByte(r, byteAtPos, 1);

                        byteAtPos = (insertedByte[i] / (1 << (j * 3 + 1))) % 2;
                        g = (byte)ToByte(g, byteAtPos, 1);

                        if (j < 2)
                        {
                            byteAtPos = (insertedByte[i] / (1 << (j * 3 + 2))) % 2;
                            b = (byte)ToByte(b, byteAtPos, 1);
                        }
                    }
                    else if (LSB == 2)
                    {
                        int byteAtPos;
                        if (j < 2)
                        {
                            byteAtPos = ((insertedByte[i] / (1 << (j * 6 + 1))) % 2) * 2 +
                                ((insertedByte[i] / (1 << (j * 6))) % 2);
                            r = (byte)ToByte(r, byteAtPos, 2);
                        }

                        if (j == 0)
                        {
                            byteAtPos = ((insertedByte[i] / (1 << (j * 6 + 3))) % 2) * 2 +
                                ((insertedByte[i] / (1 << (j * 6 + 2))) % 2);
                            g = (byte)ToByte(g, byteAtPos, 2);

                            byteAtPos = ((insertedByte[i] / (1 << (j * 6 + 5))) % 2) * 2 +
                                ((insertedByte[i] / (1 << (j * 6 + 4))) % 2);
                            b = (byte)ToByte(b, byteAtPos, 2);
                        }
                    }

                    bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                    pos = RandIndex();
                }
            }
            writer.AddFrame(bitmap);
        }

        public byte[] GetByteFromNextFrame()
        {
            Bitmap bitmap = reader.GetNextFrame();
            int remaining = ((Length - CurrentLength) >  BytePerFrame) ? BytePerFrame : (Length - CurrentLength);
            byte[] ret = new byte[remaining];

            ResetRandIndex();
            int pos = RandIndex();
            for (int i = 0; i < remaining; i++)
            {
                byte nowByte = 0;
                for (int j = 0; j < 3; j++)
                {
                    int x = pos / bitmap.Size.Width;
                    int y = pos % bitmap.Size.Width;

                    byte r = bitmap.GetPixel(x, y).R;
                    byte g = bitmap.GetPixel(x, y).G;
                    byte b = bitmap.GetPixel(x, y).B;

                    if (LSB == 1)
                    {
                        nowByte += (byte)((r % 2) * (1 << (j * 3)));
                        nowByte += (byte)((g % 2) * (1 << (j * 3 + 1)));
                        if (j < 2)
                        {
                            nowByte += (byte)((b % 2) * (1 << (j * 3 + 2)));
                        }
                    }
                    else if (LSB == 2)
                    {
                        if (j < 2)
                        {
                            nowByte += (byte)((r % 2) * (1 << (j * 6)));
                            nowByte += (byte)(((r / 2) % 2) * (1 << (j * 6 + 1)));
                        }
                        if (j < 1)
                        {
                            nowByte += (byte)((g % 2) * (1 << (j * 6 + 2)));
                            nowByte += (byte)(((g / 2) % 2) * (1 << (j * 6 + 3)));
                        }
                        if (j < 1)
                        {
                            nowByte += (byte)((b % 2) * (1 << (j * 6 + 4)));
                            nowByte += (byte)(((b / 2) % 2) * (1 << (j * 6 + 5)));
                        }
                    }

                    pos = RandIndex();
                }
                ret[i] = nowByte;
            }
            CurrentLength += BytePerFrame;
            return ret;
        }

        private void ExtractHeader()
        {
            Bitmap bitmap = reader.GetNextFrame();
            ResetRandIndex();
            int pos = RandIndex();
            int len = 0;

            // getting length header
            for (int i = 0; i < 4; i++)
            {
                int nowByte = 0;
                for (int j = 0; j < 3; j++)
                {
                    int x = pos / bitmap.Size.Width;
                    int y = pos % bitmap.Size.Width;

                    byte r = bitmap.GetPixel(x, y).R;
                    byte g = bitmap.GetPixel(x, y).G;
                    byte b = bitmap.GetPixel(x, y).B;

                    if (LSB == 1)
                    {
                        nowByte += (byte)((r % 2) * (1 << (j * 3)));
                        nowByte += (byte)((g % 2) * (1 << (j * 3 + 1)));
                        if (j < 2)
                        {
                            nowByte += (byte)((b % 2) * (1 << (j * 3 + 2)));
                        }
                    }
                    else if (LSB == 2)
                    {
                        if (j < 2)
                        {
                            nowByte += (byte)((r % 2) * (1 << (j * 6)));
                            nowByte += (byte)(((r / 2) % 2) * (1 << (j * 6 + 1)));
                        }
                        if (j < 1)
                        {
                            nowByte += (byte)((g % 2) * (1 << (j * 6 + 2)));
                            nowByte += (byte)(((g / 2) % 2) * (1 << (j * 6 + 3)));
                        }
                        if (j < 1)
                        {
                            nowByte += (byte)((b % 2) * (1 << (j * 6 + 4)));
                            nowByte += (byte)(((b / 2) % 2) * (1 << (j * 6 + 5)));
                        }
                    }
                    pos = RandIndex();
                }
                len += nowByte;
            }

            //getting header
            byte[] ret = new byte[len];
            for (int i = 0; i < len; i++)
            {
                byte nowByte = 0;
                for (int j = 0; j < 3; j++)
                {
                    int x = pos / bitmap.Size.Width;
                    int y = pos % bitmap.Size.Width;

                    byte r = bitmap.GetPixel(x, y).R;
                    byte g = bitmap.GetPixel(x, y).G;
                    byte b = bitmap.GetPixel(x, y).B;

                    if (LSB == 1)
                    {
                        nowByte += (byte)((r % 2) * (1 << (j * 3)));
                        nowByte += (byte)((g % 2) * (1 << (j * 3 + 1)));
                        if (j < 2)
                        {
                            nowByte += (byte)((b % 2) * (1 << (j * 3 + 2)));
                        }
                    }
                    else if (LSB == 2)
                    {
                        if (j < 2)
                        {
                            nowByte += (byte)((r % 2) * (1 << (j * 6)));
                            nowByte += (byte)(((r / 2) % 2) * (1 << (j * 6 + 1)));
                        }
                        if (j < 1)
                        {
                            nowByte += (byte)((g % 2) * (1 << (j * 6 + 2)));
                            nowByte += (byte)(((g / 2) % 2) * (1 << (j * 6 + 3)));
                        }
                        if (j < 1)
                        {
                            nowByte += (byte)((b % 2) * (1 << (j * 6 + 4)));
                            nowByte += (byte)(((b / 2) % 2) * (1 << (j * 6 + 5)));
                        }
                    }
                    pos = RandIndex();
                }
                ret[i] = nowByte;
            }

            // output to header file
            FileStream fs = File.OpenWrite(HeaderFile);
            fs.Write(ret, 0, len);
            fs.Close();

            // read from Header
            TextReader tr = new StreamReader(HeaderFile);
            Length = Convert.ToInt32(tr.ReadLine());
            Extension = tr.ReadLine();
            BytePerFrame = Convert.ToInt32(tr.ReadLine());
            LSB = Convert.ToInt32(tr.ReadLine());
            tr.Close();
        }

        private void HideHeader()
        {
            // create header file
            TextWriter tw = new StreamWriter(HeaderFile);
            tw.WriteLine(Length);
            tw.WriteLine(Extension);
            tw.WriteLine(BytePerFrame);
            tw.WriteLine(LSB);
            tw.Close();

            // get stream byte from header file
            FileStream fs = File.OpenRead(HeaderFile);
            byte[] temp = new byte[fs.Length];
            fs.Read(temp, 0, Convert.ToInt32(fs.Length));

            // add 4 byte before the real header
            byte[] bytes = new byte[fs.Length + 4];
            int pos = 0;
            for (int i = 0, k = 0; i < 4; i++)
            {
                bytes[pos++] = (byte) ((Convert.ToInt32(fs.Length) / (1 << k)) % (1 << 8));
                k += 8;
            }
            for (int i = 0; i < Convert.ToInt32(fs.Length); i++)
            {
                bytes[pos++] = temp[i];
            }

            fs.Close();
            InsertToFrame(bytes);
        }

        public void ResetReadByte()
        {
            CurrentLength = 0;
        }

        public void ResetWriteByte()
        {
            reader.Position = reader.Start;
        }

        public void CloseReader()
        {
            reader.Close();
        }

        public void CloseWriter()
        {
            InsertRemaining();
            writer.Close();
        }

        private int ToPasswordSeed(String password)
        {
            int ret = 0;
            for (int i = 0; i < password.Length; i++)
            {
                ret += (int) password[i];
            }
            return ret;
        }

        private void ResetRandIndex()
        {
            RandCounter = 0;
            RandUsed = new bool[8192];
            for (int i = 0; i < 8192; i++)
			{
                RandUsed[i] = false;
			}
        }

        private int RandIndex()
        {
            int r = (++RandCounter * Password) % 4093;
            while (RandUsed[r])
            {
                r++;
            }
            RandUsed[r] = true;
            return r;
        }

        private void InsertRemaining()
        {
            while (true)
            {
                Bitmap bitmap = null;
                try
                {
                    bitmap = reader.GetNextFrame();
                    writer.AddFrame(bitmap);
                }
                catch (Exception e)
                {
                    return;
                }
            }
        }

        private int ToByte(byte x, int byteAtPos, int len)
        {
            x -= (byte)(x % (1 << len));
            x += (byte)byteAtPos;
            return x;
        }
    }
}
