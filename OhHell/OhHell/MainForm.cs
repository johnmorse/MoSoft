using Eto.Forms;
using Eto.Drawing;
using OhHell.Controls;
using System.ComponentModel;
using OhHell.Core;
using OhHell.Commands;

namespace OhHell
{
  public class MainForm : Form
  {
    public MainForm(IGameServiceProvider service)
    {
      Instance = this;
      Service = service;
      Title = "Oh-Hell Score Sheet";
      ClientSize = new Size(800, 550);

      // Hello message and button to create a new game
      Game = service.LoadCachedGame();

      // Create menu bar
      CreateMenu();

      // Create Toolbar
      CreateToolBar();
    }
    public static MainForm Instance { get; private set; }
    public IGameServiceProvider Service { get; }

    public Game Game
    {
      get => DataContext as Game;
      set
      {
        if (DataContext == value && Content != null)
          return;
        DataContext = value;
        value?.UpdateStatus();
        Content = value == null
          ? (Control)new NewGamePanel(NewGameCommand)
          : new GamePanel(value, NextRoundCommand, PreviousRoundCommand);
        UpdateButtonStates();
        if (Game != null)
          Game.RoundChanged += (sender, e) => UpdateButtonStates();
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      Game?.SaveGame();
      base.OnClosing(e);
    }

    #region Create main window menus and tool bars
    void CreateMenu()
    {
      if (Menu == null)
        Menu = new MenuBar
        {
          Items =
          {
            // File submenu
            new ButtonMenuItem
            {
              Text = "&File",
              Items =
              {
                NewGameCommand,
                HistoryCommand,
                //NextRoundCommand
              }
            },
            // new ButtonMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
            // new ButtonMenuItem { Text = "&View", Items = { /* commands/items */ } },
          },
          ApplicationItems =
          {
            // application (OS X) or file menu (others)
            new ButtonMenuItem { Text = "&Preferences..." },
          },
          QuitItem = QuitCommand,
          AboutItem = AboutCommand
        };
    }

    void CreateToolBar()
    {
      if (ToolBar == null)
        ToolBar = new ToolBar
        {
          Items =
          {
            NewGameCommand,
            HistoryCommand
          }
        };
    }
    #endregion Create main window menus and tool bars

    #region Methods used to update buttons
    internal void DisableButtons()
    {
      NextRoundCommand.Enabled = false;
      PreviousRoundCommand.Enabled = false;
      NewGameCommand.Enabled = false;
    }

    internal void UpdateButtonStates()
    {
      NextRoundCommand.Enabled = Game?.CanIncrement(out string s) ?? false;
      PreviousRoundCommand.Enabled = Game != null && Game.Round > 0;
      NewGameCommand.Enabled = true;
    }
    #endregion Methods used to update buttons

    #region Commands
    QuitCommand QuitCommand { get; } = new QuitCommand();
    AboutCommand AboutCommand { get; } = new AboutCommand();
    NewGameCommand NewGameCommand { get; } = new NewGameCommand();
    NextRoundCommand NextRoundCommand { get; } = new NextRoundCommand();
    PreviousRoundCommand PreviousRoundCommand { get; } = new PreviousRoundCommand();
    HistoryCommand HistoryCommand { get; } = new HistoryCommand();
    #endregion Commands
  }
}
