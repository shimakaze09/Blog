using AutoMapper;
using Data.Models;
using Web.ViewModels.Blog;

namespace Web.Properties.AutoMapper;

public class PostProfile : Profile
{
    public PostProfile()
    {
        CreateMap<PostUpdateDto, Post>();
        CreateMap<PostCreationDto, Post>();
    }
}