using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Pages.Basket;
using Microsoft.eShopWeb.Web.ViewModels;
using NSubstitute;
using Xunit;

namespace HybridTests;

[Trait("Stage","01")]
public class Stage01
{
    private readonly IBasketService _service=Substitute.For<IBasketService>();
    private readonly IBasketViewModelService _views=Substitute.For<IBasketViewModelService>();
    private readonly IRepository<CatalogItem> _catalog=Substitute.For<IRepository<CatalogItem>>();
    private IndexModel Page => Support.Page(_service,_views,_catalog);
    [Fact]
    public async Task S01_GetDelegatesToViewService()
    {
        var expected=new BasketViewModel {Id=5,BuyerId=Support.Buyer};
        _views.GetOrCreateBasketForUser(Support.Buyer).Returns(expected);
        var page=Page;await page.OnGet();Assert.Same(expected,page.BasketModel);
        await _views.Received(1).GetOrCreateBasketForUser(Support.Buyer);
    }
    [Fact]
    public async Task S02_MissingProductDoesNotCallBasketService()
    {
        var result=await Page.OnPost(new CatalogItemViewModel {Id=999});
        Assert.Equal("/Index",Assert.IsType<RedirectToPageResult>(result).PageName);
        Assert.Empty(_service.ReceivedCalls());
    }
    [Fact]
    public async Task S03_ServerPriceOverridesClientPrice()
    {
        var product=Support.Product();_catalog.GetByIdAsync(1,default).Returns(product);
        var basket=new Basket(Support.Buyer);_service.AddItemToBasket(Support.Buyer,1,8.50m,1).Returns(basket);
        _views.Map(basket).Returns(new BasketViewModel());
        await Page.OnPost(new CatalogItemViewModel {Id=1,Price=0.01m});
        await _service.Received(1).AddItemToBasket(Support.Buyer,1,8.50m,1);
    }
    [Fact]
    public async Task S04_InvalidModelStateStopsLowerCalls()
    {
        var page=Page;page.ModelState.AddModelError("Quantity","Invalid test input");
        await page.OnPostUpdate(new[] {new BasketItemViewModel {Id=1,Quantity=-1}});
        Assert.Empty(_service.ReceivedCalls());Assert.Empty(_views.ReceivedCalls());
    }
    [Fact]
    public async Task S05_DuplicateIdsShouldNotThrow()
    {
        _views.GetOrCreateBasketForUser(Support.Buyer).Returns(new BasketViewModel {Id=1});
        var page=Page;
        var exception=await Record.ExceptionAsync(()=>page.OnPostUpdate(new[] {
            new BasketItemViewModel {Id=1,Quantity=1},new BasketItemViewModel {Id=1,Quantity=2}}));
        Assert.Empty(_service.ReceivedCalls());
        // Deliberately tests desired behavior, not the existing defect.
        Assert.Null(exception);
    }
}
