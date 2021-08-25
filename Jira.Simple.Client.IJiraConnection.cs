using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Simple.Client {
  
  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Jira Connection Interface
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public interface IJiraConnection {
    /// <summary>
    /// Login
    /// </summary>
    string Login { get; }

    /// <summary>
    /// Password
    /// </summary>
    string Password { get; }

    /// <summary>
    /// Server
    /// </summary>
    string Server { get; }

    /// <summary>
    /// Is Connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connect Async
    /// </summary>
    Task ConnectAsync(CancellationToken token);

    /// <summary>
    /// Create new Command
    /// </summary>
    IJiraCommand Command();
  }

}
