using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketmasterMonitor.Helpers;
using TicketmasterMonitor.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using TicketmasterMonitor.DataTransferObjects;
using TicketmasterMonitor.Webhooks;
using Azure.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace TicketmasterMonitor.Services
{
    public class CheckoutLinkValidatorService
    {
        private readonly HttpClientService httpClientService;

        public CheckoutLinkValidatorService(HttpClientService httpClientService)
        {
            this.httpClientService = httpClientService;
        }

        public async Task<CheckoutInfoDTO> ValidateCheckoutLinkAsync(HttpClient httpClient, string checkoutUrl)
        {
            // for (int retryCount = 0; retryCount < 5; retryCount++)
            // {
            //     try
            //     {
            //         var request = new HttpRequestMessage(HttpMethod.Get, checkoutUrl);
            // 
            //         request.Headers.Clear();
            // 
            //         request.Headers.Add("authority", "www.ticketmaster.com");
            //         request.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            //         request.Headers.Add("accept-language", "en-US,en;q=0.9");
            //         request.Headers.Add("cache-control", "no-cache");
            //         request.Headers.Add("dnt", "1");
            //         request.Headers.Add("pragma", "no-cache");
            //         request.Headers.Add("sec-ch-ua", "Google Chrome\";v=\"113\", \"Chromium\";v=\"116\", \"Not-A.Brand\";v=\"24");
            //         request.Headers.Add("sec-ch-ua-mobile", "?0");
            //         request.Headers.Add("sec-ch-ua-platform", "Windows");
            //         request.Headers.Add("sec-fetch-dest", "document");
            //         request.Headers.Add("sec-fetch-mode", "navigate");
            //         request.Headers.Add("sec-fetch-site", "none");
            //         request.Headers.Add("sec-fetch-user", "?1");
            //         request.Headers.Add("upgrade-insecure-requests", "1");
            //         request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
            // 
            //         var response = await httpClient.SendAsync(request);
            // 
            //         if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            //         {
            //             return null; // Handle the error condition appropriately
            //         }
            //         else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            //         {
            //             return null;
            //         }
            //         else
            //         {
            //             var content = await response.Content.ReadAsStringAsync();
            // 
            //             if (content.Length > 40000)
            //             {
            //                 var checkoutInfo = await ExtractCheckoutInfoAsync(content);
            //                 checkoutInfo.CheckoutLink = checkoutUrl;
            //                 if (checkoutInfo != null)
            //                 {
            //                     var webhookService = new WebhookService();
            // 
            //                     await webhookService.SendSuccessWebhook(checkoutInfo);
            // 
            //                     return checkoutInfo;
            //                 }
            //             }
            //             else
            //             {
            //                 return null;
            //             }
            //         }
            //     }
            //     catch (Exception e)
            //     {
            //         continue;
            //     }
            // }
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, checkoutUrl);

                request.Headers.Clear();

                request.Headers.Add("authority", "www.ticketmaster.com");
                request.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.Add("accept-language", "en-US,en;q=0.9");
                request.Headers.Add("cache-control", "no-cache");
                request.Headers.Add("dnt", "1");
                request.Headers.Add("pragma", "no-cache");
                request.Headers.Add("sec-ch-ua", "Google Chrome\";v=\"113\", \"Chromium\";v=\"116\", \"Not-A.Brand\";v=\"24");
                request.Headers.Add("sec-ch-ua-mobile", "?0");
                request.Headers.Add("sec-ch-ua-platform", "Windows");
                request.Headers.Add("sec-fetch-dest", "document");
                request.Headers.Add("sec-fetch-mode", "navigate");
                request.Headers.Add("sec-fetch-site", "none");
                request.Headers.Add("sec-fetch-user", "?1");
                request.Headers.Add("upgrade-insecure-requests", "1");
                request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");

                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    return null; // Handle the error condition appropriately
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    return null;
                else
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (content.Length > 40000)
                    {
                        var checkoutInfo = await ExtractCheckoutInfoAsync(content);
                        checkoutInfo.CheckoutLink = checkoutUrl;
                        if (checkoutInfo != null)
                        {
                            var webhookService = new WebhookService();

                            await webhookService.SendSuccessWebhook(checkoutInfo);

                            return checkoutInfo;
                        }
                    }
                    else
                        return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
            return null;
        }

        private async Task<CheckoutInfoDTO> ExtractCheckoutInfoAsync(string htmlContent)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var eventNameNode = htmlDoc.DocumentNode.SelectSingleNode("//p[@data-tid='event-name']");
            var eventDateTimeNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@data-tid='event-datetime']/span/span");
            var eventVenueNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@data-tid='event-venue']");
            var ticketInfoNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@data-tid='ticket-list-item']");
            var seatInfoNode = htmlDoc.DocumentNode.SelectSingleNode("//span[@data-tid='seat-info']");
            var sectionInfoNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='style__SecNameList-sc-obbmhz-8 iMPVsv']");
            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='SplitContent-sc-xyj90u-0']//div[not(@data-tid)]");

            var checkoutInfo = new CheckoutInfoDTO
            {
                EventName = eventNameNode?.InnerText.Trim(),
                EventDateTime = eventDateTimeNode?.InnerText.Trim(),
                EventVenue = eventVenueNode?.InnerText.Trim(),
                TicketInfo = ticketInfoNode?.InnerText.Trim(),
                SeatInfo = seatInfoNode?.InnerText.Trim(),
                Section = sectionInfoNode?.InnerText.Trim(),
                Price = priceNode?.InnerText.Trim(),
            };

            if (seatInfoNode != null)
            {
                var seatInfo = seatInfoNode.InnerText.Trim();

                var match = Regex.Match(seatInfo, @"Sec (\d+), Row (\d+), (Seats? (\d+)(?:-(\d+))?)");

                if (match.Success)
                {
                    checkoutInfo.Section = match.Groups[1].Value;
                    checkoutInfo.Row = match.Groups[2].Value;

                    if (match.Groups[4].Success)
                    {
                        checkoutInfo.SeatInfo = $"Seats {match.Groups[3].Value} - {match.Groups[4].Value}";
                    }
                    else
                    {
                        checkoutInfo.SeatInfo = $"Seat {match.Groups[3].Value}";
                    }

                    if (int.TryParse(ticketInfoNode.InnerText.Trim(), out int quantity))
                    {
                        checkoutInfo.Quantity = quantity;
                    }
                }
            }

            return checkoutInfo;
        }
    }
}
