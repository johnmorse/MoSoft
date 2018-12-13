using System;
using Eto.Forms;

namespace OhHell.Commands
{
  class HistoryCommand : Base
  {
    public HistoryCommand()
    {
      MenuText = "Game history";
      ToolBarText = "History";
      ToolTip = "Previous game history";
      Image = Controls.ControlUtilities.GetIcon("History.png");
      Enabled = true;
    }
    protected override void OnExecuted(EventArgs e)
    {
      base.OnExecuted(e);
      var form = new HistoryForm(Service)
      {
        Owner = MainForm,
        DisplayMode = DialogDisplayMode.Attached
      };
      form.ShowModal(MainForm);
      if (form.Game == null || !OkToQuitGame())
        return;
      AutoArchiveGame();
      MainForm.Game = form.Game;
    }
  }
}
