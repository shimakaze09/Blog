using Data.Models;
using X.PagedList;

namespace Web.ViewModels;

public class PhotographyViewModel
{
    public IPagedList<Photo> Photos { get; set; }
}