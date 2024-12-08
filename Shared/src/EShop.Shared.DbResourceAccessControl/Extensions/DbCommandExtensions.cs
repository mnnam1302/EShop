using Npgsql;
using NpgsqlTypes;
using System.Data.Common;

namespace EShop.Shared.DbResourceAccessControl.Extensions;

internal static class DbCommandExtensions
{
    /// <summary>
    /// See https://www.npgsql.org/doc/basic-usage.html
    /// </summary>
    /// <param name="dbCommand"></param>
    /// <param name="commandText"></param>
    /// <returns></returns>
    public static DbCommand WithPostgreSqlCommandText(this DbCommand dbCommand, string commandText)
    {
        dbCommand.CommandText = commandText;
        return dbCommand;
    }

    /// <summary>
    /// See https://www.npgsql.org/doc/basic-usage.html
    /// </summary>
    /// <param name="dbCommand"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static DbCommand WithPostgreSqlPositionalParameter<T>(this DbCommand dbCommand, T? value, NpgsqlDbType? type = null)
    {
        var typedParam = new NpgsqlParameter<T> { TypedValue = value };
        if (type.HasValue)
        {
            typedParam.NpgsqlDbType = type.Value;
        }

        dbCommand.Parameters.Add(typedParam);

        return dbCommand;
    }
}