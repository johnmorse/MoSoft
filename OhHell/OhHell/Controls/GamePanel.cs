using Eto.Forms;
using OhHell.Core;

namespace OhHell.Controls
{
  class GamePanel : Panel
  {
    public GamePanel(Game game, Command nextCommand, Command prevCommand)
    {
      Padding = ControlUtilities.Padding;
      Content = new TabControl
      {
        Pages =
        {
          new TabPage(new BidPanel(game, nextCommand, prevCommand), null)
          {
            Text = "Bids" 
          },
          new TabPage(new ScoreSheet(game), null)
          {
            Text = "Score Sheet"
          },
          new TabPage(new StatisticsPanel(game), null)
          {
            Text = "Stats"
          },
        },
        SelectedIndex = 0
      };
    }
  }
}
