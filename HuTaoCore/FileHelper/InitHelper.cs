using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HuTaoCore.FileHelper
{
    public static class InitHelper
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key,
                    string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def,
                    StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);
        //声明读写INI文件的API函数  

        public static void IniWriteValue(string category, string Key, string Value, string path)
        {
            WritePrivateProfileString(category, Key, Value, path);
        }

        public static string IniReadValue(string category, string Key, string path)
        {
            if (!File.Exists(path)) return null;
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(category, Key, "", temp, 255, path);
            return temp.ToString();
        }
        public static Dictionary<string, string> GetValues(string category, string path)
        {
            if (!File.Exists(path)) return null;
            byte[] buffer = new byte[2048];
            GetPrivateProfileSection(category, buffer, 2048, path);
            String[] tmp = Encoding.Default.GetString(buffer).Trim('\0').Split('\0');
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (String entry in tmp)
            {
                string[] v = entry.Split('=');
                result.Add(v[0], v[1]);
            }
            return result;
        }
        public static void WriteValuse(string category, Dictionary<string, string> dic, string path)
        {
            string[] keys = dic.Keys.ToArray();
            foreach (var item in keys)
            {
                IniWriteValue(category, item, dic[item], path);
            }
        }
    }
}

