using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PetGameForum.Data; 

public class Ban {
	public ObjectId Id;
	public ObjectId User;
	public float Duration;
	public DateTime BanEnd;
	public string Reason;
}