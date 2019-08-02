using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using Eto.Drawing;

namespace OhHell.Core
{
  /// <summary>
  /// Game.
  /// </summary>
  public class Game : Model
  {
    public Game(IGameServiceProvider service, params string[] players)
    {
      // Provided by the OS specific Program.cs file provides access for reading
      // and writing settings, game history and cached current game
      Service = service;
      // If more than 5 players then we need to use a double deck
      var cards = players.Length > 5 ? 104 : 52;
      // Calculate the number of rounds, clamp at 10, will be less if more than
      // 10 players
      Rounds = Math.Min(10, cards / players.Length);
      // Iniaialize list of players, each player record contains current 
      // staticts and array of bids, one for each game round
      Players = new Player[players.Length];
      for (var i = 0; i < Players.Length; i++)
        Players[i] = new Player(players[i], TotalRounds);
      // Index of player to deal each round
      Dealers = new int[TotalRounds];
      var dealer_index = 0;
      var count = TotalRounds;
      for (var round = 1; round <= count; round++)
      {
        Dealers[round - 1] = dealer_index++;
        if (dealer_index >= Players.Length)
          dealer_index = 0;
      }
      // Updates the current game status based on the Round
      UpdateStatus();
      // Update the current dealer message
      UpdateDealer();
    }
    /// <summary>
    /// The settings and game serialization service
    /// </summary>
    /// <value>
    /// Returns the settings and game serialization service
    /// </value>
    IGameServiceProvider Service { get; }

    /// <summary>
    /// Gets the name of the archive file, it is based on the current date and
    /// time.
    /// </summary>
    /// <value>
    /// Returns the name of the archive file, it is based on the current date
    /// and time.
    /// </value>
    string ArchiveName => _archiveName ?? (_archiveName = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
    string _archiveName;

    /// <summary>
    /// Call this method to save the current game to the settings cache.
    /// </summary>
    public void SaveGame()
    {
      Service?.CacheGame(this);
    }

    /// <summary>
    /// Call this method to archive the current game to the history cache.
    /// </summary>
    /// <returns>
    /// Returns the name of the archived game
    /// </returns>
    public string ArchiveGame()
    {
      Service?.ArchiveGame(this, ArchiveName);
      return ArchiveName;
    }

    /// <summary>
    /// This gets set to true when reading a game to avoid the game getting 
    /// cached wile reading.
    /// </summary>
    /// <value>
    /// If <c>true</c> if suspend game cache; otherwise the current game will
    /// get cached when the round changes or when a game is completed.
    /// </value>
    bool SuspendSaveGame { get; set; }

    /// <summary>
    /// Write this game as XML to the specified stream.
    /// </summary>
    /// <param name="stream">Stream.</param>
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
        Service.ReportException(e);
      }
    }

