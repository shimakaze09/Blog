using AutoMapper;
using Data.Models;
using Web.ViewModels.LinkExchange;

namespace Web.Properties.AutoMapper;

public class LinkExchangeProfile : Profile
{
    public LinkExchangeProfile()
    {
        CreateMap<LinkExchangeAddViewModel, LinkExchange>();
    }
}