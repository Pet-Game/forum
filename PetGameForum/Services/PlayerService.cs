using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using PetGameForum.Data;

namespace PetGameForum.Services; 

public class PlayerService {
	public readonly UserManager<User> AspUserManager;
	public readonly RoleManager<Role> AspRoleManager;
	public readonly IConfiguration Config;
	public readonly SignInManager<User> AspSigninManager;

	private readonly IMongoCollection<User> userCollection;
	private readonly IMongoCollection<Ban> banCollection;

	public PlayerService(UserManager<User> aspUserManager, RoleManager<Role> aspRoleManager, IConfiguration  config, 
			MongoClient dbClient, SignInManager<User> aspSigninManager) {
		AspUserManager = aspUserManager;
		AspRoleManager = aspRoleManager;
		Config = config;
		AspSigninManager = aspSigninManager;

		var database = dbClient.GetDatabase("identity");
		userCollection = database.GetCollection<User>("users");
		banCollection = database.GetCollection<Ban>("bans");

	}

	public async Task BanUser(ObjectId userId, float duration, string reason) {
		await AspSigninManager.SignOutAsync();

		var unbanTime = DateTime.UtcNow.AddDays(duration);
		var ban = new Ban {
			Id = ObjectId.GenerateNewId(),
			User = userId,
			BanEnd = unbanTime,
			Duration = duration,
			Reason = reason,
		};
		await banCollection.InsertOneAsync(ban);

		var update = Builders<User>.Update.Set(u => u.Banned, ban.Id);
		await userCollection.UpdateOneAsync(u => u.Id == userId, update);
	}
	
	public async Task<bool> IsUserBanned(User user) {
		if (!user.Banned.HasValue) return false;
		var ban = await banCollection.Find(b => b.Id == user.Banned.Value).FirstOrDefaultAsync();
		if (ban is null) throw new Exception("unknown ban!"); //too fragile?
		if (ban.BanEnd < DateTime.UtcNow) { //ban *just* ran out, so lets update the files
			var update = Builders<User>.Update.Set(u => u.Banned, null);
			await userCollection.UpdateOneAsync(u => u.Id == user.Id, update);
			return false;
		}
		return true; //still banned v_v
	}

	public async Task<bool> IsUserBanned(ObjectId userId) {
		return await IsUserBanned(await userCollection.Find(u=>u.Id==userId).FirstOrDefaultAsync());
	}
}