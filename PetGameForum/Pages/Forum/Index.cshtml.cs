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
	public string Topic { get; set; }

	public ForumHubModel(ForumService forumService, UserManager<User> userManager, IConfiguration  config) {
		ForumService = forumService;
		UserManager = userManager;
		Config = config;
	}

	public void OnGet() {
		// :)
	}

	public async Task<IActionResult> OnPostAsync() {
		if(string.IsNullOrWhiteSpace(Topic) || 
				Topic.Length > Config.GetSection("ForumSettings").GetValue<int>("MaxThreadNameLength"))
			return Redirect(Request.Path); //todo: error communication and clientside validation
		var user = await UserManager.GetUserAsync(User);
		var newThread = new ForumThread {
			Author = ForumThreadAuthor.FromUser(user),
			Topic = Topic,
		};
		await ForumService.CreateThread(newThread, user);
		return Redirect(Request.Path); //redirect to self makes it a get request again and reloading doesnt post again
	}
}