﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Plato.Internal.Layout.ViewProviders;
using Plato.Users.Google.reCAPTCHA3.Services;
using Plato.Users.ViewModels;
using Plato.Users.Google.reCAPTCHA3.ViewModels;

namespace Plato.Users.Google.reCAPTCHA3.ViewProviders
{
    public class LoginViewProvider : BaseViewProvider<LoginViewModel>
    {

        private readonly IReCaptchaService _recaptchaService;
        private readonly HttpRequest _request;

        public LoginViewProvider(
            IHttpContextAccessor httpContextAccessor,
            IReCaptchaService recaptchaService)
        {
            _recaptchaService = recaptchaService;
            _request = httpContextAccessor.HttpContext.Request;
        }

        public override Task<IViewProviderResult> BuildIndexAsync(LoginViewModel viewModel, IViewProviderContext context)
        {

            var recaptchaViewMddel = new ReCaptchaViewModel();

            return Task.FromResult(Views(          
                View<ReCaptchaViewModel>("Register.Google.reCAPTCHA3", model => recaptchaViewMddel).Zone("content").Order(int.MaxValue)
            ));

        }

        public override Task<IViewProviderResult> BuildDisplayAsync(LoginViewModel viewModel, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));

        }
        
        public override Task<IViewProviderResult> BuildEditAsync(LoginViewModel viewModel, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));

        }

        public override async Task<IViewProviderResult> BuildUpdateAsync(LoginViewModel viewModel, IViewProviderContext context)
        {
            
            var recaptchaResponse = _request.Form["g-recaptcha-response"];

            if (String.IsNullOrEmpty(recaptchaResponse) )
            {
                context.Updater.ModelState.AddModelError(string.Empty, "Sorry but we could not obtain a captcha response from Google");
            }

            _recaptchaService.Validate(recaptchaResponse);
            _recaptchaService.OnComplete += (sender, response) =>
            {

            };

            return await BuildIndexAsync(viewModel, context);


        }
    }
}