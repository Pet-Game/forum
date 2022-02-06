using System;
using System.Web;
using Microsoft.AspNetCore.Html;

namespace PetGameForum.Util; 

public static class JS {
	public static HtmlString JsString<T>(this T value, char quote = '\'') {
		return new HtmlString($"{quote}{HttpUtility.HtmlEncode(value)}{quote}");
	}
}