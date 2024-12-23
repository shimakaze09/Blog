﻿namespace Data.Models;

/// <summary>
///     Anonymous User
/// </summary>
public class AnonymousUser : ModelBase
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Url { get; set; }
    public string? Ip { get; set; }
}