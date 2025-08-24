using System;
using AbeckDev.Amadeus;
using AbeckDev.Amadeus.Models;
using AbeckDev.Amadeus.Exceptions;

namespace AbeckDev.Amadeus.Api.Airlines;

public class Airlines
{
    AmadeusClient amadeusClient { get; set; }

    string apiPath = "v2/reference-data/urls/checkin-links";
    public Airlines(AmadeusClient amadeusClient)
    {
        this.amadeusClient = amadeusClient;
    }

    public async Task<string> ListCheckInLinks(string airlineCode, string language="en-GB", CancellationToken cancellationToken = default)
    {
        //Build the API Path
        var uri = new Uri(amadeusClient.clientContext.Options.Endpoint.AbsoluteUri + apiPath + $"?airlineCode={airlineCode}&language={language}");
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await amadeusClient.SendApiRequestAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            AmadeusRequestException.FromResponse(response,"","");

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        

        return responseContent;
    }

}
