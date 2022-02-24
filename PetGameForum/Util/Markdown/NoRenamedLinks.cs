using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

public static class NoRenamedLinks {
	public static MarkdownPipelineBuilder UseNoRenamedLinks(this MarkdownPipelineBuilder builder) {
		builder.Use<NoRenamedLinksExtension>();
		return builder;
	}
}

public class NoRenamedLinksExtension : IMarkdownExtension {
	public void Setup(MarkdownPipelineBuilder pipeline) {}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) {
		var linkRenderer = renderer.ObjectRenderers.Find<LinkInlineRenderer>();
		if (linkRenderer is null) return;
		linkRenderer.TryWriters.Remove(CheckIllegalLinks);
		linkRenderer.TryWriters.Add(CheckIllegalLinks);
	}

	private static bool CheckIllegalLinks(HtmlRenderer renderer, LinkInline linkInline) {
		if (linkInline.IsShortcut || linkInline.IsImage || linkInline.IsAutoLink) {
			return false;
		}

		foreach (Inline inline in linkInline) {
			if(inline is LiteralInline litInline){
				renderer.Write(litInline.Content);
			}
		}
		return true;
	}
}