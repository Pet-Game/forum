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

	public int MaxPostLength;

	[BindProperty] 
	public string UserPostInput { get; set; }
	
	[BindProperty]
	public PostAction PostAction { get; set; }

	public ThreadModel(ForumService forumService, UserManager<User> userManager, IConfiguration  config, RoleService roleService) {
		ForumService = forumService;
		UserManager = userManager;
		Config = config;
		RoleService = roleService;
		
		MaxPostLength = Config.GetSection("ForumSettings").GetValue<int>("MaxPostLength");
	}
	
	public async Task<IActionResult> OnGet(string id) {
		if (!await FindThread(id) || !await FindPosts()) return NotFound();
		return Page();
	}

	public async Task<IActionResult> OnPostAsync(string id) {
		if (!await FindThread(id) || !await FindPosts()) return NotFound();
		var user = await UserManager.GetUserAsync(User);
		if (UserPostInput is not null) {
			if(ValidatePost(UserPostInput) != PostValidation.Ok) return Redirect(Request.Path); //todo: error comm & clientside
			var compiledText = CompilePost(UserPostInput);
			if(string.IsNullOrEmpty(compiledText)) return Redirect(Request.Path); //todo: error comm

			var newPost = new ForumPost {
				Id = ObjectId.GenerateNewId(),
				Author = ForumPostAuthor.FromUser(user),
				UserInput = UserPostInput,
				CompiledContent = compiledText,
				Thread = Thread.Id,
			};
			await ForumService.CreatePost(newPost, user);
			return Redirect(Request.Path + $"#{newPost.Id}");
		}

		if (PostAction is not null) {
			if (!ObjectId.TryParse(PostAction.Post, out var post)) return Redirect(Request.Path); //todo: error comm
			var postIndex = Posts.FindIndex(p => p.Id == post);
			if(postIndex < 0) return Redirect(Request.Path); //todo: error comm
			postIndex --; //show previous post
			var previousPost = postIndex < Posts.Count && postIndex >= 0 ? Posts[postIndex] : null;
			switch (PostAction.Type) {
				case "delete":
					if(!await RoleService.HasPermission(user, Permission.DeletePosts)) return Redirect(Request.Path); //todo: error comm
					await ForumService.DeletePost(post, user);
					break;
				case "nuke": 
					if(!await RoleService.HasPermission(user, Permission.NukePosts)) return Redirect(Request.Path); //todo: error comm
					await ForumService.NukePost(post, user);
					break;
				case "restore":
					if(!await RoleService.HasPermission(user, Permission.RestorePosts)) return Redirect(Request.Path); //todo: error comm
					await ForumService.RestorePost(post, user);
					break;
			}
			return Redirect(Request.Path + $"#{previousPost?.Id}");
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

	private PostValidation ValidatePost(string userInput) {
		if (string.IsNullOrWhiteSpace(userInput)) return PostValidation.Empty;
		if (userInput.Length > MaxPostLength) 
			return PostValidation.TooLong;
		return PostValidation.Ok;
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

public enum PostValidation {
	Ok,
	Empty,
	TooLong,
}