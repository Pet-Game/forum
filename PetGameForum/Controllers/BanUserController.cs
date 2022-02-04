using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PetGameForum.Data;
using PetGameForum.Services;

namespace PetGameForum.Controllers; 

public class BanUserController : Controller {
	public readonly RoleService RoleService;
	public readonly PlayerService PlayerService;
	public readonly UserManager<User> UserManager;

	public BanUserController(RoleService roleService, PlayerService playerService, UserManager<User> userManager) {
		RoleService = roleService;
		PlayerService = playerService;
		UserManager = userManager;
	}

	[HttpGet]
	[Route("BanUser")]
	public async Task<ActionResult> Ban(string userId, float duration, string reason, string returnUrl = null) {
		if (!ObjectId.TryParse(userId, out var id)) return BadRequest("argh");
		var banPermission = duration switch {
			>=0 and <= 30 => Permission.TempBanShort,
			<=1500 => Permission.TempBanLong,
			_ => Permission.PermaBan,
		};
		if (!await RoleService.HasPermission(User, banPermission)) return Unauthorized();

		var actor = await UserManager.GetUserAsync(User);
		await PlayerService.BanUser(id, duration, reason, actor);

		returnUrl ??= Url.Content("~/");
		return Redirect( returnUrl );
	}
}