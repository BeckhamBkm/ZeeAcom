using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZeeAcom.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t =>
            t.IsClass
            && !t.IsAbstract
            && t.GetProperty("Id") != null
            && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute))
            && !t.Name.Contains("Model")
            );

            foreach (var type in entityTypes)
            {
                modelBuilder.Entity(type);
            }

            base.OnModelCreating(modelBuilder);

        }
    }
}
