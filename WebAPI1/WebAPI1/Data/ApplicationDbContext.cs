using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Reflection.Metadata;
using WebAPI1.Models;

namespace WebAPI1.Data
{
	/*public class ApplicationDbContext : DbContext
	 {
		 public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		 {
		 }

		 public DbSet<User> Users { get; set; }
	 }*/
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
		public DbSet<User> Users { get; set; }

	}
}
