using System.Reflection.Metadata.Ecma335;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

public class IdentityService(UserManager<AppUser> userManager,IUserClaimsPrincipalFactory<AppUser> UserClaim,IAuthorizationService authorizationService) : IIdentityService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IUserClaimsPrincipalFactory<AppUser> _userClaimPrincipalFactory = UserClaim;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if(user is null )
        {
            return Error.NotFound(code:"User_Not_Found",description:$"User with email {UtilityService.MaskEmail(email)} was not found");
        }
        if(!user.EmailConfirmed)
        {
            return Error.Conflict("Email_NOt_Confirmed",$"User with email {UtilityService.MaskEmail(email)} was not Confirmed");
        }
        if(!await _userManager.CheckPasswordAsync(user,password))
        {
            return Error.Conflict("Invalid_Login_Attempt","Email/Password Incorrect");
        }

        return new AppUserDto(user.Id,user.Email!, await _userManager.GetRolesAsync(user),await _userManager.GetClaimsAsync(user));
    }

    public async Task<bool> AuthorizeAsync(string userId, string? policyName)
    {
        var user= await _userManager.FindByIdAsync(userId);

        if(user is null) 
        return false;

        var principal= await _userClaimPrincipalFactory.CreateAsync(user);

        var Authorized= await _authorizationService.AuthorizeAsync(principal, policyName!);
       
        return Authorized.Succeeded;
         
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
    {
        var User= await _userManager.FindByIdAsync(userId)??throw new InvalidOperationException(nameof(userId));
        
        var UserRoles=await _userManager.GetRolesAsync(User);

        var UserClaims=await _userManager.GetClaimsAsync(User);
    
        return  new AppUserDto(User.Id,User.Email!,UserRoles,UserClaims);
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
       var User= await _userManager.FindByIdAsync(userId);

       return User?.UserName;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
       var User= await _userManager.FindByIdAsync(userId);
      
      return User!=null && await _userManager.IsInRoleAsync(User,role);
    }
}