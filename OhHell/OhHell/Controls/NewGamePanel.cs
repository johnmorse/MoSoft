using Eto.Forms;
using Eto.Drawing;

namespace OhHell.Controls
{
  public class NewGamePanel : Panel
  {
    public NewGamePanel(Command newGameCommand)
    {
      Content = new Button
      {
        Text = "Click here to start a new game",
        Command = newGameCommand,
        Image = ControlUtilities.GetIcon("Ace.png"),
        ImagePosition = ButtonImagePosition.Below,
        Font = ControlUtilities.BigFont
      };
    }
    Control NewGameRow(Control control)
    {
      return new TableRow(null, control, null);
    }
  }
}
