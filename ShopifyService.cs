using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;

namespace DotNetShopifyExample;

public class ShopifyService
{
    private string _shop;
    private string _accessToken;
    
    private string customerQuery = @"#graphql
        fragment CustomerFields on Customer {
            id
            displayName
            firstName
            lastName
            email
            note
        }
    ";

    private ShopifyService(string shop, string accessToken)
    {
        _shop = shop;
        _accessToken = accessToken;
    }
    
    public static ShopifyService Create(IConfiguration configuration)
    {
        var shop = configuration["Shopify:Shop"];
        if (shop == null)
        {
            throw new Exception("Shopify:Shop is not configured");
        }

        var accessToken = configuration["Shopify:AccessToken"];
        if (accessToken == null)
        {
            throw new Exception("Shopify:AccessToken is not configured");
        }
        return new ShopifyService(shop, accessToken);
    }
    
    public static string ToGid(string _namespace, string _id) => $"gid://shopify/{_namespace}/{_id}";

    public async Task<GraphQLResponse<T>> QueryAsync<T>(string query, object? variables = null)
    {
        var client = new GraphQLHttpClient($"https://{_shop}.myshopify.com/admin/api/2023-04/graphql.json", new NewtonsoftJsonSerializer());
        client.HttpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", _accessToken);
        var request = new GraphQLRequest
        {
            Query = query,
            Variables = variables
        };
        return await client.SendQueryAsync<T>(request);
    }
    
    public async Task<List<Customer>> GetCustomersAsync(string query, int first = 25)
    {
        var gqlQuery = $@"#graphql
            {customerQuery}
            query ($query: String!, $first: Int!) {{
                customers(query: $query, first: $first) {{
                    nodes {{
                        ...CustomerFields
                    }}
                }}
            }}
        ";
        var response = await QueryAsync<Dictionary<string, Connection<Customer>>>(
            gqlQuery,
            new Dictionary<string, object>
            {
                {"query", query},
                {"first", first}
            }
        );
        return response.Data["customers"].Nodes;
    }
    
    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        var gqlQuery = $@"#graphql
            {customerQuery}
            mutation customerCreate($input: CustomerInput!) {{
                customerCreate(input: $input) {{
                    customer {{
                        ...CustomerFields
                    }}
                    userErrors {{
                        field
                        message
                    }}
                }}
            }}
        ";
        var response = await QueryAsync<Dictionary<string, CustomerCreateResponse>>(
            gqlQuery,
            new Dictionary<string, object>
            {
                {
                    "input", new Dictionary<string, object> {
                        {"firstName", customer.FirstName},
                        {"lastName", customer.LastName},
                        {"email", customer.Email},
                    }
                }
            }
        );
        var customerCreate = response.Data["customerCreate"];
        if (customerCreate == null)
        {
            throw new Exception("customerCreate is null");
        }

        if (customerCreate.UserErrors.Count > 0)
        {
            throw new Exception(customerCreate.UserErrors[0].Message);
        }
        return customerCreate.Customer;
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        var gqlQuery = $@"#graphql
            {customerQuery}
            mutation customerCreate($input: CustomerInput!) {{
                customerUpdate(input: $input) {{
                    customer {{
                        ...CustomerFields
                    }}
                    userErrors {{
                        field
                        message
                    }}
                }}
            }}
        ";
        var response = await QueryAsync<Dictionary<string, CustomerCreateResponse>>(
            gqlQuery,
            new Dictionary<string, object>
            {
                {
                    "input", new Dictionary<string, object> {
                        {"id", customer.Id!},
                        {"note", customer.Note ?? ""}
                    }
                }
            }
        );
        var customerUpdate = response.Data["customerUpdate"];
        if (customerUpdate == null)
        {
            throw new Exception("customerUpdate is null");
        }

        if (customerUpdate.UserErrors.Count > 0)
        {
            throw new Exception(customerUpdate.UserErrors[0].Message);
        }
        return customerUpdate.Customer;
    }

    public async Task<string> DeleteCustomerAsync(string id)
    {
        var gqlQuery = @"#graphql
            mutation customerDelete($input: CustomerDeleteInput!) {
                customerDelete(input: $input) {
                    deletedCustomerId
                    userErrors {
                        field
                        message
                    }
                }
            }
        ";
        Console.WriteLine($"Deleting customer {id}");
        var response = await QueryAsync<Dictionary<string, CustomerDeleteResponse>>(
            gqlQuery,
            new Dictionary<string, object>
            {
                {
                    "input", new Dictionary<string, object> {
                        {"id", id}
                    }
                }
            }
        );
        var customerDelete = response.Data["customerDelete"];
        if (customerDelete == null)
        {
            throw new Exception("customerDelete is null");
        }

        if (customerDelete.UserErrors.Count > 0)
        {
            throw new Exception(customerDelete.UserErrors[0].Message);
        }
        return customerDelete.DeletedCustomerId!;
    }
}

class Connection<T>
{
    [JsonProperty("nodes")]
    public List<T> Nodes { get; set; }
}

class CustomerCreateResponse
{
    [JsonProperty("customer")]
    public Customer Customer { get; set; }
    
    [JsonProperty("userErrors")]
    public List<UserError> UserErrors { get; set; }
}

class CustomerDeleteResponse
{
    [JsonProperty("deletedCustomerId")]
    public string? DeletedCustomerId { get; set; }
    
    [JsonProperty("userErrors")]
    public List<UserError> UserErrors { get; set; }
}

class UserError
{
    [JsonProperty("field")]
    public string Field { get; set; }
    
    [JsonProperty("message")]
    public string Message { get; set; }
}

public class Customer
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    
    [JsonProperty("displayName")]
    public string? DisplayName { get; set; }
    
    [JsonProperty("firstName")]
    public string FirstName { get; set; }
    
    [JsonProperty("lastName")]
    public string LastName { get; set; }
    
    [JsonProperty("email")]
    public string Email { get; set; }
    
    [JsonProperty("note")]
    public string? Note { get; set; }
}
