namespace Cart.API.Data;
using Cart.API.Models;
using System;
using System.Threading.Tasks;

public interface ICartRepository
{
    Task<Cart_> GetByUserIdAsync(Guid userId);
    Task UpsertCartAsync(Cart_ cart);
}