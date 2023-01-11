using ChristMusic.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChristMusic.TagHelpers
{
    //HTML element that can use the custom tag helper. 
    //And the element must contain a attribute called "PageModel"
    [HtmlTargetElement("div", Attributes ="page-model")] 
    public class PageLinkTagHelper:TagHelper
    {
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext viewContext { get; set; }
        public PagingInfo PageModel { get; set; }
        public string PageAction { get; set; }
        public bool PageClassesEnabled { get; set; }
        public string PageClass { get; set; }
        public string pageClassNormal { get; set; }
        public string PageClassSelected { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            TagBuilder result = new TagBuilder("div");

            for(int i=1; i<= PageModel.TotalPage; i++)
            {
                TagBuilder tag = new TagBuilder("a");
                string url = PageModel.urlParam.Replace(":", i.ToString());
                tag.Attributes["href"] = url;

                if(PageClassesEnabled)
                {
                    tag.AddCssClass(PageClass);
                    tag.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected : pageClassNormal);
                }

                tag.InnerHtml.Append(i.ToString());
                result.InnerHtml.AppendHtml(tag);
            }

            output.Content.AppendHtml(result.InnerHtml);
        }
    }
}
