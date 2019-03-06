﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Plato.Categories.Models;
using Plato.Categories.Services;
using Plato.Categories.Stores;
using Plato.Articles.Categories.Models;
using Plato.Articles.Categories.ViewModels;
using Plato.Articles.Models;
using Plato.Articles.ViewModels;
using Plato.Entities.Models;
using Plato.Entities.ViewModels;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Layout.ViewProviders;

namespace Plato.Articles.Categories.ViewProviders
{
    public class ChannelViewProvider : BaseViewProvider<Channel>
    {

        private readonly IContextFacade _contextFacade;
        private readonly ICategoryStore<Channel> _categoryStore;
        private readonly ICategoryManager<Channel> _categoryManager;
        private readonly IFeatureFacade _featureFacade;
        private readonly IActionContextAccessor _actionContextAccessor;

        public ChannelViewProvider(
            IContextFacade contextFacade,
            ICategoryStore<Channel> categoryStore,
            ICategoryManager<Channel> categoryManager,
            IFeatureFacade featureFacade,
            IActionContextAccessor actionContextAccessor)
        {
            _contextFacade = contextFacade;
            _categoryStore = categoryStore;
            _categoryManager = categoryManager;
            _featureFacade = featureFacade;
            _actionContextAccessor = actionContextAccessor;
        }

        public override async Task<IViewProviderResult> BuildIndexAsync(Channel channel, IViewProviderContext context)
        {

            // Ensure we explicitly set the featureId
            var feature = await _featureFacade.GetFeatureByIdAsync("Plato.Articles.Categories");
            if (feature == null)
            {
                return default(IViewProviderResult);
            }

            var categories = await _categoryStore.GetByFeatureIdAsync(feature.Id);

            CategoryBase categoryBase = null;
            if (channel?.Id > 0)
            {
                categoryBase = await _categoryStore.GetByIdAsync(channel.Id);
            }

            // channel filter options
            var channelViewOpts = new ChannelIndexOptions
            {
                ChannelId = categoryBase?.Id ?? 0
            };
            
            // Get index view model from context
            var viewModel = context.Controller.HttpContext.Items[typeof(EntityIndexViewModel<Article>)] as EntityIndexViewModel<Article>;
            if (viewModel == null)
            {
                throw new Exception($"A view model of type {typeof(EntityIndexViewModel<Article>).ToString()} has not been registered on the HttpContext!");
            }
            
            var indexViewModel = new ChannelIndexViewModel
            {
                ChannelIndexOpts = channelViewOpts,
                TopicIndexOpts = viewModel?.Options,
                PagerOpts = viewModel?.Pager
            };

            return Views(
                View<CategoryBase>("Home.Index.Header", model => categoryBase).Zone("header").Order(1),
                View<CategoryBase>("Home.Index.Tools", model => categoryBase).Zone("tools").Order(1),
                View<ChannelIndexViewModel>("Home.Index.Content", model => indexViewModel).Zone("content").Order(1),
                View<ChannelListViewModel>("Topic.Channels.Index.Sidebar", model =>
                {
                    model.SelectedChannelId = channel?.Id ?? 0;
                    model.Channels = categories;
                    return model;
                }).Zone("sidebar").Order(1)
            );

        }

        public override Task<IViewProviderResult> BuildDisplayAsync(Channel indexViewModel, IViewProviderContext updater)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override Task<IViewProviderResult> BuildEditAsync(Channel category, IViewProviderContext updater)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override Task<IViewProviderResult> BuildUpdateAsync(Channel category, IViewProviderContext updater)
        {
            return Task.FromResult(default(IViewProviderResult));
        }
        
    }
}