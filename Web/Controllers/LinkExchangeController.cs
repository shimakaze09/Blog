﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Contrib.SiteMessage;
using Data.Models;
using Web.Services;
using Web.ViewModels.LinkExchange;

namespace Web.Controllers;

public class LinkExchangeController : Controller
{
    private readonly ILogger<LinkExchangeController> _logger;
    private readonly LinkExchangeService _service;
    private readonly IMapper _mapper;
    private readonly Messages _messages;

    public LinkExchangeController(ILogger<LinkExchangeController> logger, LinkExchangeService service, IMapper mapper,
        Messages messages)
    {
        _logger = logger;
        _service = service;
        _mapper = mapper;
        _messages = messages;
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(LinkExchangeAddViewModel vm)
    {
        if (!ModelState.IsValid) return View();

        if (await _service.HasUrl(vm.Url))
        {
            _messages.Error("A link exchange request for the same URL has already been submitted!");
            return View();
        }

        var item = _mapper.Map<LinkExchange>(vm);
        await _service.AddOrUpdate(item);

        _messages.Info("Link exchange request submitted. Please check your email for updates.");
        return RedirectToAction("Index", "Home");
    }
}