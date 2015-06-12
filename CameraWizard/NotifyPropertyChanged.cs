using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CameraWizard
{
  public class NotifyPropertyChanged : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void RaisePropertyChanged(string propertyName = null)
    {
      var handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(T value, ref T property, [CallerMemberName] string propertyName = null)
    {
      if (value.Equals(property))
        return false;
      property = value;
      if (!string.IsNullOrEmpty(propertyName))
        RaisePropertyChanged(propertyName);
      return true;
    }

    protected static void WriteException(Exception e)
    {
      System.Diagnostics.Debug.WriteLine(e.Message);
      System.Diagnostics.Debug.WriteLine(e.StackTrace);
    }
  }
}
