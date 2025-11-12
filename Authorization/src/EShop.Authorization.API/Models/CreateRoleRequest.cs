namespace EShop.Authorization.API.Models
{
    public sealed class CreateRoleRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required IEnumerable<string> PermissionIds { get; set; }
    }
}
