using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketmasterMonitor.Context;
using TicketmasterMonitor.DataTransferObjects;
using TicketmasterMonitor.Models;

namespace TicketmasterMonitor.Services
{
    public class SaveCartsToDatabase
    {
        private readonly DatabaseContext dbContext;

        public SaveCartsToDatabase(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<bool> SaveCartToDatabase(CheckoutInfoDTO checkoutInfo, string eventId)
        {
            string agent = Environment.MachineName;
            try
            {
                var cart = new Cart
                {
                    EventId = eventId,
                    Section = checkoutInfo.Section,
                    Row = checkoutInfo.Row,
                    Seats = checkoutInfo.SeatInfo,
                    Quantity = checkoutInfo.Quantity,
                    CheckoutLink = checkoutInfo.CheckoutLink,
                    CreatedAt = DateTime.UtcNow,
                    Agent = agent
                };

                dbContext.Carts.Add(cart);
                await dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
