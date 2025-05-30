﻿using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.AuthTokenProcessor;

public interface IAuthTokenService
{
    (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiration);
}