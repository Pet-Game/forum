using System.ComponentModel;
using MongoDB.Bson;
using PetGameForum.Services;
using PetGameForum.Util;

namespace PetGameForum.Data; 

public class ForumPost {
	public ObjectId Id { get; set; }
	public ObjectId Thread { get; set; }
	public bool Deleted { get; set; }
	public string UserInput { get; set; }
	public string CompiledContent { get; set; }
	public ForumPostAuthor Author { get; set; }
	
	public BsonDocument ExtraElements;
	
	public string Link() => $"/Forum/Thread/{Thread}#{Id}";
}

public class ForumPostAuthor : ISupportInitialize {
	public static readonly int CurrentVersion = 1;
	
	public ObjectId Id;
	public string Name;
	public string Pfp;
	//name color
	//roles?
	//forum signature!
	public int Version;
	
	public string Link() => $"/Player/{Id}";

	public static ForumPostAuthor FromUser(User user) {
		return new ForumPostAuthor() {
			Id = user.Id,
			Name = user.UserName,
			Pfp = user.PfpUrl,
		};
	}

	void ISupportInitialize.BeginInit() {
		
	}

	void ISupportInitialize.EndInit() {
		if (Version < CurrentVersion) {
			using var scope = StaticStore.Services.CreateScope();
			var playerService = scope.ServiceProvider.GetService<PlayerService>();
			var player = playerService!.Get(Id).Sync();
			if (player is not null) {
				Pfp = player.Pfp();
				Name = player.UserName;
			}
		}
	}
}