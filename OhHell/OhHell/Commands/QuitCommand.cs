using System;
using Eto.Forms;

namespace OhHell.Commands
{
  class QuitCommand : Base
  {
    public QuitCommand()
    {
      MenuText = "Quit";
      ToolBarText = "Quit";
    }
    protected override void OnExecuted(EventArgs e)
    {
      base.OnExecuted(e);
      if (Game != null)
      {
        // If game in progress then ask if the game should quit, if not then bail
        if (Game.Round < Game.TotalRounds && QuitGameWarning() != DialogResult.Yes)
          return;
        // If the game is completed then archive it
        if (Game.Rounds >= Game.TotalRounds)
          Game.ArchiveGame();
        else // Game in progress so save it
          Service?.CacheGame(Game);
      }
      Application.Instance.Quit();
    }
  }
}
