using Eto.Forms;
using OhHell.Core;

namespace OhHell.Controls
{
  class BidNumericStepper : NumericStepper
  {
    public BidNumericStepper(Bid bid = null)
    {
      DataContext = bid;
      DecimalPlaces = 0;
      MaximumDecimalPlaces = 0;
      MinValue = 0;
      Width = 80;
      Increment = 1;
      MaxValue = 10;
      ValueBinding.BindDataContext<Bid>(m => m.Value);
    }
  }
}
