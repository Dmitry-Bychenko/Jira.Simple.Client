using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Simple.Client {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Jira Rest Command
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public abstract class JiraCommand<T> : IJiraCommand where T : JiraConnection {
    #region Constants

    /// <summary>
    /// Default Page Size for QueryPagedAsync
    /// </summary>
    public const int DEFAULT_PAGE_SIZE = 500;

    #endregion Constants

    #region Algorithm

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <param name="method">Http Method</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns>Answer Root Element (JSON)</returns>
    protected abstract Task<JsonDocument> CoreQueryAsync(string address,
                                                         string query,
                                                         HttpMethod method,
                                                         CancellationToken token);

    /// <summary>
    /// Paged Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query</param>
    /// <param name="method">Method</param>
    /// <param name="pageSize">Page Size</param>
    /// <param name="token">Token</param>
    /// <returns></returns>
    protected abstract IAsyncEnumerable<JsonDocument> CoreQueryPagedAsync(string address,
                                                                          string query,
                                                                          HttpMethod method,
                                                                          int pageSize,
                                                                          CancellationToken token);

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public JiraCommand(T connection) {
      Connection =
          connection is null ? throw new ArgumentNullException(nameof(connection))
        : connection.IsDisposed ? throw new ObjectDisposedException(nameof(connection))
        : connection;
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Connection
    /// </summary>
    public virtual T Connection { get; }

    // Connection
    IJiraConnection IJiraCommand.Connection => Connection;

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <param name="method">Http Method</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public async Task<JsonDocument> QueryAsync(string address,
                                               string query,
                                               HttpMethod method,
                                               CancellationToken token) {
      if (string.IsNullOrEmpty(address))
        throw new ArgumentNullException(nameof(address));

      if (Connection.IsDisposed)
        throw new ObjectDisposedException(nameof(Connection));

      if (!Connection.IsConnected)
        await Connection.ConnectAsync().ConfigureAwait(false);

      query ??= "";

      return await CoreQueryAsync(address, query, method, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Paged Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query</param>
    /// <param name="method">Method</param>
    /// <param name="pageSize">Page Size</param>
    /// <param name="token">Token</param>
    /// <returns></returns>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                string query,
                                                                HttpMethod method,
                                                                int pageSize,
                                                               [EnumeratorCancellation]
                                                                CancellationToken token) {
      if (string.IsNullOrEmpty(address))
        throw new ArgumentNullException(nameof(address));

      if (Connection.IsDisposed)
        throw new ObjectDisposedException(nameof(Connection));

      if (!Connection.IsConnected)
        await Connection.ConnectAsync().ConfigureAwait(false);

      pageSize = pageSize <= 0 ? DEFAULT_PAGE_SIZE : pageSize;

      query ??= "";

      await foreach (var item in CoreQueryPagedAsync(address, query, method, pageSize, token))
        yield return item;
    }

    #endregion Public
  }

}
