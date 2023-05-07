using System;
using System.Threading.Tasks;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        /// <summary>
        /// Method to add an item to database
        /// </summary>
        /// <returns>Created Item</returns>
        public async Task<T> AddItemAsync<T>(object obj)
        {
            var item = (T)Convert.ChangeType(obj, typeof(T));

            await Dataservice.InsertAsync(item);

            return item;
        }

        /// <summary>
        /// Method to update an Item in Database
        /// </summary>
        /// <returns>Updated Item</returns>
        public async Task<T> UpdateItemAsync<T>(object obj)
        {
            var item = (T)Convert.ChangeType(obj, typeof(T));

            await Dataservice.UpdateAsync(item);

            return item;
        }

        /// <summary>
        /// Method to delete an Item in Database
        /// </summary>
        /// <returns>Deleted Item</returns>
        public async Task<T> DeleteItemAsync<T>(object obj)
        {
            var item = (T)Convert.ChangeType(obj, typeof(T));

            await Dataservice.DeleteAsync(item);

            return item;
        }

        /// <summary>
        /// Deletes All Items
        /// </summary>
        public async Task DeleteAllItemsAsync<T>()
        {
            await Dataservice.DeleteAllAsync<T>();
        }
    }
}
