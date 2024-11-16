using Data.Models;
using X.PagedList;

namespace Web.ViewModels.Photography;

public class PhotographyViewModel
{
    public IPagedList<Photo> Photos { get; set; }
}