using AutoMapper;
using Data.Models;
using Web.ViewModels.Links;

namespace Web.Properties.AutoMapper;

public class LinkProfile : Profile
{
    public LinkProfile()
    {
        CreateMap<LinkCreationDto, Link>();
    }
}