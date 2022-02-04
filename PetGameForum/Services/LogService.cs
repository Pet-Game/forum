using MongoDB.Driver;
using PetGameForum.Data;

namespace PetGameForum.Services; 

public class LogService {
	private readonly IMongoCollection<LogEntry> logCollection;
	
	public LogService(MongoClient dbClient) {
		var database = dbClient.GetDatabase("logs");
		logCollection = database.GetCollection<LogEntry>("log");
	}

	public async Task Log(LogKind kind, string link, string message, User user) {
		var entry = new LogEntry {
			Kind = kind,
			Link = link,
			Message = message,
			Author = user.Id,
		};
		await logCollection.InsertOneAsync(entry);
	}

	public async Task<List<LogEntry>> Get(int amount, int offset) {
		return await logCollection.Find(FilterDefinition<LogEntry>.Empty).Skip(offset).Limit(amount).ToListAsync();
	}
}