global using Player = PetGameForum.Data.User;

using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;

namespace PetGameForum.Data; 

public class User : MongoIdentityUser<ObjectId> {
	public static readonly string defaultPfp = "/defaultResources/pfp.gif";
	
	public ObjectId? Banned;
	public string PfpUrl;
	public string Description;

	public string Link() => $"/Player/{Id}";
	public string Pfp() => string.IsNullOrWhiteSpace(PfpUrl) ? defaultPfp : PfpUrl;
}