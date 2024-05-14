using Apps.Airtable.Dtos;
using Apps.Airtable.Invocables;
using Apps.Airtable.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Airtable.DataSourceHandlers
{
    public class SingleFieldDataSourceHandler : AirtableInvocable, IAsyncDataSourceHandler
    {
        private readonly SingleFieldIdentifier _field;

        public SingleFieldDataSourceHandler(InvocationContext invocationContext, [ActionParameter] SingleFieldIdentifier field) : base(
            invocationContext)
        {
            _field = field;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_field.TableId))
                throw new("You should specify the Table ID first");

            var tableRequest = new AirtableRequest("/tables", Method.Get, InvocationContext.AuthenticationCredentialsProviders); ;
            var tables = await MetaClient.ExecuteWithErrorHandling<TableDtoWrapper<FullTableDto>>(tableRequest);

            var table = tables.Tables.FirstOrDefault(x => x.Id == _field.TableId);

            if (table == null) throw new Exception($"Could not find table with ID {_field.TableId}");

            return table.Fields
                .Where(x => context.SearchString is null ||
                            x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(x => x.Id, x => x.Name);
        }
    }
}
