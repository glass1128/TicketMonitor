using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketmasterMonitor.Services
{
    public class EventPageDataRetrieverService
    {
        private readonly HttpClientService httpClientService;

        public EventPageDataRetrieverService(HttpClientService httpClientService)
        {
            this.httpClientService = httpClientService;
        }

        public async Task<(string eventName, string dateTime, string venue)> GetEventPageDataAsync(string eventID, HttpClient httpClient)
        {
            while (true)
            {
                try
                {
                    var response = await httpClient.GetAsync($"https://www.ticketmaster.com/event/{eventID}");

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        continue;
                    }
                    else
                    {
                        var htmlContent = await response.Content.ReadAsStringAsync();

                        var document = new HtmlDocument();
                        document.LoadHtml(htmlContent);

                        var eventNameNode = document.DocumentNode.SelectSingleNode("//span[@class='event-header__event-name-text']");
                        var eventName = eventNameNode?.InnerText?.Trim();

                        var dateTimeNode = document.DocumentNode.SelectSingleNode("//div[@class='event-header__event-date']");
                        var dateTime = dateTimeNode?.InnerText?.Trim();

                        var venueNode = document.DocumentNode.SelectSingleNode("//a[@class='event-header__event-location']//span");
                        var venue = venueNode?.InnerText?.Trim();

                        return (eventName, dateTime, venue);
                    } 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }
            }
        }
    }
}
