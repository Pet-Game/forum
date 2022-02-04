using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PetGameForum.Data; 

public class ForumThread {
	public ObjectId Id { get; set; }

	[BsonElement]
	public string Topic { get; set; }
	public ForumThreadAuthor Author { get; set; }

	public BsonDocument ExtraElements;

	public string Link() => $"/Forum/Thread/{Id}";
}

public class ForumThreadAuthor {
	public ObjectId Id;
	public string Name;
	//whatever else we need here

	public static ForumThreadAuthor FromUser(User user) {
		return new () {
			Id = user.Id,
			Name = user.UserName,
		};
	}
}