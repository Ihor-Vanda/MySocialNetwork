using Microsoft.AspNetCore.Http;
using Ocelot.Middleware;
using Ocelot.Multiplexer;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class RegisterAggregator : IDefinedAggregator
{
    public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
    {
        var responseMessages = await Task.WhenAll(responses.Select(r => r.Items.DownstreamResponse().Content.ReadAsStringAsync()));

        var content = string.Join("\n", responseMessages);
        return new DownstreamResponse(new StringContent(content), System.Net.HttpStatusCode.OK, new List<Header>(), "application/json");
    }
}
