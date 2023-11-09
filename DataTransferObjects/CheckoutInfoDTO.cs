using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketmasterMonitor.DataTransferObjects
{
    public class CheckoutInfoDTO
    {
        public string EventName { get; set; }
        public string EventDateTime { get; set; }
        public string EventVenue { get; set; }
        public string TicketInfo { get; set; }
        public string SeatInfo { get; set; }
        public string Section { get; set; }
        public string Row { get; set; }
        public int Quantity { get; set; }
        public string Price { get; set; }
        public string CheckoutLink { get; set; }
    }
}
