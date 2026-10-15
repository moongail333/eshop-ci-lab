using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Xunit;
namespace HybridTests;

// Test methods are drivers of repositories; no business service or PageModel is constructed.
[Trait("Stage","02")]
public class Stage02
{
    [Fact] public async Task S01_AddAndReloadBasketWithItems()
    {
        using var s=new Stand();var basket=new Basket(Stand.Buyer);basket.AddItem(1,8.50m,2);
        await s.Baskets.AddAsync(basket);var id=basket.Id;s.Reopen();
        var loaded=await s.Baskets.FirstOrDefaultAsync(new BasketWithItemsSpecification(id));
        Assert.NotNull(loaded);Assert.Equal(Stand.Buyer,loaded!.BuyerId);Assert.Equal(2,loaded.Items.Single().Quantity);Assert.Equal(8.50m,loaded.Items.Single().UnitPrice);
    }
    [Fact] public async Task S02_UpdatePersistsAcrossContexts()
    {
        using var s=new Stand();var basket=new Basket(Stand.Buyer);basket.AddItem(1,8.50m);await s.Baskets.AddAsync(basket);
        basket.Items.Single().SetQuantity(3);await s.Baskets.UpdateAsync(basket);var id=basket.Id;s.Reopen();
        var loaded=await s.Baskets.FirstOrDefaultAsync(new BasketWithItemsSpecification(id));Assert.Equal(3,loaded!.Items.Single().Quantity);
    }
    [Fact] public async Task S03_DeleteBasketCascadesToItems()
    {
        using var s=new Stand();var basket=new Basket(Stand.Buyer);basket.AddItem(1,8.50m);await s.Baskets.AddAsync(basket);
        await s.Baskets.DeleteAsync(basket);s.Reopen();Assert.Equal(0,await s.Db.Baskets.CountAsync());Assert.Equal(0,await s.Db.BasketItems.CountAsync());
    }
    [Fact] public async Task S04_SpecificationSeparatesBuyers()
    {
        using var s=new Stand();await s.Baskets.AddAsync(new Basket("alice"));await s.Baskets.AddAsync(new Basket("bob"));s.Reopen();
        var basket=await s.Baskets.FirstOrDefaultAsync(new BasketWithItemsSpecification("alice"));Assert.Equal("alice",basket!.BuyerId);Assert.Equal(2,await s.Db.Baskets.CountAsync());
    }
    [Fact] public async Task S05_UnknownProductReturnsNull()
    {
        using var s=new Stand();Assert.Null(await s.Products.GetByIdAsync(999999));Assert.Equal(8.50m,(await s.Products.GetByIdAsync(1))!.Price);
    }
}
