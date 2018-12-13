using Eto.Forms;
using Eto.Drawing;
using OhHell.Core;

namespace OhHell.Controls
{
  class StatisticsPanel : Scrollable
  {
    public StatisticsPanel(Game game)
    {
      Border = BorderType.None;
      Padding = 0;

      RankLabels = new PlayerRankLabel[game.Players.Length];
      Players = new PlayerLabel[game.Players.Length];
      Scores = new PlayerScoreLabel[game.Players.Length];

      Content = new TableLayout
      {
        Padding = 0,
        Spacing = new Size(0,0),
        Rows =
        {
          Header(),
          Body(game),
          null
        }
      };
    }
    Game Game => DataContext as Game;
    TableLayout Header()
    {
      var status = new Label();
      status.TextBinding.BindDataContext((Game m) => m.Status);

      var dealer = new Label();
      dealer.TextBinding.BindDataContext((Game m) => m.Dealer);

      return new TableLayout
      {
        Padding = 0,
        Spacing = ControlUtilities.Spacing,
        Rows = { new TableRow(dealer, status) }
      };
    }

    TableLayout Body(Game game)
    {
      var table = new TableLayout
      {
        Padding = 0,
        Spacing = new Size(ControlUtilities.Spacing.Width * 2, ControlUtilities.Spacing.Height)
      };
      table.Rows.Add(new TableRow(new Label { Text = "Rank" }, new Label { Text = "Player" }, new Label { Text = "Score" }));
      for (var i = 0; i < game.Players.Length; i++)
      {
        RankLabels[i] = new PlayerRankLabel();
        Players[i] = new PlayerLabel();
        Scores[i] = new PlayerScoreLabel();
        table.Rows.Add(new TableRow(
          RankLabels[i],
          Players[i],
          Scores[i],
          null
        ));
      }
      game.RankingsChanged += (sender, e) => Sort();
      Sort(game);
      return table;
    }

    void Sort(Game game = null)
    {
      var rankings = game?.Rankings ?? Game?.Rankings;
      if (rankings != null)
        for (var i = 0; i < rankings.Length; i++)
          RankLabels[i].DataContext = Players[i].DataContext = Scores[i].DataContext = rankings[i];
    }

    PlayerRankLabel[] RankLabels { get; }
    PlayerLabel[] Players { get; }
    PlayerScoreLabel[] Scores { get; }
  }
}
