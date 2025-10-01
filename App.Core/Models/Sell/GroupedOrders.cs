using App.Core.DTOs.Sell;

namespace App.Core.Models.Sell;

public class GroupedOrders
{
    public string OrderNumber { get; set; }
    public IEnumerable<OrderDto> Orders { get; set; }
}