using System;
using System.IO;
using Eto.Forms;
using OhHell.Core;

namespace OhHell.Wpf
{
  class WpfServiceProvider : IGameServiceProvider
  {
    private WpfServiceProvider()
    {
    }

    public void ReportException(Exception e)
    {
      while (e != null)
      {
        //System.Diagnostics.Debug.WriteLine(e.Message);
        //System.Diagnostics.Debug.WriteLine(e.Message);
        e = e.InnerException;
      }
    }

    public string[] HistoryList()
    {
      return null;
    }

    public void CacheGame(Game game)
    {
      try
      {
        using (var stream = new FileStream(CacheFileName, FileMode.Create))
          game.ToXmlStream(stream);
      }
      catch (Exception e)
      {
        ReportException(e);
      }
    }

    public void ArchiveGame(Game game, string fileName)
    {
    }

    public Game LoadArchivedGame(string fileName)
    {
      return null;
    }

    public Game LoadCachedGame()
    {
      try
      {
        // File stream to Game
        using (var stream = new FileStream(CacheFileName, FileMode.Open))
          return Game.FromXmlStream(this, stream);
      }
      catch (Exception e)
      {
        ReportException(e);
        return null;
      }
    }

    static string CacheFileName => _cacheFileName ?? (_cacheFileName = Path.Combine(DataDirectory, "OhHell-CurrentGame.xml"));
    static string _cacheFileName;

    static string DataDirectory
    {
      get
      {
        if (string.IsNullOrEmpty(_dataDirectory))
          _dataDirectory = Path.Combine(Path.Combine(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MoSoft"), "OhHell"));
        if (!Directory.Exists(_dataDirectory))
          Directory.CreateDirectory(_dataDirectory);
        return _dataDirectory;
      }
    }
    private static string _dataDirectory;

    public static WpfServiceProvider Service => _service ?? (_service = new WpfServiceProvider());
    static WpfServiceProvider _service;
  }

  class MainClass
  {
    [STAThread]
    public static void Main(string[] args)
    {
      new Application(Eto.Platforms.Wpf).Run(new MainForm(WpfServiceProvider.Service));
    }
  }
}
