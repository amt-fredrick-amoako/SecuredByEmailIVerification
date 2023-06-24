using Microsoft.IdentityModel.Tokens;
using SecuredByEmailIVerification.Model;
using SecuredByEmailIVerification.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecuredByEmailIVerification.Services
{
    public class JwtService : IJwtService
    {
        private IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthenticationResponse CreateJwtToken(ApplicationUser user)
        {
            // expiration time
            DateTime expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));
            // user payload
            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // subject user id
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),// JWT unique ID
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()), // issued at: date and time of token generation
                new Claim(ClaimTypes.NameIdentifier, user.Email.ToString()),// unique name identifier of the user: email
                new Claim(ClaimTypes.NameIdentifier, user.PersonName.ToString()) // unique name identifier of the user: person name
            };
            // security key
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            // hashing
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken tokenGenerator = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims, expires: expiration, signingCredentials: signingCredentials);
            // create tokenHandler to write the token itself
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            string token = handler.WriteToken(tokenGenerator);
            return new AuthenticationResponse
            {
                Token = token,
                Email = user.Email,
                PersonName = user.PersonName,
                Expiration = expiration,
            };
        }
    }
}
