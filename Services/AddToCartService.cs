using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TicketmasterMonitor.Context;
using TicketmasterMonitor.DataTransferObjects;
using TicketmasterMonitor.Models;

namespace TicketmasterMonitor.Services
{
    public class AddToCartService
    {
        private readonly HttpClientService httpClientService;
        private readonly CheckoutLinkValidatorService checkoutLinkValidatorService;

        public AddToCartService(HttpClientService httpClientService, CheckoutLinkValidatorService checkoutLinkValidatorService)
        {
            this.httpClientService = httpClientService;
            this.checkoutLinkValidatorService = checkoutLinkValidatorService;
        }

        public async Task<string> AddToCartNormalAsync(string eventId, string requesterId, string row, string section, HttpClient httpClient)
        {
            const int maxRetries = 5;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    string randomUUID = Guid.NewGuid().ToString();
                    string spanID = GenerateRandomHex(8);
                    string traceID = GenerateRandomHex(8);
                    int random_number = new Random().Next(10000000, 99999999);
                    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    string monetateID = $"5.{random_number}.{timestamp}";

                    var requestContent = new
                    {
                        query = @"
                        mutation reserve($reserveInput: ReserveInput!) {
                            reserve(reserveInput: $reserveInput) {
                                errors {
                                    code
                                    data {
                                        key
                                        value
                                    }
                                    message
                                }
                                requestId
                                status
                            }
                        }",
                        variables = new
                        {
                            reserveInput = new
                            {
                                eventId = eventId,
                                requestorId = requesterId,
                                requestContext = new
                                {
                                    channel = "desktop.ticketmaster.us",
                                    locale = "en-us",
                                },
                                tickets = new[]
                                {
                                    new
                                    {
                                        row = row,
                                        section = section,
                                        ticketTypes = new[]
                                        {
                                            new
                                            {
                                                id = "000000000001",
                                                quantity = "1",
                                            },
                                        },
                                        inventoryDetail = new
                                        {
                                            type = "Primary",
                                        },
                                    },
                                },
                            },
                        },
                    };

                    var jsonString = JsonConvert.SerializeObject(requestContent);
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Post, "https://checkout.ticketmaster.com/graphql");

                    request.Headers.Clear();

                    request.Headers.Add("authority", "checkout.ticketmaster.com");
                    request.Headers.Add("accept", "*/*");
                    request.Headers.Add("accept-language", "en-US,en;q=0.9");
                    request.Headers.Add("cache-control", "no-cache");
                    request.Headers.Add("dnt", "1");
                    request.Headers.Add("origin", "https://www.ticketmaster.com");
                    request.Headers.Add("ot-tracer-sampled", "true");
                    request.Headers.Add("ot-tracer-spanid", spanID);
                    request.Headers.Add("ot-tracer-traceid", traceID);
                    request.Headers.Add("pragma", "no-cache");
                    request.Headers.Add("referer", "https://www.ticketmaster.com");
                    request.Headers.Add("sec-ch-ua", "Google Chrome\";v=\"113\", \"Chromium\";v=\"116\", \"Not-A.Brand\";v=\"24");
                    request.Headers.Add("sec-ch-ua-mobile", "?0");
                    request.Headers.Add("sec-ch-ua-platform", "Windows");
                    request.Headers.Add("sec-fetch-dest", "empty");
                    request.Headers.Add("sec-fetch-mode", "cors");
                    request.Headers.Add("sec-fetch-site", "same-site");
                    request.Headers.Add("tmps-correlation-id", randomUUID);
                    request.Headers.Add("tmps-monetate-id", monetateID);
                    request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
                    request.Headers.Add("x-cmd", "reserve");
                    request.Headers.Add("x-eid", eventId);
                    request.Headers.Add("x-region", "east");
                    request.Content = content;

                    var response = await httpClient.SendAsync(request);

                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new HttpStatusCodeForbiddenException("Received a 403 Forbidden response.");
                    }

                    response.EnsureSuccessStatusCode();
                    string checkoutId = "";
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var ret = JsonConvert.DeserializeObject<CheckoutResponse>(responseContent);

                    if (ret != null && !string.IsNullOrEmpty(ret.data?.reserve?.requestId))
                    {
                        checkoutId = ret.data.reserve.requestId;
                        string checkoutURL = $"https://checkout.ticketmaster.com/{checkoutId}";
                        Thread.Sleep(10000);
                        Console.WriteLine($"event-{eventId},row-{row},section-{section}");
                        await checkoutLinkValidatorService.ValidateCheckoutLinkAsync(httpClient, checkoutURL);
                        
                        return checkoutURL;
                    }

                    retryCount++;

                }
                catch (HttpStatusCodeForbiddenException ex)
                {
                    // Handle the forbidden exception as needed
                    Console.WriteLine($"Retry {retryCount + 1}: HttpStatusCodeForbiddenException - {ex.Message}");
                    await httpClientService.InitializeAsync();
                    httpClient = httpClientService.GetHttpClient();
                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        Console.WriteLine("Max retries reached.");
                        throw;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        Console.WriteLine("Max retries reached.");
                        throw;
                    }
                }
            }

            return null;
        }

        private string GenerateRandomHex(int length)
        {
            byte[] data = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
            }
            return BitConverter.ToString(data).Replace("-", "").ToLower();
        }

    }

    public class HttpStatusCodeForbiddenException : Exception
    {
        public HttpStatusCodeForbiddenException(string message) : base(message)
        {
        }
    }

    public class CheckoutResponse
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public Reserve reserve { get; set; }
    }

    public class Reserve
    {
        public string requestId { get; set; }
    }
}