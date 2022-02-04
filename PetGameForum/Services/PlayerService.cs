using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using NuGet.Protocol.Core.Types;
using PetGameForum.Data;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PetGameForum.Services; 

public class PlayerService {
	public readonly UserManager<User> AspUserManager;
	public readonly RoleManager<Role> AspRoleManager;
	public readonly IConfiguration Config;
	public readonly SignInManager<User> AspSigninManager;
	public readonly IHttpContextAccessor HttpContext;
	public readonly IAuthenticationHandlerProvider Handlers;
	private readonly LogService logging;
	private IAuthenticationSchemeProvider AuthThemeProvider;

	private readonly IMongoCollection<User> userCollection;
	private readonly IMongoCollection<Ban> banCollection;

	public PlayerService(UserManager<User> aspUserManager, RoleManager<Role> aspRoleManager, IConfiguration  config, 
			MongoClient dbClient, SignInManager<User> aspSigninManager, IHttpContextAccessor httpContext, IAuthenticationHandlerProvider handlers, IAuthenticationSchemeProvider authThemeProvider, LogService logging) {
		AspUserManager = aspUserManager;
		AspRoleManager = aspRoleManager;
		Config = config;
		AspSigninManager = aspSigninManager;
		HttpContext = httpContext;
		Handlers = handlers;
		AuthThemeProvider = authThemeProvider;
		this.logging = logging;

		var database = dbClient.GetDatabase("identity");
		userCollection = database.GetCollection<User>("users");
		banCollection = database.GetCollection<Ban>("bans");

	}
	
	public async Task<LoginResult> Login2FA(string code, string password, bool remember) {
		var signInResult = await AspSigninManager.TwoFactorAuthenticatorSignInAsync(code, remember, remember);
		return GetLoginResult(signInResult);
	}
	
	public async Task<LoginResult> LoginRecovery(string code) {
		var signInResult = await AspSigninManager.TwoFactorRecoveryCodeSignInAsync(code);
		return GetLoginResult(signInResult);
	}

	public async Task<LoginResult> Login(string email, string password, bool remember) {
		var user = await AspUserManager.FindByEmailAsync(email);
		var signInResult = await AspSigninManager.PasswordSignInAsync(user, password, remember, false);
		return GetLoginResult(signInResult);
	}
	
	public LoginResult GetLoginResult(SignInResult signInResult) {
		if (signInResult.Succeeded) {
			return LoginResult.Ok;
		}
		if (signInResult.IsLockedOut) {
			return LoginResult.LockedOut;
		}
		if (signInResult.RequiresTwoFactor) {
			return LoginResult.Needs2FA;
		}
		if (signInResult.IsNotAllowed) {
			return LoginResult.NotAllowed;
		}
		return LoginResult.Unknown;
	}

	public async Task BanUser(ObjectId userId, float duration, string reason, User actor) {
		var user = await AspUserManager.FindByIdAsync(userId.ToString()); //consider accessing collection
		await BanUser(user, duration, reason, actor);
	}

	public async Task BanUser(User user, float duration, string reason, User actor) {
		await AspSigninManager.SignOutAsync();

		var unbanTime = DateTime.UtcNow.AddDays(duration);
		if(duration < 0) unbanTime = DateTime.MaxValue;
		await AspUserManager.SetLockoutEndDateAsync(user, unbanTime);
		
		var ban = new Ban {
			Id = ObjectId.GenerateNewId(),
			User = user.Id,
			BanEnd = unbanTime,
			Duration = duration,
			Reason = reason,
		};
		await banCollection.InsertOneAsync(ban);

		var update = Builders<User>.Update.Set(u => u.Banned, ban.Id);
		await userCollection.UpdateOneAsync(u => u.Id == user.Id, update);

		await logging.Log(LogKind.BanUser, user.Link(), $"{user.UserName} was banned for {duration} days because of {reason}", actor);
	}
	
	public async Task<bool> IsUserBanned(User user) {
		if (!user.Banned.HasValue) return false;
		var ban = await banCollection.Find(b => b.Id == user.Banned.Value).FirstOrDefaultAsync();
		if (ban is null) throw new Exception("unknown ban!"); //too fragile?
		if (ban.BanEnd < DateTime.UtcNow) { //ban *just* ran out, so lets update the files
			var update = Builders<User>.Update.Set(u => u.Banned, null);
			await userCollection.UpdateOneAsync(u => u.Id == user.Id, update);
			await logging.Log(LogKind.BanEnd, user.Link(), 
				$"{user.UserName} who was banned for {ban.Reason} for {ban.Duration} days was just unbanned.", user);
			return false;
		}
		return true; //still banned v_v
	}
	
	public async Task<bool> IsUserBanned(ClaimsPrincipal principal) {
		var user = await AspUserManager.GetUserAsync(principal);
		return await IsUserBanned(user);
	}

	public async Task<bool> IsUserBanned(ObjectId userId) {
		return await IsUserBanned(await userCollection.Find(u=>u.Id==userId).FirstOrDefaultAsync());
	}

	public async Task<bool> EnsureBan(ClaimsPrincipal user) {
		if(!AspSigninManager.IsSignedIn(user)) return false;
		if (!await IsUserBanned(user)) return false;
		await AspSigninManager.SignOutAsync();
		return true;
	}

	public async Task<List<User>> GetAll() {
		return await userCollection.Find(FilterDefinition<User>.Empty).ToListAsync();
	}
}

public enum LoginResult {
	Ok,
	Banned,
	LockedOut,
	Needs2FA,
	NotAllowed,
	Unknown,
}