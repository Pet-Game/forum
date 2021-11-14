using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using PetGameForum.Data;

namespace PetGameForum.Pages.Forum; 

public class ThreadModel : PageModel {
	public UserManager<User> UserManager { get; }
	public ForumService ForumService { get; }
	
	public ForumThread Thread;
	public List<ForumPost> Posts;

	[BindProperty] 
	public ForumPost NewPost { get; set; }

	public ThreadModel(ForumService forumService, UserManager<User> userManager) {
		ForumService = forumService;
		UserManager = userManager;
	}
	
	public async Task<IActionResult> OnGet(string id) {
		if (!await FindThread(id) || !await FindPosts()) return NotFound();
		return Page();
	}

	public async Task<IActionResult> OnPostAsync(string id) {
		if (!await FindThread(id) || !await FindPosts()) return NotFound();
		
		NewPost.Author = ForumPostAuthor.FromUser(await UserManager.GetUserAsync(User));
		NewPost.CompiledContent = NewPost.UserInput;
		NewPost.Thread = Thread.Id;
		//todo: better validation
		await ForumService.CreatePost(NewPost);
		return Redirect(Request.Path);
	}

	private async Task<bool> FindThread(string id) {
		if (!ObjectId.TryParse(id, out var threadId)) {
			return false;
		}
		Thread = await ForumService.GetThread(threadId);
		if (Thread == default) return false;
		return true;
	}

	private async Task<bool> FindPosts() {
		Posts = (await ForumService.GetPostsOfThread(Thread.Id)).ToList();
		return true;
	}
}