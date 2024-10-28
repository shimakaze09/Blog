using Data.Models;

namespace Web.ViewModels;

public class SearchResultViewModel
{
    public string Keyword { get; set; }
    public List<Post> Posts { get; set; }
}