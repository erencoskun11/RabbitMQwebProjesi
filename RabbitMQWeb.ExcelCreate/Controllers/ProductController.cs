using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using RabbitMQWeb.ExcelCreate.Models;
using RabbitMQWeb.ExcelCreate.Services;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;



        private readonly RabbitMQPublisher _rabbitMQPublisher;
        public ProductController(AppDbContext context, UserManager<IdentityUser> userManager,
            RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _userManager = userManager;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }



        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1,10)}";

            UserFile userfile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating

            };
            await _context.userFiles.AddAsync(userfile);

            await _context.SaveChangesAsync();

            _rabbitMQPublisher.Publish(new CreateExcelMessage() 
            {FileId = userfile.Id, UserId=user.Id});

            TempData["StartCreatingExcel"] = true;
            return RedirectToAction(nameof(Files));
        }
        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            return View(await _context.userFiles.Where(x=>x.UserId==user.Id).ToListAsync());
        }
    }
}
