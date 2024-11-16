﻿using Data.Models;
using FreeSql;

namespace Web.Services;

/// <summary>
///     Link exchange application
/// </summary>
public class LinkExchangeService
{
    private readonly LinkService _linkService;
    private readonly IBaseRepository<LinkExchange> _repo;

    public LinkExchangeService(IBaseRepository<LinkExchange> repo, LinkService linkService)
    {
        _repo = repo;
        _linkService = linkService;
    }

    //// <summary>
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
            if (link != null) await _linkService.DeleteById(link.Id);
        }

        return await GetById(id);
    }

    public async Task<int> DeleteById(int id)
    {
        return await _repo.DeleteAsync(a => a.Id == id);
    }
}