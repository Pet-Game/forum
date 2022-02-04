using System.Security.Claims;
using AspNetCore.Identity.MongoDbCore.Extensions;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using PetGameForum.Data;

namespace PetGameForum.Services; 

public class RoleService {
	public const string PermissionClaimType = "permissions";
	public const string PermissionClaimPrefix = "permission.";
	
	public readonly UserManager<User> UserManager;
	public readonly RoleManager<Role> RoleManager;
	public readonly IConfiguration Config;

	public RoleService(UserManager<User> userManager, RoleManager<Role> roleManager, IConfiguration  config) {
		UserManager = userManager;
		RoleManager = roleManager;
		Config = config;
	}
	
	public async Task<bool> HasPermission(ClaimsPrincipal claims, Permission permission) {
		if (claims == null) return false;
		var user = await UserManager.GetUserAsync(claims);
		return await HasPermission(user, permission);
	}

	public async Task<bool> HasPermission(User user, Permission permission) {
		if (user == null) return false;
		foreach (ObjectId roleId in user.Roles) {
			var role = await RoleManager.FindByIdAsync(roleId.ToString());
			if (role != null && role.HasPermission(permission))
				return true;
		}
		return false;
	}

	public async Task<bool> HasPermission(ClaimsPrincipal claims, string permission) {
		if (claims == null) return false;
		var user = await UserManager.GetUserAsync(claims);
		return await HasPermission(user, permission);
	}

	public async Task<bool> HasPermission(User user, string permission) {
		if (user == null) return false;
		foreach (ObjectId roleId in user.Roles) {
			var role = await RoleManager.FindByIdAsync(roleId.ToString());
			if (role != null && role.HasPermission(permission))
				return true;
		}
		return false;
	}

	public static string Policy(Permission permission) => PermissionClaimPrefix + permission;

	public List<Role> GetAll() {
		return RoleManager.Roles.ToList();
	}
}