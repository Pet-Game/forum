using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using PetGameForum.Data;

namespace PetGameForum.Pages; 

public class ForumHubModel : PageModel {
	public readonly ForumService ForumService;
	public readonly UserManager<User> UserManager;
	public readonly IConfiguration  Config;
	
	[BindProperty]
	public ForumThread NewThread { get; set; }

	public ForumHubModel(ForumService forumService, UserManager<User> userManager, IConfiguration  config) {
		ForumService = forumService;
		UserManager = userManager;
		Config = config;
	}

	public async Task OnGet() {
		// :)
	}

	public async Task<IActionResult> OnPostAsync() {
		if(string.IsNullOrWhiteSpace(NewThread.Topic) || 
				NewThread.Topic.Length > Config.GetSection("ForumSettings").GetValue<int>("MaxThreadNameLength"))
			return Redirect(Request.Path); //todo: error communication and clientside validation
		NewThread.Author = ForumThreadAuthor.FromUser(await UserManager.GetUserAsync(User));
		await ForumService.CreateThread(NewThread);
		return Redirect(Request.Path); //redirect to self makes it a get request again and reloading doesnt post again
	}
}