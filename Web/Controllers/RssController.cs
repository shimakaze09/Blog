using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Data.Models;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;

namespace Web.Controllers;

[ApiController]
[Route("feed")]
[ApiExplorerSettings(IgnoreApi = true)]
public class RssController : ControllerBase
{
    private readonly IBaseRepository<Post> _postRepo;

    public RssController(IBaseRepository<Post> postRepo)
    {
        _postRepo = postRepo;
    }

    [ResponseCache(Duration = 1200)]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var posts = await _postRepo.Where(a => a.IsPublish && a.CreationTime.Year == DateTime.Now.Year)
            .OrderByDescending(a => a.LastUpdateTime)
            .Include(a => a.Category)
            .ToListAsync();

        var feed = new SyndicationFeed(
            "Blog",
            "My blog",
            new Uri("http://localhost:5205"), "RSSUrl", posts.First().LastUpdateTime
        )
        {
            Copyright = new TextSyndicationContent($"{DateTime.Now.Year} John")
        };

        var items = new List<SyndicationItem>();
        foreach (var item in posts)
        {
            var postUrl = Url.Action("Post", "Blog", new { id = item.Id }, HttpContext.Request.Scheme);
            items.Add(new SyndicationItem(item.Title,
                new TextSyndicationContent(PostService.GetContentHtml(item), TextSyndicationContentKind.Html),
                new Uri(postUrl), item.Id, item.LastUpdateTime
            )
            {
                Categories = { new SyndicationCategory(item.Category?.Name) },
                Authors = { new SyndicationPerson("yangtianzhuo970425@gmail.com", "John", "https://localhost:5205") },
                PublishDate = item.CreationTime,
                Summary = new TextSyndicationContent(item.Summary)
            });
        }

        feed.Items = items;

        var settings = new XmlWriterSettings
        {
            Async = true,
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            NewLineOnAttributes = true,
            Indent = true
        };
        using var stream = new MemoryStream();
        await using var xmlWriter = XmlWriter.Create(stream, settings);
        var rssFormatter = new Atom10FeedFormatter(feed);
        rssFormatter.WriteTo(xmlWriter);
        await xmlWriter.FlushAsync();

        return File(stream.ToArray(), "application/xml; charset=utf-8");
    }
}