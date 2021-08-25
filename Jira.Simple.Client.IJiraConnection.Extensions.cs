﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Simple.Client {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// IJiraConnection Extensions
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class JiraConnectionExtensions {
    #region Public

    /// <summary>
    /// Connect Async
    /// </summary>
    public static async Task ConnectAsync(this IJiraConnection connection) => 
      await (connection ?? throw new ArgumentNullException(nameof(connection)))
        .ConnectAsync(CancellationToken.None);

    #endregion Public
  }

}