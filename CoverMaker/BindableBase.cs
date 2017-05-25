using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CoverMaker {
    public abstract class BindableBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
