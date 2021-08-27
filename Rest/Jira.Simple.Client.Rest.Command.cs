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

  public sealed class JiraRestCommand : JiraCommand<JiraRestConnection> {
    #region Algorithm

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <param name="method">Http Method</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns>Answer Root Element (JSON)</returns>
    protected override async Task<JsonDocument> CoreQueryAsync(string address, string query, HttpMethod method, CancellationToken token) {
      address = string.Join("/", Connection.Server, "rest/api/latest", address.TrimStart('/'));

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

      return await JsonDocument
        .ParseAsync(stream, default, token)
        .ConfigureAwait(false);
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
    protected override async IAsyncEnumerable<JsonDocument> CoreQueryPagedAsync(string address,
                                                                                string query,
                                                                                HttpMethod method,
                                                                                int pageSize,
                                                                               [EnumeratorCancellation]
                                                                                CancellationToken token) {
      address = string.Join("/", Connection.Server, "rest/api/latest", address.TrimStart('/'));

      address += $"{(address.Contains('?') ? '&' : '?')}&maxResults={pageSize}";

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

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public JiraRestCommand(JiraRestConnection connection)
      : base(connection) { }

    #endregion Create
  }

}
