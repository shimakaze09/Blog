using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Extensions;

namespace Web.Apis.Blog;

[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
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
        var feed = new SyndicationFeed(
            "Blog",
            "My blog",
            new Uri("http://localhost:5205"), "RSSUrl", DateTime.Now
        )
        {
            Copyright = new TextSyndicationContent($"{DateTime.Now.Year} DealiAxy")
        };

        var items = new List<SyndicationItem>();
        var posts = await _postRepo.Where(a => a.IsPublish)
            .Include(a => a.Category)
            .ToListAsync();
        foreach (var item in posts)
        {
            var postUrl = Url.Action("Post", "Blog", new { id = item.Id }, HttpContext.Request.Scheme);
            items.Add(new SyndicationItem(item.Title, item.Summary, new Uri(postUrl), item.Id, item.LastUpdateTime)
            {
                Categories = { new SyndicationCategory(item.Category?.Name) },
                Authors = { new SyndicationPerson("admin@deali.cn", "DealiAxy", "https://deali.cn") },
                PublishDate = item.CreationTime
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
        var rssFormatter = new Rss20FeedFormatter(feed, false);
        rssFormatter.WriteTo(xmlWriter);
        await xmlWriter.FlushAsync();

        return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
    }
}