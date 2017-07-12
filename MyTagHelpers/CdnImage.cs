using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace TagHelpers.MyTagHelpers
{
  [HtmlTargetElement("img")]
  public class CdnImage : ImageTagHelper
  {

    public CdnImage(IHostingEnvironment hostingEnvironment, IMemoryCache cache, HtmlEncoder htmlEncoder, IUrlHelperFactory urlHelperFactory) : base(hostingEnvironment, cache, htmlEncoder, urlHelperFactory)
    {

    }

    private const string OnErrorAttributeName = "onerror";
    private const string SrcAttributeName = "src";
    private FileVersionProvider _fileVersionProvider;
    private const string FallbackSrcAttributeName = "asp-fallback-src";
    private const string CdnUriAttributeName = "asp-cdn-uri";

    /// <summary>
    /// The URL of the Image tag to fallback to in the case the primary one fails
    /// </summary>
    /// <remarks>
    /// Utilizes the onerror JavaScript handler for fallback
    /// </remarks>
    [HtmlAttributeName(FallbackSrcAttributeName)]
    public string FallbackSrc { get; set; }

    /// <summary>
    /// The base URL of the CDN.
    /// </summary>
    /// <remarks>
    /// This value prefixes the value in src
    /// </remarks>
    [HtmlAttributeName(CdnUriAttributeName)]
    public string CdnUri { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
      if (context == null)
      {
        throw new ArgumentNullException(nameof(context));
      }

      if (output == null)
      {
        throw new ArgumentNullException(nameof(output));
      }

      output.CopyHtmlAttribute(SrcAttributeName, context);
      ProcessUrlAttribute(SrcAttributeName, output);

      if (AppendVersion)
      {
        EnsureFileVersionProvider();

        Src = output.Attributes[SrcAttributeName].Value as string;

        // Check if the CDN Base URI is set and add to the src attribute
        if (!string.IsNullOrWhiteSpace(CdnUri))
        {
          output.Attributes.SetAttribute(SrcAttributeName, string.Format("{0}{1}", CdnUri, _fileVersionProvider.AddFileVersionToPath(Src)));
        }
      }

      //Retrieve any existing onerror handler code
      var onError = output.Attributes[OnErrorAttributeName]?.Value as string;

      //Check if there's a fallback source and no onerror handler
      if (!string.IsNullOrWhiteSpace(FallbackSrc) && string.IsNullOrWhiteSpace(onError))
      {
        string resolvedUrl;
        if (TryResolveUrl(FallbackSrc, out resolvedUrl))
        {
          FallbackSrc = resolvedUrl;
        }

        if (AppendVersion)
        {
          FallbackSrc = _fileVersionProvider.AddFileVersionToPath(FallbackSrc);
        }

        //Apply fallback handler code
        onError = $"this.src='{FallbackSrc}';console.log('{Src} NOT FOUND.')";
        output.Attributes.SetAttribute(OnErrorAttributeName, onError);
      }
    }

    private void EnsureFileVersionProvider()
    {
      if (_fileVersionProvider == null)
      {
        _fileVersionProvider = new FileVersionProvider(
            HostingEnvironment.WebRootFileProvider,
            Cache,
            ViewContext.HttpContext.Request.PathBase);
      }
    }
  }
}