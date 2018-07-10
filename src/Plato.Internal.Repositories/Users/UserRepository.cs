﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Models.Users;

namespace Plato.Internal.Repositories.Users
{
    public class UserRepository : IUserRepository<User>
    {


        #region "Private Variables"

        private readonly IDbContext _dbContext;
        private readonly IUserSecretRepository<UserSecret> _userSecretRepository;
        //private readonly IUserDetailRepository<UserDetail> _userDetailRepository;
        //private readonly IUserPhotoRepository<UserPhoto> _userPhotoRepository;

        private readonly ILogger<UserSecretRepository> _logger;

        #endregion

        #region "Constructor"

        public UserRepository(
            IDbContext dbContext,
            IUserSecretRepository<UserSecret> userSecretRepository,
            ILogger<UserSecretRepository> logger)
        {
            _dbContext = dbContext;
            _userSecretRepository = userSecretRepository;
            //_userDetailRepository = userDetailRepository;
            _logger = logger;
        }

        #endregion


        #region "Implementation"

        public Task<bool> DeleteAsync(int id)
        {
            // TODO
            throw new NotImplementedException();
        }

        public async Task<User> InsertUpdateAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // ensure we have our dependencies
            if (_userSecretRepository == null)
                throw new ArgumentNullException(nameof(_userSecretRepository));

            //if (_userDetailRepository == null)
            //    throw new ArgumentNullException(nameof(_userDetailRepository));
            
            var id = await InsertUpdateInternal(
                user.Id,
                user.PrimaryRoleId,
                user.UserName,
                user.NormalizedUserName,
                user.Email,
                user.NormalizedEmail,
                user.EmailConfirmed,
                user.DisplayName,
                user.SamAccountName,
                user.PasswordHash,
                user.SecurityStamp,
                user.PhoneNumber,
                user.PhoneNumberConfirmed,
                user.TwoFactorEnabled,
                user.LockoutEnd,
                user.LockoutEnabled,
                user.AccessFailedCount,
                user.ApiKey);

            if (id > 0)
            {
                //// secerts

                //if (user.Secret == null)
                //    user.Secret = new UserSecret();
                //if ((user.Id == 0) || (user.Secret.UserId == 0))
                //    user.Secret.UserId = id;
                //await _userSecretRepository.InsertUpdateAsync(user.Secret);

                //// detail

                //if (user.Detail == null)
                //    user.Detail = new UserDetail();
                //if ((user.Id == 0) || (user.Detail.UserId == 0))
                //    user.Detail.UserId = id;
                //await _userDetailRepository.InsertUpdateAsync(user.Detail);
                
                // return

                return await SelectByIdAsync(id);
            }

            return null;
        }

