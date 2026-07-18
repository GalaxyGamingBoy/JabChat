using System.Runtime.InteropServices.JavaScript;
using App.Services;

namespace App.Browser.Services;

public partial class BrowserSettingsStorageService : ISettingsStorageService
{
  [JSImport("setItem", "storage")]
  private static partial void SetItem(string key, string value);
  
  [JSImport("getItem", "storage")]
  private static partial string? GetItem(string key);
  
  public async Task<string?> ReadAsync()
  {
    try
    {
      await InitializeAsync();
      return GetItem("JabChat.Settings");
    }
    catch
    {
      return null;
    }
  }

  public async Task WriteAsync(string data)
  {
    await InitializeAsync();
    SetItem("JabChat.Settings", data);
  }

  private async Task InitializeAsync()
  {
    const string storageModule = "../storage.js";
    await JSHost.ImportAsync("storage",  storageModule);
  }
}