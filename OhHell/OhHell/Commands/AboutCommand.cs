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
      var dialog = new AboutDialog
      {
        Designers = new string[] { "John Morse" },
        Developers = new string[] { "John Morse" },
        Logo = Controls.ControlUtilities.GetIcon("Ace.png"),
        ProgramDescription =
        "Simple Oh Hell score keeping program.  Archives games automatically.",
        Title = "Oh Hell Score Keeper"
      };
      dialog.ShowDialog(MainForm);
    }
  }
}
