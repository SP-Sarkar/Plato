﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatoCore.Abstractions.Extensions;
using PlatoCore.Badges.NotificationTypes;
using PlatoCore.Cache.Abstractions;
using PlatoCore.Data.Abstractions;
using PlatoCore.Models.Notifications;
using PlatoCore.Models.Users;
using PlatoCore.Notifications.Abstractions;
using PlatoCore.Stores.Abstractions.Users;
using PlatoCore.Stores.Users;
using PlatoCore.Tasks.Abstractions;
using PlatoCore.Models.Badges;
using PlatoCore.Notifications.Extensions;
using PlatoCore.Reputations.Abstractions;
using PlatoCore.Stores.Badges;

namespace Plato.Users.Badges.Tasks
{

    public class ConfirmedMemberBadgeAwarder : IBackgroundTaskProvider
    {

        private const string Sql = @"                   
                DECLARE @date datetimeoffset = SYSDATETIMEOFFSET(); 
                DECLARE @badgeName nvarchar(255) = '{name}';
                DECLARE @threshold int = {threshold};                  
                DECLARE @userId int;
                DECLARE @myTable TABLE
                (
                    Id int IDENTITY (1, 1) NOT NULL PRIMARY KEY,
                    UserId int NOT NULL
                );
                DECLARE MSGCURSOR CURSOR FOR SELECT TOP 200 u.Id FROM {prefix}_Users AS u
                WHERE (u.EmailConfirmed = 1)
                AND NOT EXISTS (
                   SELECT Id FROM {prefix}_UserBadges ub 
                   WHERE ub.UserId = u.Id AND ub.BadgeName = @badgeName
                 )
                ORDER BY u.Id DESC;

                OPEN MSGCURSOR FETCH NEXT FROM MSGCURSOR INTO @userId;                    
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    DECLARE @identity int;
                    EXEC {prefix}_InsertUpdateUserBadge 0, @badgeName, @userId, @date, @identity OUTPUT;
                    IF (@identity > 0)
                    BEGIN
                        INSERT INTO @myTable (UserId) VALUES (@userId);                     
                    END
                    FETCH NEXT FROM MSGCURSOR INTO @userId;	                    
                END;
                CLOSE MSGCURSOR;
                DEALLOCATE MSGCURSOR;
                SELECT UserId FROM @myTable;";

        public int IntervalInSeconds => 120;

        public IBadge Badge => ProfileBadges.ConfirmedMember;

        private readonly ICacheManager _cacheManager;
        private readonly IDbHelper _dbHelper;
        private readonly IPlatoUserStore<User> _userStore;
        private readonly INotificationManager<Badge> _notificationManager;
        private readonly IUserReputationAwarder _userReputationAwarder;
        private readonly IUserNotificationTypeDefaults _userNotificationTypeDefaults;

        public ConfirmedMemberBadgeAwarder(
            ICacheManager cacheManager,
            IDbHelper dbHelper,
            IPlatoUserStore<User> userStore,
            INotificationManager<Badge> notificationManager,
            IUserReputationAwarder userReputationAwarder,
            IUserNotificationTypeDefaults userNotificationTypeDefaults)
        {
            _cacheManager = cacheManager;
            _dbHelper = dbHelper;
            _userStore = userStore;
            _notificationManager = notificationManager;
            _userReputationAwarder = userReputationAwarder;
            _userNotificationTypeDefaults = userNotificationTypeDefaults;
        }

        public async Task ExecuteAsync(object sender, SafeTimerEventArgs args)
        {
            
            // Replacements for SQL script
            var replacements = new Dictionary<string, string>()
            {
                ["{name}"] = Badge.Name,
                ["{threshold}"] = Badge.Threshold.ToString()
            };

            var userIds = await _dbHelper.ExecuteReaderAsync<IList<int>>(Sql, replacements, async reader =>
            {
                var users = new List<int>();
                while (await reader.ReadAsync())
                {
                    if (reader.ColumnIsNotNull("UserId"))
                    {
                        users.Add(Convert.ToInt32(reader["UserId"]));
                    }
                }

                return users;
            });

            if (userIds?.Count > 0)
            {

                // Get all users awarded the badge
                var users = await _userStore.QueryAsync()
                    .Take(userIds.Count, false)
                    .Select<UserQueryParams>(q => { q.Id.IsIn(userIds.ToArray()); })
                    .OrderBy("LastLoginDate", OrderBy.Desc)
                    .ToList();

                // Send notifications
                if (users != null)
                {
                    var bot = await _userStore.GetPlatoBotAsync();
                    foreach (var user in users.Data)
                    {

                        // ---------------
                        // Award badge points 
                        // ---------------

                        var badgeReputation = Badge.GetReputation();
                        if (badgeReputation.Points != 0)
                        {
                            await _userReputationAwarder.AwardAsync(badgeReputation, user.Id, $"Confirmed my email address");
                        }

                        // ---------------
                        // Trigger notifications
                        // ---------------

                        // Email notification
                        if (user.NotificationEnabled(_userNotificationTypeDefaults, EmailNotifications.NewBadge))
                        {
                            await _notificationManager.SendAsync(new Notification(EmailNotifications.NewBadge)
                            {
                                To = user
                            }, (Badge)Badge);
                        }

                        // Web notification
                        if (user.NotificationEnabled(_userNotificationTypeDefaults, WebNotifications.NewBadge))
                        {
                            await _notificationManager.SendAsync(new Notification(WebNotifications.NewBadge)
                            {
                                To = user,
                                From = bot
                            }, (Badge) Badge);
                        }

                    }
                }

                _cacheManager.CancelTokens(typeof(UserBadgeStore));
            }

        }

    }

}
