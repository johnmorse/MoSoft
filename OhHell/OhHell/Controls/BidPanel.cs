using Eto.Forms;
using Eto.Drawing;
using OhHell.Core;

namespace OhHell.Controls
{
  class BidPanel : Scrollable
  {
    public BidPanel(Game game, Command nextCommand, Command prevCommand)
    {
      Padding = 0;
      Border = BorderType.None;

      // Create control arrays
      ControlRows = new ControllRow[game.Players.Length];

      var message = new Label();
      message.TextBinding.BindDataContext((Game m) => m.LastCanIncrementError);

      Content = new TableLayout
      {
        Padding = 0,
        Rows =
        {
          // Create header that appears above the bid list
          NewHeader(),
          message,
          new Label(),
          // Create list of bidders, gets reorderd when the round changes
          NewBidTableLayout(game, nextCommand),
          // Next/Previous round buttons at the bottom 
          NewButtonRow(nextCommand, prevCommand),
          null
        }
      };

      game.RoundChanged += (sender, e) => SetRow(sender as Game);
    }
    ControllRow[] ControlRows { get; }
    Game Game => DataContext as Game;

    TableLayout NewButtonRow(Command nextCommand, Command prevCommand)
    {
      return new TableLayout
      {
        Padding = 0,
        Spacing = new Size(2, 0),
        Rows =
        {
          new TableRow(
            NewButton("PrevArrow.png", "Previous round", prevCommand),
            NewButton("NextArrow.png", "Next round", nextCommand),
            null)
        }
      };
    }

    Button NewButton(string resource, string toolTip, Command command)
    {
      return new Button
      {

        Image = ControlUtilities.GetIcon(resource),
        ImagePosition = ButtonImagePosition.Overlay,
        Size = new Size(48, 48),
        ToolTip = toolTip,
        Command = command
      };
    }

    TableLayout NewHeader()
    {
      var status = new Label();
      status.TextBinding.BindDataContext((Game m) => m.Status);

      var dealer = new Label();
      dealer.TextBinding.BindDataContext((Game m) => m.Dealer);

      var bid_status = new Label();
      bid_status.TextBinding.BindDataContext((Game m) => m.RoundBidStatus);

      return new TableLayout
      {
        Padding = 0,
        Spacing = new Size(0, 0),
        Rows = { new TableRow(dealer, status, bid_status) }
      };
    }

    TableLayout NewBidTableLayout(Game game, Command nextCommand)
    {
      var table = new TableLayout
      {
        Padding = 0,
        Spacing = ControlUtilities.Spacing,
        Rows =
        {
          NewTableHeader()
        }
      };

      // Add player bid rows
      for (var i = 0; i < game.Players.Length; i++)
        AddTableRow(game, nextCommand, table, i);

      // Orders the bid list based on the game round
      SetRow(game);
      return table;
    }

    TableRow NewTableHeader()
    {
      return new TableRow(
        new Label(),
        new Label { Text = "Player" },
        new Label { Text = "Bid" },
        new Label(),
        new Label { Text = "Set" },
        new Label { Text = "Score" }
      );
    }

    void AddTableRow(Game game, Command nextCommand, TableLayout table, int index)
    {
      var row = new ControllRow(this, game,  nextCommand, index);
      ControlRows[index] = row;
      var updown = TableLayout.Horizontal(row.UpButton, row.DownButton);
      updown.Spacing = new Size(0, 0);
      var tr = new TableRow(
        new NumberLabel { Text = $"{index + 1}" },
        row.PlayerLabel,
        row.BidLabel,
        updown,
        row.SetCheckBox,
        row.ScoreLabel,
        null
      );
      table.Rows.Add(tr);
    }

    void SetRow(Game game)
    {
      if (game == null || game.Round < 0 || game.Round >= game.Dealers.Length)
      {
        EnableInputControls(false);
        return;
      }
      if (!ControlRows[0].SetCheckBox.Enabled)
        EnableInputControls(true);
      var j = game.Dealers[game.Round] + 1;
      if (j >= game.Players.Length)
        j = 0;
      for (var i = 0; i < ControlRows.Length; i++)
      {
        var player = game.Players[j];
        ControlRows[i].SetRound(game, player);
        if (++j >= game.Players.Length)
          j = 0;
      }
    }

    void EnableInputControls(bool enable)
    {
      foreach (var c in ControlRows)
        c.EnableInputControls(enable);
    }

    class ControllRow
    {
      public ControllRow(BidPanel parent, Game game, Command nextCommand, int index)
      {
        Parent = parent;
        NextCommand = nextCommand;
        PlayerLabel = new PlayerLabel();
        BidLabel = new BidLabel();
        SetCheckBox = new BidCheckBox();
        UpButton = NewUpDownButton(index, true);
        DownButton = NewUpDownButton(index, false);
        ScoreLabel = new PlayerScoreLabel();
        SetCheckBox.CheckedChanged += (s, e) => NextCommand.Enabled = Game?.CanIncrement(out string m) ?? false;
      }

      Button NewUpDownButton(int index, bool up)
      {
        var button = new Button
        {
          Image = ControlUtilities.GetIcon(up ? "Plus.png" : "Minus.png"),
          ImagePosition = ButtonImagePosition.Overlay,
          Size = new Size(24, 24),
          Enabled = false
        };

        button.Click += (sender, e) =>
        {
          var bid = (sender as Button)?.DataContext as Bid;
          if (bid != null && Game != null)
          {
            if (up && bid.Value < Game.Bid)
              bid.Value++;
            else if (!up && bid.Value > 0)
              bid.Value--;
            UpButton.Enabled = bid.Value < Game.Bid;
            DownButton.Enabled = bid.Value > 0;
            NextCommand.Enabled = Game?.CanIncrement(out string s) ?? false;
            Game.UpdateRoundBidStatus();
          }
        };
        return button;
      }

      public void SetRound(Game game, Player player)
      {
        PlayerLabel.DataContext = ScoreLabel.DataContext = player;
        var bid = (game?.Round ?? 0) < (player?.Bids.Length ?? 0)
          ? player.Bids[game?.Round ?? 0]
          : null;
        BidLabel.DataContext = bid;
        UpButton.DataContext = bid;
        UpButton.Enabled = bid != null && bid.Value < (game?.Bid ?? 0);
        DownButton.DataContext = bid;
        DownButton.Enabled = bid != null && bid.Value > 0;
        SetCheckBox.DataContext = bid;
      }

      public void EnableInputControls(bool enable)
      {
        SetCheckBox.Enabled = enable;

        // Only disable up/down buttons, the approriate state will get set
        // when the new round initializes
        if (enable)
        {
          var bid = UpButton.DataContext as Bid;
          DownButton.Enabled = (bid?.Value ?? 0) > 0;
          UpButton.Enabled = Game?.CanIncrement(out string s) ?? false;
        }
        else
        {
          UpButton.Enabled = DownButton.Enabled = false;
        }
      }

      //Game Game { get; }
      Game Game => Parent?.Game;
      BidPanel Parent { get; }
      Command NextCommand { get; }
      public PlayerLabel PlayerLabel { get; }
      public NumberLabel BidLabel { get; }
      public Button UpButton { get; }
      public Button DownButton { get; }
      public BidCheckBox SetCheckBox { get; }
      public PlayerScoreLabel ScoreLabel { get; }
    }
  }
}
