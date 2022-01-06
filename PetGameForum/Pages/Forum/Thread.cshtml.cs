using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using PetGameForum.Data;
using PetGameForum.Services;

namespace PetGameForum.Pages.Forum; 

public class ThreadModel : PageModel {
	public readonly UserManager<User> UserManager;
	public readonly ForumService ForumService;
	public readonly IConfiguration  Config;
	public readonly RoleService RoleService;
	
	public ForumThread Thread;
	public List<ForumPost> Posts;

	[BindProperty] 
	public string UserPostInput { get; set; }
	
	[BindProperty]
	public PostAction PostAction { get; set; }

	public ThreadModel(ForumService forumService, UserManager<User> userManager, IConfiguration  config, RoleService roleService) {
		ForumService = forumService;
		UserManager = userManager;
		Config = config;
		RoleService = roleService;
	}
	
	public async Task<IActionResult> OnGet(string id) {
		if (!await FindThread(id) || !await FindPosts()) return NotFound();
		return Page();
	}

	public async Task<IActionResult> OnPostAsync(string id) {
		if (!await FindThread(id) || !await FindPosts()) return NotFound();
		if (UserPostInput is not null) {
			if(!ValidatePost(UserPostInput)) return Redirect(Request.Path); //todo: error comm & clientside
			var compiledText = CompilePost(UserPostInput);
			if(string.IsNullOrEmpty(compiledText)) return Redirect(Request.Path); //todo: error comm

			var newPost = new ForumPost {
				Id = ObjectId.GenerateNewId(),
				Author = ForumPostAuthor.FromUser(await UserManager.GetUserAsync(User)),
				UserInput = UserPostInput,
				CompiledContent = compiledText,
				Thread = Thread.Id,
			};
			await ForumService.CreatePost(newPost);
			return Redirect(Request.Path + $"#{newPost.Id}");
		}

		if (PostAction is not null) {
			if (!ObjectId.TryParse(PostAction.Post, out var post)) return Redirect(Request.Path); //todo: error comm
			var postIndex = Posts.FindIndex(p => p.Id == post);
			if(postIndex < 0) return Redirect(Request.Path); //todo: error comm
			var previousPost = Posts[postIndex == 0 ? 1 : postIndex - 1];
			switch (PostAction.Type) {
				case "delete":
					if(!await RoleService.HasPermission(User, Permission.DeletePosts)) return Redirect(Request.Path); //todo: error comm
					await ForumService.DeletePost(post);
					break;
				case "nuke": 
					if(!await RoleService.HasPermission(User, Permission.NukePosts)) return Redirect(Request.Path); //todo: error comm
					await ForumService.NukePost(post);
					break;
				case "ban":
					//todo: ban checks
					break;
				case "restore":
					if(!await RoleService.HasPermission(User, Permission.RestorePosts)) return Redirect(Request.Path); //todo: error comm
					await ForumService.RestorePost(post);
					break;
			}
			return Redirect(Request.Path + $"#{previousPost.Id}");
		}

		return NotFound();
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
		Posts ??= (await ForumService.GetPostsOfThread(Thread.Id)).ToList();
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

public class PostAction {
	public string Type { get; set; }
	public string Post { get; set; }
	public string Reason { get; set; }
}