        public async Task<User> SelectByIdAsync(int id)
        {
            using (var context = _dbContext)
            {
                _dbContext.OnException += (sender, args) =>
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogInformation(
                            $"SelectUser for Id {id} failed with the following error {args.Exception.Message}");
                };

                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserById", id);

                return await BuildUserFromResultSets(reader);
            }
        }

        public async Task<User> SelectByUserNameNormalizedAsync(string userNameNormalized)
        {

            if (string.IsNullOrEmpty(userNameNormalized))
                throw new ArgumentNullException(nameof(userNameNormalized));

            using (var context = _dbContext)
            {
                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserByUserNameNormalized",
                    userNameNormalized.TrimToSize(255));

                return await BuildUserFromResultSets(reader);
            }
        }

        public async Task<User> SelectByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            using (var context = _dbContext)
            {
                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserByEmail", 
                    email.TrimToSize(255));

                return await BuildUserFromResultSets(reader);
            }
        }

        public async Task<User> SelectByUserNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            using (var context = _dbContext)
            {
                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserByUserName", 
                    userName.TrimToSize(255));
                return await BuildUserFromResultSets(reader);
            }
        }

        public async Task<User> SelectByUserNameAndPasswordAsync(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            using (var context = _dbContext)
            {

                _dbContext.OnException += (sender, args) =>
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogInformation($"SelectUserByUserNameAndPassword failed with the following error {args.Exception.Message}");
                };
                
                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserByUserNameAndPassword",
                    userName.TrimToSize(255),
                    password.TrimToSize(255));

                return await BuildUserFromResultSets(reader);
            }
        }

        public async Task<User> SelectByEmailAndPasswordAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            using (var context = _dbContext)
            {

                _dbContext.OnException += (sender, args) =>
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogInformation($"SelectUserByEmailAndPassword failed with the following error {args.Exception.Message}");
                };

                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserByEmailAndPassword",
                    email.TrimToSize(255),
                    password.TrimToSize(255));

                return await BuildUserFromResultSets(reader);
            }
        }

        public async Task<User> SelectByApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            using (var context = _dbContext)
            {

                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserByApiKey",
                    apiKey.TrimToSize(255));

                return await BuildUserFromResultSets(reader);
            }
        }

        public async Task<IPagedResults<T>> SelectAsync<T>(params object[] inputParameters) where T : class
        {
            PagedResults<T> output = null;
            using (var context = _dbContext)
            {

                _dbContext.OnException += (sender, args) =>
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogInformation($"SelectUsersPaged failed with the following error {args.Exception.Message}");
                };

                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUsersPaged",
                    inputParameters
                );

                if ((reader != null) && (reader.HasRows))
                {
                    output = new PagedResults<T>();
                    while (await reader.ReadAsync())
                    {
                        var user = new User();
                        user.PopulateModel(reader);
                        output.Data.Add((T)Convert.ChangeType(user, typeof(T)));
                    }

                    if (await reader.NextResultAsync())
                    {
                        await reader.ReadAsync();
                        output.PopulateTotal(reader);
                    }
                }
            }

            return output;
        }


        public async Task<IPagedResults<User>> SelectAsync(params object[] inputParams)
        {
            PagedResults<User> output = null;
            using (var context = _dbContext)
            {

                _dbContext.OnException += (sender, args) =>
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogInformation($"SelectUsersPaged failed with the following error {args.Exception.Message}");
                };

                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUsersPaged",
                    inputParams
                );

                if ((reader != null) && (reader.HasRows))
                {
                    output = new PagedResults<User>();
                    while (await reader.ReadAsync())
                    {
                        var user = new User();
                        user.PopulateModel(reader);
                        output.Data.Add(user);
                    }

                    if (await reader.NextResultAsync())
                    {
                        await reader.ReadAsync();
                        output.PopulateTotal(reader);
                    }
                }
            }

            return output;
        }

        #endregion

        #region "Private Methods"

        private async Task<User> BuildUserFromResultSets(DbDataReader reader)
        {
            User user = null;
            if ((reader != null) && (reader.HasRows))
            {
                // user

                user = new User();
                await reader.ReadAsync();
                if (reader.HasRows)
                {
                    user.PopulateModel(reader);
                }

                // data

                if (await reader.NextResultAsync())
                {
                    if (reader.HasRows)
                    {
                        var data = new List<UserData>();
                        while (await reader.ReadAsync())
                        {
                            var entityData = new UserData(reader);
                            data.Add(entityData);
                        }

                        user.Data = data;
                    }
                }

            }

            return user;

        }

        private async Task<int> InsertUpdateInternal(
            int id,
            int primaryRoleId,
            string userName,
            string normalizedUserName,
            string email,
            string normalizedEmail,
            bool emailConfirmed,
            string displayName,
            string samAccountName,
            string passwordHash,
            string securityStamp,
            string phoneNumber,
            bool phoneNumberConfirmed,
            bool twoFactorEnabled,
            DateTimeOffset? lockoutEnd,
            bool lockoutEnabled,
            int accessFailedCount,
            string apiKey)
        {
     
            var userId = 0;
            using (var context = _dbContext)
            {

                userId = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "InsertUpdateUser",
                    id,
                    primaryRoleId,
                    userName.ToEmptyIfNull().TrimToSize(255),
                    normalizedUserName.ToEmptyIfNull().TrimToSize(255),
                    email.ToEmptyIfNull().TrimToSize(255),
                    normalizedEmail.ToEmptyIfNull().TrimToSize(255),
                    emailConfirmed,
                    displayName.ToEmptyIfNull().TrimToSize(255),
                    samAccountName.ToEmptyIfNull().TrimToSize(255),
                    passwordHash.ToEmptyIfNull().TrimToSize(255),
                    securityStamp.ToEmptyIfNull().TrimToSize(255),
                    phoneNumber.ToEmptyIfNull().TrimToSize(255),
                    phoneNumberConfirmed,
                    twoFactorEnabled,
                    lockoutEnd,
                    lockoutEnabled,
                    accessFailedCount,
                    apiKey.ToEmptyIfNull().TrimToSize(255)
                );
            }

            return userId;

        }

        #endregion
    }
}