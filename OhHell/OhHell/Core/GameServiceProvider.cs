namespace OhHell.Core
{
  public interface IGameServiceProvider
  {
    void CacheGame(Game game);
    void ArchiveGame(Game game, string fileName);
    Game LoadCachedGame();
    Game LoadArchivedGame(string fileName);
    string[] HistoryList();
    void ReportException(System.Exception e);
  }
}
