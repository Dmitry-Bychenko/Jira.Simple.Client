using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Simple.Client.Rest {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Jira Rest Connection
  /// JiraRestConnection is intended to be instantiated once per application, rather than per-use
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class JiraRestConnection : JiraConnection, IEquatable<JiraRestConnection> {
    #region Algorithm

    /// <summary>
    /// Connect Async
    /// </summary>
    protected override async Task CoreConnectAsync(CancellationToken token) {
      string query =
         @$"{{
             ""username"": ""{Escape(Login)}"",
             ""password"": ""{Escape(Password)}""
           }}";

      using var req = new HttpRequestMessage {
        Method = HttpMethod.Post,
        RequestUri = new Uri(string.Join('/', Server, "rest/auth/1/session")),
        Headers = {
          { HttpRequestHeader.Accept.ToString(), "application/json" },
        },
        Content = new StringContent(query, Encoding.UTF8, "application/json")
      };

      var response = await Client
        .SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token)
        .ConfigureAwait(false);

      if (!response.IsSuccessStatusCode)
        throw new DataException($"Failed to Connect to Jira: {response.ReasonPhrase}");
    }

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    /// <param name="login">Login</param>
    /// <param name="password">Password</param>
    /// <param name="server">Server</param>
    public JiraRestConnection(string login, string password, string server)
      : base(login, password, server) { }

    /// <summary>
    /// Standard Constructor
    /// </summary>
    /// <param name="connectionString">Connection String</param>
    public JiraRestConnection(string connectionString)
      : base(connectionString) { }

    #endregion Create

    #region Public

    /// <summary>
    /// Session Id
    /// </summary>
    public string SessionId {
      get {
        if (!IsConnected || IsDisposed)
          return null;

        var cookies = CookieContainer.GetCookies(new Uri(Server));

        return cookies["JSessionID"]?.Value;
      }
    }

    /// <summary>
    /// Token
    /// </summary>
    public string Token {
      get {
        if (!IsConnected || IsDisposed)
          return null;

        var cookies = CookieContainer.GetCookies(new Uri(Server));

        return cookies["atlassian.xsrf.token"]?.Value;
      }
    }

    /// <summary>
    /// Create Command
    /// </summary>
    public override JiraRestCommand Command() => IsDisposed
      ? throw new ObjectDisposedException("this")
      : new(this);

    #endregion Public

    #region IEquatable<JiraRestConnection>

    /// <summary>
    /// Equals
    /// </summary>
    public bool Equals(JiraRestConnection other) {
      if (ReferenceEquals(this, other))
        return true;
      if (other is null)
        return true;

      return IsConnected == other.IsConnected &&
             IsDisposed == other.IsDisposed &&
             string.Equals(Server, other.Server, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(Login, other.Login, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Equals
    /// </summary>
    public override bool Equals(object obj) => obj is JiraRestConnection other && Equals(other);

    /// <summary>
    /// Hash Code
    /// </summary>
    public override int GetHashCode() =>
      Server.GetHashCode(StringComparison.OrdinalIgnoreCase) ^ Login.GetHashCode(StringComparison.OrdinalIgnoreCase);

    #endregion IEquatable<JiraRestConnection>
  }

}
