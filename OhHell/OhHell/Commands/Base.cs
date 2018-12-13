using Eto.Forms;
using OhHell.Core;

namespace OhHell.Commands
{
  abstract class Base : Command
  {
    protected Game Game
    {
      get => MainForm.Instance.Game;
      set => MainForm.Instance.Game = value;
    }
    protected MainForm MainForm => MainForm.Instance;
    /// <summary>
    /// Get the IGameServiceProvider associated with the MainForm.
    /// </summary>
    /// <value>
    /// The IGameServiceProvider associated with the MainForm.
    /// </value>
    protected IGameServiceProvider Service => MainForm.Instance.Service;
    /// <summary>
    /// Display "Quit Game?" message for a game in progress.
    /// </summary>
    /// <returns>
    /// DialogResult.Yes if the game should quit otherwise DialogResult.No.
    /// </returns>
    protected DialogResult QuitGameWarning()
    {
      return MessageBox.Show(
        MainForm.Instance,
        "Game in progress, do you quit this game and start a new one?",
        "Quit Game?",
        MessageBoxButtons.YesNo,
        MessageBoxType.Question,
        MessageBoxDefaultButton.No);
    }
    /// <summary>
    /// Check to see if there is a game in progress and displays the
    /// QuitGameWarning message if the game is not complted.
    /// </summary>
    /// <returns>
    /// Returns <c>true</c> if there is not game in progress or, the game has
    /// been completed or DialogResult.Yes was returned by QuitGameWarning
    /// otherwise; <c>false</c> is returned.
    /// </returns>
    protected bool OkToQuitGame()
    {
      return
           Game == null // No game
        || Game.Round >= Game.TotalRounds // Game complted
        || QuitGameWarning() == DialogResult.Yes;
    }
    /// <summary>
    /// Archive if there is a game in progress and at least three rounds have
    /// been played.
    /// </summary>
    protected void AutoArchiveGame()
    {
      if ((Game?.Round ?? 0) > 2)
        Game.ArchiveGame();
    }
  }
}
