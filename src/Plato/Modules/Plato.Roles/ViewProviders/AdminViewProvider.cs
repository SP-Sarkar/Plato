﻿using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Layout.ModelBinding;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Models.Roles;
using Plato.Internal.Models.Users;
using Plato.Internal.Navigation;
using Plato.Internal.Security.Abstractions;
using Plato.Internal.Stores.Abstractions.Roles;
using Plato.Internal.Stores.Roles;
using Plato.Roles.ViewModels;

namespace Plato.Roles.ViewProviders
{
    public class AdminViewProvider : BaseViewProvider<Role>
    {

        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IPlatoRoleStore _platoRoleStore;
        private readonly IPermissionsManager2<Permission> _permissionsManager;
        private readonly IAuthorizationService _authorizationService;

        public AdminViewProvider(
            UserManager<User> userManager,
            IPlatoRoleStore platoRoleStore,
            RoleManager<Role> roleManager,
            IPermissionsManager2<Permission> permissionsManager, 
            IAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _platoRoleStore = platoRoleStore;
            _roleManager = roleManager;
            _permissionsManager = permissionsManager;
            _authorizationService = authorizationService;
        }

        #region "Implementation"

        public override Task<IViewProviderResult> BuildDisplayAsync(Role role, IUpdateModel updater)
        {

            return Task.FromResult(
                Views(
                    View<Role>("Admin.Display.Header", model => role).Zone("header"),
                    View<Role>("Admin.Display.Meta", model => role).Zone("meta"),
                    View<Role>("Admin.Display.Content", model => role).Zone("content"),
                    View<Role>("Admin.Display.Footer", model => role).Zone("footer")
                ));

        }

        public override async Task<IViewProviderResult> BuildIndexAsync(Role role, IUpdateModel updater)
        {

            var filterOptions = new FilterOptions();

            var pagerOptions = new PagerOptions
            {
                Page = GetPageIndex(updater)
            };

            var viewModel = await GetPagedModel(
                filterOptions,
                pagerOptions);

            return Views(
                View<RolesIndexViewModel>("Admin.Index.Header", model => viewModel).Zone("header"),
                View<RolesIndexViewModel>("Admin.Index.Tools", model => viewModel).Zone("tools"),
                View<RolesIndexViewModel>("Admin.Index.Content", model => viewModel).Zone("content")
            );

        }

        public override async Task<IViewProviderResult> BuildEditAsync(Role role, IUpdateModel updater)
        {
        
            var editRoleViewModel = new EditRoleViewModel()
            {
                Id = role.Id,
                RoleName = role.Name,
                Role = role,
                IsNewRole = await IsNewRole(role.Id),
                EnabledPermissions = await GetEnabledRolePermissionsAsync(role),
                CategorizedPermissions = await _permissionsManager.GetCategorizedPermissionsAsync()
            };

            return Views(
                View<EditRoleViewModel>("Admin.Edit.Header", model => editRoleViewModel).Zone("header"),
                View<EditRoleViewModel>("Admin.Edit.Meta", model => editRoleViewModel).Zone("meta"),
                View<EditRoleViewModel>("Admin.Edit.Content", model => editRoleViewModel).Zone("content"),
                View<EditRoleViewModel>("Admin.Edit.Footer", model => editRoleViewModel).Zone("footer"),
                View<EditRoleViewModel>("Admin.Edit.Actions", model => editRoleViewModel).Zone("actions")
            );

        }

        public override async Task<IViewProviderResult> BuildUpdateAsync(Role role, IUpdateModel updater)
        {

            var model = new EditRoleViewModel();

            if (!await updater.TryUpdateModelAsync(model))
            {
                return await BuildEditAsync(role, updater);
            }

            if (updater.ModelState.IsValid)
            {

                role.Name = model.RoleName?.Trim();
                
                var result = await _roleManager.CreateAsync(role);
                foreach (var error in result.Errors)
                {
                    updater.ModelState.AddModelError(string.Empty, error.Description);
                }

            }

            return await BuildEditAsync(role, updater);

        }

        #endregion

        #region "Private Methods"

        async Task<bool> IsNewRole(int roleId)
        {
            return await _roleManager.FindByIdAsync(roleId.ToString()) == null;
        }
        
        async Task<RolesIndexViewModel> GetPagedModel(
            FilterOptions filterOptions,
            PagerOptions pagerOptions)
        {
            var roles = await GetRoles(filterOptions, pagerOptions);
            return new RolesIndexViewModel(
                roles,
                filterOptions,
                pagerOptions);
        }

        async Task<IPagedResults<Role>> GetRoles(
            FilterOptions filterOptions,
            PagerOptions pagerOptions)
        {
            return await _platoRoleStore.QueryAsync()
                .Take(pagerOptions.Page, pagerOptions.PageSize)
                .Select<RoleQueryParams>(q =>
                {
                    if (filterOptions.RoleId > 0)
                    {
                        q.Id.Equals(filterOptions.RoleId);
                    }
                    if (!string.IsNullOrEmpty(filterOptions.Search))
                    {
                        q.RoleName.IsIn(filterOptions.Search);
                    }
                })
                .OrderBy("Id", OrderBy.Desc)
                .ToList();
        }

        async Task<IEnumerable<string>> GetEnabledRolePermissionsAsync(Role role)
        {

            // We can only obtain enabled permissions for existing roles
            // Return an empty list for new roles to avoid additional null checks
            if (role.Id == 0)
            {
                return new List<string>();
            }

            // If the role is anonymous set the authtype to
            // null to ensure IsAuthenticated is set to false
            var authType = role.Name != DefaultRoles.Anonymous
                ? "UserAuthType"
                : null;

            // Dummy identity
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role.Name)
            }, authType);

            // Dummy principal
            var principal = new ClaimsPrincipal(identity);

            // Permissions grouped by feature
            var categorizedPermissions = await _permissionsManager.GetCategorizedPermissionsAsync();

            // Get flat permissions list from categorized permissions
            var permissions = categorizedPermissions.SelectMany(x => x.Value);

            var result = new List<string>();
            foreach (var permission in permissions)
            {
                if (await _authorizationService.AuthorizeAsync(principal, permission))
                {
                    result.Add(permission.Name);
                }
            }

            return result;

        }

        int GetPageIndex(IUpdateModel updater)
        {

            var page = 1;
            var routeData = updater.RouteData;
            var found = routeData.Values.TryGetValue("page", out object value);
            if (found)
            {
                int.TryParse(value.ToString(), out page);
            }

            return page;

        }

        #endregion

    }
}