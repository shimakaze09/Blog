﻿using AutoMapper;
using CodeLab.Share.ViewModels.Response;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Links;

namespace Web.Apis.Links;

/// <summary>
///     Friendly Links
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Link)]
public class LinkController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly LinkService _service;

    public LinkController(LinkService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<List<Link>> GetAll()
    {
        return await _service.GetAll(false);
    }

    [HttpGet("{id:int}")]
    public async Task<ApiResponse<Link>> Get(int id)
    {
        var item = await _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<Link>(item);
    }

    [HttpPost]
    public async Task<Link> Add(LinkCreationDto dto)
    {
        var link = _mapper.Map<Link>(dto);
        link = await _service.AddOrUpdate(link);
        return link;
    }

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<Link>> Update(int id, LinkCreationDto dto)
    {
        var item = await _service.GetById(id);
        if (item == null) return ApiResponse.NotFound();

        var link = _mapper.Map(dto, item);
        link = await _service.AddOrUpdate(link);
        return new ApiResponse<Link>(link);
    }

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        if (!await _service.HasId(id)) return ApiResponse.NotFound();
        var rows = await _service.DeleteById(id);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}