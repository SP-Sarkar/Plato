﻿using System.Threading.Tasks;
using Plato.Internal.Layout.ModelBinding;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Models.Users;
using Plato.Internal.Net.Abstractions;
using Plato.StopForumSpam.Services;

namespace Plato.Users.StopForumSpam.ViewProviders
{
    public class RegisterViewProvider : BaseViewProvider<UserRegistration>
    {
        
        private readonly ISpamOperatorManager<User> _spamOperatorManager;
        private readonly IClientIpAddress _clientIpAddress;

        public RegisterViewProvider(
            ISpamOperatorManager<User> spamOperatorManager,
            IClientIpAddress clientIpAddress)
        {
            _spamOperatorManager = spamOperatorManager;
            _clientIpAddress = clientIpAddress;
        }
        
        public override  Task<IViewProviderResult> BuildIndexAsync(UserRegistration viewModel, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override Task<IViewProviderResult> BuildDisplayAsync(UserRegistration viewModel, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));
        }
        
        public override Task<IViewProviderResult> BuildEditAsync(UserRegistration viewModel, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override async Task<bool> ValidateModelAsync(UserRegistration registration, IUpdateModel updater)
        {

            var user = new User()
            {
                UserName = registration.UserName,
                Email = registration.Email,
                IpV4Address = _clientIpAddress.GetIpV4Address()
                
            };

            // Execute any registered spam operations
            var results = await _spamOperatorManager.OperateAsync(SpamOperations.Registration, user);

            // IF any operations failed ensure we display the operation error message
            var valid = true;
            if (results != null)
            {
                foreach (var result in results)
                {
                    if (!result.Succeeded)
                    {
                        // If any of the failed operations don't allow
                        // alterations ensure we invalidate the model
                        if (result.Operation.CustomMessage)
                        {
                            updater.ModelState.AddModelError(string.Empty, result.Operation.Message);
                            valid = false;
                        }
                    }
                }
            }
            
            return valid;

        }
        
        public override async Task<IViewProviderResult> BuildUpdateAsync(UserRegistration viewModel, IViewProviderContext context)
        {
            return await BuildIndexAsync(viewModel, context);
        }

    }

}
