using Eto.Forms;
using Eto.Drawing;
using OhHell.Controls;
using OhHell.Core;

namespace OhHell
{
  class HistoryForm : Dialog
  {
    public Game Game { get; private set; }
    Game _game;
    public HistoryForm(IGameServiceProvider service)
    {
      Title = "Game History";
      Size = new Size(700, 600);
      Resizable = true;
      var content = TableLayout.Horizontal(NavigationListBox(service), StatusPanel);
      content.Spacing = ControlUtilities.Spacing;
      var button_row = TableLayout.Horizontal(null, CancelButton(), OpenButton());
      button_row.Spacing = ControlUtilities.ButtonSpacing;
      Content = new TableLayout
      {
        Padding = ControlUtilities.Padding,
        Spacing = ControlUtilities.Spacing,
        Rows =
        {
          new TableRow (content) { ScaleHeight = true },
          TableLayout.Horizontal(null, button_row)
        }
      };
    }
    Button OpenButton()
    {
      OpenCommand = new Command { Enabled = false };
      OpenCommand.Executed += (s, e) =>
      {
        Game = _game;
        Close();
      };
      return new Button
      {
        Text = "Open",
        Command = OpenCommand
      };
    }
    Command OpenCommand { get; set; }
    Button CancelButton()
    {
      var button = new Button { Text = "Cancel" };
      button.Click += (sender, e) =>
      {
        Game = null;
        Close();
      };
      return button;
    }
    ListBox NavigationListBox(IGameServiceProvider service)
    {
      var control = new ListBox
      {
        DataStore = service.HistoryList(),
        Width = 200
      };
      control.SelectedValueChanged += (s, e) =>
      {
        var file = control.SelectedValue as string;
        _game = string.IsNullOrEmpty(file) ? null : service.LoadArchivedGame(file);
        StatusPanel.Content = _game == null ? EmptyPanel() : new ScoreSheet(_game);
        OpenCommand.Enabled = _game != null;
      };
      return control;
    }
    Panel StatusPanel => _statusPanel ?? (_statusPanel = new Panel { Padding = 0, Content = EmptyPanel() });
    Panel _statusPanel;
    Control EmptyPanel()
    {
      return new Panel
      {
        Padding = 0,
        Content = new TableLayout
        {
          Padding = 0,
          Spacing = ControlUtilities.Spacing,
          Rows =
          {
            null,
            new TableRow(null, new Label { Text = "Select a game...", Font = ControlUtilities.BigFont }, null),
            null
          }
        }
      };
    }
  }
}
