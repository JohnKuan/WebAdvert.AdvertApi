using AdvertApi.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertApi.Services
{
    public class DynamoDBAdvertStorage : IAdvertStorageService
    {
        private readonly IMapper _mapper;
        private readonly IAmazonDynamoDB _client;

        public DynamoDBAdvertStorage(IMapper mapper, IAmazonDynamoDB client)
        {
            _mapper = mapper;
            _client = client;
        }

        public async Task<string> AddAsync(AdvertModel model)
        {
            // add validation in production
            AdvertDbModel dbModel = _mapper.Map<AdvertDbModel>(model);
            dbModel.Id = new Guid().ToString();
            dbModel.CreationDateTime = DateTime.UtcNow;
            dbModel.Status = AdvertStatus.Pending;

            using (var context = new DynamoDBContext(_client))
            {
                await context.SaveAsync(dbModel);
            }

            return dbModel.Id;
        }

        public async Task<bool> CheckHealthAsync()
        {
            Console.WriteLine("Health checking...");
            //using var client = new AmazonDynamoDBClient();
            var tableData = await _client.DescribeTableAsync("Adverts");
            return string.Compare(tableData.Table.TableStatus, "active", true) == 0;
        }

        public async Task ConfirmAsync(ConfirmAdvertModel model)
        {
            DynamoDBContext context = new DynamoDBContext(_client);
            var record = await context.LoadAsync<AdvertDbModel>(model.Id);
            if (record == null)
            {
                throw new KeyNotFoundException($"A record with ID={model.Id} was not found.");
            }
            if (model.Status == AdvertStatus.Active)
            {
                record.Status = AdvertStatus.Active;
                await context.SaveAsync(record);
            }
            else
            {
                await context.DeleteAsync(record);
            }
        }

        public async Task<List<AdvertModel>> GetAllAsync()
        {
            DynamoDBContext context = new DynamoDBContext(_client);
            var scanResult =
await context.ScanAsync<AdvertDbModel>(new List<ScanCondition>()).GetNextSetAsync();
            return scanResult.Select(item => _mapper.Map<AdvertModel>(item)).ToList();
        }

        public async Task<AdvertModel> GetByIdAsync(string id)
        {
            using (var context = new DynamoDBContext(_client))
            {
                var dbModel = await context.LoadAsync<AdvertDbModel>(id);
                if (dbModel != null) return _mapper.Map<AdvertModel>(dbModel);
            }

            throw new KeyNotFoundException();
        }
    }
}