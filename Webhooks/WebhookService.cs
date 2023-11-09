using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Webhook;
using TicketmasterMonitor.DataTransferObjects;

namespace TicketmasterMonitor.Webhooks
{
    public class WebhookService
    {
        private readonly Random _random = new Random();

        public async Task SendSuccessWebhook(CheckoutInfoDTO checkoutInfo)
        {
            string[] publicSuccessWebhooks =
            {
                "https://discord.com/api/webhooks/1166309707577823252/c_bfopoAHCYYz3pr59sshV85j4iPQVnE0YXFspAsQhgpnz_wi3mtxcf-HecBD31pSwLs",
                "https://discord.com/api/webhooks/1166309713915433061/G7wizBkeTFOuiSdzUpMh5nPGZzL9RseicruyixP8QdXUrB7-61TNxdCyhhgM_m6bmBaa",
                "https://discord.com/api/webhooks/1166309719120551996/gvA_4kPs2fRNlV9bDUrY6TZ2FMhoeMW3GWHup9t47xOIgDOYRstSfgvx0Fd4ohywmZKj",
                "https://discord.com/api/webhooks/1166309722073337897/-Py4k6VWVC_4OFSvHM-tApCLVfV8vTIX6AZ6Ae43YTOYo_CW2m1bHzrBaxeTpXSuxq2L",
                "https://discord.com/api/webhooks/1166309725315530773/1p3B19xYAtyw9E9HX4cJmpoSm3BlmEhA0SlUycEn00Gn76sZzON7QAJxX9e8rUTI94Dn"
            };

            string randomPublicSuccessWebhook = publicSuccessWebhooks[_random.Next(publicSuccessWebhooks.Length)];

            try
            {
                var webhook = new DiscordWebhookClient(randomPublicSuccessWebhook);

                var builder = new EmbedBuilder()
                    .WithTitle(checkoutInfo.EventName)
                    .WithColor(Color.Orange) // Adjust color as needed
                    .WithAuthor("Ticketmaster Restock Found")
                    .AddField("Info", checkoutInfo.SeatInfo, true)
                    .AddField("Section", checkoutInfo.Section, false)
                    .AddField("Row", checkoutInfo.Row, true)
                    .AddField("Seat", checkoutInfo.SeatInfo, true)
                    .AddField("Quantity", checkoutInfo.Quantity, false)
                    .AddField("Checkout", checkoutInfo.CheckoutLink, false)
                    .WithFooter("Ticketmaster - Blake#8678")
                    .WithCurrentTimestamp();

                await webhook.SendMessageAsync(embeds: new[] { builder.Build() });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Handle the exception as needed
            }
        }

        public async Task SendStartupWebhook(string timetoBoot,
            string info,
            string agent)
        {
            string[] adminServerWebhooks =
            {
                "https://discord.com/api/webhooks/1161282192941522955/lZp-FIwq-7IlY2LJ6JdsQZyuwBk-uS-ECNjGQvHeSwYIQf945Bd2ZM6eAor0-3PL9lkq",
                "https://discord.com/api/webhooks/1161282224281354260/aUQqDMSgDyKv76x6QIyBShUWmk1GsosvFN715RjABlpoh3gXlcab8adhKSzUZCuwfdAr",
                "https://discord.com/api/webhooks/1161282254228684873/i70U8_5NgxgXR2CKrfxVRC1sSqqDuvcrFYNdFO9FNJuUbXna5Kr_F3yCcaZh_cht5hPV",
                "https://discord.com/api/webhooks/1161282440837484585/BxXJ50n59d_xC_7X3_WJ2TRmn2xCOXhC8FCD0AyRo7b4t2ZGVZcrRaLf1djwW_5Q9aFD"
            };

            string randomAdminServerWebhook = adminServerWebhooks[_random.Next(adminServerWebhooks.Length)];

            try
            {
                var webhook = new DiscordWebhookClient(randomAdminServerWebhook);

                var builder = new EmbedBuilder()
                    .WithColor(Color.Orange) // Adjust color as needed
                    .WithAuthor("Monitor Booting")
                    .AddField("Info", info, false)
                    .AddField("Time to boot", timetoBoot, false)
                    .AddField("Agent", agent, false)

                    .WithFooter("Ticketmaster - Blake#8678")
                    .WithCurrentTimestamp();

                await webhook.SendMessageAsync(embeds: new[] { builder.Build() });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Handle the exception as needed
            }
        }

        public async Task ATCFailure(string eventName,
            string url,
            string failureCount,
            string message)
        {
            string[] adminFailureWebhooks =
            {
                "https://discord.com/api/webhooks/1161282192941522955/lZp-FIwq-7IlY2LJ6JdsQZyuwBk-uS-ECNjGQvHeSwYIQf945Bd2ZM6eAor0-3PL9lkq",
                "https://discord.com/api/webhooks/1161282224281354260/aUQqDMSgDyKv76x6QIyBShUWmk1GsosvFN715RjABlpoh3gXlcab8adhKSzUZCuwfdAr",
                "https://discord.com/api/webhooks/1161282254228684873/i70U8_5NgxgXR2CKrfxVRC1sSqqDuvcrFYNdFO9FNJuUbXna5Kr_F3yCcaZh_cht5hPV",
                "https://discord.com/api/webhooks/1161282440837484585/BxXJ50n59d_xC_7X3_WJ2TRmn2xCOXhC8FCD0AyRo7b4t2ZGVZcrRaLf1djwW_5Q9aFD"
            };

            string randomFailureWebhook = adminFailureWebhooks[_random.Next(adminFailureWebhooks.Length)];

            try
            {
                var webhook = new DiscordWebhookClient(randomFailureWebhook);

                var builder = new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithAuthor("Pausing Event, FCFS or excess non-standard stock detected")
                    .AddField("Event", eventName, false)
                    .AddField("Url", url, false)
                    .AddField("Failure Count", failureCount, false)
                    .AddField("Message", message, false)

                    .WithFooter("Ticketmaster - Blake#8678")
                    .WithCurrentTimestamp();

                await webhook.SendMessageAsync(embeds: new[] { builder.Build() });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Handle the exception as needed
            }
        }
    }
}
