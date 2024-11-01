using Contrib.Extensions;
using Contrib.Utils;
using Data.Models;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Migrate;

public class PostProcessor
{
    private readonly string _assetsPath;
    private readonly string _importPath;
    private readonly Post _post;

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
            if (node is not ParagraphBlock { Inline: not null } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline)
            {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;
                if (linkInline.Url == null) continue;
                if (linkInline.Url.StartsWith("http")) continue;

                // Process paths
                var imgPath = Path.Combine(_importPath, _post.Path, linkInline.Url);
                var imgFilename = Path.GetFileName(linkInline.Url);
                var destDir = Path.Combine(_assetsPath, _post.Id);
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                var destPath = Path.Combine(destDir, imgFilename);
                if (File.Exists(destPath))
                {
                    var imgId = GuidUtils.GuidTo16String();
                    imgFilename = $"{imgId}-{Path.GetFileName(linkInline.Url)}";
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

    public string GetSummary(int length)
    {
        return Markdown.ToPlainText(_post.Content).Limit(length);
    }
}