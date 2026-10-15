using System.Security.Claims;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Pages.Basket;
using Microsoft.eShopWeb.Web.Services;
using NSubstitute;

namespace HybridTests;

public static class Support
{
    public const string Buyer = "topdown@example.com";
    public static IndexModel Page(IBasketService service, IBasketViewModelService views, IRepository<CatalogItem> catalog)
    {
        return new IndexModel(service, views, catalog)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim(ClaimTypes.Name,Buyer)},"test"))
                }
            }
        };
    }
    public static void Id(BaseEntity entity,int value) => typeof(BaseEntity).GetProperty("Id")!.SetValue(entity,value);
    public static CatalogItem Product()
    {
        var item=new CatalogItem(1,1,"Test mug","Test mug",8.50m,"mug.png");Id(item,1);return item;
    }
    public static IndexModel RealServices(IRepository<Basket> baskets,IRepository<CatalogItem> products,IBasketQueryService query)
    {
        var service=new BasketService(baskets,new QuietLogger<BasketService>());
        var views=new BasketViewModelService(baskets,products,new UriComposer(new CatalogSettings {CatalogBaseUrl=""}),query);
        return Page(service,views,products);
    }
    public static BasketItemViewModel[] Quantity(IndexModel page,int value) => new[] {new BasketItemViewModel {Id=page.BasketModel.Items.Single().Id,Quantity=value}};
}
