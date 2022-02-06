using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PetGameForum.Services;

namespace PetGameForum.Data; 

public class Role : MongoIdentityRole<ObjectId> {
	[BsonRepresentation(BsonType.String)]
	public List<Permission> Permissions = new();

	public bool HasPermission(Permission permission) {
		return Permissions.Any(perm => perm == permission);
	}
	
	public bool HasPermission(string permissionPolicy) {
		return Permissions.Any(perm => RoleService.Policy(perm).Equals(permissionPolicy, StringComparison.OrdinalIgnoreCase));
	}
}