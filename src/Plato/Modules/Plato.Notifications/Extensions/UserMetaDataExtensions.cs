﻿using System;
using Plato.Internal.Models.Notifications;
using Plato.Internal.Models.Users;
using Plato.Notifications.Models;

namespace Plato.Notifications.Extensions
{

    public static class UserMetaDataExtensions
    {

        public static bool NotificationEnabled(
            this IUserMetaData<UserData> user,
            INotificationType notificationType)
        {

            var settings = user.GetOrCreate<UserNotificationTypes>();
            var notificationTypes = settings?.NotificationTypes;
            if (notificationTypes != null)
            {
                foreach (var localNotificationType in notificationTypes)
                {
                    if (localNotificationType.Id.Equals(notificationType.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;

        }

    }

}