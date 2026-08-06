namespace App.Services;

public sealed class DefaultDatabaseService : IDatabaseService
{
  private string DatabaseDirectory
    => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JabChat"); 
  private string DatabaseFile
    => Path.Combine(DatabaseDirectory, "jabchat.db");

  public string GetDatabaseUri()
  {
    if (!Directory.Exists(DatabaseDirectory))
      Directory.CreateDirectory(DatabaseDirectory);
    return DatabaseFile;
  }

  public Task SaveAsync() => Task.CompletedTask;
}