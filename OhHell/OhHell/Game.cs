using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using Eto.Drawing;

namespace OhHell
{
  interface IGameServiceProvider
  {
    void CacheGame(Game game);
    Game LoadCachedGame();
  }

  class MacWinServiceProvider : IGameServiceProvider
  {
    private MacWinServiceProvider()
    {
    }

    public void CacheGame(Game game)
    {
      try
      {
        // Write XML to string
        //using (var ms = new MemoryStream())
        //{
        //  game.ToXmlStream(ms);
        //  var xml  = System.Text.Encoding.UTF8.GetString(ms.ToArray());
        //}
        using (var stream = new FileStream(CacheFileName, FileMode.Create))
          game.ToXmlStream(stream);
      }
      catch (Exception e)
      {
        DumpException(e);
      }
    }

    public Game LoadCachedGame()
    {
      try
      {
        // XML as text string to Game
        //var xml = File.ReadAllText(CacheFileName, System.Text.Encoding.UTF8);
        //using (var s = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml ?? "")))
          //return Game.FromXmlStream(this, s);

        // File stream to Game
        using (var stream = new FileStream(CacheFileName, FileMode.Open))
          return Game.FromXmlStream(this, stream);
      }
      catch (Exception e)
      {
        DumpException(e);
        return null;
      }
    }

    void DumpException(Exception e)
    {
    }

    static string CacheFileName => "/Users/john.morse/Downloads/OhHell-CurrentGame.xml";

    public static MacWinServiceProvider Service => _service ?? (_service = new MacWinServiceProvider());
    static MacWinServiceProvider _service;
  }

  /// <summary>
  /// Game.
  /// </summary>
  class Game : Model
  {
    public Game(IGameServiceProvider service, params string[] players)
    {
      Service = service;
      var cards = players.Length > 5 ? 104 : 52;
      Rounds = Math.Min(10, cards / players.Length);
      Players = new Player[players.Length];
      for (var i = 0; i < Players.Length; i++)
        Players[i] = new Player(players[i], TotalRounds);
      Dealers = new int[TotalRounds];
      var dealer_index = 0;
      var count = TotalRounds;
      for (var round = 1; round <= count; round++)
      {
        Dealers[round - 1] = dealer_index++;
        if (dealer_index >= Players.Length)
          dealer_index = 0;
      }
      UpdateStatus();
      UpdateDealer();
    }
    IGameServiceProvider Service { get; }
    public void SaveGame()
    {
      Service?.CacheGame(this);
    }
    bool SuspendSaveGame { get; set; }
    public void ToXmlStream(Stream stream)
    {
      if (SuspendSaveGame)
        return;
      try
      {
        using (var writer = XmlWriter.Create(stream, WriterSettings))
        {
          WriteXml(writer);
          stream.Flush();
        }
      }
      catch (Exception e)
      {
        while (e != null)
        {
          //System.Diagnostics.Debug.WriteLine(e.Message);
          //System.Diagnostics.Debug.WriteLine(e.Message);
          e = e.InnerException;
        }
      }
    }

