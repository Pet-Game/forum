using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using PetGameForum.Data;

namespace PetGameForum.Pages.Forum; 

public class ThreadModel : PageModel {
	public readonly UserManager<User> UserManager;
	public readonly ForumService ForumService;
	public readonly IConfiguration  Config;
	
	public ForumThread Thread;
	public List<ForumPost> Posts;

	[BindProperty] 
	public string UserPostInput { get; set; }

	public ThreadModel(ForumService forumService, UserManager<User> userManager, IConfiguration  config) {
		ForumService = forumService;
		UserManager = userManager;
		Config = config;
	}
	
	public async Task<IActionResult> OnGet(string id) {
		if (!await FindThread(id) || !await FindPosts()) return NotFound();
		return Page();
	}

	public async Task<IActionResult> OnPostAsync(string id) {
		if (!await FindThread(id) || !await FindPosts()) return NotFound();
		if(!ValidatePost(UserPostInput)) return Redirect(Request.Path); //todo: error comm & clientside
		var compiledText = CompilePost(UserPostInput);
		if(string.IsNullOrEmpty(compiledText)) return Redirect(Request.Path); //todo: error comm

		var newPost = new ForumPost {
			Author = ForumPostAuthor.FromUser(await UserManager.GetUserAsync(User)),
			UserInput = UserPostInput,
			CompiledContent = compiledText,
			Thread = Thread.Id,
		};
		await ForumService.CreatePost(newPost);
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

	private bool ValidatePost(string userInput) {
		if (string.IsNullOrWhiteSpace(userInput)) return false;
		if (userInput.Length > Config.GetSection("ForumSettings").GetValue<int>("MaxPostLength")) 
			return false; //todo: clientside validation
		return true;
	}
	
	private string CompilePost(string userInput) {
		//ASPNET does all html encoding for us already
		return userInput;
	}
}