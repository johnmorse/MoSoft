using System;
using Eto.Forms;

namespace OhHell.Commands
{
  class AboutCommand : Base
  {
    public AboutCommand()
    {
      MenuText = "About...";
      ToolBarText = "About";
    }
    protected override void OnExecuted(EventArgs e)
    {
      base.OnExecuted(e);
      new AboutDialog().ShowDialog(MainForm);
    }
  }
}
