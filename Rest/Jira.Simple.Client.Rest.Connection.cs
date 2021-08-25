using System;
using System.Collections.Generic;
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
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class JiraRestConnection : IDisposable, IEquatable<JiraRestConnection> {
    #region Private Data

    private static readonly IReadOnlyDictionary<char, string> s_Escape = new Dictionary<char, string>() {
      { '\\', "\\\\" },
      { '"', "\\\"" },
      { '\n', "\\n" },
      { '\r', "\\r" },
      { '\t', "\\t" },
      { '\f', "\\f" },
      { '\b', "\\b" },
    };

    private CookieContainer m_CookieContainer;

    private HttpClient m_HttpClient;

    #endregion Private Data

    #region Algorithm

    private static string Escape(string value) {
      if (value is null)
        return "null";

      StringBuilder sb = new(value.Length * 2);

      foreach (char c in value)
        sb.Append(s_Escape.TryGetValue(c, out var s) ? s : c);

      return sb.ToString();
    }

    private void CoreCreateClient() {
      try {
        ServicePointManager.SecurityProtocol =
          SecurityProtocolType.Tls |
          SecurityProtocolType.Tls11 |
          SecurityProtocolType.Tls12;
      }
      catch (NotSupportedException) {
        ;
      }

      m_CookieContainer = new CookieContainer();

      var handler = new HttpClientHandler() {
        CookieContainer = m_CookieContainer,
        Credentials = CredentialCache.DefaultCredentials,
      };

      m_HttpClient = new HttpClient(handler) {
        Timeout = Timeout.InfiniteTimeSpan,
      };
    }

    private async Task CoreConnectAsync(CancellationToken token) {
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

      var response = await m_HttpClient
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
    /// <param name="server">Server, e.g. https://jira-my-server.com</param>
    public JiraRestConnection(string login, string password, string server) {
      Login = login ?? throw new ArgumentNullException(nameof(login));
      Password = password ?? throw new ArgumentNullException(nameof(password));
      Server = server?.TrimEnd(' ', '/') ?? throw new ArgumentNullException(nameof(server));

      CoreCreateClient();
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Http Client
    /// </summary>
    public HttpClient Client => m_HttpClient;

    /// <summary>
    /// Login
    /// </summary>
    public string Login { get; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Server
    /// </summary>
    public string Server { get; }

    /// <summary>
    /// Session Id
    /// </summary>
    public string SessionId {
      get {
        if (!IsConnected || IsDisposed)
          return null;

        var cookies = m_CookieContainer.GetCookies(new Uri(Server));

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

        var cookies = m_CookieContainer.GetCookies(new Uri(Server));

        return cookies["atlassian.xsrf.token"]?.Value;
      }
    }

    /// <summary>
    /// Is Connected
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Connect Async
    /// </summary>
    public async Task ConnectAsync(CancellationToken token) {
      if (IsDisposed)
        throw new ObjectDisposedException("this");

      if (IsConnected)
        return;

      await CoreConnectAsync(token).ConfigureAwait(false);

      IsConnected = true;
    }

    /// <summary>
    /// Connect Async
    /// </summary>
    public async Task ConnectAsync() => await ConnectAsync(CancellationToken.None);

    /// <summary>
    /// To String
    /// </summary>
    public override string ToString() => $"{Login}@{Server}{(IsConnected ? " (connected)" : "")}";

    #endregion Public

    #region IDisposable

    /// <summary>
    /// Is Disposed
    /// </summary>
    public bool IsDisposed { get; private set; }

    // Dispose
    private void Dispose(bool disposing) {
      if (IsDisposed)
        return;

      if (disposing) {
        m_HttpClient.Dispose();

        IsConnected = false;
        IsDisposed = true;        
      }
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose() => Dispose(true);

    #endregion IDisposable

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
