using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetGameForum.Data;
using PetGameForum.Services;

namespace PetGameForum.Pages.Moderation; 

[AuthorizePermission(Permission.SeeModeratorArea)]
public class ModerationIndexModel : PageModel {
	public void OnGet() {
		
	}
}