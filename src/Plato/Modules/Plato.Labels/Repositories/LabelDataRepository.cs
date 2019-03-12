﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Data.Abstractions;
using Plato.Labels.Models;

namespace Plato.Labels.Repositories
{

    public class LabelDataRepository : ILabelDataRepository<LabelData>
    {

        #region Private Variables"

        private readonly IDbContext _dbContext;
        private readonly ILogger<LabelDataRepository> _logger;

        #endregion

        #region "Constructor"

        public LabelDataRepository(
            IDbContext dbContext,
            ILogger<LabelDataRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #endregion

        #region "Implementation"

        public async Task<LabelData> SelectByIdAsync(int id)
        {
            LabelData data = null;
            using (var context = _dbContext)
            {
                data = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectLabelDatumById",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            data = new LabelData();
                            await reader.ReadAsync();
                            data.PopulateModel(reader);
                        }
                        return data;
                    },
                    id);
            }

            return data;

        }

        public async Task<IEnumerable<LabelData>> SelectByLabelIdAsync(int labelId)
        {
            IList<LabelData> data = null;
            using (var context = _dbContext)
            {
                data = await context.ExecuteReaderAsync<IList<LabelData>>(
                    CommandType.StoredProcedure,
                    "SelectLabelDatumByLabelId",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            data = new List<LabelData>();
                            while (await reader.ReadAsync())
                            {
                                var entityData = new LabelData();
                                entityData.PopulateModel(reader);
                                data.Add(entityData);
                            }
                        }

                        return data;
                    },
                    labelId);
              
            }
            return data;

        }

        public async Task<LabelData> InsertUpdateAsync(LabelData data)
        {
            var id = await InsertUpdateInternal(
                data.Id,
                data.LabelId,
                data.Key.ToEmptyIfNull().TrimToSize(255),
                data.Value.ToEmptyIfNull(),
                data.CreatedDate.ToDateIfNull(),
                data.CreatedUserId,
                data.ModifiedDate,
                data.ModifiedUserId);
            if (id > 0)
            {
                return await SelectByIdAsync(id);
            }
                
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Deleting entity data id: {id}");
            }

            var success = 0;
            using (var context = _dbContext)
            {
                success = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "DeleteLabelDatumById", id);
            }

            return success > 0 ? true : false;

        }

        public async Task<IPagedResults<LabelData>> SelectAsync(params object[] inputParams)
        {
            IPagedResults<LabelData> output = null;
            using (var context = _dbContext)
            {
                output = await context.ExecuteReaderAsync<IPagedResults<LabelData>>(
                    CommandType.StoredProcedure,
                    "SelectLabelDatumPaged",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            output = new PagedResults<LabelData>();
                            while (await reader.ReadAsync())
                            {
                                var data = new LabelData();
                                data.PopulateModel(reader);
                                output.Data.Add(data);
                            }

                            if (await reader.NextResultAsync())
                            {
                                await reader.ReadAsync();
                                output.PopulateTotal(reader);
                            }

                        }

                        return output;
                    },
                    inputParams);

            }

            return output;
        }

        #endregion

        #region "Private Methods"

        private async Task<int> InsertUpdateInternal(
            int id,
            int LabelId,
            string key,
            string value,
            DateTimeOffset? createdDate,
            int createdUserId,
            DateTimeOffset? modifiedDate,
            int modifiedUserId)
        {

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(id == 0
                    ? $"Inserting Label data with key: {key}"
                    : $"Updating Label data with id: {id}");
            }

            using (var context = _dbContext)
            {
                if (context == null)
                    return 0;
                return await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "InsertUpdateLabelDatum",
                    id,
                    LabelId,
                    key.ToEmptyIfNull().TrimToSize(255),
                    value.ToEmptyIfNull(),
                    createdDate.ToDateIfNull(),
                    createdUserId,
                    modifiedDate,
                    modifiedUserId,
                    new DbDataParameter(DbType.Int32, ParameterDirection.Output));
            }

        }

        #endregion

    }



}
