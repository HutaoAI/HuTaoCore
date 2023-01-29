using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HalconDotNet
{
    public static class HalconExtention
    {
        public static Bitmap ToBitmap(this HObject ho_image)
        {
            HOperatorSet.CountChannels(ho_image, out HTuple ht);
            if (ht.I == 1)
                return HObjectToBitmap_8(ho_image);

            Bitmap res24;
            HTuple width0, height0, type, width, height;
            //获取图像尺寸
            HOperatorSet.GetImageSize(ho_image, out width0, out height0);
            //创建交错格式图像
            HOperatorSet.InterleaveChannels(ho_image, out HObject InterImage, "argb", "match", 255);  //"rgb", 4 * width0, 0     "argb", "match", 255

            //获取交错格式图像指针
            HOperatorSet.GetImagePointer1(InterImage, out HTuple Pointer, out type, out width, out height);
            IntPtr ptr = Pointer;
            //构建新Bitmap图像
            Bitmap res32 = new Bitmap(width / 4, height, width, PixelFormat.Format32bppArgb, ptr);  // Format32bppArgb     Format24bppRgb

            //32位Bitmap转24位
            res24 = new Bitmap(res32.Width, res32.Height, PixelFormat.Format24bppRgb);
            Graphics graphics = Graphics.FromImage(res24);
            graphics.DrawImage(res32, new Rectangle(0, 0, res32.Width, res32.Height));
            res32.Dispose();
            return res24;
        }

        static Bitmap HObjectToBitmap_8(HObject image)
        {
            HTuple hpoint, type, width, height;
            Bitmap res;
            const int Alpha = 255;
            HOperatorSet.GetImagePointer1(image, out hpoint, out type, out width, out height);

            res = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            ColorPalette pal = res.Palette;
            for (int i = 0; i <= 255; i++)
            {
                pal.Entries[i] = Color.FromArgb(Alpha, i, i, i);
            }
            res.Palette = pal;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bitmapData = res.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            int PixelSize = Bitmap.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
            IntPtr ptr1 = bitmapData.Scan0;
            IntPtr ptr2 = hpoint;
            int bytes = width * height;
            byte[] rgbvalues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr2, rgbvalues, 0, bytes);
            System.Runtime.InteropServices.Marshal.Copy(rgbvalues, 0, ptr1, bytes);
            res.UnlockBits(bitmapData);
            return res;
        }

        public static HObject CropImage(this HObject ho_image,HTuple row1, HTuple column1, HTuple row2, HTuple column2)
        {
            return Crop_Image(ho_image,row1, column1, row2 - row1, column2 - column1);
        }
        public static HObject Crop_Image(this HObject ho_image,HTuple row1, HTuple column1, HTuple Width, HTuple Height)
        {
            HOperatorSet.CropPart(ho_image, out HObject NewHobj, row1, column1, Width, Height);
            return NewHobj;
        }
        /// <summary>
        /// 围绕图像中心旋转
        /// </summary>
        /// <param name="hobj"></param>
        /// <param name="Angle"></param>
        /// <returns></returns>
        public static HObject RotateImage(this HObject hobj,double Angle)
        {
            HOperatorSet.RotateImage(hobj,out HObject Newhobj, Angle,"constant");
            return Newhobj;
        }
    }
}
