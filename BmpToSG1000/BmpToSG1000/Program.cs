using System;
using System.IO;
using System.Collections.Generic;

namespace BmpToSG1000
{
    class Program
    {
        const int color_max = 1;

        static void Main(string[] args)
        {
            int width  = 0;
            int height = 0;

            string fileName = "convert.bmp";
            if (args.Length > 0)
            {
                string[] pathName = args[0].Split('\\');
                fileName = pathName[pathName.Length-1];
            }
            //Console.WriteLine("fileName="+fileName);

            // ファイルサイズの取得
            FileInfo file = new FileInfo(fileName);
            long fileSize = file.Length;
            int file_end_address = (int)fileSize - 1;
            //Console.WriteLine("file_end_address=" + file_end_address);

            int[] ints = new int[fileSize];
            //List<byte> imageList = new List<byte>();
            //List<byte> maskList = new List<byte>();

            // 1バイトずつ読み出し。
            using (BinaryReader w = new BinaryReader(File.OpenRead(fileName)))
            {
                try
                {
                    for (int i = 0; i < fileSize; i++)
                    {
                        ints[i] = w.ReadByte();
                    }
                }
                catch (EndOfStreamException)
                {
                    Console.Write("\n");
                }
            }

            // ここでファイルサイズを取得する
            // 0x12 x                0x16 y
            width   = ints[0x12];
            width  += ints[0x13] * 256;
            height  = ints[0x16];
            height += ints[0x17] * 256;

            //Console.WriteLine("width=" + width);
            //Console.WriteLine("height=" + height);

            // 出力時のサイズ
            int width_size = width / 8;
            int height_size = height;

            for (int color = 0; color < color_max; color++)
            {
                int[,] b = new int[height, width];
                int gp = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            int index = (file_end_address - y * width * 3 - x * 3 - l);
                            b[y, x] += ((ints[index]));
                        }
                        switch (color_max)
                        {
                            case 1:
                                b[y, x] = Reversal2(b[y, x]);
                                break;
                            case 2:
                                b[y, x] = Reversal3(b[y, x], color);
                                break;
                            case 3:
                                b[y, x] = Reversal4(b[y, x], color);
                                break;
                            case 4:
                                b[y, x] = Reversal5(b[y, x], color);
                                break;
                        }
                    }
                }


                int count = 0;

