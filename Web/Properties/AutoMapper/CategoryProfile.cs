using AutoMapper;
using Data.Models;
using StarBlog.Web.ViewModels.Categories;

namespace StarBlog.Web.Properties.AutoMapper;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryCreationDto, Category>();
    }
}