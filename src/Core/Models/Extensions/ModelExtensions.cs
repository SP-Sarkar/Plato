﻿using Newtonsoft.Json;
using PlatoCore.Abstractions;

namespace PlatoCore.Models.Extensions
{
    public static class ModelExtensions 
    {

        public static string Serialize<T>(this IDbModel<object> model)
        {
            return JsonConvert.SerializeObject(model);
        }

    }
}