                Console.Write("const unsigned char "+fileName.Split('.')[0]+"TileData[] = {");
                for (int y = 0; y < height / 8; y++)
                {
                    for (int x = 0; x < width / 8; x++)
                    {
                        for (int yy = 0; yy < 8; yy++)
                        {
                            if (count % 8 == 0)
                            {
                                Console.Write(" ");
                            }

                            if (count % 32 == 0)
                            {
                                Console.WriteLine("");
                                Console.Write("\t");
                            }

                            gp = (byte)(
                                  b[y * 8 + yy, width - 1 - 7 - x * 8]
                                + b[y * 8 + yy, width - 1 - 6 - x * 8] * 0x02
                                + b[y * 8 + yy, width - 1 - 5 - x * 8] * 0x04
                                + b[y * 8 + yy, width - 1 - 4 - x * 8] * 0x08
                                + b[y * 8 + yy, width - 1 - 3 - x * 8] * 0x10
                                + b[y * 8 + yy, width - 1 - 2 - x * 8] * 0x20
                                + b[y * 8 + yy, width - 1 - 1 - x * 8] * 0x40
                                + b[y * 8 + yy, width - 1 - 0 - x * 8] * 0x80
                                );

                            /*
                            if (color == 0)
                            {
                                imageList.Add((byte)gp[i]);
                            }
                            else if (color == 1)
                            {
                                maskList.Add((byte)gp[i]);
                            }
                            */

                            Console.Write("0x" + gp.ToString("X2"));
                            //if (x == width / 8 - 1 )
                            //{
                            //    Console.WriteLine(",");
                            //}
                            //else if (x % 8 == 7)
                            //{
                            //    Console.WriteLine(",");
                            //    Console.Write("\t");
                            //}
                            //else
                            //{
                            Console.Write(",");
                            //}

                            count++;

                            //if (x == width_size - 1)
                            //{
                            //    break;
                            //}
                        }
                    }
                    //Console.WriteLine("");

                    //if (y == height_size - 1)
                    //{
                    //    break;
                    //}
                }
                Console.WriteLine("");
                Console.WriteLine("};");
                Console.WriteLine("#define "+fileName.Split('.')[0]+"TileDataSize " + count);
                Console.WriteLine("");
            }

            // ファイル書き込み
            /*
            using (Stream stream = File.OpenWrite("image.dat"))
            {
                // streamに書き込むためのBinaryWriterを作成
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < imageList.Count; i++)
                    {
                        writer.Write((byte)imageList[i]);
                    }
                }
            }
            */

            /*
            using (Stream stream = File.OpenWrite("mask.dat"))
            {
                // streamに書き込むためのBinaryWriterを作成
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < maskList.Count; i++)
                    {
                        writer.Write((byte)maskList[i]);
                    }
                }
            }
            */

            // タイルマップ（ダミー）
            const int mapCountStart = 1;   // １スタート
            int mapCount = mapCountStart;
            Console.Write("const unsigned char " + fileName.Split('.')[0] + "TileMapData[] = {");
            for (int y = 0; y < height / 8; y++)
            {
                for (int x = 0; x < width / 8; x++)
                {
                    if (mapCount % 8 == mapCountStart)
                    {
                        Console.Write(" ");
                    }

                    if (mapCount % 32 == mapCountStart)
                    {
                        Console.WriteLine("");
                        Console.Write("\t");
                    }

                    Console.Write("0x" + mapCount.ToString("X2") + ",");
                    mapCount++;
                }
            }
            Console.WriteLine("");
            Console.WriteLine("};");
            Console.WriteLine("#define " + fileName.Split('.')[0] + "TileMapDataSize " + (mapCount - mapCountStart));
            Console.WriteLine("#define " + fileName.Split('.')[0] + "TileMapDataWidth " + width / 8);
            Console.WriteLine("#define " + fileName.Split('.')[0] + "TileMapDataHeight " + height / 8);
            Console.WriteLine("");

            // 色（白黒固定）
            int colourCount = 0;
            Console.Write("const unsigned char " + fileName.Split('.')[0] + "ColourData[] = {");
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width / 8; x++)
                {
                    if (colourCount % 8 == 0)
                    {
                        Console.Write(" ");
                    }

                    if (colourCount % 32 == 0)
                    {
                        Console.WriteLine("");
                        Console.Write("\t");
                    }

                    // TODO 白黒固定
                    Console.Write("0xF1,");
                    colourCount++;
                }
            }
            Console.WriteLine("");
            Console.WriteLine("};");
            Console.WriteLine("#define " + fileName.Split('.')[0] + "ColourDataSize " + colourCount);
            Console.WriteLine("");

            System.Threading.Thread.Sleep(100000);
        }

        /*
         * Fと0が逆なので、逆にしている
         */
        private static int Reversal(int b)
        {
            return (int)((b + 1) % 2);
        }

        /*
         * 5階調
         * BMPは白が765 黒が0
         * SG1000は黒が1、白が0
         */
        private static int Reversal5(int b, int type)
        {
            switch (type)
            {
                case 0:
                    if (b < 616)
                    {
                        return 1;
                    }
                    break;
                case 1:
                    if (b < 308)
                    {
                        return 1;
                    }
                    break;
                case 2:
                    if (b < 462)
                    {
                        return 1;
                    }
                    break;
                case 3:
                    if (b < 154)
                    {
                        return 1;
                    }
                    break;
            }
            return 0;
        }

        /*
         * 4階調
         * BMPは白が765 黒が0
         * SG1000は黒が1、白が0
         */
        private static int Reversal4(int b, int type)
        {
            switch (type)
            {
                case 0:
                    if (b < 576)
                    {
                        return 1;
                    }
                    break;
                case 1:
                    if (b < 384)
                    {
                        return 1;
                    }
                    break;
                case 2:
                    if (b < 192)
                    {
                        return 1;
                    }
                    break;
            }
            return 0;
        }

        /*
         * 3階調
         * BMPは白が765 黒が0
         * SG1000は黒が1、白が0
         */
        private static int Reversal3(int b, int type)
        {
            switch (type)
            {
                case 0:
                    if (b < 100)
                    {
                        return 1;
                    }
                    break;
                case 1:
                    if (b < 700)
                    {
                        return 1;
                    }
                    break;
            }
            return 0;
        }

        /*
         * 2階調
         * BMPは白が765 黒が0
         * SG1000は黒が1、白が0
         */
        private static int Reversal2(int b)
        {
            if (b < 100)
            {
                return 1;
            }
            return 0;
        }
    }

}