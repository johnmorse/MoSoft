using Eto.Forms;

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
}
