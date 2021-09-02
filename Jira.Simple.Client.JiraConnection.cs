using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Simple.Client {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Jira Connection
  /// Jira Connection is intended to be instantiated once per application, rather than per-use
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public abstract class JiraConnection : IDisposable, IJiraConnection {
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

    protected CookieContainer CookieContainer { get; private set; }

    private HttpClient m_HttpClient;

    private readonly SemaphoreSlim m_Locker;

    #endregion Private Data

    #region Algorithm

    /// <summary>
    /// Escape To JSON
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected static string Escape(string value) {
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

      CookieContainer = new CookieContainer();

      var handler = new HttpClientHandler() {
        CookieContainer = CookieContainer,
        Credentials = CredentialCache.DefaultCredentials,
      };

      m_HttpClient = new HttpClient(handler) {
        Timeout = Timeout.InfiniteTimeSpan,
      };
    }

    /// <summary>
    /// Connect Async
    /// </summary>
    protected abstract Task CoreConnectAsync(CancellationToken token);

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    /// <param name="login">Login</param>
    /// <param name="password">Password</param>
    /// <param name="server">Server, e.g. https://jira-my-server.com</param>
    public JiraConnection(string login, string password, string server) {
      Login = login ?? throw new ArgumentNullException(nameof(login));
      Password = password ?? throw new ArgumentNullException(nameof(password));
      Server = server?.TrimEnd(' ', '/') ?? throw new ArgumentNullException(nameof(server));

      m_Locker = new SemaphoreSlim(1);

      CoreCreateClient();
    }

    // Data Source=http address;User ID=myUsername;password=myPassword;
    /// <summary>
    /// Standard Constructor
    /// </summary>
    /// <param name="connectionString">Connection String</param>
    public JiraConnection(string connectionString) {
      if (connectionString is null)
        throw new ArgumentNullException(nameof(connectionString));

      DbConnectionStringBuilder builder = new() {
        ConnectionString = connectionString
      };

      if (builder.TryGetValue("User ID", out var login) &&
          builder.TryGetValue("password", out var password) &&
          builder.TryGetValue("Data Source", out var server)) {
        Login = login?.ToString() ?? throw new ArgumentException("Login not found", nameof(connectionString));
        Password = password?.ToString() ?? throw new ArgumentException("Password not found", nameof(connectionString));
        Server = server?.ToString()?.Trim()?.TrimEnd('/') ?? throw new ArgumentException("Server not found", nameof(connectionString));

        m_Locker = new SemaphoreSlim(1);

        CoreCreateClient();
      }
      else
        throw new ArgumentException("Invalid connection string", nameof(connectionString));
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Http Client
    /// </summary>
    public HttpClient Client => m_HttpClient;

    /// <summary>
    /// To String
    /// </summary>
    public override string ToString() => $"{Login}@{Server}{(IsConnected ? " (connected)" : "")}";

    #endregion Public

    #region IJiraConnection

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
    /// Is Connected
    /// </summary>
    public bool IsConnected { get; protected set; }

    /// <summary>
    /// Connect Async
    /// </summary>
    public async Task ConnectAsync(CancellationToken token) {
      if (IsDisposed)
        throw new ObjectDisposedException("this");

      if (IsConnected)
        return;

      await m_Locker.WaitAsync(token).ConfigureAwait(false);

      try {
        if (IsDisposed)
          throw new ObjectDisposedException("this");

        if (!IsConnected)
          await CoreConnectAsync(token).ConfigureAwait(false);

        IsConnected = true;
      }
      finally {
        m_Locker.Release();
      }
    }

    /// <summary>
    /// Command
    /// </summary>
    public abstract IJiraCommand Command();

    #endregion IJiraConnection

    #region IDisposable

    /// <summary>
    /// Is Disposed
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Dispose
    /// </summary>
    protected virtual void Dispose(bool disposing) {
      if (IsDisposed)
        return;

      if (disposing) {
        m_Locker.Wait();

        try {
          IsConnected = false;
          IsDisposed = true;

          m_HttpClient.Dispose();
        }
        finally {
          m_Locker.Release();
        }

        m_Locker.Dispose();
      }
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose() {
      Dispose(true);

      GC.SuppressFinalize(this);
    }

    #endregion IDisposable
  }

}
