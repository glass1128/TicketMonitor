using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace TicketmasterMonitor
{
    public class DiscordWebhookSender
    {
        private readonly HttpClient httpClient;

        public DiscordWebhookSender()
        {
            httpClient = new HttpClient();
        }

        public async Task SendWebhookAsync(string webhookUrl, string eventName, string dateTime, string venue, string eventURL, string checkoutURL, string row, string section, string quantity, string seats)
        {
            try
            {
                var payload = new
                {
                    content = $"**{eventName}**\n\n" +
                              $"Date: {dateTime}\n" +
                              $"Location: {venue}\n" +
                              $"Section: {section}\n" +
                              $"Row: {row}\n" +
                              $"Seat: {seats}\n" +
                              $"Quantity: {quantity}\n" +
                              $"Checkout: {checkoutURL}",
                };

                var response = await httpClient.PostAsJsonAsync(webhookUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Webhook sent successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to send webhook. Status code: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending webhook: {e.Message}");
            }
        }
    }
}
