﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StackExchange.Profiling
{
    /// <summary>
    /// Tag helper to profile script execution in ASP.NET Core views, e.g. 
    /// &lt;profile-script name="My Step" /&gt;
    /// ...script blocks...
    /// &lt;/profile-script&gt;
    /// Include as self closing to provide initialization only.
    /// </summary>
    [HtmlTargetElement("profile-script", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ProfileScriptTagHelper : TagHelper
    {
        private static readonly object ClientTimingKey = Guid.NewGuid();

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// The name of this <see cref="MiniProfiler"/> step.
        /// </summary>
        [HtmlAttributeName("name")]
        public string Name { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            output.Content = await output.GetChildContentAsync();

            if (MiniProfiler.Current == null)
                return;

            if (!ViewContext.HttpContext.Items.ContainsKey(ClientTimingKey))
            {
                output.PreContent.AppendHtml(ClientTimingHelper.InitScript);
                ViewContext.HttpContext.Items[ClientTimingKey] = true;
            }

            if (output.TagMode == TagMode.SelfClosing)
                return;

            output.PreContent.AppendHtml($"<script>mPt.start('{Name}')</script>");
            output.PostContent.SetHtmlContent($"<script>mPt.end('{Name}')</script>");
        }
    }
}
