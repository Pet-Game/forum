using AspNetCore.Identity.MongoDbCore.Models;

namespace PetGameForum.Data; 

public class User : MongoIdentityUser {
	//todo: generate id myself

	public User() : base() {}
	public User(string name, string email) : base(name, email) {}
}