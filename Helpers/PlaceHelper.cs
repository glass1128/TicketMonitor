using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketmasterMonitor.Helpers
{
    public class PlaceHelper
    {
        private readonly HttpClient httpClient;

        public PlaceHelper(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
