using System;
using Eto.Forms;

namespace OhHell.Commands
{
  class NextRoundCommand : Base
  {
    public NextRoundCommand()
    {
      MenuText = "Advance game to the next round";
      ToolBarText = "Next Round";
      ToolTip = "Advance game to the next round";
      Enabled = false;
    }
    protected override void OnExecuted(EventArgs e)
    {
      base.OnExecuted(e);
      string error = string.Empty;
      if (!Game?.CanIncrement(out error) ?? false)
      {
        MessageBox.Show(MainForm, error, "Can't proceed to the next round", MessageBoxType.Error);
        return;
      }
      MainForm.DisableButtons();
      Game?.IncrementRound();
      MainForm.UpdateButtonStates();
    }
  }
}
