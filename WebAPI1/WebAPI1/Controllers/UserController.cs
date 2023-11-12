using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using WebAPI1.Data;
using WebAPI1.Models;
using WebAPI1.Services;
using System.Diagnostics.Eventing.Reader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace UserApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UserController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly JwtService _jwtService;
		private const string Salt = "450d0b0db2bcf4adde5032eca1a7c416e560cf44";

		public UserController(ApplicationDbContext applicationDbContext, JwtService jwtService)
		{
			this._context = applicationDbContext;

			_jwtService = jwtService;
		}

		[HttpPost]
		public IActionResult CreateUser([FromBody] User user)
		{
			// Generate SHA1 hash for Id.
			user.Id = GenerateSha1Hash(user.Email + Salt);

			// Generate accessToken
			var token = _jwtService.GenerateToken(user.Id);

			_context.Users.Add(user);

			//Check if the Id is exist in DB.
			var result = _context.Users.Where(x => user.Id.Contains(x.Id)).ToList();
			if (result.Any())
			{
				return BadRequest("This Email already exist in DB.");
			}
			// Save to DB
			_context.SaveChanges();

			return Ok(new { user.Id, accessToken = token });

		}

		[HttpGet("{id}")]
		[Authorize]
		public IActionResult GetUser(string id, [FromHeader(Name = "Authorization")] string accessToken)
		{
			var jwtToken = ValidateToken(accessToken);
			if (jwtToken == null)
			{
				return Unauthorized("Invalid Token");
			}

			var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
			if (userIdClaim != id)
			{
				return Unauthorized("Access Denied");
			}

			var user = _context.Users.Find(id);
			if (user == null)
			{
				return NotFound();
			}
			//show user “email” if MarketingConsent is true otherwise omit it
			if (user.MarketingConsent == true)
			{
				return Ok(new { user.Id, user.FirstName, user.LastName, user.Email, user.MarketingConsent });
			}
			else
			{
				return Ok(new { user.Id, user.FirstName, user.LastName, user.MarketingConsent });
			}

		}

		private static string GenerateSha1Hash(string input)
		{
			using (var sha1 = new SHA1Managed())
			{
				var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
				var sb = new StringBuilder(hash.Length * 2);

				foreach (byte b in hash)
				{
					//format string - Convert From Decimal to Hexadecimal
					sb.Append(b.ToString("X2"));
				}

				return sb.ToString();
			}
		}

		private JwtSecurityToken ValidateToken(string token)
		{
			if (token.StartsWith("Bearer ", System.StringComparison.InvariantCultureIgnoreCase))
			{
				token = token.Substring("Bearer ".Length).Trim();
			}

			if (string.IsNullOrEmpty(token))
			{
				return null;
			}

			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var validationParameters = _jwtService.GetValidationParameters();
				tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

				return validatedToken as JwtSecurityToken;
			}
			catch
			{
				return null;
			}
		}
	}
}
