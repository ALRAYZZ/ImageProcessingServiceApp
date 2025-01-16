using ImageProcessingService.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessingService.DataAccess
{
	public class UsersDbContext : DbContext
	{
		public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
		{
		}
		public DbSet<UserModel> Users { get; set; }
	}
}
