using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace WebAPI1.Services
{
	public class JwtService
	{
		private readonly string _secret;
		private readonly string _issuer;
		private readonly string _audience;

		public JwtService(IConfiguration config)
		{
			_secret = config["Jwt:Key"];
			_issuer = config["Jwt:Issuer"];
			_audience = config["Jwt:Audience"];
		}

		public string GenerateToken(string id)
		{
			//Convert "Stored Key" From ASCII To Decimal
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, id),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			//The access Token should be stored in the dataBase with issue date and expired date.
			var token = new JwtSecurityToken(
				_issuer,
				_audience,
				claims,
				// This access Token valid for one day.
				expires: DateTime.Now.AddDays(1),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public TokenValidationParameters GetValidationParameters()
		{
			return new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = _issuer,
				ValidAudience = _audience,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret))
			};
		}
	}
}
