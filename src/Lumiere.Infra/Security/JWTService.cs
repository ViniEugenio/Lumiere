using Lumiere.Application.DTOs.User;
using Lumiere.Application.Interfaces.Services;
using Lumiere.Infra.EnvConfigurationModels;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lumiere.Infra.Security
{
    public class JWTService(IOptions<SecurityModel> securityModelOptions) : IJWTService
    {

        public string GenerateJWT(JWTUserDataDto jwtUserData)
        {

            JWTConfigurationModel jwtConfiguration = securityModelOptions
                .Value
                .JWTConfigurations;

            SecurityTokenDescriptor tokenDescriptor = FormatSecurityTokenDescriptor(jwtConfiguration, jwtUserData);

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }

        private static SecurityTokenDescriptor FormatSecurityTokenDescriptor(JWTConfigurationModel jwtConfiguration, JWTUserDataDto jwtUserData)
        {

            List<Claim> claims = FormatUserClaims(jwtUserData);
            SigningCredentials signingCredentials = FormatSigningCredentials(jwtConfiguration);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new(claims),
                Expires = DateTime.UtcNow.AddMinutes(jwtConfiguration.JWTExpireTimeInMinutes),
                SigningCredentials = signingCredentials
            };

            return tokenDescriptor;

        }

        private static List<Claim> FormatUserClaims(JWTUserDataDto jwtUserData)
        {

            List<Claim> userClaims = [..

                jwtUserData
                    .Roles
                    .Select(role => new Claim(ClaimTypes.Role, role.ToString()))

            ];

            userClaims.Add(new(ClaimTypes.Name, jwtUserData.Name));
            userClaims.Add(new(ClaimTypes.Surname, jwtUserData.Surname));
            userClaims.Add(new(ClaimTypes.Email, jwtUserData.Email));

            return userClaims;

        }

        private static SigningCredentials FormatSigningCredentials(JWTConfigurationModel jwtConfiguration)
        {

            byte[] keyInBytes = Encoding.ASCII.GetBytes(jwtConfiguration.JwtKey);

            SymmetricSecurityKey securityKey = new(keyInBytes);
            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            return signingCredentials;

        }

    }
}
