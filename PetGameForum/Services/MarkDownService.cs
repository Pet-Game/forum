using Markdig;
using PetGameForum.Util.Markdown;

namespace PetGameForum.Services; 

public class MarkDownService {
	private readonly MarkdownPipeline mdPipeline;

	public MarkDownService() {
		var pipelineBuilder = new MarkdownPipelineBuilder();
		//core functionality
		pipelineBuilder.DisableHtml();
		pipelineBuilder.UseNoRenamedLinks();
		pipelineBuilder.UseSoftlineBreakAsHardlineBreak();
		pipelineBuilder.UseAutoLinks();

		//sparkle
		pipelineBuilder.UseSitelenPona();
		pipelineBuilder.UseListExtras();
		pipelineBuilder.UseEmphasisExtras();
		pipelineBuilder.UseMediaLinks();
		
		mdPipeline = pipelineBuilder.Build();
	}

	public string ToHtml(string markdown) {
		return Markdown.ToHtml(markdown, mdPipeline);
	}
}