using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Airtable.Base;
public class TestBase
{
    public IEnumerable<AuthenticationCredentialsProvider> Creds { get; private set; }
    public InvocationContext InvocationContext { get; private set; }
    public FileManagementClient FileManagementClient { get; private set; }

    public TestBase()
    {
        InitializeCredentials();
        InitializeInvocationContext();
        InitializeFileManager();
    }

    private void InitializeCredentials()
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        Creds = config.GetSection("ConnectionDefinition")
                     .GetChildren()
                     .Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value))
                     .ToList();
    }

    private void InitializeInvocationContext()
    {
        InvocationContext = new InvocationContext
        {
            AuthenticationCredentialsProviders = Creds
        };
    }

    private void InitializeFileManager()
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var folderLocation = config.GetSection("TestFolder").Value;
        FileManagementClient = new FileManagementClient(folderLocation!);
    }
}
