using Newtonsoft.Json.Linq;
using Wiry.Base32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using TicketmasterMonitor.Flatbuffer;
using Microsoft.Extensions.Logging;

namespace TicketmasterMonitor.Services
{
    public class WebsocketService
    {
        private readonly FlatbufferDecode flatbufferDecode;
        private readonly AddToCartService addToCartService;
        private readonly HttpClientService httpClientService;

        private const string WebSocketUrl = "wss://marketplace.prod.pub-tmaws.io/avpp/v2/graphql?app=PRD2663_EDPAPP_ICCP&sessionId=3%3ASS%2Bje7d5E5aXCL68WJFg3A%3D%3D%3AhBrvV42%2F5iWuHAoM4oo7f%2FhIQAwCjvHz4s12jjPfXmfOVXSzJ%2F%2FlZSXMGHx3mXGS2AKhpjoZDGC%2F9jy7KBElYJq6HY9j1uqDYLKNfVJIpY42%2B%2B62FvucxIQEo3dfAesO4%2FY2pqRMzpjVNObnLOshxMQfelksYJbWR0pkPQpCXmjUgZYJNpdv4xkLYxhfh9phgN9w3pBba9fi5Qz%2FQu1zMhT1okqu%2BBIl%2B8PZX%2FtAuPXSX3qK%2FxobpPmBDe75UvC1RihmXmtnFn4LvvT7DUqSSv3G2bQSK9JW1zpaH6vzVrCoNt77Vr079W0dhsJDQ2dSGgiyg9PQzL5YeUpGEjieJ8yFYh%2Bga4HDgnUbkaj9EFuztg0kpBSsX9DZvb%2F0Vivf3fbCBPz%2FKn%2BrDeCp512n6gyLvAgJqkC52hlYmjvJky9%2BMg1xf4SOuKpCOTfn0nVKuIQ7h0fTvpvaj9K8EpnRu3TNH6n%2B7240E1eoqydA4Hxx86AEgLZ%2F3iGGRIEd9ae7wbd9cqYVwiaaS4nEC0WVqg%3D%3D%3AChOVxvLZnAImeFOF5%2F0R7PSCgWytbG9zSCaTm83i2h8%3D";

        private readonly ClientWebSocket clientWebSocket = new ClientWebSocket();
        private CancellationTokenSource cancellationTokenSource;

        public WebsocketService(FlatbufferDecode flatbufferDecode, AddToCartService addToCartService, HttpClientService httpClientService)
        {
            this.flatbufferDecode = flatbufferDecode;
            this.addToCartService = addToCartService;
            this.httpClientService = httpClientService;
        }

        public async Task StartWebSocketAsync(string eventId, string requesterId, HttpClient httpClient)
        {
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await clientWebSocket.ConnectAsync(new Uri(WebSocketUrl), cancellationTokenSource.Token);
                Console.WriteLine("WebSocket connection established successfully.");
                await SendInitMessageAsync();
                await SendStartMessageAsync(eventId);

                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await ReceiveMessageAsync(eventId, requesterId, httpClient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket connection error: {ex}");
            }
        }

        public async Task StopWebSocketAsync()
        {
            if (clientWebSocket.State == WebSocketState.Open)
            {
                cancellationTokenSource.Cancel();
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
        }

        private async Task SendInitMessageAsync()
        {
            var initMessage = new
            {
                type = "connection_init",
                payload = new { }
            };

            await SendMessageAsync(initMessage);
            Console.WriteLine("Sent connection_init message successfully.");
        }

        private async Task SendStartMessageAsync(string eventId)
        {
            var startMessage = new
            {
                id = "1",
                type = "start",
                payload = new
                {
                    variables = new
                    {
                        eventId = eventId,
                        lastReceivedVersion = (string)null
                    },
                    extensions = new { },
                    operationName = "AvailabilityChanged",
                    query = "subscription AvailabilityChanged($eventId: String!, $unlockToken: String, $lastReceivedVersion: String, $displayId: String) {\n availability(\n eventId: $eventId\n unlockToken: $unlockToken\n lastReceivedVersion: $lastReceivedVersion\n displayId: $displayId\n ) {\n buffer\n __typename\n }\n }"
                }
            };

            await SendMessageAsync(startMessage);
            Console.WriteLine("Sent start message successfully.");
        }

        private async Task SendMessageAsync(object message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message));
            var buffer = new ArraySegment<byte>(messageBytes);

            await clientWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationTokenSource.Token);
        }

        private async Task ReceiveMessageAsync(string eventId, string requesterId, HttpClient httpClient)
        {
            var buffer = new Memory<byte>(new byte[16384]);
            var result = await clientWebSocket.ReceiveAsync(buffer, cancellationTokenSource.Token);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var messageBytes = buffer.Slice(0, result.Count).ToArray();
                var message = Encoding.UTF8.GetString(messageBytes);

                Console.WriteLine($"Received Misc Message");

                if (message.Contains("buffer"))
                {
                    Console.WriteLine($"Received Availability For: {eventId}");
                    var availableSeats = await flatbufferDecode.Decode(message);
                    if (availableSeats.TryGetValue("statuses", out var statusesArray) && statusesArray is JArray)
                    {
                        foreach (var statusToken in statusesArray)
                        {
                            string encodedStatus = statusToken.Value<string>();

                            try
                            {
                                int padding = (8 - (encodedStatus.Length % 8)) % 8;
                                encodedStatus = encodedStatus.PadRight(encodedStatus.Length + padding, '=');
                                byte[] decodedBytes = Base32Encoding.Standard.ToBytes(encodedStatus);
                                string decodedStatus = Encoding.UTF8.GetString(decodedBytes);

                                string[] parts = decodedStatus.Split(':');

                                string section = parts[0];
                                string row = parts[1];
                                string seat = parts[2];
                                Thread.Sleep(2000);
                                Console.WriteLine(decodedStatus);
                                new Thread((ThreadStart)(async () =>
                                {
                                    var checkoutURL = await addToCartService.AddToCartNormalAsync(eventId, requesterId, row, section, httpClient);
                                })).Start();
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Invalid base32 data: " + encodedStatus);
                            }
                            catch (HttpStatusCodeForbiddenException)
                            {
                                await httpClientService.InitializeAsync();
                                httpClient = httpClientService.GetHttpClient();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Exception in AddToCartNormalAsync: " + ex.Message);
                            }
                        }
                    }
                }
            }
        }
    }
}