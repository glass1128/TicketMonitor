using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketmasterMonitor.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Section { get; set; }
        public string? Row { get; set; }
        public string? Seats { get; set; }
        public string? Price { get; set; }
        public string? EventId { get; set; }
        public string? CheckoutLink { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CartInformation { get; set; }
        public int? Quantity { get; set; }
        public string? EstimatedExpiry { get; set; }
        public string? CartedBy { get; set; }
        public DateTime? CartedAt { get; set; }
        public string? Agent {  get; set; }

    }
}
