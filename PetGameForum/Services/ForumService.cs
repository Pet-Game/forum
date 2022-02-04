

using MongoDB.Bson;
using MongoDB.Driver;
using PetGameForum.Data;
using PetGameForum.Services;

public class ForumService {
	private readonly IMongoCollection<ForumThread> threadCollection;
	private readonly IMongoCollection<ForumPost> postCollection;
	private readonly LogService logging;
	
	public ForumService(MongoClient dbClient, LogService logging) {
		this.logging = logging;
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
		var thread = new ForumThread { Author = ForumThreadAuthor.FromUser(author), Topic = topic };
		await CreateThread(thread, author);
	}
	
	public async Task CreateThread(ForumThread thread, User actor) {
		//todo: do more validation here
		await threadCollection.InsertOneAsync(thread);
		await logging.Log(LogKind.CreateThread, thread.Link(), $"{thread.Author.Name} created thread {thread.Topic}.", actor);
	}

	public async Task<ForumPost> GetPost(ObjectId id) {
		return await postCollection.Find(post => post.Id == id).FirstOrDefaultAsync();
	}

	public async Task<IAsyncCursor<ForumPost>> GetPostsOfThread(ObjectId threadId) {
		return await postCollection.Find(post => post.Thread == threadId).SortBy(post => post.Id).ToCursorAsync();
	}

	public async Task CreatePost(ForumPost newPost, User actor) {
		//todo: do more validation here
		await postCollection.InsertOneAsync(newPost);
		await logging.Log(LogKind.CreatePost, newPost.Link(), $"{newPost.Author.Name} created post.", actor);
	}

	public async Task DeletePost(ObjectId postId, User actor) => await DeletePost(await GetPost(postId), actor);
	
	public async Task DeletePost(ForumPost post, User actor) {
		var postId = post.Id;
		var update = Builders<ForumPost>.Update.Set(p => p.Deleted, true);
		await postCollection.UpdateOneAsync(p => p.Id == postId, update);
		var thread = await GetThread(post.Thread);

		await logging.Log(LogKind.DeletePost, post.Link(), $"Post by {post.Author.Name} in {thread.Topic} was deleted.", actor);
	}
	
	public async Task RestorePost(ObjectId postId, User actor) {
		var update = Builders<ForumPost>.Update.Set(p => p.Deleted, false);
		await postCollection.UpdateOneAsync(post => post.Id == postId, update);
		
		var post = await GetPost(postId);
		var thread = await GetThread(post.Thread);
		await logging.Log(LogKind.NukePost, post.Link(), $"Post by {post.Author.Name} in {thread.Topic} was restored.", actor);
	}

	public async Task<bool> NukePost(ObjectId postId, User actor) {
		var post = await GetPost(postId);
		var thread = await GetThread(post.Thread);
		await logging.Log(LogKind.CreatePost, thread.Link(), $"Post by {post.Author.Name} in {thread.Topic} was nuked.", actor);
		
		var res = await postCollection.DeleteOneAsync(p => p.Id == postId);
		return res.IsAcknowledged && res.DeletedCount > 0;
	}
}