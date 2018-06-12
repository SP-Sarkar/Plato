﻿using System.Threading.Tasks;

namespace Plato.Internal.Stores.Abstractions
{
    public interface IDictionaryStore<T> where T : class
    {

        Task<T> GetAsync();

        Task<T> SaveAsync(T model);

        Task<bool> DeleteAsync();

    }
}
