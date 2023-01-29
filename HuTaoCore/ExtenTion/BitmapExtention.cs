using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using static System.Net.Mime.MediaTypeNames;

namespace System.Drawing
{
    public static class BitmapExtention
    {
        public static HObject ToHObject(this Bitmap bmp)
        {
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed) //8位
            {
                Rectangle rect1 = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData srcBmpDatas = bmp.LockBits(rect1, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                HOperatorSet.GenImage1(out HObject hobj, "byte", bmp.Width, bmp.Height, srcBmpDatas.Scan0);
                bmp.UnlockBits(srcBmpDatas);
                return hobj;
            }
            //24位
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData srcBmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            HOperatorSet.GenImageInterleaved(out HObject image, srcBmpData.Scan0, "bgr", bmp.Width, bmp.Height, 0, "byte", 0, 0, 0, 0, -1, 0);
            bmp.UnlockBits(srcBmpData);
            return image;
        }
    }
}
