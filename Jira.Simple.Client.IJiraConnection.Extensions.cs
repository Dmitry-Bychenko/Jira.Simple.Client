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

    #endregion Public
  }

}
