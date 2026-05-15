//// Change this line in your CategoriesController.cs
//using Itihas360.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//[HttpGet]
//[AllowAnonymous] // Add this so the public can see categories
//public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
//{
//    return await _context.Categories
//        .Where(c => (c.IsActive ?? false) == true)
//        .OrderBy(c => c.DisplayOrder)
//        .ToListAsync();
//}