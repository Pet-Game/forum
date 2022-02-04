using Microsoft.AspNetCore.Mvc.RazorPages;
using PetGameForum.Data;
using PetGameForum.Services;

namespace PetGameForum.Pages.Moderation; 

public class Users : PageModel {
	private readonly PlayerService playerService;

	public List<User> UserList;
	public Users(PlayerService playerService) {
		this.playerService = playerService;
	}

	public async Task OnGet() {
		UserList = await playerService.GetAll();
	}
}