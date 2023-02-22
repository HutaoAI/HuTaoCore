using HuTaoCore.Camera;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HuTaoCore.Camera
{
    public class HKCamera : ICamera
    {
        readonly Dictionary<string, Hikvision> CamereDic = new Dictionary<string, Hikvision>();

        public int Count => CamereDic.Count;

        public HKCamera(params string[] CamName)
        {
            foreach (var item in CamName)
            {
                if (!CamereDic.Keys.Contains(item))
                    CamereDic.Add(item, new Hikvision());
            }
        }

        public void ConnectCamera(string ID)
        {
            if (!CamereDic.Keys.Contains(ID))
                return;

            if (!CamereDic[ID].bl_CamOpen)
            {
                CamereDic[ID].connectCamera(ID);
                if (CamereDic[ID].bl_CamOpen)
                {
                    CamereDic[ID].setTriggerMode(1);
                    CamereDic[ID].setTriggerSource(7);
                    CamereDic[ID].startCamera();//开启相机采集
                }
            }
        }

        public void ConnectCameras()
        {
            foreach (var item in CamereDic.Keys)
            {
                if (!CamereDic[item].bl_CamOpen)
                    CamereDic[item].connectCamera(item);
                if (CamereDic[item].bl_CamOpen)
                {
                    CamereDic[item].setTriggerMode(1);
                    CamereDic[item].setTriggerSource(7);
                    CamereDic[item].startCamera();//开启相机采集
                }
            }
        }

        public void DisConnect(string CamID)
        {
            if (!CamereDic.Keys.Contains(CamID) | !CamereDic[CamID].bl_CamOpen)
                return;
            CamereDic[CamID].closeCamera();
        }

        public void DisConnectAll()
        {
            foreach (var item in CamereDic.Keys)
            {
                if (CamereDic[item].bl_CamOpen)
                {
                    CamereDic[item].closeCamera();
                }
            }
        }

        /// <summary>
        /// 获取图像
        /// </summary>
        /// <returns></returns>
        public async Task<Bitmap> GetImageAsync(string CamID, int exposure)
        {
            if (!CamereDic[CamID].bl_CamOpen)
                return null;
#pragma warning disable CA1416 // 验证平台兼容性
            Bitmap imgBytes = await Task.Run(async () =>
            {
                CamereDic[CamID].setExposureTime((uint)exposure);//设置曝光
                CamereDic[CamID].softTrigger();//发送软触发采集图像
                await Task.Delay(100);
                return (Bitmap)CamereDic[CamID].GetImage();
            });
#pragma warning restore CA1416 // 验证平台兼容性
            return imgBytes;
        }

        public Bitmap GetImage(string CamID, int exposure)
        {
            if (!CamereDic[CamID].bl_CamOpen)
                return null;

            CamereDic[CamID].setExposureTime((uint)exposure);//设置曝光
            CamereDic[CamID].softTrigger();//发送软触发采集图像
            Thread.Sleep(100);
            return (Bitmap)CamereDic[CamID].GetImage();
        }

        public bool IsConnected(string ID)
        {
            if (!CamereDic.Keys.Contains(ID))
                throw new Exception("没有找到相机ID");

            return CamereDic[ID].bl_CamOpen;
        }

        public void Add(string ID)
        {
            if (CamereDic.ContainsKey(ID))
                return;
            CamereDic.Add(ID,new Hikvision());
        }

        public void Clear()
        {
            CamereDic.Clear();
        }

        public bool Remove(string ID)
        {
            if(CamereDic.ContainsKey(ID))
                return CamereDic.Remove(ID);
            else
                return false;
        }
    }
}
