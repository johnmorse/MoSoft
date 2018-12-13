using Eto.Forms;
using System;

namespace OhHell.Commands
{
  class NewGameCommand : Base
  {
    public NewGameCommand()
    {
      MenuText = "Start new name";
      ToolBarText = "New Game";
      ToolTip = "Close the current game and start a new one";
      Image = Controls.ControlUtilities.GetIcon("Ace64.png");
    }
    protected override void OnExecuted(EventArgs e)
    {
      base.OnExecuted(e);
      if (!OkToQuitGame())
        return;
      var form = new NewGameForm
      {
        Owner = MainForm.Instance,
        DisplayMode = DialogDisplayMode.Attached,
        Resizable = false
      };
      form.ShowModal(MainForm.Instance);
      if (form.Result == DialogResult.Ok)
      {
        var players = form.Players;
        var count = players?.Length ?? 0;
        if (count > 3)
        {
          AutoArchiveGame();
          Game = new Core.Game(Service, players);
        }
        else if (count == 0)
          MessageBox.Show(MainForm, "The player list is empty", "Error Starting New Game", MessageBoxType.Error);
        else
          MessageBox.Show(MainForm, $"Not enough players to start a game ({count})", "Error Starting New Game", MessageBoxType.Error);
      }
    }
  }
}
