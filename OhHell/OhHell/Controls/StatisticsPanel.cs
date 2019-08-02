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

      StatControls = new PlayerStats[game.Players.Length];

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
      table.Rows.Add(new TableRow(
        new Label { Text = "Rank" },
        new Label { Text = "Player" }, 
        new Label { Text = "Score" },
        new Label { Text = "Times set" },
        new Label { Text = "Total bid" },
        new Label { Text = "Points from tricks" }));
      for (var i = 0; i < game.Players.Length; i++)
      {
        var control = new PlayerStats();
        StatControls[i] = control;
        table.Rows.Add(new TableRow(
          control.Rank,
          control.Name,
          control.Score,
          control.Set,
          control.Bid,
          control.Tricks,
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
          StatControls[i].DataContext(rankings[i]);
    }

    class PlayerStats
    {
      public PlayerStats()
      {
        Rank.TextAlignment = 
        Score.TextAlignment = 
        Bid.TextAlignment = 
        Set.TextAlignment = 
        Tricks.TextAlignment = 
          TextAlignment.Center;
      }
      public void DataContext(Player player)
      {
        Rank.DataContext = 
        Name.DataContext = 
        Score.DataContext = 
        Bid.DataContext = 
        Set.DataContext =
        Tricks.DataContext =
          player;
      }
      public PlayerRankLabel Rank { get; } = new PlayerRankLabel();
      public PlayerLabel Name { get; } = new PlayerLabel();
      public PlayerScoreLabel Score { get; } = new PlayerScoreLabel();
      public PlayerStatLabel Bid { get; } = new PlayerStatLabel(PlayerStatLabelType.BidTotal);
      public PlayerStatLabel Set { get; } = new PlayerStatLabel(PlayerStatLabelType.SetCount);
      public PlayerStatLabel Tricks { get; } = new PlayerStatLabel(PlayerStatLabelType.PointsFromTricks);
    }
    PlayerStats[] StatControls { get; }
  }
}
