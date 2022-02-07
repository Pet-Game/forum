namespace PetGameForum.Util; 

public static class Extensions {
	public static bool IsNullOrWhiteSpace(this string str) {
		return string.IsNullOrWhiteSpace(str);
	}

	public static void Sync(this Task task) {
		task.GetAwaiter().GetResult();
	}
	
	public static T Sync<T>(this Task<T> task) {
		return task.GetAwaiter().GetResult();
	}
}