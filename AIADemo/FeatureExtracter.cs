using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace AIADemo
{
    class FeatureExtracter
    {
        private string pathImgDB;
        private int BinPerImg;
        private int bins;
        private int NumPerQuery;

        public FeatureExtracter(string pathImgDB, int BinPerImg, int NumPerQuery )
        {
            this.pathImgDB = pathImgDB;
            this.BinPerImg = BinPerImg;
            this.NumPerQuery = NumPerQuery;

            this.bins = BinPerImg * BinPerImg * BinPerImg;
        }

        public bool extractImgs(string query)
        {

            StreamWriter fileFeature = null;
            Bitmap img = null;

            try
            {
                fileFeature = new StreamWriter(pathImgDB + "\\" + query + "\\" + "feature.dat");
                fileFeature.WriteLine("BINs:");
                fileFeature.WriteLine(bins);
                for (int i = 1; i <= NumPerQuery; i++)
                {
                    img = new Bitmap(pathImgDB + "\\" + query + "\\" + i + ".jpg");
                    float[] feat = getRGBFeature(img);

                    fileFeature.WriteLine(i);
                    for (int j = 0; j < bins; j++)
                    {
                        fileFeature.Write(feat[j]);
                        fileFeature.Write(' ');
                    }
                    fileFeature.Write('\n');
                }

                fileFeature.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public float[] getRGBFeature(Bitmap bmp)
        {
            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;
            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            //RGB histogram
            int r, g, b;
            float[] feat = new float[bins];
            for (int i = 0; i + 2 < rgbValues.Length; i += 3)
            {
                r = (rgbValues[i] * BinPerImg) / 256;
                g = (rgbValues[i + 1] * BinPerImg) / 256;
                b = (rgbValues[i + 2] * BinPerImg) / 256;
                feat[r * BinPerImg * BinPerImg + g * BinPerImg + b] += 1;
            }
            for (int i = 0; i < bins; i++)
                feat[i] /= rgbValues.Length;
            return feat;
        }
    }
}
