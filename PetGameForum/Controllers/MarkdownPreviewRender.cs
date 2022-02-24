using Microsoft.AspNetCore.Mvc;
using PetGameForum.Services;

namespace PetGameForum.Controllers; 

public class MarkdownPreviewRender : Controller {
	private readonly MarkDownService mdService;
	
	public MarkdownPreviewRender(MarkDownService mdService) {
		this.mdService = mdService;
	}

	[HttpPost]
	[Route("MarkdownPreview")]
	public async Task<ContentResult> Index() {
		var req = Request.Body;
		//req.Seek(0, SeekOrigin.Begin);
		var body = await new StreamReader(req).ReadToEndAsync();
		//todo: rate limit? user checking?
		return Content(mdService.ToHtml(body));
	}
}