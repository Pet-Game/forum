﻿using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;

namespace PetGameForum.Data; 

public class Role : MongoIdentityRole<ObjectId> {
	
}