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
using System.Threading.Tasks;

namespace Jira.Simple.Client.Rest {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Jira Rest Command
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class JiraRestCommand : IJiraCommand {
    #region Constants

    public const int DEFAULT_PAGE_SIZE = 500;

    #endregion Constants

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

    /// <summary>
    /// Connection
    /// </summary>
    IJiraConnection IJiraCommand.Connection => Connection;

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <param name="method">Http Method</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns>Answer Root Element (JSON)</returns>
    public async Task<JsonDocument> QueryAsync(string address, string query, HttpMethod method, CancellationToken token) {
      if (string.IsNullOrEmpty(address))
        throw new ArgumentNullException(nameof(address));

      if (Connection.IsDisposed)
        throw new ObjectDisposedException(nameof(Connection));

      if (!Connection.IsConnected)
        await Connection.ConnectAsync().ConfigureAwait(false);

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

      return document;
    }

    /// <summary>
    /// Paged Query
    /// </summary>
    /// <param name="address"></param>
    /// <param name="query"></param>
    /// <param name="method"></param>
    /// <param name="pageSize"></param>
    /// <param name="token"></param>
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

          yield return jsonDocument;

          startAt += pageSize;
        }
        else {
          yield return jsonDocument;

          startAt = 0;
        }
      }
    }

    #endregion Public
  }

}
