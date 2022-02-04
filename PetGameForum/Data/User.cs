﻿using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;

namespace PetGameForum.Data; 

public class User : MongoIdentityUser<ObjectId> {
	public ObjectId? Banned;

	public string Link() => $"/User/{Id}";
}