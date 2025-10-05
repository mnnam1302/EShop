# ApiGateway Integration Example

## How to integrate the decoupled JWT authentication in ApiGateway

### 1. Update Startup.cs or Program.cs

```csharp
using EShop.Shared.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add decoupled JWT authentication
builder.Services.AddDecoupledJwtAuthentication(builder.Configuration);

// Add other services...
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Use authentication
app.UseAuthentication();
app.UseAuthorization();

// Map reverse proxy with authentication
app.MapReverseProxy();

app.Run();
```

### 2. Configuration in appsettings.json

```json
{
  "Services": {
    "Authorization": {
      "BaseUrl": "https://localhost:7001"
    }
  },
  "ReverseProxy": {
    "Routes": {
      "catalog-route": {
        "ClusterId": "catalog-cluster",
        "AuthorizationPolicy": "default",
        "Match": {
          "Path": "/api/catalog/{**catch-all}"
        }
      },
      "identity-route": {
        "ClusterId": "identity-cluster",
        "AuthorizationPolicy": "default",
        "Match": {
          "Path": "/api/identity/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "catalog-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "https://localhost:7002/"
          }
        }
      },
      "identity-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "https://localhost:7003/"
          }
        }
      }
    }
  }
}
```

### 3. Example Usage in Other Microservices

#### Catalog Service Example

```csharp
// In Catalog service Program.cs
using EShop.Shared.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add decoupled JWT authentication
builder.Services.AddDecoupledJwtAuthentication(builder.Configuration);

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

#### Catalog Controller Example

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // This will use our decoupled JWT authentication
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        // Access user claims from JWT token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantId = User.FindFirst("tenant_id")?.Value;
        var userType = User.FindFirst("user_type")?.Value;

        // Your business logic here...
        var products = await GetProductsForTenant(tenantId);
        
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        // Authenticated user can create products
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantId = User.FindFirst("tenant_id")?.Value;

        // Your business logic here...
        var product = await CreateProductAsync(request, tenantId, userId);
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
}
```

## Flow Diagram

```
???????????????    ????????????????    ???????????????????    ???????????????????
?   Client    ??????  ApiGateway  ?????? Catalog Service ??????   Database      ?
???????????????    ????????????????    ???????????????????    ???????????????????
                           ?                      ?
                           ?                      ?
                   ????????????????      ???????????????????
                   ? JWT Validation?      ? Business Logic  ?
                   ? (Local Cache) ?      ? (Tenant Aware)  ?
                   ????????????????      ???????????????????
                           ?
                           ?
                   ????????????????
                   ?Authorization ? (Only if cache miss)
                   ?   Service    ? 
                   ????????????????
```

## Benefits

? **High Availability**: Services work even if Authorization service is down
? **Performance**: Local JWT validation with caching
? **Security**: RSA-signed tokens, tenant isolation
? **Scalability**: No centralized bottleneck
? **Loose Coupling**: Services are independent