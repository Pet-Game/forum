using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using PetGameForum.Data;

namespace PetGameForum.Services; 

public class RoleService {
	public const string PermissionClaimType = "permissions";
	public const string PermissionClaimPrefix = "permission.";

	private readonly UserManager<User> userManager;
	private readonly RoleManager<Role> roleManager;
	private readonly LogService logging;
	private readonly IConfiguration Config;

	public RoleService(UserManager<User> userManager, RoleManager<Role> roleManager, IConfiguration  config, LogService logging) {
		this.userManager = userManager;
		this.roleManager = roleManager;
		Config = config;
		this.logging = logging;
	}
	
	public async Task<bool> HasPermission(ClaimsPrincipal claims, Permission permission) {
		if (claims == null) return false;
		var user = await userManager.GetUserAsync(claims);
		return await HasPermission(user, permission);
	}

	public async Task<bool> HasPermission(User user, Permission permission) {
		if (user == null) return false;
		foreach (ObjectId roleId in user.Roles) {
			var role = await roleManager.FindByIdAsync(roleId.ToString());
			if (role != null && role.HasPermission(permission))
				return true;
		}
		return false;
	}

	public async Task<bool> HasPermission(ClaimsPrincipal claims, string permission) {
		if (claims == null) return false;
		var user = await userManager.GetUserAsync(claims);
		return await HasPermission(user, permission);
	}

	public async Task<bool> HasPermission(User user, string permission) {
		if (user == null) return false;
		foreach (ObjectId roleId in user.Roles) {
			var role = await roleManager.FindByIdAsync(roleId.ToString());
			if (role != null && role.HasPermission(permission))
				return true;
		}
		return false;
	}

	public static string Policy(Permission permission) => PermissionClaimPrefix + permission;

	public List<Role> GetAll() {
		return roleManager.Roles.ToList();
	}

	public async Task<Role> Get(ObjectId id) {
		return await roleManager.FindByIdAsync(id.ToString());
	}

	public async Task<Result> AddPermission(ObjectId roleId, Permission perm, User actor) {
		return await AddPermission(await roleManager.FindByIdAsync(roleId.ToString()), perm, actor);
	}

	public async Task<Result> AddPermission(Role role, Permission perm, User actor) {
		if (!await HasPermission(actor, Permission.EditRoles)) {
			return Err("Can't change role: insufficient permissions");
		}
		if (role is null) {
			return Err("Can't change role: invalid role");
		}
		if (role.Permissions.Contains(perm)) {
			return Err("Can't change role: Role already has that permission");
		}

		role.Permissions.Add(perm);
		await roleManager.UpdateAsync(role);

		await logging.Log(LogKind.EditRoles, null, $"added {perm} permission to {role.Name}", actor);
		return Ok();
	}
	
	public async Task<Result> RemovePermission(ObjectId roleId, Permission perm, User actor) {
		return await RemovePermission(await roleManager.FindByIdAsync(roleId.ToString()), perm, actor);
	}

	public async Task<Result> RemovePermission(Role role, Permission perm, User actor) {
		if (!await HasPermission(actor, Permission.EditRoles)) {
			return Err("Can't change role: insufficient permissions");
		}
		if (role is null) {
			return Err("Can't change role: invalid role");
		}
		if (!role.Permissions.Contains(perm)) {
			return Err("Can't change role: Role doesnt have that permission");
		}
		
		role.Permissions.Remove(perm);
		await roleManager.UpdateAsync(role);

		await logging.Log(LogKind.EditRoles, null, $"removed {perm} permission from {role.Name}", actor);
		return Ok();
	}

	public async Task<Result> CreateRole(string name, User actor) {
		if (!await HasPermission(actor, Permission.EditRoles)) {
			return Err("Can't change role: insufficient permissions");
		}
		if (string.IsNullOrWhiteSpace(name)) {
			return Err("Can't change role: invalid role name");
		}

		var role = new Role() {
			Id = ObjectId.GenerateNewId(),
			Name = name,
		};

		await roleManager.CreateAsync(role);
		
		await logging.Log(LogKind.EditRoles, null, $"created new role {name}", actor);
		return Ok();
	}
	
	public async Task<Result> DeleteRole(Role role, User actor) {
		if (!await HasPermission(actor, Permission.EditRoles)) {
			return Err("Can't change role: insufficient permissions");
		}
		if (role is null) {
			return Err("Can't change role: invalid role");
		}
		await roleManager.DeleteAsync(role);
		
		await logging.Log(LogKind.EditRoles, null, $"deleted role {role.Name}", actor);
		return Ok();
	}

	public async Task<Result> RenameRole(Role role, string name, User actor) {
		if (!await HasPermission(actor, Permission.EditRoles)) {
			return Err("Can't change role: insufficient permissions");
		}
		if (role is null) {
			return Err("Can't change role: invalid role");
		}
		if (string.IsNullOrWhiteSpace(name)) {
			return Err("Can't change role: invalid role name");
		}

		var oldName = role.Name;
		role.Name = name;
		await roleManager.UpdateAsync(role);

		await logging.Log(LogKind.EditRoles, null, $"renamed role from {oldName} to {name}", actor);
		return Ok();
	}
}