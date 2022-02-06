using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using PetGameForum.Data;
using PetGameForum.Services;

namespace PetGameForum.Pages.Moderation; 



public class Roles : PageModel {
	private RoleService roleService;
	private PlayerService playerService;
	
	public List<Role> RoleList;

	[BindProperty] public RoleCommand Command { get; set; }
	[BindProperty] public ChangePermission ChangePermission { get; set; }
	[BindProperty] public Rename RenameRole { get; set; }
	[BindProperty] public string NewRoleName { get; set; }
	[BindProperty] public string DeleteRole { get; set; }

	public Roles(RoleService roleService, PlayerService playerService) {
		this.roleService = roleService;
		this.playerService = playerService;
	}

	public void OnGet() {
		RoleList = roleService.GetAll();
	}

	public async Task<IActionResult> OnPostAsync() {
		var actor = await playerService.Get(User);
		switch (Command) {
			case RoleCommand.ChangePermission:{
				if (!ObjectId.TryParse(ChangePermission.RoleId, out var roleId)) return Redirect(Request.Path); //todo: error comm
				Result result;
				if(ChangePermission.Action == ChangePermission.ChangeAction.Add)
					result = await roleService.AddPermission(roleId, ChangePermission.Permission, actor);
				else
					result = await roleService.RemovePermission(roleId, ChangePermission.Permission, actor);
				if(!result) return Redirect(Request.Path); //todo: error comm
			} break;
			case RoleCommand.AddRole: {
				var result = await roleService.CreateRole(NewRoleName, actor);
				if(!result) return Redirect(Request.Path); //todo: error comm
			} break;
			case RoleCommand.DeleteRole: {
				ObjectId.TryParse(DeleteRole, out var roleId);
				var role = await roleService.Get(roleId);
				var result = await roleService.DeleteRole(role, actor);
				if(!result) return Redirect(Request.Path); //todo: error comm
			} break;
			case RoleCommand.Rename: {
				ObjectId.TryParse(RenameRole.RoleId, out var roleId);
				var role = await roleService.Get(roleId);
				var result = await roleService.RenameRole(role, RenameRole.Name, actor);
				if(!result) return Redirect(Request.Path); //todo: error comm
			} break;
			case RoleCommand.None:
			default: return Redirect(Request.Path); //todo: error comm
		}
		return Redirect(Request.Path);
	}
}

public enum RoleCommand {
	None,
	ChangePermission,
	AddRole,
	DeleteRole,
	Rename
}

public class Rename {
	public string RoleId { get; set; }
	public string Name { get; set; }
}

public class ChangePermission {
	public string RoleId { get; set; }
	public Permission Permission { get; set; }
	public ChangeAction Action { get; set; }
	public enum ChangeAction {
		Add,
		Remove,
	}
}

