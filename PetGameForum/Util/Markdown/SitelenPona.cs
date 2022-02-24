using System.Diagnostics;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

namespace PetGameForum.Util.Markdown;

public static class SitelenPonaExtensionMethod {
	public static MarkdownPipelineBuilder UseSitelenPona(this MarkdownPipelineBuilder builder) {
		builder.Use<SitelenPonaExtension>();
		return builder;
	}
}

public class SitelenPonaExtension: IMarkdownExtension {
	private readonly SitelenPonaOptions options;

	public SitelenPonaExtension(SitelenPonaOptions options) {
		this.options = options;
	}

	public SitelenPonaExtension() {
		options = new SitelenPonaOptions();
	}
	
	public void Setup(MarkdownPipelineBuilder pipeline) {
		pipeline.InlineParsers.AddIfNotAlready<SitelenPonaInlineParser>();
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) {
		renderer.ObjectRenderers.AddIfNotAlready(new SitelenPonaRenderer(options));
	}
}

public class SitelenPonaInlineParser : InlineParser {
	private static readonly char[] _openingCharacters = {
		'{'
	};

	public SitelenPonaInlineParser() {
		OpeningCharacters = _openingCharacters;
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice) {
		var next = slice.PeekChar();
		if (next != '{') return false; //only on double {{
		slice.NextChar();
		slice.NextChar();

		var current = slice.CurrentChar;
		var start = slice.Start;

		while (!current.IsZero() && !(current == '}' && slice.PeekChar() == '}')) {
			current = slice.NextChar();
		}
		
		if(current.IsZero())
			return false;
		
		//consume }}
		slice.NextChar();
		slice.NextChar();

		int inlineStart = processor.GetSourcePosition(slice.Start, out int line, out int column);
		var end = slice.Start;
		processor.Inline = new SitelenPona {
			Span = {
				Start = inlineStart,
				End = inlineStart + (end - start),
			},
			Line = line,
			Column = column,
			Text = new StringSlice(slice.Text, start, end - 3),
		};

		return true;
	}
}

public class SitelenPonaRenderer : HtmlObjectRenderer<SitelenPona>
{
	private SitelenPonaOptions _options;

	public SitelenPonaRenderer(SitelenPonaOptions options) {
		_options = options;
	}

	protected override void Write(HtmlRenderer renderer, SitelenPona obj) {
		StringSlice text = obj.Text;

		if (renderer.EnableHtmlForInline) {
			renderer.Write($"<p class=\"sitelen_pona\" title=\"{text}\">");
			renderer.Write(text);
			renderer.Write("</p>");
		} else {
			renderer.Write(text);
		}
	}
}

[DebuggerDisplay($"sitelen pona: #{{{nameof(Text)}}}")]
public class SitelenPona : Inline {
	public StringSlice Text { get; set; }
}

public class SitelenPonaOptions {
	public string Font { get; set; }
	public bool TooltipOnHover { get; set; }
	
	public SitelenPonaOptions() {
		Font = "linja_sike";
		TooltipOnHover = true;
	}

	public SitelenPonaOptions(string font) : this() {
		Font = font;
	}
	
	public SitelenPonaOptions(string font, bool showTooltip) : this() {
		Font = font;
		TooltipOnHover = showTooltip;
	}
}