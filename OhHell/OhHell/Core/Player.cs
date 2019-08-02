namespace OhHell.Core
{
  /// <summary>
  /// Player.
  /// </summary>
  public class Player : Model
  {
    public Player(string name, int rounds)
    {
      Name = name;
      Bids = new Bid[rounds];
      for (var i = 0; i < Bids.Length; i++)
        Bids[i] = new Bid();
      // Show bids for first round
      Bids[0].TextIsEmpty = false;
    }
    internal void WriteToXml(System.Xml.XmlWriter writer)
    {
      writer.WriteStartElement(GetType().Name);
      {
        writer.WriteAttributeString(nameof(Score), $"{Score}");
        writer.WriteAttributeString(nameof(Ranking), $"{Ranking}");
        writer.WriteValue(Name);
        writer.WriteStartElement(nameof(Bids));
        {
          var row = 0;
          foreach (var bid in Bids)
            bid.ToXml(writer, row++);
        }
        writer.WriteEndElement();
      }
      writer.WriteEndElement();
    }
    public void UpdateStats(Bid bid)
    {
      BidTotal += bid.Value;
      if (bid.Set)
        SetCount++;
      else
        PointsFromTricks += bid.Value * 2;
    }
    public string Name { get; }
    public Bid[] Bids { get; }
    public int Score
    {
      get => _score;
      set
      {
        if (SetProperty(value, ref _score))
          RaisePropertyChanged(nameof(ScoreText));
      }
    }
    private int _score;
    public string ScoreText => $"{Score}";
    public int Ranking
    {
      get => _ranking;
      set
      {
        if (SetProperty(value, ref _ranking))
          RaisePropertyChanged(nameof(RankingText));
      }
    }
    private int _ranking;
    public string RankingText => Ranking > 0 ? $"{Ranking}" : "Unranked";
    public int SetCount
    {
      get => _setCount;
      set
      {
        if (value < 0)
          return;
        SetProperty(value, ref _setCount);
      }
    }
    private int _setCount;
    public int BidTotal
    {
      get => _bidTotal;
      set
      {
        if (value < 0)
          return;
        SetProperty(value, ref _bidTotal);
      }
    }
    private int _bidTotal;
    public int PointsFromTricks
    {
      get => _pointsFromTricks;
      set
      {
        if (value < 0)
          return;
        SetProperty(value, ref _pointsFromTricks);
      }
    }
    private int _pointsFromTricks;
  }
}
