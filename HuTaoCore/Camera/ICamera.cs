using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectCamera
{
    public interface ICamera
    {
        void ConnectCamera(string ID);
        void ConnectCameras();
        void DisConnect(string ID);
        void DisConnectAll();
        Task<Bitmap> GetImageAsync(string CamID, int exposure);
        Bitmap GetImage(string CamID, int exposure);
        bool IsConnected(string ID);
    }
}
