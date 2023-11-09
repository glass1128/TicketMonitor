using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketmasterMonitor.Models
{
    public class ReserveRequest
    {
        public string Query { get; set; }
        public Variables Variables { get; set; }
    }

    public class Variables
    {
        public ReserveInput ReserveInput { get; set; }
    }

    public class ReserveInput
    {
        public string EventId { get; set; }
        public string RequestorId { get; set; }
        public RequestContext RequestContext { get; set; }
        public Ticket[] Tickets { get; set; }
    }

    public class RequestContext
    {
        public string Channel { get; set; }
        public string Locale { get; set; }
    }

    public class Ticket
    {
        public string Row { get; set; }
        public string Section { get; set; }
        public TicketType[] TicketTypes { get; set; }
        public InventoryDetail InventoryDetail { get; set; }
    }

    public class TicketType
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
    }

    public class InventoryDetail
    {
        public string Type { get; set; }
    }
}
