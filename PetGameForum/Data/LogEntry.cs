using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PetGameForum.Data; 

public class LogEntry {
	public ObjectId Id;
	[BsonRepresentation(BsonType.String)] public LogKind Kind;
	public string Link;
	public string Message;
	public ObjectId Author;
}

public enum LogKind {
	CreateTopic,
	CreateThread,
	CreatePost,
	EditPost,
	DeletePost,
	NukePost,
	BanUser,
	BanEnd,
	EditRoles
}