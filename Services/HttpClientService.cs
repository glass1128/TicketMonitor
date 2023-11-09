using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TicketmasterMonitor.Helpers;

namespace TicketmasterMonitor.Services
{
    public class HttpClientService
    {
        private HttpClient _httpClient;

        private readonly IncapsulaHelper _incapsulaHelper;

        public HttpClientService(IncapsulaHelper incapsulaHelper)
        {
            _incapsulaHelper = incapsulaHelper;
        }

        public HttpClient GetHttpClient()
        {
            if (_httpClient == null)
            {
                throw new InvalidOperationException("HttpClient has not been initialized. Call InitializeHttpClient first.");
            }

            return _httpClient;
        }

        public async Task InitializeAsync()
        {
            InitializeHttpClient();
        }

        public void Dispose()
        {
            DisposeHttpClient();
        }

        public void InitializeHttpClient()
        {
            if (_httpClient != null)
            {
                DisposeHttpClient();
            }

            var httpClientHandler = CreateHttpClientHandler().Result;
            _httpClient = new HttpClient(httpClientHandler);

            _httpClient.DefaultRequestHeaders.Add("authority", "www.ticketmaster.com");
            _httpClient.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            _httpClient.DefaultRequestHeaders.Add("accept-language", "en-US,en;q=0.9");
            _httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
            _httpClient.DefaultRequestHeaders.Add("dnt", "1");
            _httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Google Chrome\";v=\"113\", \"Chromium\";v=\"113\", \"Not-A.Brand\";v=\"24\"");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "none");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-user", "?1");
            _httpClient.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36");
        }

        public void DisposeHttpClient()
        {
            _httpClient?.Dispose();
            _httpClient = null;
        }
        
        private async Task<HttpClientHandler> CreateHttpClientHandler()
        {
            var (incapsulaProxyInfo, reeseToken) = await _incapsulaHelper.GetIncapsulaTokenAsync();

            var proxyUri = new Uri(incapsulaProxyInfo.Address.ToString());
            var proxyUsername = proxyUri.UserInfo.Split(':')[0];
            var proxyPassword = proxyUri.UserInfo.Split(':')[1];

            var proxyInfo = new WebProxy
            {
                Address = proxyUri,
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(userName: proxyUsername, password: proxyPassword)
            };

            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxyInfo,
                UseProxy = true,
            };

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("https://checkout.ticketmaster.com"), new Cookie("reese84", reeseToken));
            httpClientHandler.CookieContainer = cookieContainer;

            return httpClientHandler;
        }
    }
}
