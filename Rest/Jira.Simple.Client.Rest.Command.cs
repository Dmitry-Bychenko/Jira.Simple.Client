using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Jira.Simple.Client.Rest {
  
  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Jira Rest Command
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class JiraRestCommand {
    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public JiraRestCommand(JiraRestConnection connection) {
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
    public JiraRestConnection Connection { get; }

    #region Query

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <param name="method">Http Method</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public async Task<JsonElement> QueryAsync(string address, string query, HttpMethod method, CancellationToken token) {
      if (string.IsNullOrEmpty(address))
        throw new ArgumentNullException(nameof(address));

      if (Connection.IsDisposed)
        throw new ObjectDisposedException(nameof(Connection));

      if (!Connection.IsConnected)
        throw new DataException("Not connected");

      address = string.Join("/", Connection.Server, "rest/api/latest", address.TrimStart('/'));

      query ??= "{}";

      using var req = new HttpRequestMessage {
        Method = method,
        RequestUri = new Uri(address),
        Headers = {
          { HttpRequestHeader.Accept.ToString(), "application/json" },
        },
        Content = new StringContent(query, Encoding.UTF8, "application/json")
      };

      var response = await Connection
        .Client
        .SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token)
        .ConfigureAwait(false);

      if (!response.IsSuccessStatusCode)
        throw new DataException(response.ReasonPhrase);

      using Stream stream = await response
        .Content
        .ReadAsStreamAsync(token)
        .ConfigureAwait(false);

      var document = await JsonDocument
        .ParseAsync(stream, default, token)
        .ConfigureAwait(false);

      return document.RootElement;
    }

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <param name="method">Http Method</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public async Task<JsonElement> QueryAsync(string address, string query, HttpMethod method) =>
      await QueryAsync(address, query, method, CancellationToken.None).ConfigureAwait(false);

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public async Task<JsonElement> QueryAsync(string address, string query, CancellationToken token) =>
       await (string.IsNullOrWhiteSpace(query)
        ? QueryAsync(address, null, HttpMethod.Get, token).ConfigureAwait(false)
        : QueryAsync(address, query, HttpMethod.Post, token).ConfigureAwait(false));

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public async Task<JsonElement> QueryAsync(string address, string query) =>
      await (string.IsNullOrWhiteSpace(query)
        ? QueryAsync(address, null, HttpMethod.Get, CancellationToken.None).ConfigureAwait(false)
        : QueryAsync(address, query, HttpMethod.Post, CancellationToken.None).ConfigureAwait(false));

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public async Task<JsonElement> QueryAsync(string address, CancellationToken token) =>
      await QueryAsync(address, "", HttpMethod.Get, token).ConfigureAwait(false);

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    public async Task<JsonElement> QueryAsync(string address) =>
      await QueryAsync(address, "", HttpMethod.Get, CancellationToken.None).ConfigureAwait(false);

    #endregion Query

    #region Paged Query

    /// <summary>
    /// Paged Query
    /// </summary>
    /// <param name="address"></param>
    /// <param name="query"></param>
    /// <param name="method"></param>
    /// <param name="pageSize"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<JsonElement> QueryPagedAsync(string address,
                                                               string query,
                                                               HttpMethod method,
                                                               int pageSize,
                                                              [EnumeratorCancellation]
                                                               CancellationToken token) {
      if (string.IsNullOrEmpty(address))
        throw new ArgumentNullException(nameof(address));

      if (!Connection.IsConnected)
        throw new DataException("Not connected");

      address = string.Join("/", Connection.Server, "rest/api/latest", address.TrimStart('/'));

      address += $"{(address.Contains('?') ? '&' : '?')}&maxResults={pageSize}";

      query ??= "{}";
      int startAt = 0;

      while (startAt >= 0) {
        using var req = new HttpRequestMessage {
          Method = method,
          RequestUri = new Uri(address + $"&startAt={startAt}"),
          Headers = {
          { HttpRequestHeader.Accept.ToString(), "application/json" },
        },
          Content = new StringContent(query, Encoding.UTF8, "application/json")
        };

        var response = await Connection
          .Client
          .SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token)
          .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
          throw new DataException(response.ReasonPhrase);

        using Stream stream = await response
          .Content
          .ReadAsStreamAsync(token)
          .ConfigureAwait(false);

        var jsonDocument = await JsonDocument
          .ParseAsync(stream, default, token)
          .ConfigureAwait(false);

        if (jsonDocument.RootElement.TryGetProperty("startAt", out var startAtItem)) {
          using var en = jsonDocument.RootElement.EnumerateObject();

          while (en.MoveNext()) {
            if (en.Current.Value.ValueKind == JsonValueKind.Array) {
              if (en.Current.Value.GetArrayLength() <= 0) {
                yield break;
              }
            }
          }

          yield return jsonDocument.RootElement;

          startAt += pageSize;
        }
        else {
          yield return jsonDocument.RootElement;

          startAt = 0;
        }
      }
    }

    #endregion Paged Query

    #endregion Public
  }

}
