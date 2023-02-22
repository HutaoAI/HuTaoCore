using System.Drawing;
using System.Runtime.InteropServices;
using System.Net;
using System.Windows;
using System.Drawing.Imaging;
using MvCamCtrl.NET;
using System.Collections.Generic;
using System.IO;
using System;
using HalconDotNet;

namespace HuTaoCore.Camera
{
    [Serializable]
    /// <summary>
    /// 海康SDK
    /// </summary>
    class Hikvision
    {
        [NonSerialized]
        public MyCamera myCamera;//相机对象
        [NonSerialized]
        private MyCamera.MV_CC_DEVICE_INFO_LIST deviceList;//设备列表
        [NonSerialized]
        private MyCamera.MV_CC_DEVICE_INFO deviceInfo;//设备对象

        private string seriesStr;//接收相机序列号
        [NonSerialized]
        private MyCamera.MVCC_INTVALUE stParam;//用于接收特定的参数

        //为读取、保存图像创建的数组
        uint m_nBufSizeForDriver = 25 * 1024 * 1024;
        byte[] m_pBufForDriver = new byte[25 * 1024 * 1024];
        uint m_nBufSizeForSaveImage = 25 * 1024 * 1024 * 3 + 3000;
        byte[] m_pBufForSaveImage = new byte[25 * 1024 * 1024 * 3 + 3000];
        /// <summary>
        /// 相机是否连接成功
        /// </summary>TZ180XS-PM22H201*202206020177*16024 X3200402001[A]

        public bool bl_CamOpen = false;

        Bitmap bmp = null;

        //在构造函数中实例化设备列表对象
        public Hikvision()
        {
            deviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            //DeviceListAcq();
        }

        /// <summary>
        /// 相机在线列表，用于客户端配置相机ID
        /// </summary>
        public List<string> CameraId_list = new List<string>();


        #region 获取在线相机ID列表 方法
        public void DeviceListAcq()
        {
            int nRet;
            /*创建设备列表*/
            GC.Collect();
            CameraId_list.Clear();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref deviceList);
            if (0 != nRet)
            {

                return;
            }

            //在窗体列表中显示设备名
            for (int i = 0; i < deviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(deviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (gigeInfo.chUserDefinedName != "")
                    {
                        CameraId_list.Add(gigeInfo.chUserDefinedName);
                    }
                    else
                    {
                        CameraId_list.Add(gigeInfo.chSerialNumber);
                    }



                    //if (gigeInfo.chUserDefinedName != "")
                    //{
                    //    ////CameraId_list.Add("GigE: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")");
                    //    CameraId_list.Add(gigeInfo.chSerialNumber);
                    //}
                    //else
                    //{
                    //    ////CameraId_list.Add("GigE: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
                    //    CameraId_list.Add(gigeInfo.chSerialNumber);
                    //}

                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (usbInfo.chUserDefinedName != "")
                    {
                        CameraId_list.Add("USB: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        CameraId_list.Add("USB: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                    }
                }
            }
        }
        #endregion

