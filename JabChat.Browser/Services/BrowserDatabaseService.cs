using System.Runtime.InteropServices.JavaScript;
using App.Services;

namespace App.Browser.Services;

public partial class BrowserDatabaseService : IDatabaseService
{
  public string GetDatabaseUri()
  {
    return "jabchat.db";
  }

  public async Task SaveAsync()
  {
    try
    {
      await SaveDbJs();
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }
  }
  
  [JSImport("globalThis.saveDatabase")]
  private static partial Task SaveDbJs();
}