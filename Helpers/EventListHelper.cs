using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TicketmasterMonitor.Models;
using TicketmasterMonitor.Services;

namespace TicketmasterMonitor.Helpers
{
    public class EventListHelper
    {

        public EventListHelper()
        {
        }

        public async Task<List<Event>> GetEventsToMonitor()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // You can add any additional configuration you need for this HttpClient
                    //httpClient.DefaultRequestHeaders.Add("User-Agent", "YourUserAgentString");

                    var response = await httpClient.GetAsync("https://api.beadmissions.com/api/events");

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        var events = JsonConvert.DeserializeObject<List<Event>>(content);

                        return events;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string GetAccessToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("4F8F0946B4A9E858DA3F5972E3FFCB20955F167257A959F868D9563FB608BA7C");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "System"),
                    new Claim(ClaimTypes.Name, "System"),
                    new Claim(ClaimTypes.Email, "System@beadmissions.com"),
                    new Claim("isAdmin", "true")
                }),
                        Expires = DateTime.MaxValue,
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                            SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
