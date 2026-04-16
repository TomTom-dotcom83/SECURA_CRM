using Microsoft.AspNetCore.Authorization;

namespace SECURA.Web.Auth;

public static class SecuraPolicies
{
    public const string CanManageAgencies = nameof(CanManageAgencies);
    public const string CanViewSubmissions = nameof(CanViewSubmissions);
    public const string CanApproveSubmissions = nameof(CanApproveSubmissions);
    public const string CanViewClaims = nameof(CanViewClaims);
    public const string CanManageClaims = nameof(CanManageClaims);
    public const string CanViewCompliance = nameof(CanViewCompliance);
    public const string CanManageCompliance = nameof(CanManageCompliance);
    public const string CanViewReports = nameof(CanViewReports);
    public const string CanAdminister = nameof(CanAdminister);
    public const string ReadOnly = nameof(ReadOnly);

    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        options.AddPolicy(CanManageAgencies, p => p.RequireRole(
            RoleConstants.CarrierAdmin, RoleConstants.SalesManager, RoleConstants.AgencyCoordinator));

        options.AddPolicy(CanViewSubmissions, p => p.RequireRole(
            RoleConstants.CarrierAdmin, RoleConstants.SalesManager,
            RoleConstants.Underwriter, RoleConstants.UnderwritingManager,
            RoleConstants.AgencyCoordinator));

        options.AddPolicy(CanApproveSubmissions, p => p.RequireRole(
            RoleConstants.CarrierAdmin, RoleConstants.UnderwritingManager));

        options.AddPolicy(CanViewClaims, p => p.RequireRole(
            RoleConstants.CarrierAdmin, RoleConstants.ClaimsAdjuster, RoleConstants.ClaimsManager,
            RoleConstants.SalesManager, RoleConstants.AgencyCoordinator));

        options.AddPolicy(CanManageClaims, p => p.RequireRole(
            RoleConstants.CarrierAdmin, RoleConstants.ClaimsAdjuster, RoleConstants.ClaimsManager));

        options.AddPolicy(CanViewCompliance, p => p.RequireRole(
            RoleConstants.CarrierAdmin, RoleConstants.ComplianceAnalyst,
            RoleConstants.SalesManager, RoleConstants.AgencyCoordinator));

        options.AddPolicy(CanManageCompliance, p => p.RequireRole(
            RoleConstants.CarrierAdmin, RoleConstants.ComplianceAnalyst));

        options.AddPolicy(CanViewReports, p => p.RequireRole(RoleConstants.AllRoles));

        options.AddPolicy(CanAdminister, p => p.RequireRole(RoleConstants.CarrierAdmin));

        // Any authenticated user in any role
        options.AddPolicy(ReadOnly, p => p.RequireRole(RoleConstants.AllRoles));
    }
}