        //成功返回0失败返回-1
        //调用函数时可以传入需要改变的目标IP，如过没有传入则将相机IP设置为其所连接的网卡地址+1或-1
        public int changeIP(string IP = "")
        {
            try
            {
                //获取相机相关信息，例如相机所连接网卡的网址
                IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(deviceInfo.SpecialInfo.stGigEInfo, 0);
                MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                IPAddress cameraIPAddress;
                string tempStr = "";
                if (IP.Trim().Equals("") || !IPAddress.TryParse(IP, out cameraIPAddress))
                {
                    //当前网卡的IP地址
                    uint nNetIp1 = (gigeInfo.nNetExport & 0xFF000000) >> 24;
                    uint nNetIp2 = (gigeInfo.nNetExport & 0x00FF0000) >> 16;
                    uint nNetIp3 = (gigeInfo.nNetExport & 0x0000FF00) >> 8;
                    uint nNetIp4 = gigeInfo.nNetExport & 0x000000FF;
                    //根据网卡IP设定相机IP，如果网卡ip第四位小于252，则相机ip第四位+1，否则相机IP第四位-1
                    uint cameraIp1 = nNetIp1;
                    uint cameraIp2 = nNetIp2;
                    uint cameraIp3 = nNetIp3;
                    uint cameraIp4 = nNetIp4;
                    if (nNetIp4 < 252)
                    {
                        cameraIp4++;
                    }
                    else
                    {
                        cameraIp4--;
                    }
                    tempStr = cameraIp1 + "." + cameraIp2 + "." + cameraIp3 + "." + cameraIp4;
                }
                else
                {
                    tempStr = IP;
                }
                IPAddress.TryParse(tempStr, out cameraIPAddress);
                long cameraIP = IPAddress.NetworkToHostOrder(cameraIPAddress.Address);
                //设置相机掩码
                uint maskIp1 = (gigeInfo.nCurrentSubNetMask & 0xFF000000) >> 24;
                uint maskIp2 = (gigeInfo.nCurrentSubNetMask & 0x00FF0000) >> 16;
                uint maskIp3 = (gigeInfo.nCurrentSubNetMask & 0x0000FF00) >> 8;
                uint maskIp4 = gigeInfo.nCurrentSubNetMask & 0x000000FF;
                IPAddress subMaskAddress;
                tempStr = maskIp1 + "." + maskIp2 + "." + maskIp3 + "." + maskIp4;
                IPAddress.TryParse(tempStr, out subMaskAddress);
                long maskIP = IPAddress.NetworkToHostOrder(subMaskAddress.Address);
                //设置网关
                uint gateIp1 = (gigeInfo.nDefultGateWay & 0xFF000000) >> 24;
                uint gateIp2 = (gigeInfo.nDefultGateWay & 0x00FF0000) >> 16;
                uint gateIp3 = (gigeInfo.nDefultGateWay & 0x0000FF00) >> 8;
                uint gateIp4 = gigeInfo.nDefultGateWay & 0x000000FF;
                IPAddress gateAddress;
                tempStr = gateIp1 + "." + gateIp2 + "." + gateIp3 + "." + gateIp4;
                IPAddress.TryParse(tempStr, out gateAddress);
                long gateIP = IPAddress.NetworkToHostOrder(gateAddress.Address);

                int temp = myCamera.MV_GIGE_ForceIpEx_NET((uint)(cameraIP >> 32), (uint)(maskIP >> 32), (uint)(gateIP >> 32));//执行更改相机IP的命令
                if (temp == 0)
                    //强制IP成功
                    return 0;
                //强制IP失败
                return -1;
            }
            catch
            {
                return -1;
            }
        }



        public int connectCamera(string id)//连接相机，返回-1为失败，0为成功
        {
            seriesStr = id;
            string m_SerialNumber = "";//接收设备返回的序列号
            int temp;//接收命令执行结果
            myCamera = new MyCamera();
            try
            {
                temp = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref deviceList);//更新设备列表
                if (temp != 0)
                {
                    //设备更新成功接收命令的返回值为0，返回值不为0则为异常
                    bl_CamOpen = false;
                    return -1;
                }

                if (deviceList.nDeviceNum == 0)
                {
                    bl_CamOpen = false;
                    return -1;
                }
                //强制相机IP
                for (int i = 0; i < deviceList.nDeviceNum; i++)
                {

                    /*******该部分用于获取相机名称、序列号等，从而对指定的相机进行IP更改******/
                    //更改IP的函数中也有该部分，重叠部分程序可进行相应的简化，本文暂不做处理

                    deviceInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(deviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));//获取设备信息
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(deviceInfo.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                    /*****************************************************************/

                    m_SerialNumber = gigeInfo.chUserDefinedName;
                    if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        //判断是否为网口相机
                        if (seriesStr.Equals(m_SerialNumber))
                        {
                            //如果相机用户名正确则修改IP
                            temp = myCamera.MV_CC_CreateDevice_NET(ref deviceInfo);//更改IP前需要创建设备对象
                            changeIP();
                        }
                    }
                }


                //更改IP后需要重新刷新设备列表，否则打开相机时会报错
                temp = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref deviceList);//更新设备列表

