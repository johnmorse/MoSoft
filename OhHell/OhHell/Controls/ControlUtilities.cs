using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

namespace OhHell.Controls
{
  static class ControlUtilities
  {
    public static Size ButtonSpacing => (_buttonSpacing ?? (_buttonSpacing = new Button())).Platform.IsMac ? new Size(0, 0) : Spacing;
    private static Control _buttonSpacing;
    public static Size Spacing => new Size(4, 4);
    public static Padding Padding => new Padding(10);
    public static Font BigFont => _bigFont ?? (_bigFont = SystemFonts.Bold(SystemFonts.Label().Size * 2f));
    private static Font _bigFont;
    public static Font BoldFont => _boldFont ?? (_boldFont = SystemFonts.Bold());
    private static Font _boldFont; 

    public static Icon GetIcon(string resourceId)
    {
      var id = $"OhHell.Resources.{resourceId}";
      IconCache.TryGetValue(id, out Icon icon);
      if (icon == null)
        IconCache[id] = icon = Icon.FromResource(id);
      return icon;
    }
    private static Dictionary<string, Icon> IconCache { get; } = new Dictionary<string, Icon>();
  }
}
