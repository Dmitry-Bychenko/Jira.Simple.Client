using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Simple.Client {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// IJiraConnection Extensions
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class JiraConnectionExtensions {
    #region Public

    /// <summary>
    /// Connect Async
    /// </summary>
    public static async Task ConnectAsync(this IJiraConnection connection) =>
      await (connection ?? throw new ArgumentNullException(nameof(connection)))
        .ConnectAsync(CancellationToken.None)
        .ConfigureAwait(false);

    /// <summary>
    /// Jira Server Info
    /// </summary>
    public static async Task<JiraServerInfo> ServerInfoAsync(this IJiraConnection connection, CancellationToken token) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      var cmd = connection.Command();

      using var json = await cmd.QueryAsync("serverInfo", token).ConfigureAwait(false);

      return new JiraServerInfo(json);
    }

    /// <summary>
    /// Jira Server Info
    /// </summary>
    public static async Task<JiraServerInfo> ServerInfoAsync(this IJiraConnection connection) =>
      await ServerInfoAsync(connection, CancellationToken.None).ConfigureAwait(false);

    #endregion Public
  }
}
