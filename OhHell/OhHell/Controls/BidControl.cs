using Eto.Forms;
using Eto.Drawing;
using OhHell.Core;

namespace OhHell.Controls
{
  class BidControl : TableLayout
  {
    public BidControl(Bid bid)
    {
      Padding = 0;
      DataContext = bid;
      Spacing = new Size(0, 0);
      Rows.Add(new TableRow(new BidLabel(bid, false), new BidLabel(bid, true)));
    }
  }
}
