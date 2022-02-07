namespace PetGameForum.Util; 

public static class Rusty {
	public static string MapOrNOW(this string str, string other) {
		return str.IsNullOrWhiteSpace() ? other : str;
	}
}