using System;

namespace OhHell.Commands
{
  class PreviousRoundCommand : Base
  {
    public PreviousRoundCommand()
    {
      MenuText = "Return to previous round to modify or play over";
      ToolBarText = "Previous Round";
      ToolTip = "Return to previous round to modify or play over";
      Enabled = false;
    }
    protected override void OnExecuted(EventArgs e)
    {
      base.OnExecuted(e);
      MainForm.DisableButtons();
      Game?.DecrementRound();
      MainForm.UpdateButtonStates();
    }
  }
}
