using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TicketmasterMonitor.Helpers
{
    public class IncapsulaHelper
    {
        private readonly HttpClient httpClient;
        private readonly Random random = new Random();
        private readonly string reese84Url = "https://epsf.ticketmaster.com/eps-d?d=www.ticketmaster.com";

        public IncapsulaHelper()
        {
            httpClient = new HttpClient();
        }

        public async Task<(string sensor, string userAgent)> GetIncapsulaSensorAsync(string apiKey, string proxy)
        {
            while (true)
            {
                try
                {
                    string url = $"https://api.yoghurtbot.net/incapsula/reese84?url={reese84Url}";

                    // Create an HttpClient instance for this request

                    httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    // Set the proxy using HttpClientHandler
                    var httpClientHandler = new HttpClientHandler();
                    httpClientHandler.Proxy = new WebProxy(proxy);
                    httpClientHandler.UseProxy = true;

                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        // Add headers to the request
                        requestMessage.Headers.Add("accept", "application/json");

                        // Send the request and get the response
                        var response = await httpClient.SendAsync(requestMessage);

                        // Check if the response is successful
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var jsonObject = JObject.Parse(json);

                            if (jsonObject["success"].Value<bool>())
                            {
                                var userAgent = jsonObject["userAgent"].Value<string>();
                                var reese84Payload = jsonObject["reese84Payload"];


                                return (reese84Payload.ToString(), userAgent);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    await Task.Delay(2000); // Sleep for 2 seconds before retrying
                }
            }
        }

        public async Task<(WebProxy ProxyInfo, string Reese84Token)> GetIncapsulaTokenAsync()
        {
            while (true)
            {

                try
                {
                    string filePath = @"Proxies\isps.txt";

                    string[] lines = File.ReadAllLines(filePath);
                    string randomLine = lines[random.Next(lines.Length)];
                    string[] proxyInfoArray = randomLine.Split(':');
                    string proxyInfoString = $"http://{proxyInfoArray[2]}:{proxyInfoArray[3]}@{proxyInfoArray[0]}:{proxyInfoArray[1]}";

                    var (sensor, userAgent) = await GetIncapsulaSensorAsync("e72aba95-4ce3-48f9-b4b7-7093c1d2ebde", proxyInfoString);

                    var request = new HttpRequestMessage(HttpMethod.Post, reese84Url);
                    request.Headers.Add("user-agent", userAgent);
                    request.Headers.Add("accept", "application/json; charset=utf-8");
                    request.Content = new StringContent(sensor);

                    var proxyInfo = new WebProxy(proxyInfoString);
                    var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                    var reese84TokenJson = await response.Content.ReadAsStringAsync();

                    var reese84TokenObject = JObject.Parse(reese84TokenJson);
                    var tokenValue = reese84TokenObject["token"].Value<string>();

                    return (proxyInfo, tokenValue);
                }
                catch (Exception e)
                {
                    await Task.Delay(5000); // Sleep for 5 seconds
                }
            }
        }
    }
}
