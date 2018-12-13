using Eto.Drawing;

namespace OhHell.Core
{
  /// <summary>
  /// Bid.
  /// </summary>
  public class Bid : Model
  {
    internal void ToXml(System.Xml.XmlWriter writer, int round)
    {
      writer.WriteStartElement(GetType().Name);
      writer.WriteAttributeString("Round", $"{round}");
      writer.WriteAttributeString(nameof(Value), ValueText);
      writer.WriteAttributeString(nameof(Set), Set ? "1" : "0");
      writer.WriteEndElement();
    }
    public int CalculateScore() => Set ? 0 : Value * 2 + 10;
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    public int Value
    {
      get => _value;
      set
      {
        if (SetProperty(value, ref _value) && !TextIsEmpty)
        {
          RaisePropertyChanged(nameof(Text));
          RaisePropertyChanged(nameof(ValueText));
        }
      }
    }
    private int _value;
    public string ValueText => $"{Value}";
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:OhHell.Bid"/> text is empty.
    /// </summary>
    /// <value><c>true</c> if text is empty; otherwise, <c>false</c>.</value>
    public bool TextIsEmpty
    {
      get => _textIsEmpty;
      set
      {
        if (SetProperty(value, ref _textIsEmpty))
          RaisePropertyChanged(nameof(Text));
      }
    }
    bool _textIsEmpty = true;
    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <value>The text.</value>
    public string Text => TextIsEmpty ? string.Empty : $"{Value} - ";
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:OhHell.Bid"/> is set.
    /// </summary>
    /// <value><c>true</c> if set; otherwise, <c>false</c>.</value>
    public bool Set
    {
      get => _set;
      set => SetProperty(value, ref _set);
    }
    bool _set;
    /// <summary>
    /// Gets or sets the score.
    /// </summary>
    /// <value>The score.</value>
    public int Score
    {
      get => _score;
      set
      {
        if (SetProperty(value, ref _score) && !ScoreIsEmpty)
          RaisePropertyChanged(nameof(ScoreText));
      }
    }
    int _score;
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:OhHell.Bid"/> score is empty.
    /// </summary>
    /// <value><c>true</c> if score is empty; otherwise, <c>false</c>.</value>
    public bool ScoreIsEmpty
    {
      get => _scoreIsEmpty;
      set
      {
        if (SetProperty(value, ref _scoreIsEmpty))
          RaisePropertyChanged(nameof(ScoreText));
      }
    }
    bool _scoreIsEmpty = true;
    /// <summary>
    /// Gets the score text.
    /// </summary>
    /// <value>The score text.</value>
    public string ScoreText => ScoreIsEmpty ? string.Empty : $"{Score}";
    public Color ScoreTextColor
    {
      get => _scoreTextColor;
      set => SetProperty(value, ref _scoreTextColor);
    }
    Color _scoreTextColor = SystemColors.ControlText;
  }
}
