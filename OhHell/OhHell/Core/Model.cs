using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OhHell.Core
{
  public class Model : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    protected bool SetProperty<T>(T value, ref T member, [CallerMemberName] string propertyName = null)
    {
      if (Equals(member, value)) return false;
      member = value;
      RaisePropertyChanged(propertyName);
      return true;
    }
    protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