    public static Game FromXmlStream(IGameServiceProvider service, Stream stream)
    {
      Game game = null;
      try
      {
        using (var reader = XmlReader.Create(stream))
        {
          reader.MoveToContent();
          int.TryParse(reader.GetAttribute(nameof(Rounds)) ?? "", out int rounds);
          int.TryParse(reader.GetAttribute(nameof(Round)) ?? "", out int round);
          if (rounds < 1 || round < 0 || !nameof(Game).Equals(reader.Name, StringComparison.Ordinal))
            return null;
          var dictionary = new Dictionary<string,Bid[]>();
          var total_rounds = rounds * 2 - 1;
          while (reader.Read())
          {
            if (reader.NodeType == XmlNodeType.Element)
            {
              var name = reader.Name ?? string.Empty;
              if (nameof(Player).Equals(name, StringComparison.Ordinal))
              {
                var bids = ReadPlayer(reader, total_rounds, out string player);
                if (player != null)
                  dictionary[player] = bids;
              }
            }
          }
          var players = new List<string>(dictionary.Count);
          foreach (var item in dictionary)
            players.Add(item.Key);
          game = new Game(service, players.ToArray());
          game.SuspendSaveGame = true;
          foreach (var player in game.Players)
          {
            if (dictionary.TryGetValue(player.Name, out Bid[] bids))
            {
              var count = Math.Min(bids.Length, player.Bids.Length);
              for (var i = 0; i < count; i++)
              {
                var source = bids[i];
                var destination = player.Bids[i];
                destination.Value = source.Value;
                destination.Set = source.Set;
                destination.TextIsEmpty = i >= round;
                destination.ScoreIsEmpty = i > round;
                destination.ScoreTextColor = destination.Set ? game.SetColor : game.SuccessColor;
              }
            }
          }
          // Sets the round
          game._round = round;
          game.RecalulateScores();
          game.CalculateRankings();
          game.UpdateStatus();
          game.UpdateDealer();
          game.SuspendSaveGame = false;
        }
      }
      catch (Exception e)
      {
        while (e != null)
        {
          //System.Diagnostics.Debug.WriteLine(e.Message);
          //System.Diagnostics.Debug.WriteLine(e.Message);
          e = e.InnerException;
        }
      }
      return game;
    }
    static Bid[] ReadPlayer(XmlReader reader, int rounds, out string name)
    {
      name = null;
      int.TryParse(reader.GetAttribute(nameof(Player.Score)), out int score);
      int.TryParse(reader.GetAttribute(nameof(Player.Ranking)), out int ranking);
      while (reader.Read() && name == null)
        if (reader.NodeType == XmlNodeType.Text)
          name = reader.Value ?? string.Empty;
      if (name == null)
        return new Bid[0];
      var bids = new List<Bid>();
      while (bids.Count < rounds && reader.Read())
      {
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (nameof(Bid).Equals(reader.Name ?? string.Empty, StringComparison.Ordinal))
          {
            int.TryParse(reader.GetAttribute("Round"), out int round);
            var bid = new Bid();
            int.TryParse(reader.GetAttribute(nameof(bid.Value)), out int value);
            int.TryParse(reader.GetAttribute(nameof(bid.Set)), out int set);
            bid.Value = value;
            bid.Set = set > 0;
            bids.Add(bid);
          }
        }
      }
      return bids.ToArray();
    }
    static XmlWriterSettings WriterSettings =>
      new XmlWriterSettings
      {
        Encoding = System.Text.Encoding.UTF8,
        Indent = true,
        IndentChars = "  "
      };
    string ToXml()
    {
      try
      {
        using (var stream = new StringWriter())
        using (var writer = XmlWriter.Create(stream, WriterSettings))
        {
          WriteXml(writer);
          var xml = stream.ToString();
          return xml;
        }
      }
      catch (Exception e)
      {
        while (e != null)
        {
          //System.Diagnostics.Debug.WriteLine(e.Message);
          //System.Diagnostics.Debug.WriteLine(e.Message);
          e = e.InnerException;
        }
        return null;
      }
    }
    void WriteXml(XmlWriter writer)
    {
      //Serialization.XmlSerializer x = new Serialization.XmlSerializer(p.GetType());
      writer.WriteStartDocument();
      {
        writer.WriteStartElement(GetType().Name);
        {
          writer.WriteAttributeString(nameof(Rounds), $"{Rounds}");
          writer.WriteAttributeString(nameof(Round), $"{Round}");
          writer.WriteStartElement(nameof(Players));
            foreach (var player in Players)
              player.WriteToXml(writer);
          writer.WriteEndElement();
        }
        writer.WriteEndElement();
      }
      writer.WriteEndDocument();
      // Flush the writer to the stream
      writer.Flush();
    }
    /// <summary>
    /// Creates a new test game
    /// </summary>
    /// <returns>The test game.</returns>
    internal static Game NewTestGame(IGameServiceProvider service)
    {
      return new Game(service, new[]
      {
        "Player-1",
        "Player-2",
        "Player-3",
        "Player-4",
        "Player-5",
        "Player-6",
        "Player-7",
        "Player-8",
        "Player-9",
        "Player-10",
      });
    }
    public int[] Dealers { get; }
    public string Status
    {
      get => _status;
      set => SetProperty(value, ref _status);
    }
    string _status = "Start game";
    public string RoundBidStatus
    {
      get => _roundBidStatus ?? string.Empty;
      set => SetProperty(value, ref _roundBidStatus);
    }
    string _roundBidStatus;
    public void UpdaRoundBidStatus()
    {
      var bid = GetTotalBid();
      if (bid == Bid)
        RoundBidStatus = "Bid is even";
      else if (bid < Bid)
        RoundBidStatus = $"Under by {Bid - bid}";
      else
        RoundBidStatus = $"Over by {bid - Bid}";
    }
    public string Dealer
    {
      get => _dealer ?? string.Empty;
      set => SetProperty(value, ref _dealer);
    }
    string _dealer;
    void UpdateDealer()
    {
      Dealer = Round >= 0 && Round < Dealers.Length
        ? $"Dealer: {Players[Dealers[Round]].Name}"
        : "";
    }
    public int Rounds { get; }
    public int TotalRounds => Rounds * 2 - 1;
    public Player[] Players { get; }
    public string LasCanIncrementError
    {
      get => _lasCanIncrementError ?? string.Empty;
      set => SetProperty(value, ref _lasCanIncrementError);
    }
    private string _lasCanIncrementError;
    public bool CanIncrement(out string message)
    {
      LasCanIncrementError = message = null;
      if (Round < 0 || Round >= TotalRounds)
      {
        LasCanIncrementError = message = $"Can't commit, not a vlid round ({Round})";
        return false;
      }
      // Allow even scores in the first and second rounds
      if (Bid < 3)
        return ValidateFirstTwoRounds(ref message);
      // Round three and above requires the total bid to be over or under and 
      // one or more players to go set
      var bid = GetTotalBid();
      if (bid == Bid)
      {
        LasCanIncrementError = message = "Oh hell, the score can't come out even";
        return false;
      }
      // Check to see if one or more players went set, someone has to go set 
      // for rounds > 3
      foreach (var player in Players)
        if (player.Bids[Round].Set)
          return true; // Somebody went set
      LasCanIncrementError = message = $"Oh hell, somebody has to go set if the round is three or above";
      return false;
    }
    bool ValidateFirstTwoRounds(ref string message)
    {
      var bid = GetTotalBid();
      // Early round where you are allowed to come out even, if even then return
      // true.
      if (Bid == bid)
        return true;
      // The bid is over or under so somebody must go set!
      foreach (var player in Players)
        if (player.Bids[Round].Set)
          return true; // Somebody went set
      var over_under = bid > Bid ? "over" : "under";
      LasCanIncrementError = message = $"Oh hell, somebody has to go set if the bid is {over_under}";
      return false;
    }
    public int GetTotalBid()
    {
      if (Round < 0 || Round >= TotalRounds)
        return 0;
      var total = 0;
      foreach (var player in Players)
        total += player.Bids[Round].Value;
      return total;
    }
    public bool IncrementRound()
    {
      if (Round >= TotalRounds)
        return false;
      var next = Round + 1;
      foreach (var player in Players)
      {
        var bid = player.Bids[Round];
        bid.Score = player.Score += bid.CalculateScore();
        bid.ScoreIsEmpty = false;
        bid.ScoreTextColor = bid.Set ? SetColor : SuccessColor;
        if (next < player.Bids.Length)
        {
          player.Bids[next].TextIsEmpty = false;
        }
      }
      Round++;
      CalculateRankings();
      UpdateStatus();
      return true;
    }
    public void DecrementRound()
    {
      if (Round < 1)
        return;
      Round--;
      RecalulateScores();
    }

