using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetGameForum.Data;
using PetGameForum.Services;

namespace PetGameForum.Pages; 

public class PlayerModel : PageModel {
	private readonly UserManager<User> userManager;
	private readonly PlayerService playerService;

	public User User;
	
	public PlayerModel(UserManager<User> userManager, PlayerService playerService) {
		this.userManager = userManager;
		this.playerService = playerService;
	}

	public async Task<IActionResult> OnGet(string id) {
		User = await playerService.Get(id);
		if (User is null) return NotFound("Can't find player with id '{id}'.");
		return Page();
	}
}