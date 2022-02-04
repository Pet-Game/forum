using Microsoft.AspNetCore.Mvc.RazorPages;
using PetGameForum.Data;
using PetGameForum.Services;

namespace PetGameForum.Pages.Moderation; 

public class Roles : PageModel {
	private RoleService roleService;
	
	public List<Role> RoleList;
	
	public Roles(RoleService roleService) {
		this.roleService = roleService;
	}

	public void OnGet() {
		RoleList = roleService.GetAll();
	}
}