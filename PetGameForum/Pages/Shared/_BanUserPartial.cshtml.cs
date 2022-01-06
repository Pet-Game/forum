using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetGameForum.Pages.Shared; 

public class BanUserPartialModel : PageModel {
	[BindProperty] public BanAction Ban { get; set; }
	
	
}

public class BanAction {
	public string UserId { get; set; }
	public string PostId { get; set; }
	public string Reason { get; set; }
	public float Duration { get; set; }
}