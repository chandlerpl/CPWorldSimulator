using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WorldGenerator.VisualTests
{
    public class PageValues : INotifyPropertyChanged
    {
        public string name { get; set; }
        public string Name { get => name; set { this.name = value; NotifyPropertyChanged("Name"); } }
        protected object value { get; set; }
        public object Value { get => value; set { object old = this.value; this.value = value; ValueChanged?.Invoke(this, old); NotifyPropertyChanged("Value"); } }

        public event Action<PageValues, object> ValueChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class PageValues<T> : PageValues
    {
        public new T Value { get => (T)base.Value; set => base.Value = value; }
    }
}
