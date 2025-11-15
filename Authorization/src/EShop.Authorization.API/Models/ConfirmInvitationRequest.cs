namespace EShop.Authorization.API.Models;

public sealed class ConfirmInvitationRequest
{
    public required string TemporaryPassword { get; init; }
    public required string NewPassword { get; init; }
}
