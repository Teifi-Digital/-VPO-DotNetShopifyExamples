using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DotNetShopifyExample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ShopifyService _shopifyService;

    public CustomersController(IConfiguration configuration)
    {
        _shopifyService = ShopifyService.Create(configuration);
    }

    [HttpGet]
    public async Task<List<Customer>> Get([FromQuery(Name = "q")] string q = "")
    {
        return await _shopifyService.GetCustomersAsync(q);
    }
    
    [HttpPost]
    public async Task<Customer> Create(Customer customer)
    {
        return await _shopifyService.CreateCustomerAsync(customer);
    }
    
    [HttpPost("{id}")]
    public async Task<Customer> Update(string id, Customer customer)
    {
        customer.Id = ShopifyService.ToGid("Customer", id);
        return await _shopifyService.UpdateCustomerAsync(customer);
    }
    
    [HttpDelete("{id}")]
    public async Task<CustomerDeleteResponse> Delete(string id)
    {
        var deletedCustomerId = await _shopifyService.DeleteCustomerAsync(ShopifyService.ToGid("Customer", id));
        return new CustomerDeleteResponse(deletedCustomerId);
    }
}

public class CustomerDeleteResponse
{
    [JsonProperty("deletedCustomerId")]
    public string DeletedCustomerId { get; set; }
    
    public CustomerDeleteResponse(string deletedCustomerId)
    {
        DeletedCustomerId = deletedCustomerId;
    }
}
