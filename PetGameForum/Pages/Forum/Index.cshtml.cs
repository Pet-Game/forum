using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using PetGameForum.Data;

namespace PetGameForum.Pages; 

public class ForumHubModel : PageModel {
	public readonly ForumService ForumService;
	public List<ForumThread> Threads;
	private readonly UserManager<User> userManager;
	
	[BindProperty]
	public ForumThread NewThread { get; set; }

	public ForumHubModel(ForumService forumService, UserManager<User> userManager) {
		this.ForumService = forumService;
		this.userManager = userManager;
	}
	
	public async Task OnGet() {
		
	}

	public async Task<IActionResult> OnPostAsync() {
		NewThread.Author = ForumThreadAuthor.FromUser(await userManager.GetUserAsync(User));
		await ForumService.CreateThread(NewThread);
		return Redirect(Request.Path); //redirect to self makes it a get request again and reloading doesnt post again
	}
}