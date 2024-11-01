using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Contrib.Extensions;
using Contrib.Utils;
using Data.Models;

namespace Migrate;

public class PostProcessor
{
    private readonly Post _post;
    private readonly string _importPath;
    private readonly string _assetsPath;

    public PostProcessor(string importPath, string assetsPath, Post post)
    {
        _post = post;
        _assetsPath = assetsPath;
        _importPath = importPath;
    }

    /// <summary>
    ///     Parse Markdown content, copy images & replace image links
    /// </summary>
    /// <returns></returns>
    public string MarkdownParse()
    {
        var document = Markdown.Parse(_post.Content);

        foreach (var node in document.AsEnumerable())
        {
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline)
            {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;

                if (linkInline.Url == null) continue;
                if (linkInline.Url.StartsWith("http")) continue;

                // Path processing
                var imgPath = Path.Combine(_importPath, _post.Path, linkInline.Url);
                var imgFilename = Path.GetFileName(linkInline.Url);
                var destDir = Path.Combine(_assetsPath, _post.Id);
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                var destPath = Path.Combine(destDir, imgFilename);
                if (File.Exists(destPath))
                {
                    // Image renaming
                    var imgId = GuidUtils.GuidTo16String();
                    imgFilename =
                        $"{Path.GetFileNameWithoutExtension(imgFilename)}-{imgId}.{Path.GetExtension(imgFilename)}";
                    destPath = Path.Combine(destDir, imgFilename);
                }

                // Replace image link
                linkInline.Url = imgFilename;
                // Copy image
                File.Copy(imgPath, destPath);

                Console.WriteLine($"Copy {imgPath} to {destPath}");
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }

    /// <summary>
    /// Extract a summary of length characters from the beginning of the article content
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public string GetSummary(int length)
    {
        return _post.Content == null
            ? string.Empty
            : Markdown.ToPlainText(_post.Content).Limit(length);
    }

    /// <summary>
    /// Fill in article status and title
    /// </summary>
    /// <returns></returns>
    public (string, string) InflateStatusTitle()
    {
        const string pattern = @"（(.+)）(.+)";
        var status = "Published";
        var title = _post.Title;
        if (title == null) return (status, "");
        var result = Regex.Match(title, pattern);
        if (!result.Success) return (status, title);

        status = result.Groups[1].Value;
        title = result.Groups[2].Value;

        _post.Status = status;
        _post.Title = title;

        return (status, title);
    }
}