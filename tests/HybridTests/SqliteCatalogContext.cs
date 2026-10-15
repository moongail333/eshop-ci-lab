using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Data.Queries;
using Microsoft.eShopWeb.Web.Pages.Basket;
using Microsoft.eShopWeb.Web.ViewModels;
using Xunit;

namespace HybridTests;

// Provider adapter for this test stand only; production SQL Server mappings stay unchanged.
public class SqliteCatalogContext : CatalogContext
{
    public SqliteCatalogContext(DbContextOptions<CatalogContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        foreach (var sequence in builder.Model.GetSequences().ToList())
            builder.Model.RemoveSequence(sequence.Name, sequence.Schema);
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            foreach (var annotation in entity.GetAnnotations().Where(a => a.Name.StartsWith("SqlServer:")).ToList())
                entity.RemoveAnnotation(annotation.Name);
            foreach (var property in entity.GetProperties())
                foreach (var annotation in property.GetAnnotations().Where(a => a.Name.StartsWith("SqlServer:")).ToList())
                    property.RemoveAnnotation(annotation.Name);
        }
        foreach (var annotation in builder.Model.GetAnnotations().Where(a => a.Name.StartsWith("SqlServer:")).ToList())
            builder.Model.RemoveAnnotation(annotation.Name);
    }
}
