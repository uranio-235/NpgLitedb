namespace Test.Entity;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
