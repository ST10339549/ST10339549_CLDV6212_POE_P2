using Microsoft.Azure.Cosmos.Table;
using System.ComponentModel.DataAnnotations;

namespace ST10339549_CLDV6212_POE.Models
{
  public class Customer : TableEntity
  {
    [Required]
    public string? CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerSurname { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerAddress { get; set; }
    public Customer()
    {
      PartitionKey = "Customer";
      RowKey = CustomerId ?? Guid.NewGuid().ToString();
    }
  }
}
