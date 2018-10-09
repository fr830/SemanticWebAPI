﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SemanticAPI.Models.Options;

namespace SemanticAPI.Auth
{
    public interface ITokenManager {
        string GenerateTokenForUser(string username);
        string GenerateAndRefreshToken(string jwtStr);
        bool isRefreshable(string jwtStr);
    }

    public class JwtManager: ITokenManager
    {
        private JwtOptions _jwtOpts;

        public JwtManager(IOptions<JwtOptions> jwtOpts)
        {
            _jwtOpts = jwtOpts.Value;
        }

        public string GenerateTokenForUser(string username)
        {
            var claims = new[]
                {
                    new Claim(ClaimTypes.Name, username)
                };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtOpts.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOpts.Issuer,
                audience: _jwtOpts.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtOpts.DurationMinutes),
                signingCredentials: creds
            );

            //Send a new token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool isRefreshable(string jwtStr)
        {
            var jwt = new JwtSecurityToken(jwtStr);
            return jwt.ValidTo.ToUniversalTime().Subtract(DateTime.Now.ToUniversalTime()).TotalMinutes < _jwtOpts.RefreshTime;
        }

        public string GenerateAndRefreshToken(string jwtStr)
        {
            var jwt = new JwtSecurityToken(jwtStr);

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtOpts.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOpts.Issuer,
                audience: _jwtOpts.Audience,
                claims: jwt.Claims,
                expires: DateTime.Now.AddMinutes(_jwtOpts.DurationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
