using Microsoft.AspNetCore.SignalR;

namespace StocksApp.Core.Hubs
{
    /// <summary>
    /// A hub for managing real-time trade updates.
    /// Clients can connect to this hub to receive notifications about trade executions, price changes, and other relevant events. 
    /// </summary>
    public class TradeHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
