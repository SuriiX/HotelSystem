// File: ~/IdentityModels/ApplicationIdentityDbContext.cs
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity; // Para DbModelBuilder

namespace HotelAuroraDreams.Api_Framework.IdentityModels
{
    public class ApplicationIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationIdentityDbContext()
            : base("HotelDBModelEntitiesIdentity", throwIfV1Schema: false)
        {
        }

        public static ApplicationIdentityDbContext Create()
        {
            return new ApplicationIdentityDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .Property(p => p.Salario)
                .HasPrecision(10, 2); // Especificando precisión para Salario aquí
        }
    }
}