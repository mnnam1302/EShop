using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.AppHost.Bootstrapping;

public static class ServiceConnectionNames
{
    public const string PostgreSql = "postgres";
    public const string Redis = "redis";
    public const string RabbitMq = "rabbitmq";
    public const string MongoDb = "mongodb";

    public const string TenancyApi = "tenancy";
    public const string AuthorizationApi = "authorization";
}