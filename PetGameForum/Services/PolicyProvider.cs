using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using PetGameForum.Data;

namespace PetGameForum.Services;

class AuthorizePermissionAttribute : AuthorizeAttribute {
	public AuthorizePermissionAttribute(Permission permission) {
		Policy = RoleService.PermissionClaimPrefix + permission;
	}
}

class PermissionRequirement : IAuthorizationRequirement {
	public string Permission { get; }

	public PermissionRequirement(string permission) {
		Permission = permission;
	}
}

class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement> {

	public readonly RoleService RoleService;
	
	public PermissionAuthorizationHandler(RoleService roleService) {
		RoleService = roleService;
	}
	
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement) {
		var valid = await RoleService.HasPermission(context.User, requirement.Permission);
		if(valid) {
			context.Succeed(requirement);
		}
	}
}

public class PolicyProvider : IAuthorizationPolicyProvider {
	public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

	public PolicyProvider(IOptions<AuthorizationOptions> options) {
		FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
	}

	async Task<AuthorizationPolicy> IAuthorizationPolicyProvider.GetPolicyAsync(string policyName) {
		if (policyName.StartsWith(RoleService.PermissionClaimPrefix, StringComparison.OrdinalIgnoreCase)) {
			var policy = new AuthorizationPolicyBuilder();
			policy.AddRequirements(new PermissionRequirement(policyName));
			return policy.Build();
		}

		return await FallbackPolicyProvider.GetPolicyAsync(policyName);
	}

	Task<AuthorizationPolicy> IAuthorizationPolicyProvider.GetDefaultPolicyAsync() {
		return FallbackPolicyProvider.GetDefaultPolicyAsync();
	}

	Task<AuthorizationPolicy> IAuthorizationPolicyProvider.GetFallbackPolicyAsync() {
		return FallbackPolicyProvider.GetFallbackPolicyAsync();
	}
}