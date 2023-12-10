﻿using Domain.Base;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Identity;

[Index(nameof(UserId), nameof(Token), nameof(JwtHash), nameof(ExpiresAt))]
public class RefreshToken : BaseIdDbEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Token { get; set; } = Guid.NewGuid().ToString();

    public DateTime ExpiresAt { get; set; }

    public required string JwtHash { get; set; }
}