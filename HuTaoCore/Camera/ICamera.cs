using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;

namespace HuTaoCore.Camera
{
    public interface ICamera
    {
        void ConnectCamera(string ID);
        void ConnectCameras();
        void DisConnect(string ID);
        void DisConnectAll();
        Task<Bitmap> GetImageAsync(string CamID, int exposure);
        Bitmap GetImage(string CamID, int exposure);
        HImage GetHImage(string CamID, int exposure);
        bool IsConnected(string ID);
    }
}
