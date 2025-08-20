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
<<<<<<< HEAD

        [HttpPost] 
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromQuery] int fileId)
=======
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file,int fileId)
>>>>>>> origin/main
        {
            if (file is not { Length: > 0 })
                return BadRequest("Dosya boş ya da bulunamadı.");

            var userFile = await _context.userFiles.FirstOrDefaultAsync(x => x.Id == fileId);
            if (userFile is null)
                return NotFound($"Id'si {fileId} olan kayıt bulunamadı.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            Directory.CreateDirectory(uploadsFolder);

<<<<<<< HEAD
            var filePath = Path.Combine(uploadsFolder, userFile.FileName + Path.GetExtension(file.FileName));
=======
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

            using FileStream stream = new(path, FileMode.Create);
            await file.CopyToAsync(stream);

            userFile.CreateDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;

            await _context.SaveChangesAsync();
            //SignalIR burada oalcak 

            return Ok();
>>>>>>> origin/main

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Kaydı güncellemek istersen:
            // userFile.FilePath = filePath;
            // await _context.SaveChangesAsync();

            return Ok(new { message = "Dosya başarıyla yüklendi.", path = filePath });
        }
    }
}
