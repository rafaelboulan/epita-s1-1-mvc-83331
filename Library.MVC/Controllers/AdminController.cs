using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.MVC.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [Route("Roles")]
    public async Task<IActionResult> Roles()
    {
        var roles = await Task.FromResult(_roleManager.Roles.ToList());
        return View(roles);
    }

    [Route("Roles")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
        {
            ModelState.AddModelError("", "Role name cannot be empty.");
        }

        if (ModelState.IsValid)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
            else
            {
                ModelState.AddModelError("", "Role already exists.");
            }
        }

        var roles = await Task.FromResult(_roleManager.Roles.ToList());
        return View(nameof(Roles), roles);
    }

    [Route("Roles/Delete/{roleId}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role != null)
        {
            await _roleManager.DeleteAsync(role);
        }

        return RedirectToAction(nameof(Roles));
    }
}
