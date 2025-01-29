using EShop.Shared.DomainTools.DomainExceptions;

namespace EShop.Shared.DomainTools.Extensions
{
    public static class Check
    {
        public static T NotNullOrWhiteSpace<T>(T value, string parameterName)
        {
            if (value is string str && string.IsNullOrWhiteSpace(str))
            {
                throw new BadRequestException($"{parameterName} cannot be null or whitespace");
            }

            return value;
        }
    }
}