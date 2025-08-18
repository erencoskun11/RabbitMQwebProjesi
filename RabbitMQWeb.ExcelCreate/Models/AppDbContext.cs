using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RabbitMQWeb.ExcelCreate.Models
{
    public class AppDbContext : IdentityDbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext>options): base(options)
        {
            
        }
        public DbSet<UserFile> userFiles { get; set; }
    }
}
