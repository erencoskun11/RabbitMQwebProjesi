using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreate.Models;
using System.ComponentModel.DataAnnotations;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FilesController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Upload(IFormFile file,int fileId)
        {
            if (file is not { Length: > 0 }) return BadRequest();

            var userFile = await _context.userFiles.FirstAsync(x=>x.Id==fileId);

             var filePath = userFile.FileName + Path.GetExtension(file.FileName);




        }

    }
}
