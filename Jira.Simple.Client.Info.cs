using System;
using System.Text.Json;

namespace Jira.Simple.Client {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Server Info 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class JiraServerInfo {
    #region Create

    internal JiraServerInfo(JsonDocument document) {
      if (document is null)
        throw new ArgumentNullException(nameof(document));

      var root = document.RootElement;

      Url = new Uri(root.GetProperty("baseUrl").GetString());
      Version = Version.Parse(root.GetProperty("version").GetString());
      BuildNumber = root.GetProperty("buildNumber").GetInt32();
      BuildDate = DateTime.ParseExact(root.GetProperty("buildDate").GetString(), "yyyy-M-d'T'H:m:s.fffzzz", null).ToUniversalTime();

      ServerDate = DateTime.ParseExact(root.GetProperty("serverTime").GetString(), "yyyy-M-d'T'H:m:s.fffzzz", null).ToUniversalTime();
      Title = root.GetProperty("serverTitle").GetString();
      Scn = root.GetProperty("scmInfo").GetString();
    }

    #endregion Create

    #region Public

    /// <summary>
    /// URL to server
    /// </summary>
    public Uri Url { get; }

    /// <summary>
    /// Version
    /// </summary>
    public Version Version { get; }

    /// <summary>
    /// Build Date (UTC)
    /// </summary>
    public DateTime BuildDate { get; }

    /// <summary>
    /// Build Number: Major + Minor + Revision
    /// </summary>
    public int BuildNumber { get; }

    /// <summary>
    /// Build Date (UTC)
    /// </summary>
    public DateTime ServerDate { get; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Scn
    /// </summary>
    public string Scn { get; }

    /// <summary>
    /// To String
    /// </summary>
    public override string ToString() => Title;

    #endregion Public
  }

}
