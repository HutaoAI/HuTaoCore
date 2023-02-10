using HuTaoCore.FileHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    [Serializable]
    public class BindBase : INotifyPropertyChanged
    {
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void SetProperty<T>(T value, ref T Content, [CallerMemberName] string propertyName = null)
        {
            if (!object.Equals(value,Content))
            {
                Content = value;
                OnPropertyChanged(propertyName);
            }
        }
        /// <summary>
        /// 序列化当前类
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual bool Save(string path)
        {
            return ClassSerializeHelper.Serialize(path,this);
        }

        /// <summary>
        /// 反序列化加载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual object Laod(string path)
        {
            return ClassSerializeHelper.DeSerialize(path);
        }
    }
}
