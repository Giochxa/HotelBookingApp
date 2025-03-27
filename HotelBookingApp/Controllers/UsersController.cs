using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HotelBookingApp.Models;
using HotelBookingApp.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBookingApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                SelectedRole = userRoles.FirstOrDefault()
            };

            //ViewBag.AllRoles = new SelectList(allRoles, userRoles);
            ViewBag.AllRoles = new SelectList(allRoles, viewModel.SelectedRole);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.Email = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to update user.");
                ViewBag.AllRoles = new SelectList(await _roleManager.Roles.Select(r => r.Name).ToListAsync(), model.SelectedRole);
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!string.IsNullOrEmpty(model.SelectedRole))
                await _userManager.AddToRoleAsync(user, model.SelectedRole);

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, "NewSecurePassword123!");

            TempData[resetResult.Succeeded ? "Success" : "Error"] = resetResult.Succeeded
                ? "Password reset to 'NewSecurePassword123!'"
                : "Failed to reset password.";

            return RedirectToAction("Index");
        }
    }
}
