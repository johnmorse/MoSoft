using System;
using System.IO;
using Eto.Forms;
using OhHell.Core;
using MonoMac.Foundation;

namespace OhHell.Mac
{
  class MacServiceProvider : IGameServiceProvider
  {
    private MacServiceProvider()
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

    static string CacheKey => "CurrentSession";

    public string[] HistoryList()
    {
      var keys = new System.Collections.Generic.List<string>();
      foreach (var item in NSUserDefaults.StandardUserDefaults.AsDictionary())
        if (item.Key.ToString().StartsWith(ArchiveKey, StringComparison.Ordinal))
          keys.Add(item.Key.ToString().Substring(ArchiveKey.Length));
      return keys.ToArray();
    }

    public void CacheGame(Game game)
    {
      WriteGame(game, CacheKey);
    }

    static string ArchiveKey => "Archive/";

    public void ArchiveGame(Game game, string fileName)
    {
      var now = DateTime.Now;
      WriteGame(game, $"{ArchiveKey}{fileName}");
    }

    void WriteGame(Game game, string key)
    {
      try
      {
        // Write XML to string
        using (var stream = new MemoryStream())
        {
          // Write XML to memory stream
          game.ToXmlStream(stream);
          // Write as a UTF8 to com.mosoft.ohhell.plist
          NSUserDefaults.StandardUserDefaults.SetString(System.Text.Encoding.UTF8.GetString(stream.ToArray()), key);
        }
      }
      catch (Exception e)
      {
        ReportException(e);
      }
    }

    public Game LoadArchivedGame(string fileName) => GameFromKey(ArchiveKey + fileName, fileName);

    public Game LoadCachedGame() => GameFromKey(CacheKey);

    Game GameFromKey(string key, string archiveName = null)
    {
      try
      {
        // Get cached game as NSString from com.mosoft.ohhell.plist
        var value = NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString(key)) as NSString;
        // Put the NSString into a MemoryStream as a UTF8 string and pass to the
        // Game engine for parsing
        using (var s = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(value?.ToString() ?? "")))
          return Game.FromXmlStream(this, s, archiveName);
      }
      catch (Exception e)
      {
        ReportException(e);
        return null;
      }
    }

    public static MacServiceProvider Service => _service ?? (_service = new MacServiceProvider());
    static MacServiceProvider _service;
  }

  class MainClass
  {
    [STAThread]
    public static void Main(string[] args)
    {
      try
      {
        new Application(Eto.Platforms.Mac64).Run(new MainForm(MacServiceProvider.Service));
      }
      catch (Exception e)
      {
        MacServiceProvider.Service.ReportException(e);
      }
    }
  }
}
