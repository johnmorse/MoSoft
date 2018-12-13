using Eto.Forms;
using Eto.Drawing;
using System.ComponentModel;
using OhHell.Controls;

namespace OhHell
{
  class NewGameForm : Dialog<DialogResult>
  {
    public NewGameForm()
    {
      Title = "New Oh Hell Game";
      Content = new Button { Text = "New game..." };
      ShowInTaskbar = false;
      Result = DialogResult.Cancel;
      Width = 600;

      // Player names
      TextBoxes = new[]
      {
        NewTextBox(),
        NewTextBox(),
        NewTextBox(),
        NewTextBox(),
        NewTextBox(),
        NewTextBox(),
        NewTextBox(),
        NewTextBox(),
        NewTextBox(),
        NewTextBox()
      };
      Content = new TableLayout
      {
        Padding = ControlUtilities.Padding,
        Spacing = new Size(0, ControlUtilities.Spacing.Height),
        Rows =
        {
          new Label
          {
            Text = "Enter player names starting with the first dealer",
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Font = ControlUtilities.BoldFont
          },
          MakePlayersTable(),
          MakeButtonRow()
        }
      };

      LoadComplete += (s, e) => TextBoxes[0].Focus(); 
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (Result == DialogResult.Ok && !HasValidNumberOfPlayers())
      {
        var result = MessageBox.Show(
          this,
          "You must have four or more players, do you still want cancel making a new game?",
          MessageBoxButtons.YesNo,
          MessageBoxType.Question,
          MessageBoxDefaultButton.No);
        if (result != DialogResult.No)
          e.Cancel = true;
        Result = DialogResult.Cancel;
      }
      Players = GetPlayers();
      base.OnClosing(e);
    }

    public string [] Players { get; private set; }

    Command NewGameCommand
    {
      get
      {
        if (_newGameCommand == null)
        {
          _newGameCommand = new Command { Enabled = false };
          _newGameCommand.Executed += (sender, e) =>
          {
            Result = DialogResult.Ok;
            Players = GetPlayers();
            Close();
          };
        }
        return _newGameCommand;
      }
    }
    Command _newGameCommand;
    Control MakeButtonRow()
    {
      var okbutton = new Button
      {
       Text = "Start Game",
        Command = NewGameCommand,
      };
      DefaultButton = okbutton;
      var cancel = new Button
      {
        Text = "Cancel"
      };
      AbortButton = cancel;
      cancel.Click += (sender, e) =>
      {
        Result = DialogResult.Cancel;
        Close();
      };
      return new TableLayout
      {
        Padding = 0,
        Spacing = ControlUtilities.ButtonSpacing,
        Rows = { new TableRow(null, okbutton, cancel) }
      };
    }

    Control MakePlayersTable()
    {
      var table = new TableLayout
      {
        Padding = 0,
        Spacing = ControlUtilities.Spacing,
      };

      for (var i = 0; i < TextBoxes.Length; i++)
        table.Rows.Add(NewTableRow(i));

      return table;
    }

    TextBox NewTextBox()
    {
      var tb = new TextBox();
      tb.LostFocus += (s, e) => NewGameCommand.Enabled = HasValidNumberOfPlayers();
      tb.TextChanged += (s, e) => NewGameCommand.Enabled = HasValidNumberOfPlayers();
      return tb;
    }

    bool HasValidNumberOfPlayers()
    {
      var players = GetPlayers();
      return (players?.Length ?? 0) > 3;
    }

    string [] GetPlayers()
    {
      var players = new System.Collections.Generic.List<string>();
      foreach (var tb in TextBoxes)
      {
        var player = tb.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(player))
          players.Add(player);
      }
      return players.ToArray();
    }

    TableRow NewTableRow(int index)
    {
      return new TableRow(
        new Label { Text = $"Player {index + 1}:" },
        TextBoxes[index]
      );
    }

    TextBox[] TextBoxes { get; }
  }
}
