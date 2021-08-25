# Jira.Simple.Client

Typical usage:

```c#

using Jira.Simple.Client;
using Jira.Simple.Client.Json;
using Jira.Simple.Client.Rest;

...

private static IJiraConnection s_Connection;

...
// Create Connection
s_Connection = new JiraRestConnection("MyLogIn", "MyPassword", "https://My_Jira_Server.com");
// Connect
await s_Connection.ConnectAsync();
...
// Create Command for Connection
var cmd = s_Connection.Command();

// Get all projects
using var doc = await cmd.QueryAsync("project");

string report = string.Join(Environment.NewLine, doc
        .RootElement
        .AsEnumerable()
        .Select(item => item.Read("name").StringOrNull()));
        
Console.Write(report);

```
Jira [reference](https://docs.atlassian.com/software/jira/docs/api/REST/8.7.0/)
