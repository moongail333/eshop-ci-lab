using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Web.Pages.Basket;
using Microsoft.eShopWeb.Web.ViewModels;
using Xunit;
namespace HybridTests;

// Meet-in-the-middle: connect the independently tested upper and lower branches
// through real business services. No service/repository mocks remain here.
[Trait("Stage","03")]
public class Stage03
{
    [Fact] public async Task S06_DeletedCatalogItemShouldNotBreakJoinedChain()
    {
        using var s=new Stand();await s.Page().OnPost(new CatalogItemViewModel{Id=1});
        await s.Products.DeleteAsync((await s.Products.GetByIdAsync(1))!);s.Reopen();
        var exception=await Record.ExceptionAsync(()=>s.Page().OnGet());Assert.Null(exception);
    }
    [Fact] public async Task S01_PageUsesServerPriceAndPersistsBasket()
    {
        using var s=new Stand();await s.Page().OnPost(new CatalogItemViewModel{Id=1,Price=0.01m});s.Reopen();var page=s.Page();await page.OnGet();
        var item = Assert.Single(page.BasketModel.Items);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(8.50m, item.UnitPrice);
        Assert.Equal(8.50m,page.BasketModel.Total());
    }
    [Fact] public async Task S02_UnknownProductRedirectsWithoutCreatingBasket()
    {
        using var s=new Stand();var response=await s.Page().OnPost(new CatalogItemViewModel{Id=999});
        Assert.Equal("/Index",Assert.IsType<RedirectToPageResult>(response).PageName);Assert.Equal(0,await s.Db.Baskets.CountAsync());
    }
    [Fact] public async Task S03_PageUpdatePersistsTotal()
    {
        using var s=new Stand();var page=s.Page();await page.OnPost(new CatalogItemViewModel{Id=1});
        await page.OnPostUpdate(new[]{new BasketItemViewModel{Id=page.BasketModel.Items.Single().Id,Quantity=3}});
        s.Reopen();page=s.Page();await page.OnGet();Assert.Equal(25.50m,page.BasketModel.Total());
    }
    [Fact] public async Task S04_PageZeroQuantityRemovesItem()
    {
        using var s=new Stand();var page=s.Page();await page.OnPost(new CatalogItemViewModel{Id=1});
        await page.OnPostUpdate(new[]{new BasketItemViewModel{Id=page.BasketModel.Items.Single().Id,Quantity=0}});
        s.Reopen();page=s.Page();await page.OnGet();Assert.Empty(page.BasketModel.Items);Assert.Equal(0,await s.Db.BasketItems.CountAsync());
    }
    [Fact] public async Task S05_DuplicateIdsShouldBeHandledWithoutException()
    {
        using var s=new Stand();var page=s.Page();await page.OnPost(new CatalogItemViewModel{Id=1});var id=page.BasketModel.Items.Single().Id;
        var exception=await Record.ExceptionAsync(()=>page.OnPostUpdate(new[]{new BasketItemViewModel{Id=id,Quantity=1},new BasketItemViewModel{Id=id,Quantity=2}}));
        Assert.Null(exception);
    }
}
