using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.AppHost.Bootstrapping;

public static class ServiceConnectionNames
{
    public const string Redis = "redis";
    public const string PostgreSql = "postgres";
    public const string MongoDb = "mongodb";

    public const string RabbitMq = "rabbitmq";

    public const string TenancyApi = "tenancy-api";
    public const string AuthorizationApi = "authorization-api";
    public const string CatalogApi = "catalog-api";
}
