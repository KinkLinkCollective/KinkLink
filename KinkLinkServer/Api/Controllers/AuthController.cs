using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Network.GetToken;
using KinkLinkCommon.Domain.Network.LoginAuthentication;
using KinkLinkServer.Domain;
using KinkLinkServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace KinkLinkServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(Configuration config, DatabaseService database) : ControllerBase
{
    // Const
    private static readonly Version ExpectedVersion = new(0, 0, 0, 1);

    // Instantiated
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(config.SigningKey));

    [AllowAnonymous]
    [HttpPost("profiles")]
    public async Task<IActionResult> Profiles([FromBody] GetTokenRequest request)
    {
        if (request.Version < ExpectedVersion)
            return StatusCode(StatusCodes.Status409Conflict, new LoginAuthenticationResult(LoginAuthenticationErrorCode.VersionMismatch, null));

        var authResult = await database.AuthenticateUser(request.Secret);
        if (authResult.Status != DBAuthenticationStatus.Authorized && authResult.Uids.Count() != 0)
            return StatusCode(StatusCodes.Status401Unauthorized, new LoginAuthenticationResult(LoginAuthenticationErrorCode.UnknownSecret, null));

        // TODO: Fix this up to return a list of the profiles for a given secret key
        //     return StatusCode(StatusCodes.Status401Unauthorized, new LoginAuthenticationResult(LoginAuthenticationErrorCode.UnknownError, null));
        throw new NotImplementedException();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] GetTokenRequest request)
    {
        if (request.Version < ExpectedVersion)
            return StatusCode(StatusCodes.Status409Conflict, new LoginAuthenticationResult(LoginAuthenticationErrorCode.VersionMismatch, ""));

        var authStatus = await database.LoginUser(request.Secret, request.ProfileUID);
        if (authStatus != DBAuthenticationStatus.Authorized)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new LoginAuthenticationResult(LoginAuthenticationErrorCode.UnknownSecret, ""));
        }

        var token = GenerateJwtToken([new Claim(AuthClaimTypes.Uid, request.ProfileUID)]);
        return StatusCode(StatusCodes.Status200OK, new LoginAuthenticationResult(LoginAuthenticationErrorCode.Success, token.RawData));
    }

    private JwtSecurityToken GenerateJwtToken(List<Claim> claims)
    {
        var token = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddHours(4)
        };

        return new JwtSecurityTokenHandler().CreateJwtSecurityToken(token);
    }
}
