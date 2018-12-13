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
        Image = Icon.FromResource("OhHell.Resources.Ace.png"),
        ImagePosition = ButtonImagePosition.Below,
        Font = SystemFonts.Bold(SystemFonts.Label().Size * 2f)
      };

      //Content = new TableLayout
      //{
      //  Spacing = new Size(0, 10),
      //  Rows =
      //  {
      //    null,
      //    NewGameRow(new Label { Text = "Oh-Hell, press button to start" }),
      //    NewGameRow(button),
      //    null
      //  }
      //};
    }
    Control NewGameRow(Control control)
    {
      return new TableRow(null, control, null);
    }
  }
}
