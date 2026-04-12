using System.Net.ServerSentEvents;
using System.Runtime.Intrinsics.X86;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

//IItemRepository is the interface. But ItemRepository is the class that does the work
public interface IItemRepository // public (other parts of the program can use it) interface (defines what methods must exist)
{
    Task<List<Item>> GetAllAsync(); //Task = gives back a promise that the answer will come later. Will return a <List<Item>>
    Task<Item?> GetByIdAsync(int id);
    Task<Item> CreateAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(int id);
}