    /// <summary>
    /// Parse XML stream as new game. 
    /// </summary>
    /// <param name="service">Service to associate with new Game</param>
    /// <param name="stream">Stream containing XML to parse</param>
    /// <param name="archiveName">
    /// Name of archive file being parsed, will set the resulting Game's archive
    /// file name.
    /// </param>
    /// <returns>
    /// Return the parsed Game or null if error reading the XML stream.
    /// </returns>
    public static Game FromXmlStream(IGameServiceProvider service, Stream stream, string archiveName = null)
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
          if (!string.IsNullOrEmpty(archiveName))
            game._archiveName = archiveName;
        }
      }
      catch (Exception e)
      {
        service.ReportException(e);
      }
      return game;
    }

    /// <summary>
    /// Helper that reads player info and bid history.
    /// </summary>
    /// <returns>The player.</returns>
    /// <param name="reader">Reader.</param>
    /// <param name="rounds">Rounds.</param>
    /// <param name="name">Name.</param>
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

    /// <summary>
    /// Gets the writer settings.
    /// </summary>
    /// <value>The writer settings.</value>
    static XmlWriterSettings WriterSettings =>
      new XmlWriterSettings
      {
        Encoding = System.Text.Encoding.UTF8,
        Indent = true,
        IndentChars = "  "
      };

    /// <summary>
    /// Convert this game to XML string
    /// </summary>
    /// <returns>The xml.</returns>
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
        Service.ReportException(e);
        return null;
      }
    }

    /// <summary>
    /// Write Game to XmlWriter
    /// </summary>
    /// <param name="writer">Writer.</param>
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
    /// Index of player to deal for each game round
    /// </summary>
    /// <value>
    /// Index of player to deal for each game round
    /// </value>
    public int[] Dealers { get; }
    public string Status
    {
      get => _status;
      set => SetProperty(value, ref _status);
    }
    string _status = "Start game";

    /// <summary>
    /// The current round bid status
    /// </summary>
    /// <value>The round bid status.</value>
    public string RoundBidStatus
    {
      get => _roundBidStatus ?? string.Empty;
      set => SetProperty(value, ref _roundBidStatus);
    }
    string _roundBidStatus;

    /// <summary>
    /// Updas the round bid status.
    /// </summary>
    public void UpdateRoundBidStatus()
    {
      var bid = GetTotalBid();
      if (bid == Bid)
        RoundBidStatus = "Bid is even";
      else if (bid < Bid)
        RoundBidStatus = $"Under by {Bid - bid}";
      else
        RoundBidStatus = $"Over by {bid - Bid}";
    }

    /// <summary>
    /// Message indicating who the current dealer is
    /// </summary>
    /// <value>The dealer.</value>
    public string Dealer
    {
      get => _dealer ?? string.Empty;
      set => SetProperty(value, ref _dealer);
    }
    string _dealer;

    /// <summary>
    /// Updates the dealer status string
    /// </summary>
    void UpdateDealer()
    {
      Dealer = Round >= 0 && Round < Dealers.Length
        ? $"Dealer: {Players[Dealers[Round]].Name}"
        : "";
    }

    /// <summary>
    /// Max Round
    /// </summary>
    /// <value>The rounds.</value>
    public int Rounds { get; }

    /// <summary>
    /// Total number of rounds in the game
    /// </summary>
    /// <value>The total rounds.</value>
    public int TotalRounds => Rounds * 2 - 1;

    /// <summary>
    /// Gets the players.
    /// </summary>
    /// <value>The players.</value>
    public Player[] Players { get; }

    /// <summary>
    /// Gets or sets the las can increment error.
    /// </summary>
    /// <value>The las can increment error.</value>
    public string LastCanIncrementError
    {
      get => _lastCanIncrementError ?? string.Empty;
      set => SetProperty(value, ref _lastCanIncrementError);
    }
    private string _lastCanIncrementError;

    /// <summary>
    /// Make sure that someone goes set when appropriate before moving to the
    /// next round.
    /// </summary>
    /// <returns><c>true</c>, if increment was caned, <c>false</c> otherwise.</returns>
    /// <param name="message">Message.</param>
    public bool CanIncrement(out string message)
    {
      LastCanIncrementError = message = null;
      if (Round < 0 || Round >= TotalRounds)
      {
        LastCanIncrementError = message = $"Can't commit, not a vlid round ({Round})";
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
        LastCanIncrementError = message = "Oh hell, the score can't come out even";
        return false;
      }
      // Check to see if one or more players went set, someone has to go set 
      // for rounds > 3
      foreach (var player in Players)
        if (player.Bids[Round].Set)
          return true; // Somebody went set
      LastCanIncrementError = message = $"Oh hell, somebody has to go set if the round is three or above";
      return false;
    }

    /// <summary>
    /// Allows the bid to come out even, if it is not even then requires at
    /// at least one player to go set
    /// </summary>
    /// <returns><c>true</c>, if first two rounds was validated, <c>false</c> otherwise.</returns>
    /// <param name="message">Message.</param>
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
      LastCanIncrementError = message = $"Oh hell, somebody has to go set if the bid is {over_under}";
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
        player.UpdateStats(bid);
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
    /// Gets or sets the current round, zero based index.  Will be equal to the
    /// TotalRounds count when the game has been completed.
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
      UpdateRoundBidStatus();
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
        // Zero the score and statistic properties
        player.Score = player.BidTotal = player.PointsFromTricks = player.SetCount = 0;
        foreach (var bid in player.Bids)
        {
          // Show the bid for the current round and scores for rounds prior to
          // the current round.
          bid.TextIsEmpty = round > Round;
          bid.ScoreIsEmpty = round >= Round;
          // Only sum the score for rounds prior to the current round.  The game
          // is over when Round is equal to the total number of rounds.
          if (round < Round)
          {
            player.Score += bid.CalculateScore();
            player.UpdateStats(bid);
          }
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
