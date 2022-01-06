using MongoDB.Bson;

namespace PetGameForum.Data; 

public class ForumPost {
	public ObjectId Id { get; set; }
	public ObjectId Thread { get; set; }
	public bool Deleted { get; set; }
	public string UserInput { get; set; }
	public string CompiledContent { get; set; }
	public ForumPostAuthor Author { get; set; }
	
	public BsonDocument ExtraElements;
}

public class ForumPostAuthor {
	public ObjectId Id;
	public string Name;
	//name color
	//roles?
	//forum signature!

	public static ForumPostAuthor FromUser(User user) {
		return new ForumPostAuthor() {
			Id = user.Id,
			Name = user.UserName,
		};
	}
}