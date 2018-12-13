using Eto.Forms;
using OhHell.Core;

namespace OhHell.Controls
{
  class BidCheckBox : CheckBox
  {
    public BidCheckBox(Bid bid = null)
    {
      //Text = "Set";
      DataContext = bid;
      CheckedBinding.BindDataContext<Bid>(m => m.Set);
    }
  }
}
