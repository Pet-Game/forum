using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PetGameForum.Data;
using PetGameForum.Services;

namespace PetGameForum.Controllers; 

public class BanUserController : Controller {
	public readonly RoleService RoleService;
	public readonly PlayerService PlayerService;

	public BanUserController(RoleService roleService, PlayerService playerService) {
		RoleService = roleService;
		PlayerService = playerService;
	}

	[HttpPost]
	public async Task<ActionResult> Index(string idString, float duration, string reason) {
		if (ObjectId.TryParse(idString, out var id)) return BadRequest();
		var banPermission = duration switch {
			>=0 and <= 30 => Permission.TempBanShort,
			<=1500 => Permission.TempBanLong,
			_ => Permission.PermaBan,
		};
		if (!await RoleService.HasPermission(User, banPermission)) return Unauthorized();

		await PlayerService.BanUser(id, duration, reason);
		var fromUrl = Request.Query["from"].FirstOrDefault();
		return Redirect(fromUrl ?? "/" );
	}
}