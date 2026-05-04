using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace TechAPI.Middleware
{
    public interface IMiddlewareElementService
    {
        Task<bool> LoginRequestAsync(HttpContext context);
    }

    public class MiddlewareElement : IMiddlewareElementService
    {
        private readonly IConfiguration _configuration;

        public MiddlewareElement(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> LoginRequestAsync(HttpContext context)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            string secretKey = jwtSettings["Key"];
            string issuer = jwtSettings["Issuer"];
            string audience = jwtSettings["Audience"];
            Console.WriteLine(context.Request.Method);

            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (authHeader == null || !authHeader.StartsWith("Bearer"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Token missing" });
                return false;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true; // token is valid
            }
            catch (SecurityTokenExpiredException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Token expired" });
                return false;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Invalid signature" });
                return false;
            }
            catch (SecurityTokenException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Invalid token" });
                return false;
            }
            catch (Exception)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Authentication failed" });
                return false;
            }
        }
    }
}