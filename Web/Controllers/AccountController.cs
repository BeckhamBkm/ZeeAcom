using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Common;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.AccountModels;
using ZeeAcom.Core.Accounts.Commands;
using ZeeAcom.Web.Models;

namespace ZeeAcom.Web.Controllers;

/// <summary>
/// Landlord registration and login/logout for landlords and the admin.
/// </summary>
[AllowAnonymous]
public class AccountController(
    IMediator mediator,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public IActionResult Register()
        => User.Identity?.IsAuthenticated == true ? RedirectToHome() : View(new RegisterLandlordModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterLandlordModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await mediator.Send(new RegisterLandlordCommand { Data = model }, cancellationToken);
        if (result.IsFailure)
        {
            foreach (var error in result.Errors ?? [])
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        await signInManager.PasswordSignInAsync(model.Email.Trim(), model.Password, isPersistent: false, lockoutOnFailure: false);
        TempData["Success"] = $"Welcome, {model.FirstName}! Your landlord account is ready.";
        return RedirectToAction("Index", "Landlord");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        => User.Identity?.IsAuthenticated == true ? RedirectToHome() : View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var result = await signInManager.PasswordSignInAsync(email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Too many failed attempts. This account is locked for a few minutes; try again later.");
            return View(model);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Incorrect email or password.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        var user = await userManager.FindByEmailAsync(email);
        return user is not null && await userManager.IsInRoleAsync(user, Roles.Admin)
            ? RedirectToAction("Index", "Owners")
            : RedirectToAction("Index", "Landlord");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToHome()
        => User.IsInRole(Roles.Admin) ? RedirectToAction("Index", "Owners") : RedirectToAction("Index", "Landlord");
}