    Color SetColor => Colors.Red;
    Color SuccessColor => SystemColors.ControlText;
    /// <summary>
    /// Gets or sets the round.
    /// </summary>
    /// <value>The round.</value>
    public int Round
    {
      get => _round;
      private set
      {
        if (value >= 0 && value <= TotalRounds && value != _round)
        {
          _round = value;
          SaveGame();
          RaisePropertyChanged();
          RoundChanged?.Invoke(this, EventArgs.Empty);
          UpdateDealer();
        }
      }
    }
    int _round = 0;
    public event EventHandler RoundChanged;
    /// <summary>
    /// Updates the status text string.
    /// </summary>
    public void UpdateStatus()
    {

      Status = Round == TotalRounds
        ? "Game over"
        : $"Round {Bid} going {DirectionString}";
      UpdaRoundBidStatus();
    }
    string DirectionString => Round > Rounds ? "down" : "up";
    public int Bid
    {
      get
      {
        if (Round == TotalRounds)
          return 0;
        return Round < Rounds
          ? Round + 1
          : Rounds - (Round - Rounds) - 1;
      }
    }
    /// <summary>
    /// Calculate scores for all rounds, sores will be set to zero for bid 
    /// rounds greater than the current round.
    /// </summary>
    void RecalulateScores()
    {
      // Calculate each players total score
      foreach (var player in Players)
      {
        // Sum the bids for each round less than the current round (Game.Round)
        var round = 0;
        foreach (var bid in player.Bids)
        {
          // Zero the score if this is the first round
          if (round == 0)
            player.Score = 0;
          // Show the bid for the current round and scores for rounds prior to
          // the current round.
          bid.TextIsEmpty = round > Round;
          bid.ScoreIsEmpty = round >= Round;
          // Only sum the score for rounds prior to the current round.  The game
          // is over when Round is equal to the total number of rounds.
          if (round < Round)
            player.Score += bid.CalculateScore();
          // Keeps the total score for this round up to date so items bound to
          // this bid will display the correct value
          bid.Score = player.Score;
          bid.ScoreTextColor = bid.Set ? SetColor : SuccessColor;
          round++;
        }
      }
      CalculateRankings();
      UpdateStatus();
    }
    /// <summary>
    /// Calculates the rankings based on score so far.
    /// </summary>
    void CalculateRankings()
    {
      // Sort the player list from highest to lowest score
      var players = new List<Player>(Players);
      players.Sort((Player a, Player b) => b.Score - a.Score);
      // Calulate score based postion, allows for ties
      var rank = 1;
      var score = players[0].Score;
      foreach (var player in players)
      {
        // Only increment ranking if sore is lower than previous players, this
        // is how ties are allowed
        player.Ranking = player.Score < score ? ++rank : rank;
        // Next score will be compared to this score
        score = player.Score;
      }
      Rankings = players.ToArray();
      RankingsChanged?.Invoke(this, EventArgs.Empty);
    }
    public event EventHandler RankingsChanged;
    public Player[] Rankings { get; private set; }
  }
}
