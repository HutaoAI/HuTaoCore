using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class BindBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void NotifyPropertyChanged<T>(T value, ref T Content, [CallerMemberName] string propertyName = null)
        {
            if (!object.Equals(value,Content))
            {
                Content = value;
                OnPropertyChanged(propertyName);
            }
        }
        
    }
}
