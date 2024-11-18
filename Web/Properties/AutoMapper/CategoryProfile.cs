using AutoMapper;
using Data.Models;
using Web.ViewModels.Categories;

namespace Web.Properties.AutoMapper;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryCreationDto, Category>();
    }
}