                for (int i = 0; i < deviceList.nDeviceNum; i++)
                {
                    deviceInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(deviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));//获取设备
                    if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(deviceInfo.SpecialInfo.stGigEInfo, 0);
                        MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                        if (gigeInfo.chUserDefinedName != "")
                        {
                            m_SerialNumber = gigeInfo.chUserDefinedName;//获取用户名
                        }
                        else
                        {
                            m_SerialNumber = gigeInfo.chSerialNumber;//获取序列号
                        }

                    }
                    else if (deviceInfo.nTLayerType == MyCamera.MV_USB_DEVICE)
                    {
                        IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(deviceInfo.SpecialInfo.stUsb3VInfo, 0);
                        MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                        m_SerialNumber = usbInfo.chUserDefinedName;
                    }
                    if (seriesStr.Equals(m_SerialNumber))
                    {
                        temp = myCamera.MV_CC_CreateDevice_NET(ref deviceInfo);
                        if (MyCamera.MV_OK != temp)
                        {
                            //创建相机失败
                            bl_CamOpen = false;
                            return -1;
                        }
                        temp = myCamera.MV_CC_OpenDevice_NET();//
                        if (MyCamera.MV_OK != temp)
                        {
                            //打开相机失败
                            bl_CamOpen = false;
                            return -1;
                        }
                        bl_CamOpen = true;

                        //MyCamera.cbOutputExdelegate ImageCallback;
                        //// ch:注册回调函数 | en:Register image callback
                        //ImageCallback = new MyCamera.cbOutputExdelegate(ImageCallbackFunc);
                        //int nRet = myCamera.MV_CC_RegisterImageCallBackEx_NET(ImageCallback, IntPtr.Zero);
                        //if (MyCamera.MV_OK != nRet)
                        //{
                        //    Console.WriteLine("Register image callback failed!");
                        //    return 0;
                        //}
                        return 0;
                    }
                    continue;
                }
            }
            catch
            {
                bl_CamOpen = false;
                return -1;
            }
            bl_CamOpen = true;
            return 0;
        }

        
        public int startCamera()//相机开始采集，返回0为成功，-1为失败
        {
            int temp = myCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != temp)
                return -1;
            return 0;
        }

        public int stopCamera()//停止相机采集，返回0为成功，-1为失败
        {
            //int temp = myCamera.MV_CC_StopGrabbing_NET();
            //if (MyCamera.MV_OK != temp)
            //    return -1;
            return 0;
        }

        public int closeCamera()//关闭相机，返回0为成功，-1为失败
        {
            int temp = stopCamera();
            //停止相机采集
            if (MyCamera.MV_OK != temp)
                return -1;
            temp = myCamera.MV_CC_CloseDevice_NET();
            if (MyCamera.MV_OK != temp)
                return -1;
            temp = myCamera.MV_CC_DestroyDevice_NET();
            if (MyCamera.MV_OK != temp)
                return -1;
            return 0;
        }

        //发送成功返回0，失败返回-1
        public int softTrigger()
        {

            int temp = myCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (MyCamera.MV_OK != temp)
                return -1;
            return 0;
        }

        //1.设置Int型参数
        public int setWidth(uint width)//设置图像宽度，成功返回0失败返回-1
        {
            int temp = myCamera.MV_CC_SetIntValue_NET("Width", width);
            if (MyCamera.MV_OK != temp)
                return 0;
            return -1;
        }
        //2.设置枚举型参数
        public int setTriggerMode(uint TriggerMode)//设置触发事件，成功返回0失败返回-1
        {
            int temp = 0;
            //1:On 触发模式
            //0:Off 非触发模式
            int returnValue = myCamera.MV_CC_SetEnumValue_NET("TriggerMode", TriggerMode);
            myCamera.MV_CC_SetEnumValue_NET("TriggerSource", 7);

            if (MyCamera.MV_OK != temp)
                return 0;
            return -1;
        }


        //3.设置触发源参数
        public int setTriggerSource(uint TriggerSourceCode)//设置出发源，成功返回0失败返回-1
        {
            int temp = 0;
            //触发源选择:0 - Line0;
            //           1 - Line1;
            //           2 - Line2;
            //           3 - Line3;
            //           4 - Counter;
            //           7 - Software;
            int returnValue = myCamera.MV_CC_SetEnumValue_NET("TriggerSource", TriggerSourceCode);
            if (MyCamera.MV_OK != temp)
                return 0;
            return -1;
        }


        //3.设置浮点型型参数
        public int setExposureTime(uint ExposureTime)//设置曝光时间（us），成功返回0失败返回-1
        {
            if (bl_CamOpen)
            {
                int temp = 0;
                int returnValue = myCamera.MV_CC_SetFloatValue_NET("ExposureTime", ExposureTime);
                if (MyCamera.MV_OK != temp)
                    return 0;

            }

            return -1;

        }
        //4.判断是否为黑白图像
        private bool IsMonoData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;
                default:
                    return false;
            }
        }

        //5.设置心跳时间，成功返回0失败返回-1
        public int setHeartBeatTime(uint heartBeatTime)
        {
            //心跳时间最小为500
            uint tempTime = heartBeatTime > 500 ? heartBeatTime : 500;
            int temp = myCamera.MV_CC_SetIntValue_NET("GevHeartbeatTimeout", tempTime);
            if (MyCamera.MV_OK != temp)
                return 0;
            return -1;
        }
        public HImage GetHImage()
        {
            UInt32 nPayloadSize = 0;
            int temp = myCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
            if (MyCamera.MV_OK != temp)
            {
                return null;
            }
            nPayloadSize = stParam.nCurValue;
            if (nPayloadSize > m_nBufSizeForDriver)
            {
                m_nBufSizeForDriver = nPayloadSize;
                m_pBufForDriver = new byte[m_nBufSizeForDriver];
                m_nBufSizeForSaveImage = m_nBufSizeForDriver * 3 + 2048;
                m_pBufForSaveImage = new byte[m_nBufSizeForSaveImage];
            }
            IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForDriver, 0);
            MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
            temp = myCamera.MV_CC_GetOneFrameTimeout_NET(pData, m_nBufSizeForDriver, ref stFrameInfo, 1000);//获取一帧图像，超时时间设置为1000
            if (MyCamera.MV_OK != temp)
            {
                return null;
            }
            HImage image = new HImage();
            if (IsMonoData(stFrameInfo.enPixelType))//判断是否为黑白图像
            {
                //如果是黑白图像，则利用Halcon图像库中的GenImage1算子来构建图像
                image.GenImage1("byte", (int)stFrameInfo.nWidth, (int)stFrameInfo.nHeight, pData);
            }
            else
            {
                if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                {
                    //如果彩色图像是RGB8格式，则可以直接利用GenImageInterleaved算子来构建图像
                    image.GenImageInterleaved(pData, "rgb", (int)stFrameInfo.nWidth, (int)stFrameInfo.nHeight, 0, "byte", (int)stFrameInfo.nWidth, (int)stFrameInfo.nHeight, 0, 0, -1, 0);
                }
                else
                {
                    //如果彩色图像不是RGB8格式，则需要将图像格式转换为RGB8。
                    IntPtr pBufForSaveImage = IntPtr.Zero;
                    if (pBufForSaveImage == IntPtr.Zero)
                    {
                        pBufForSaveImage = Marshal.AllocHGlobal((int)(stFrameInfo.nWidth * stFrameInfo.nHeight * 3 + 2048));
                    }
                    MyCamera.MV_PIXEL_CONVERT_PARAM stConverPixelParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
                    stConverPixelParam.nWidth = stFrameInfo.nWidth;
                    stConverPixelParam.nHeight = stFrameInfo.nHeight;
                    stConverPixelParam.pSrcData = pData;
                    stConverPixelParam.nSrcDataLen = stFrameInfo.nFrameLen;
                    stConverPixelParam.enSrcPixelType = stFrameInfo.enPixelType;
                    stConverPixelParam.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;//在此处选择需要转换的目标类型
                    stConverPixelParam.pDstBuffer = pBufForSaveImage;
                    stConverPixelParam.nDstBufferSize = (uint)(stFrameInfo.nWidth * stFrameInfo.nHeight * 3 + 2048);
                    myCamera.MV_CC_ConvertPixelType_NET(ref stConverPixelParam);
                    image.GenImageInterleaved(pBufForSaveImage, "rgb", (int)stFrameInfo.nWidth, (int)stFrameInfo.nHeight, 0, "byte", (int)stFrameInfo.nWidth, (int)stFrameInfo.nHeight, 0, 0, -1, 0);
                    //释放指针
                    Marshal.FreeHGlobal(pBufForSaveImage);
                }
            }
            return image;
        }

        public Image GetImage()
        { 
            return GetHImage().ToBitmap();
        }
    }
}


