using Eto.Forms;
using OhHell.Core;

namespace OhHell.Controls
{
  class PlayerRankLabel : NumberLabel
  {
    public PlayerRankLabel(Player player = null)
    {
      DataContext = player;
      TextBinding.BindDataContext((Player m) => m.RankingText);
    }
  }

  class PlayerScoreLabel : NumberLabel
  {
    public PlayerScoreLabel(Player player = null)
    {
      DataContext = player;
      TextBinding.BindDataContext((Player m) => m.ScoreText);
    }
  }

  class PlayerLabel : Label
  {
    public PlayerLabel(Player player = null)
    {
      VerticalAlignment = VerticalAlignment.Center;
      DataContext = player;
      TextBinding.BindDataContext((Player m) => m.Name);
    }
  }

  enum PlayerStatLabelType
  {
    SetCount,
    BidTotal,
    PointsFromTricks
  }

  class PlayerStatLabel : NumberLabel
  {
    public PlayerStatLabel(PlayerStatLabelType type, Player player = null)
    {
      DataContext = player;
      switch (type)
      {
        case PlayerStatLabelType.SetCount:
          TextBinding.BindDataContext(Binding.Property((Player m) => m.SetCount).Convert(r => r.ToString(), v => int.Parse(v)));
          break;
        case PlayerStatLabelType.BidTotal:
          TextBinding.BindDataContext(Binding.Property((Player m) => m.BidTotal).Convert(r => r.ToString(), v => int.Parse(v)));
          break;
        case PlayerStatLabelType.PointsFromTricks:
          TextBinding.BindDataContext(Binding.Property((Player m) => m.PointsFromTricks).Convert(r => r.ToString(), v => int.Parse(v)));
          break;
      }
    }
  }
}
