using Eto.Forms;
using Eto.Drawing;
using OhHell.Controls;
using System.ComponentModel;

namespace OhHell
{
  public class MainForm : Form
  {
    public MainForm()
    {
      Title = "Oh-Hell Score Sheet";
      ClientSize = new Size(800, 550);

      // Hello message and button to create a new game
      Game = MacWinServiceProvider.Service.LoadCachedGame();

      // Create menu bar
      CreateMenu();

      // Create Toolbar
      CreateToolBar();
    }

    Game Game
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
            //NextRoundCommand
          }
        };
    }
    #endregion Create main window menus and tool bars

    #region Commands
    Command QuitCommand
    {
      get
      {
        if (_quitCommand == null)
        {
          // create a few commands that can be used for the menu and toolbar
          _quitCommand = new Command
          {
            MenuText = "Quit",
            ToolBarText = "Quit"
          };
          _quitCommand.Executed += (sender, e) =>
          {
            if (Game != null && Game.Round < Game.TotalRounds)
            {
              var result = MessageBox.Show(
                this,
                "Game in progress, do you realy want to quit this game?",
                "Quit Game?",
                MessageBoxButtons.YesNo,
                MessageBoxType.Question,
                MessageBoxDefaultButton.No);
              if (result != DialogResult.Yes)
                return;
            }
            Application.Instance.Quit();
          };
        }
        return _quitCommand;
      }
    }
    Command _quitCommand;

    Command AboutCommand
    {
      get
      {
        if (_aboutCommand == null)
        {
          // create a few commands that can be used for the menu and toolbar
          _aboutCommand = new Command
          {
            MenuText = "About...",
            ToolBarText = "About"
          };
          _aboutCommand.Executed += (sender, e) => new AboutDialog().ShowDialog(this);
        }
        return _aboutCommand;
      }
    }
    Command _aboutCommand;

    Command NewGameCommand
    {
      get
      {
        if (_newGameCommand == null)
        {
          // create a few commands that can be used for the menu and toolbar
          _newGameCommand = new Command
          {
            MenuText = "Start new name",
            ToolBarText = "New Game",
            ToolTip = "Close the current game and start a new one",
            Image = Icon.FromResource("OhHell.Resources.Ace64.png")
          };
          _newGameCommand.Executed += (sender, e) =>
          {
            if (Game != null && Game.Round <= Game.TotalRounds)
            {
              var result = MessageBox.Show(
                this,
                "Game in progress, do you quit this game and start a new one?",
                "Quit Game?",
                MessageBoxButtons.YesNo,
                MessageBoxType.Question,
                MessageBoxDefaultButton.No);
              if (result != DialogResult.Yes)
                return;
            }
            //Game = Game.NewTestGame();
            var form = new NewGameForm();
            form.Owner = this;
            // Attach to this form on Mac
            form.DisplayMode = DialogDisplayMode.Attached;
            form.ShowModal(this);
            if (form.Result == DialogResult.Ok)
            {
              var players = form.Players;
              var count = players?.Length ?? 0;
              if (count > 3)
                Game = new Game(MacWinServiceProvider.Service, players);
              else if (count == 0)
                MessageBox.Show(this, "The player list is empty", "Error Starting New Game", MessageBoxType.Error);
              else
                MessageBox.Show(this, $"Not enough players to start a game ({count})", "Error Starting New Game", MessageBoxType.Error);
            }
          };
        }
        return _newGameCommand;
      }
    }
    Command _newGameCommand;

    Command NextRoundCommand
    {
      get
      {
        if (_nextRoundCommand == null)
        {
          // create a few commands that can be used for the menu and toolbar
          _nextRoundCommand = new Command
          {
            MenuText = "Advance game to the next round",
            ToolBarText = "Next Round",
            ToolTip = "Advance game to the next round",
            Enabled = false
          };
          _nextRoundCommand.Executed += (sender, e) =>
          {
            string error = string.Empty;
            if (!Game?.CanIncrement(out error) ??false )
            {
              MessageBox.Show(this, error, "Can't proceed to the next round", MessageBoxType.Error);
              return;
            }
            DisableButtons();
            Game?.IncrementRound();
            UpdateButtonStates();
          };
        }
        return _nextRoundCommand;
      }
    }
    Command _nextRoundCommand;

    Command PreviousRoundCommand
    {
      get
      {
        if (_previousRoundCommand == null)
        {
          // create a few commands that can be used for the menu and toolbar
          _previousRoundCommand = new Command
          {
            MenuText = "Return to previous round to modify or play over",
            ToolBarText = "Previous Round",
            ToolTip = "Return to previous round to modify or play over",
            Enabled = false,
          };
          _previousRoundCommand.Executed += (sender, e) =>
          {
            DisableButtons();
            Game?.DecrementRound();
            UpdateButtonStates();
          };
        }
        return _previousRoundCommand;
      }
    }
    Command _previousRoundCommand;

    void DisableButtons()
    {
      NextRoundCommand.Enabled = false;
      PreviousRoundCommand.Enabled = false;
      NewGameCommand.Enabled = false;
    }

    void UpdateButtonStates()
    {
      NextRoundCommand.Enabled = Game?.CanIncrement(out string s) ?? false;
      PreviousRoundCommand.Enabled = Game != null && Game.Round > 0;
      NewGameCommand.Enabled = true;
    }
    #endregion Commands
  }
}
