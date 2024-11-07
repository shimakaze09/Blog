using Data.Models;
using FreeSql;

namespace Web.Services;

public class LinkService
{
    private readonly IBaseRepository<Link> _repo;

    public LinkService(IBaseRepository<Link> repo)
    {
        _repo = repo;
    }

    /// <summary>
    ///     Get all friend links
    /// </summary>
    /// <param name="onlyVisible">Only get displayed links</param>
    /// <returns></returns>
    public List<Link> GetAll(bool onlyVisible = true)
    {
        return onlyVisible
            ? _repo.Where(a => a.Visible).ToList()
            : _repo.Select.ToList();
    }

    public Link? GetById(int id)
    {
        return _repo.Where(a => a.Id == id).First();
    }

    public Link? GetByName(string name)
    {
        return _repo.Where(a => a.Name == name).First();
    }

    public Link AddOrUpdate(Link item)
    {
        return _repo.InsertOrUpdate(item);
    }

    public Link? SetVisibility(int id, bool visible)
    {
        var item = GetById(id);
        if (item == null) return null;
        item.Visible = visible;
        _repo.Update(item);
        return GetById(id);
    }

    public int DeleteById(int id)
    {
        return _repo.Delete(a => a.Id == id);
    }
}