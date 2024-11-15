using Data.Models;
using FreeSql;
using Web.Controllers;
using Web.ViewModels.Categories;
using X.PagedList;
using X.PagedList.Extensions;

namespace Web.Services;

public class CategoryService
{
    private readonly IHttpContextAccessor _accessor;
    private readonly IBaseRepository<Category> _cRepo;
    private readonly IBaseRepository<FeaturedCategory> _fcRepo;
    private readonly LinkGenerator _generator;

    public CategoryService(IBaseRepository<Category> cRepo,
        IBaseRepository<FeaturedCategory> fcRepo,
        IHttpContextAccessor accessor,
        LinkGenerator generator)
    {
        _cRepo = cRepo;
        _fcRepo = fcRepo;
        _accessor = accessor;
        _generator = generator;
    }

    public async Task<List<CategoryNode>?> GetNodes()
    {
        var categoryList = await _cRepo.Select
            .IncludeMany(a => a.Posts.Select(p => new Post { Id = p.Id }))
            .ToListAsync();
        return GetNodes(categoryList);
    }

    /// <summary>
    ///     Generate article category tree
    /// </summary>
    public List<CategoryNode>? GetNodes(List<Category> categoryList, int parentId = 0)
    {
        var categories = categoryList
            .Where(a => a.ParentId == parentId && a.Visible)
            .ToList();

        if (categories.Count == 0) return null;

        return categories.Select(category => new CategoryNode
        {
            text = category.Name,
            href = _generator.GetUriByAction(
                _accessor.HttpContext!,
                nameof(BlogController.List),
                "Blog",
                new { categoryId = category.Id }
            ),
            tags = new List<string> { category.Posts.Count.ToString() },
            nodes = GetNodes(categoryList, category.Id)
        }).ToList();
    }

    public async Task<List<Category>> GetAll()
    {
        return await _cRepo.Select.ToListAsync();
    }

    public async Task<IPagedList<Category>> GetPagedList(int page = 1, int pageSize = 10)
    {
        return (await _cRepo.Select.ToListAsync()).ToPagedList(page, pageSize);
    }

    public async Task<Category?> GetById(int id)
    {
        return await _cRepo.Where(a => a.Id == id)
            .Include(a => a.Parent).FirstAsync();
    }

    /// <summary>
    ///     Generate category word cloud data
    /// </summary>
    /// <returns></returns>
    public async Task<List<object>> GetWordCloud()
    {
        var list = await _cRepo.Select
            .Where(a => a.Visible && a.ParentId == 0)
            .IncludeMany(a => a.Posts).ToListAsync();

        var data = list.Select(item => new { name = item.Name, value = item.Posts.Count }).ToList<object>();

        return data;
    }

    public async Task<List<FeaturedCategory>> GetFeaturedCategories()
    {
        return await _fcRepo.Select.Include(a => a.Category).ToListAsync();
    }

    public async Task<FeaturedCategory?> GetFeaturedCategoryById(int id)
    {
        return await _fcRepo.Where(a => a.Id == id)
            .Include(a => a.Category).FirstAsync();
    }

    public async Task<FeaturedCategory> AddOrUpdateFeaturedCategory(Category category, FeaturedCategoryCreationDto dto)
    {
        var item = await _fcRepo.Where(a => a.CategoryId == category.Id).FirstAsync();
        if (item == null)
        {
            item = new FeaturedCategory
            {
                CategoryId = category.Id,
                Name = dto.Name,
                Description = dto.Description,
                IconCssClass = dto.IconCssClass
            };
        }
        else
        {
            item.Name = dto.Name;
            item.Description = dto.Description;
            item.IconCssClass = dto.IconCssClass;
        }

        await _fcRepo.InsertOrUpdateAsync(item);
        return item;
    }

    public async Task<int> DeleteFeaturedCategory(Category category)
    {
        return await _fcRepo.Where(a => a.CategoryId == category.Id).ToDelete().ExecuteAffrowsAsync();
    }

    public async Task<int> DeleteFeaturedCategoryById(int id)
    {
        return await _fcRepo.DeleteAsync(a => a.Id == id);
    }

    public async Task<int> SetVisibility(Category category, bool isVisible)
    {
        category.Visible = isVisible;
        return await _cRepo.UpdateAsync(category);
    }

    /// <summary>
    ///     Retrieves the hierarchical structure of a specified category.
    ///     <para>Format: 1,3,5,7,9</para>
    /// </summary>
    public string GetCategoryBreadcrumb(Category item)
    {
        var categories = new List<Category> { item };
        var parent = item.Parent;

        while (parent != null)
        {
            categories.Add(parent);
            parent = parent.Parent;
        }

        categories.Reverse();
        return string.Join(",", categories.Select(a => a.Id));
    }
}