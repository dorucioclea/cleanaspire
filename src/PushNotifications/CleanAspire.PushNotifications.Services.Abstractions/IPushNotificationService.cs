﻿using System.Threading;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;

namespace CleanAspire.PushNotifications.Services.Abstractions;

public interface IPushNotificationService
{
    string PublicKey { get; }

    Task SendNotificationAsync(PushSubscription subscription, PushMessage message);

    Task SendNotificationAsync(PushSubscription subscription, PushMessage message, CancellationToken cancellationToken);
}
