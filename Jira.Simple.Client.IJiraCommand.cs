using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Simple.Client {
  
  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Jira Command Interface
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public interface IJiraCommand {
    /// <summary>
    /// Connection
    /// </summary>
    IJiraConnection Connection { get; }

    /// <summary>
    /// Query Async
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query (JSON)</param>
    /// <param name="method">Http Method to use</param>
    /// <param name="token">Task Cancellation Token</param>
    Task<JsonDocument> QueryAsync(string address, string query, HttpMethod method, CancellationToken token);

    /// <summary>
    /// Paged Query Async
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="query">Query</param>
    /// <param name="method">Http Method to use</param>
    /// <param name="pageSize">Page Size</param>
    /// <param name="token">Task Cancellation Token</param>
    /// <returns></returns>
    IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                   string query,
                                                   HttpMethod method,
                                                   int pageSize,
                                                   CancellationToken token);
  }

}
