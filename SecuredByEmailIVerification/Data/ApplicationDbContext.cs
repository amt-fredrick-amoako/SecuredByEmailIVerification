using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecuredByEmailIVerification.Model;

namespace SecuredByEmailIVerification.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public ApplicationDbContext()
        {

        }

        public DbSet<User> People { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().HasData(new User
            {
                Id = Guid.Parse("045b13c8-5a68-4399-b66e-9369f5bab767"),
                Country = "USA",
                Name = "Freddie",
                Occupation = "Senior Developer",
                Email = "fredrickamoako1@gmail.com"
            });

            builder.Entity<User>().HasData(new User
            {
                Id = Guid.Parse("0f4048e1-b7b7-4258-8869-a4853df036a1"),
                Country = "USA",
                Name = "Maxie",
                Occupation = "Branch Manager PDT",
                Email = "maxwellantwi196@gmail.com"
            });
        }


    }
}
