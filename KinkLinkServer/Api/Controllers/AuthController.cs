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
public class AuthController(ILogger<AuthController> logger, Configuration config, DatabaseService database) : ControllerBase
{
    // Const
    private static readonly Version ExpectedVersion = new(0, 0, 0, 1);

    // Instantiated
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(config.SigningKey));

    [AllowAnonymous]
    [HttpPost("profiles")]
    public async Task<IActionResult> Profiles([FromBody] GetTokenRequest request)
    {
        logger.LogInformation("Profiles request received for version {Version}", request.Version);

        if (request.Version < ExpectedVersion)
        {
            logger.LogWarning("Version mismatch: expected {ExpectedVersion}, got {ActualVersion}", ExpectedVersion, request.Version);
            return StatusCode(StatusCodes.Status409Conflict, new LoginAuthenticationResult(LoginAuthenticationErrorCode.VersionMismatch, null));
        }

        var authResult = await database.AuthenticateUser(request.Secret);
        if (authResult.Status != DBAuthenticationStatus.Authorized && authResult.Uids.Count() != 0)
        {
            logger.LogWarning("Authentication failed for secret: {Status}", authResult.Status);
            return StatusCode(StatusCodes.Status401Unauthorized, new LoginAuthenticationResult(LoginAuthenticationErrorCode.UnknownSecret, null));
        }

        logger.LogInformation("Profiles request successful for {UidCount} UIDs", authResult.Uids.Count());
        // TODO: Fix this up to return a list of the profiles for a given secret key
        //     return StatusCode(StatusCodes.Status401Unauthorized, new LoginAuthenticationResult(LoginAuthenticationErrorCode.UnknownError, null));
        throw new NotImplementedException();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] GetTokenRequest request)
    {
        logger.LogInformation("Login request received for profile UID {ProfileUID}, version {Version}", request.ProfileUID, request.Version);

        if (request.Version < ExpectedVersion)
        {
            logger.LogWarning("Version mismatch for profile {ProfileUID}: expected {ExpectedVersion}, got {ActualVersion}", request.ProfileUID, ExpectedVersion, request.Version);
            return StatusCode(StatusCodes.Status409Conflict, new LoginAuthenticationResult(LoginAuthenticationErrorCode.VersionMismatch, ""));
        }

        var authStatus = await database.LoginUser(request.Secret, request.ProfileUID);
        if (authStatus != DBAuthenticationStatus.Authorized)
        {
            logger.LogWarning("Login failed for profile {ProfileUID}: {Status}", request.ProfileUID, authStatus);
            return StatusCode(StatusCodes.Status401Unauthorized, new LoginAuthenticationResult(LoginAuthenticationErrorCode.UnknownSecret, ""));
        }

        var token = GenerateJwtToken([new Claim(AuthClaimTypes.Uid, request.ProfileUID)]);
        logger.LogInformation("Login successful for profile {ProfileUID}", request.ProfileUID);
        return StatusCode(StatusCodes.Status200OK, new LoginAuthenticationResult(LoginAuthenticationErrorCode.Success, token.RawData));
    }

    private JwtSecurityToken GenerateJwtToken(List<Claim> claims)
    {
        logger.LogInformation("Generating JWT token with {ClaimCount} claims", claims.Count);

        var token = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddHours(4)
        };

        var jwtToken = new JwtSecurityTokenHandler().CreateJwtSecurityToken(token);
        logger.LogInformation("JWT token generated successfully, expires at {ExpiryTime}", token.Expires);
        return jwtToken;
    }
}
