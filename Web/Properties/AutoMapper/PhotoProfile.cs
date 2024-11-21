using AutoMapper;
using Data.Models;
using Web.ViewModels.Photography;

namespace Web.Properties.AutoMapper;

public class PhotoProfile : Profile
{
    public PhotoProfile()
    {
        CreateMap<PhotoUpdateDto, Photo>();
    }
}