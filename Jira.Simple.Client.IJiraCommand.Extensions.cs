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
  /// Jira Command Extensions
  /// </summary
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class JiraCommandExtensions {
    #region Public

    #region Query

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <param name="method">Http Method</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public static async Task<JsonDocument> QueryAsync(this IJiraCommand command,
                                                           string address,
                                                           string query,
                                                           HttpMethod method) =>
      await (command ?? throw new ArgumentNullException(nameof(command)))
        .QueryAsync(address, query, method, CancellationToken.None).ConfigureAwait(false);

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public static async Task<JsonDocument> QueryAsync(this IJiraCommand command,
                                                           string address,
                                                           string query,
                                                           CancellationToken token) =>
       await (string.IsNullOrWhiteSpace(query)
        ? (command ?? throw new ArgumentNullException(nameof(command)))
             .QueryAsync(address, null, HttpMethod.Get, token).ConfigureAwait(false)
        : (command ?? throw new ArgumentNullException(nameof(command)))
             .QueryAsync(address, query, HttpMethod.Post, token).ConfigureAwait(false));

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public static async Task<JsonDocument> QueryAsync(this IJiraCommand command,
                                                           string address,
                                                           string query) =>
      await (string.IsNullOrWhiteSpace(query)
        ? (command ?? throw new ArgumentNullException(nameof(command)))
             .QueryAsync(address, null, HttpMethod.Get, CancellationToken.None).ConfigureAwait(false)
        : (command ?? throw new ArgumentNullException(nameof(command)))
             .QueryAsync(address, query, HttpMethod.Post, CancellationToken.None).ConfigureAwait(false));

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public static async Task<JsonDocument> QueryAsync(this IJiraCommand command,
                                                           string address,
                                                           CancellationToken token) =>
      await (command ?? throw new ArgumentNullException(nameof(command)))
               .QueryAsync(address, "", HttpMethod.Get, token).ConfigureAwait(false);

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    public static async Task<JsonDocument> QueryAsync(this IJiraCommand command, string address) =>
      await (command ?? throw new ArgumentNullException(nameof(command)))
               .QueryAsync(address, "", HttpMethod.Get, CancellationToken.None).ConfigureAwait(false);

    #endregion Query

    #region Paged Query

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address,
                                                                            string query,
                                                                            HttpMethod method,
                                                                           [EnumeratorCancellation]
                                                                            CancellationToken token) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      await foreach (var item in command.QueryPagedAsync(address, query, method, -1, token))
        yield return item;
    }

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address,
                                                                            string query,
                                                                            HttpMethod method,
                                                                            int pageSize) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      await foreach (var item in command.QueryPagedAsync(address, query, method, pageSize, CancellationToken.None))
        yield return item;
    }

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address,
                                                                            string query,
                                                                            HttpMethod method) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      await foreach (var item in command.QueryPagedAsync(address, query, method, -1, CancellationToken.None))
        yield return item;
    }

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address,
                                                                            string query,
                                                                            int pageSize,
                                                                           [EnumeratorCancellation]
                                                                            CancellationToken token) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      HttpMethod method = HttpMethod.Post;

      if (string.IsNullOrWhiteSpace(query)) {
        query = "";

        method = HttpMethod.Get;
      }

      await foreach (var item in command.QueryPagedAsync(address, query, method, pageSize, token))
        yield return item;
    }

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address,
                                                                            string query,
                                                                           [EnumeratorCancellation]
                                                                            CancellationToken token) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      HttpMethod method = HttpMethod.Post;

      if (string.IsNullOrWhiteSpace(query)) {
        query = "";

        method = HttpMethod.Get;
      }

      await foreach (var item in command.QueryPagedAsync(address, query, method, -1, token))
        yield return item;
    }

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address,
                                                                            string query,
                                                                            int pageSize) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      HttpMethod method = HttpMethod.Post;

      if (string.IsNullOrWhiteSpace(query)) {
        query = "";

        method = HttpMethod.Get;
      }

      await foreach (var item in command.QueryPagedAsync(address, query, method, pageSize, CancellationToken.None))
        yield return item;
    }

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address,
                                                                            string query) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      HttpMethod method = HttpMethod.Post;

      if (string.IsNullOrWhiteSpace(query)) {
        query = "";

        method = HttpMethod.Get;
      }

      await foreach (var item in command.QueryPagedAsync(address, query, method, -1, CancellationToken.None))
        yield return item;
    }

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address,
                                                                            int pageSize,
                                                                           [EnumeratorCancellation]
                                                                            CancellationToken token) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      await foreach (var item in command.QueryPagedAsync(address, "", HttpMethod.Get, pageSize, token))
        yield return item;
    }

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address,
                                                                           [EnumeratorCancellation]
                                                                            CancellationToken token) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      await foreach (var item in command.QueryPagedAsync(address, "", HttpMethod.Get, -1, token))
        yield return item;
    }

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address,
                                                                            int pageSize) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      await foreach (var item in command.QueryPagedAsync(address, "", HttpMethod.Get, pageSize, CancellationToken.None))
        yield return item;
    }

    public static async IAsyncEnumerable<JsonDocument> QueryPagedAsync(this IJiraCommand command,
                                                                            string address) {
      if (command is null)
        throw new ArgumentNullException(nameof(command));

      await foreach (var item in command.QueryPagedAsync(address, "", HttpMethod.Get, -1, CancellationToken.None))
        yield return item;
    }

    #endregion Paged Query

    #endregion Public
  }

}
