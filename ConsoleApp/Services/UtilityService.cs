namespace ConsoleApp.Services;

public class UtilityService
{
    public decimal CalculateDiscount(decimal orderAmount, bool isPremiumCustomer, DateTime orderDate)
    {
        if (orderAmount <= 0)
            throw new ArgumentException("Order amount must be positive");

        decimal discount = 0;

        // loyalty discount
        if (isPremiumCustomer)
        {
            discount += 0.05m;
        }

        // volume discount
        if (orderAmount > 1000)
        {
            discount += 0.04m;
        }

        // weekend campaign
        if (orderDate.DayOfWeek == DayOfWeek.Saturday ||
            orderDate.DayOfWeek == DayOfWeek.Sunday)
        {
            discount += 0.02m;
        }

        // cap discount
        if (discount > 0.10m)
        {
            discount = 0.10m;
        }

        return orderAmount * discount;
    }
}


