﻿using AdvertApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdvertApi.Services
{
    public interface IAdvertStorageService
    {
        Task<string> AddAsync(AdvertModel model);

        Task ConfirmAsync(ConfirmAdvertModel model);

        Task<AdvertModel> GetByIdAsync(string id);

        Task<bool> CheckHealthAsync();

        Task<List<AdvertModel>> GetAllAsync();
    }
}