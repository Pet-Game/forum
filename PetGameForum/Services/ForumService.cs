

using MongoDB.Bson;
using MongoDB.Driver;
using PetGameForum.Data;

public class ForumService {
	private readonly IMongoCollection<ForumThread> threadCollection;
	private readonly IMongoCollection<ForumPost> postCollection;
	
	public ForumService(MongoClient dbClient) {
		var database = dbClient.GetDatabase("forum");
		threadCollection = database.GetCollection<ForumThread>("threads");
		postCollection = database.GetCollection<ForumPost>("posts");
	}

	public async Task<IAsyncCursor<ForumThread>> GetAllThreads() {
		return await threadCollection.Find(FilterDefinition<ForumThread>.Empty).SortByDescending(thread => thread.Id).ToCursorAsync();
	}

	public async Task<ForumThread> GetThread(ObjectId id) {
		//todo: possible migrations
		return await threadCollection.Find(thread => thread.Id == id).FirstOrDefaultAsync();
	}

	public async Task CreateThread(User author, string topic) {
		await CreateThread(new ForumThread{Author = ForumThreadAuthor.FromUser(author), Topic = topic});
	}
	
	public async Task CreateThread(ForumThread thread) {
		//todo: do more validation here
		await threadCollection.InsertOneAsync(thread);
	}

	public async Task<ForumPost> GetPost(ObjectId id) {
		return await postCollection.Find(post => post.Id == id).FirstOrDefaultAsync();
	}

	public async Task<IAsyncCursor<ForumPost>> GetPostsOfThread(ObjectId threadId) {
		return await postCollection.Find(post => post.Thread == threadId).SortBy(post => post.Id).ToCursorAsync();
	}

	public async Task CreatePost(ForumPost newPost) {
		//todo: do more validation here
		await postCollection.InsertOneAsync(newPost);
	}

	public async Task DeletePost(ObjectId postId) {
		var update = Builders<ForumPost>.Update.Set(p => p.Deleted, true);
		await postCollection.UpdateOneAsync(post => post.Id == postId, update);
	}

	public async Task RestorePost(ObjectId postId) {
		var update = Builders<ForumPost>.Update.Set(p => p.Deleted, false);
		await postCollection.UpdateOneAsync(post => post.Id == postId, update);
	}

	public async Task<bool> NukePost(ObjectId postId) {
		var res = await postCollection.DeleteOneAsync(p => p.Id == postId);
		return res.IsAcknowledged && res.DeletedCount > 0;
	}
}