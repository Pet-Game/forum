using Microsoft.AspNetCore.Mvc.RazorPages;
using PetGameForum.Data;
using PetGameForum.Services;

namespace PetGameForum.Pages.Moderation; 

public class Log : PageModel {
	private readonly LogService logging;
	
	public List<LogEntry> Logs;
	
	public Log(LogService logging) {
		this.logging = logging;
	}

	public async Task OnGet() {
		Logs = await logging.Get(100, 0);
	}
}