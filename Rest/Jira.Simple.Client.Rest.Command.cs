using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

    private string MakeAddress(string address) {
      if (string.IsNullOrWhiteSpace(address))
        return "";

      address = address.Trim('/', ' ');

      if (address.StartsWith("rest/", StringComparison.OrdinalIgnoreCase))
        return string.Join("/", Connection.Server, address);
      else {
        var match = Regex.Match(address, @"^\s*([\p{L}0-9]*)\s*[;,:]+\s*");

        if (match.Success) {
          string api = match.Groups[1].Value;

          if (string.IsNullOrWhiteSpace(api))
            api = "api";

          return string.Join("/", Connection.Server, $"rest/{api}/latest", address.Substring(match.Index + match.Length).Trim('/', ' '));
        }
        else
          return string.Join("/", Connection.Server, "rest/api/latest", address);
      }
    }

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (json)</param>
    /// <param name="method">Http Method</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns>Answer Root Element (JSON)</returns>
    protected override async Task<JsonDocument> CoreQueryAsync(string address, string query, HttpMethod method, CancellationToken token) {
      address = MakeAddress(address);

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
      address = MakeAddress(address);

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

        if (jsonDocument is null)
          yield break;

        if (jsonDocument.RootElement.GetProperty("isLast").GetBoolean()) {
          yield return jsonDocument;

          yield break;
        }
        else {
          startAt += jsonDocument.RootElement.GetProperty("maxResults").GetInt32();

          yield return jsonDocument;
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
