using Eto.Forms;

namespace OhHell.Controls
{
  class GamePanel : Panel
  {
    public GamePanel(Game game, Command nextCommand, Command prevCommand)
    {
      Padding = 10;
      Content = new TabControl
      {
        Pages =
        {
          new TabPage(new BidPanel(game, nextCommand, prevCommand), null)
          {
            Text = "Bids" 
          },
          new TabPage(new ScoreSheet(game) { Border = BorderType.None }, null)
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
