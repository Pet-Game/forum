global using static PetGameForum.Data.Result;

namespace PetGameForum.Data; 


public struct Result {
	public bool IsOk { get; }
	public string Error { get; }

	public Result() {
		IsOk = true;
		Error = null;
	}
	
	public Result(string error) {
		IsOk = false;
		Error = error;
	}

	public static Result Ok() {
		return new Result();
	}
	
	public static Result Err(string error) {
		return new Result(error);
	}
	
	public static implicit operator bool(Result res) {
		return res.IsOk;
	}
}