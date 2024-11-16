using System.Text;
using Data.Models;
using FreeSql;
using Microsoft.Extensions.Options;
using Share.Utils;

namespace Web.Services;

/// <summary>
///     Link exchange application
/// </summary>
public class LinkExchangeService
{
    private readonly LinkService _linkService;
    private readonly IBaseRepository<LinkExchange> _repo;
    private readonly EmailAccountConfig _emailAccountConfig;

    public LinkExchangeService(IBaseRepository<LinkExchange> repo, LinkService linkService,
        IOptions<EmailAccountConfig> options)
    {
        _repo = repo;
        _linkService = linkService;
        _emailAccountConfig = options.Value;
    }

    /// <summary>
    /// Check if the ID exists
    /// </summary>
    public async Task<bool> HasId(int id)
    {
        return await _repo.Where(a => a.Id == id).AnyAsync();
    }

    public async Task<bool> HasUrl(string url)
    {
        return await _repo.Where(a => a.Url.Contains(url)).AnyAsync();
    }


    public async Task<List<LinkExchange>> GetAll()
    {
        return await _repo.Select.ToListAsync();
    }

    public async Task<LinkExchange?> GetById(int id)
    {
        return await _repo.Where(a => a.Id == id).FirstAsync();
    }

    public async Task<LinkExchange> AddOrUpdate(LinkExchange item)
    {
        return await _repo.InsertOrUpdateAsync(item);
    }

    public async Task<LinkExchange?> SetVerifyStatus(int id, bool status, string? reason = null)
    {
        var item = await GetById(id);
        if (item == null) return null;

        item.Verified = status;
        item.Reason = reason;
        await _repo.UpdateAsync(item);

        var link = await _linkService.GetByName(item.Name);
        if (status)
        {
            await SendEmailOnAccept(item);
            if (link == null)
                await _linkService.AddOrUpdate(new Link
                {
                    Name = item.Name,
                    Description = item.Description,
                    Url = item.Url,
                    Visible = true
                });
            else
                await _linkService.SetVisibility(link.Id, true);
        }
        else
        {
            await SendEmailOnReject(item);
            if (link != null) await _linkService.DeleteById(link.Id);
        }

        return await GetById(id);
    }

    public async Task<int> DeleteById(int id)
    {
        return await _repo.DeleteAsync(a => a.Id == id);
    }

    public async Task SendEmailOnAdd(LinkExchange item)
    {
        const string blogLink = "<a href=\"https://deali.cn\">Blog</a>";
        var sb = new StringBuilder();
        sb.AppendLine(
            $"<p>Your link exchange application has been submitted and is being processed. Please keep an eye on email notifications.</p>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<p>Here is the information you submitted:</p>");
        sb.AppendLine($"<p>Website Name: {item.Name}</p>");
        sb.AppendLine($"<p>Description: {item.Description}</p>");
        sb.AppendLine($"<p>URL: {item.Url}</p>");
        sb.AppendLine($"<p>Webmaster: {item.WebMaster}</p>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<p>This message was automatically sent by {blogLink}. No reply is needed.</p>");
        await EmailUtils.SendEmailAsync(
            _emailAccountConfig,
            "[Blog] Link Exchange Application Submitted",
            sb.ToString(),
            item.WebMaster,
            item.Email
        );
    }

    public async Task SendEmailOnAccept(LinkExchange item)
    {
        const string blogLink = "<a href=\"https://deali.cn\">Blog</a>";
        var sb = new StringBuilder();
        sb.AppendLine(
            $"<p>Hello, your link exchange application has been approved! Thank you for your support, and feel free to visit us.</p>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<p>Here is the information you submitted:</p>");
        sb.AppendLine($"<p>Website Name: {item.Name}</p>");
        sb.AppendLine($"<p>Description: {item.Description}</p>");
        sb.AppendLine($"<p>URL: {item.Url}</p>");
        sb.AppendLine($"<p>Webmaster: {item.WebMaster}</p>");
        sb.AppendLine($"<p>Additional Information: {item.Reason}</p>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<p>This message was automatically sent by {blogLink}. No reply is needed.</p>");
        await EmailUtils.SendEmailAsync(
            _emailAccountConfig,
            "[Blog] Link Exchange Application Feedback",
            sb.ToString(),
            item.WebMaster,
            item.Email
        );
    }

    public async Task SendEmailOnReject(LinkExchange item)
    {
        const string blogLink = "<a href=\"https://deali.cn\">Blog</a>";
        var sb = new StringBuilder();
        sb.AppendLine(
            $"<p>We regret to inform you that your link exchange application was not approved. Please review the additional information and reapply. Thank you for your understanding and support.</p>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<p>Here is the information you submitted:</p>");
        sb.AppendLine($"<p>Website Name: {item.Name}</p>");
        sb.AppendLine($"<p>Description: {item.Description}</p>");
        sb.AppendLine($"<p>URL: {item.Url}</p>");
        sb.AppendLine($"<p>Webmaster: {item.WebMaster}</p>");
        sb.AppendLine($"<p>Additional Information: {item.Reason}</p>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<p>This message was automatically sent by {blogLink}. No reply is needed.</p>");
        await EmailUtils.SendEmailAsync(
            _emailAccountConfig,
            "[Blog] Link Exchange Application Feedback",
            sb.ToString(),
            item.WebMaster,
            item.Email
        );
    }
}