using Eto.Forms;

namespace OhHell.Controls
{
  class BidLabel : NumberLabel
  {
    public BidLabel(Bid bid, bool bindToScore)
    {
      if (bindToScore)
      {
        TextBinding.BindDataContext((Bid m) => m.ScoreText);
        this.Bind(c => c.TextColor, bid, m => m.ScoreTextColor);
      }
      else
        TextBinding.BindDataContext((Bid m) => m.Text);
    }
    public BidLabel()
    {
      TextBinding.BindDataContext((Bid m) => m.ValueText);
    }
  }

  class NumberLabel : Label
  {
    public NumberLabel()
    {
      TextAlignment = TextAlignment.Right;
      VerticalAlignment = VerticalAlignment.Center;
    }
  }
}
