﻿namespace Data.Models;

public interface ISoftDelete
{
    public bool IsDeleted { get; set; }
}