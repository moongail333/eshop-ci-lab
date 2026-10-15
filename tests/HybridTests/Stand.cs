using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Data.Queries;
using Microsoft.eShopWeb.Web.Pages.Basket;
using Microsoft.eShopWeb.Web.Services;

namespace HybridTests;

// Constructor connects only the database and repositories. Higher levels are opt-in.
public sealed class Stand : IDisposable
{
    public const string Buyer="hybrid@example.com";
    private readonly string _file=Path.Combine(Path.GetTempPath(),"eshop-hybrid-"+Guid.NewGuid()+".db");
    public CatalogContext Db {get;private set;}
    public EfRepository<Basket> Baskets => new(Db);
    public EfRepository<CatalogItem> Products => new(Db);
    public Stand()
    {
        Db=Context();
        try {
            Db.Database.EnsureCreated();
            var brand=new CatalogBrand("Test brand");Id(brand,1);
            var type=new CatalogType("Mug");Id(type,1);
            var product=new CatalogItem(1,1,"Test mug","Test mug",8.50m,"mug.png");Id(product,1);
            Db.AddRange(brand,type,product);Db.SaveChanges();
        } catch {Dispose();throw;}
    }
    private static void Id(BaseEntity item,int id)=>typeof(BaseEntity).GetProperty("Id")!.SetValue(item,id);
    private CatalogContext Context()=>new SqliteCatalogContext(new DbContextOptionsBuilder<CatalogContext>().UseSqlite($"Data Source={_file};Pooling=False").Options);
    public void Reopen(){Db.Dispose();Db=Context();}
    public BasketService Service()=>new(Baskets,new QuietLogger<BasketService>());
    public BasketViewModelService Views()=>new(Baskets,Products,new UriComposer(new CatalogSettings{CatalogBaseUrl=""}),new BasketQueryService(Db));
    public IndexModel Page()=>new(Service(),Views(),Products){PageContext=new PageContext{HttpContext=new DefaultHttpContext{
        User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,Buyer)},"test"))}}};
    public void Dispose(){Db.Dispose();if(File.Exists(_file))File.Delete(_file);}
}
public sealed class QuietLogger<T>:IAppLogger<T>
{
    public void LogInformation(string message,params object[] args){}
    public void LogWarning(string message,params object[] args){}
}
