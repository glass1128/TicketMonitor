using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketmasterMonitor.Models;

namespace TicketmasterMonitor.Services
{
    public class CheckoutRequesterIDService
    {
        private readonly HttpClientService httpClientService;

        public CheckoutRequesterIDService(HttpClientService httpClientService)
        {
            this.httpClientService = httpClientService;
        }

        public async Task<string> GetRequesterId(string eventId, HttpClient httpClient)
        {
            for (int retryCount = 0; retryCount < 5; retryCount++)
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Remove("authority");
                    httpClient.DefaultRequestHeaders.Add("authority", "checkout.ticketmaster.com");

                    HttpResponseMessage response = await httpClient.GetAsync($"https://checkout.ticketmaster.com/api/rules?eventId={eventId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseArray = JArray.Parse(responseContent);

                        var objectWithId = responseArray.FirstOrDefault(token => token["id"] != null);

                        if (objectWithId != null)
                        {
                            string requesterID = objectWithId["id"].Value<string>();
                            return requesterID;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            return null;
        }
    }
}
