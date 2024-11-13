using System.Text.Encodings.Web;
using System.Text.Json;
using Data.Models;
using Web.ViewModels.Categories;
using X.PagedList;

namespace Web.ViewModels.Blog;

public class BlogListViewModel
{
    public Category CurrentCategory { get; set; }
    public int CurrentCategoryId { get; set; }
    public IPagedList<Post> Posts { get; set; }
    public List<Category> Categories { get; set; }
    public List<CategoryNode>? CategoryNodes { get; set; }

    public string CategoryNodesJson => JsonSerializer.Serialize(
        CategoryNodes,
        new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
    );
}