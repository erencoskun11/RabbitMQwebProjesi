using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreate.Models;

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

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromQuery] int fileId)
        {
            if (file is not { Length: > 0 })
                return BadRequest("Dosya boş ya da bulunamadı.");

            var userFile = await _context.userFiles.FirstOrDefaultAsync(x => x.Id == fileId);
            if (userFile is null)
                return NotFound($"Id'si {fileId} olan kayıt bulunamadı.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, userFile.FileName + Path.GetExtension(file.FileName));

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Kaydı güncelle
            userFile.CreateDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;

            await _context.SaveChangesAsync();
            // SignalR burada olacak (gerekiyorsa)

            return Ok(new { message = "Dosya başarıyla yüklendi.", path = filePath });
        }
    }
}
