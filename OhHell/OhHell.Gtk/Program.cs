using System;
using System.IO;
using Eto.Forms;
using OhHell.Core;

namespace OhHell.Gtk
{
  class GtkServiceProvider : IGameServiceProvider
  {
    private GtkServiceProvider()
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
        // Write XML to string
        //using (var ms = new MemoryStream())
        //{
        //  game.ToXmlStream(ms);
        //  var xml  = System.Text.Encoding.UTF8.GetString(ms.ToArray());
        //}
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
        // XML as text string to Game
        //var xml = File.ReadAllText(CacheFileName, System.Text.Encoding.UTF8);
        //using (var s = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml ?? "")))
        //return Game.FromXmlStream(this, s);

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

    static string CacheFileName => "/Users/john.morse/Downloads/OhHell-CurrentGame.xml";

    public static GtkServiceProvider Service => _service ?? (_service = new GtkServiceProvider());
    static GtkServiceProvider _service;
  }
  class MainClass
  {
    [STAThread]
    public static void Main(string[] args)
    {
      new Application(Eto.Platforms.Gtk).Run(new MainForm(GtkServiceProvider.Service));
    }
  }
}
