using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dictee
{
    public abstract class NotifyBaseClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //protected void NotifyPropertiyChanged([CallerMemberName] string propertyName = null)
        protected void NotifyPropertiyChanged(string propertyName = null)
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                if (Application.Current.Dispatcher.CheckAccess())
                    InternDoPropertyChanged(propertyName);
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        InternDoPropertyChanged(propertyName);
                    }));
                }
            }
        }

        private void InternDoPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /*private string name;
        public string Name
        {
            get => name;
            set { SetField(ref name, value, nameof(Name)); }
        }*/
    }
}
