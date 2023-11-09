using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TicketmasterMonitor.Flatbuffer;
using TicketmasterMonitor.Helpers;
using TicketmasterMonitor.Services;
using TicketmasterMonitor.Webhooks;

namespace TicketmasterMonitor
{
    class Program
    {
        private readonly HttpClientService httpClientService;
        private readonly EventListHelper eventListHelper;
        private readonly EventPageDataRetrieverService eventPageDataRetriever;
        private readonly CheckoutRequesterIDService checkoutRequesterIDService;
        private readonly WebsocketService websocketService;

        public Program(HttpClientService httpClientService, EventListHelper eventListHelper, EventPageDataRetrieverService eventPageDataRetriever, WebsocketService websocketService, CheckoutRequesterIDService checkoutRequesterIDService)
        {
            this.httpClientService = httpClientService;
            this.eventListHelper = eventListHelper;
            this.eventPageDataRetriever = eventPageDataRetriever;
            this.websocketService = websocketService;
            this.checkoutRequesterIDService = checkoutRequesterIDService;
        }

        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<HttpClientService>()
                .AddSingleton<EventListHelper>()
                .AddSingleton<CheckoutRequesterIDService>()
                .AddTransient<CheckoutLinkValidatorService>()
                .AddTransient<IncapsulaHelper>()
                .AddTransient<WebsocketService>()
                .AddTransient<FlatbufferDecode>()
                .AddTransient<AddToCartService>()
                .BuildServiceProvider();

            var httpClientService = serviceProvider.GetRequiredService<HttpClientService>();
            var eventListHelper = serviceProvider.GetRequiredService<EventListHelper>();
            var eventPageDataRetriever = new EventPageDataRetrieverService(httpClientService);
            var websocketService = serviceProvider.GetRequiredService<WebsocketService>();
            var checkoutRequesterIDService = serviceProvider.GetRequiredService<CheckoutRequesterIDService>();

            var program = new Program(httpClientService, eventListHelper, eventPageDataRetriever, websocketService, checkoutRequesterIDService);
            await program.RunAsync();
        }

        public async Task RunAsync()
        {
            var webhookService = new WebhookService();
            string agent = Environment.MachineName;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var events = await eventListHelper.GetEventsToMonitor();

            foreach (var eventItem in events)
            {
                await httpClientService.InitializeAsync();

                var httpClient = httpClientService.GetHttpClient();

                var eventId = eventItem.EventId;

                var requesterId = await checkoutRequesterIDService.GetRequesterId(eventId, httpClient);

                await websocketService.StartWebSocketAsync(eventId, requesterId, httpClient);
            }

            stopwatch.Stop();
            var bootTime = stopwatch.Elapsed.ToString();

            await webhookService.SendStartupWebhook(bootTime, "no info to display", agent);
        }
    }
}