namespace EShop.Identity.Domain.ActionPopulators;

public enum UserActions
{
    ViewUsers,
    InviteUser,
    EditUser,
    DeleteUser,
    AssignRoles,
    SetPassword,
}

public enum UserRolesActions
{
    ViewRoles,
    ManageRoles,
}

public enum  UserOrganizationActions
{
    ViewOrganizations,
    ManageOrganizations,
}

public enum UserTenantActions
{
    ViewSystemSettings,
    ManageSystemSettings,
}