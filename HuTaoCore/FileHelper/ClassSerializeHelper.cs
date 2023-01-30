using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace HuTaoCore.FileHelper
{
    public static class ClassSerializeHelper
    {
        public static bool Serialize<T>(string path, T t) where T : class
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                bf.Serialize(fs, t);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                fs.Close();
            }
        }

        public static object DeSerialize(string path)
        {
            if (!File.Exists(path)) return null;
            using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
            {
                try
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    return binaryFormatter.Deserialize(fileStream);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
