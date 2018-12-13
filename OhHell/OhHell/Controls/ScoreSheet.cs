using Eto.Forms;
using Eto.Drawing;
using OhHell.Core;

namespace OhHell.Controls
{
  class ScoreSheet : Scrollable
  {
    public Game Game => DataContext as Game;
    public ScoreSheet(Game game)
    {
      Border = BorderType.None;
      Padding = 0;
      var header = NewTables(out TableLayout table);
      // Header
      var row = new TableRow(NewEmptyLabel(), new Label { Text = "Dealer" });
      foreach (var player in game.Players)
        row.Cells.Add(new PlayerLabel(player));
      AddTableRow(table, row);
      // Row for each round
      var dealer_index = 0;
      var count = game.TotalRounds;
      for (var round = 1; round <= count; round++)
      {
        var dealer = game.Players[dealer_index++];
        if (dealer_index >= game.Players.Length)
          dealer_index = 0;
        // <round number> <dealer>
        row = new TableRow(new NumberLabel { Text = $"{(round > game.Rounds ? game.Rounds - (round - game.Rounds) : round)}" }, new PlayerLabel(dealer));
        // <bid> <score> pairs in each player column
        foreach (var player in game.Players)
        {
          var bid = player.Bids[round - 1];
          row.Cells.Add(new BidControl(bid));
        }
        // Add the round row
        AddTableRow(table, row);
      }
      // Ranking
      row = new TableRow(NewEmptyLabel(), new Label { Text = "Ranking" });
      foreach (var player in game.Players)
        row.Cells.Add(new PlayerRankLabel(player) { TextAlignment = TextAlignment.Center });
      AddTableRow(table, row);
      table.Rows.Add(null);
      // Add new header and score sheet to the form
      SetContent(header, table);
    }

    void AddTableRow(TableLayout table, TableRow row)
    {
      row.Cells.Add(null);
      table.Rows.Add(row);
    }

    void SetContent(TableLayout header, TableLayout table)
    {
      Content = new Scrollable
      {
        Content = new TableLayout
        {
          Padding = 0,
          Spacing = new Size(0, 0),
          Rows =
          {
            header,
            table
          }
        }
      };
    }

    #region Make sheet control parts
    TableLayout NewTables(out TableLayout table)
    {
      table = new TableLayout
      {
        Padding = 0,
        Spacing = new Size(10, ControlUtilities.Spacing.Height)
      };
      var status = new Label();
      status.TextBinding.BindDataContext((Game m) => m.Status);
      return new TableLayout
      {
        Padding = table.Padding,
        Spacing = table.Spacing,
        Rows =
        {
          status
        }
      };
    }

    Label NewEmptyLabel() => new Label { Text = " " };
    #endregion Make sheet control parts
  }
}
