namespace Cart.API.Data;
using Cart.API.Models;
using System;
using System.Threading.Tasks;

public interface ICartRepository
{
    Task<ShoppingCart> GetByUserIdAsync(Guid userId);
    Task UpsertCartAsync(ShoppingCart cart);
}