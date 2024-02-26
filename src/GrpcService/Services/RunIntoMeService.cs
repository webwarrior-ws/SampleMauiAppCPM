using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;

using Grpc.Core;

using DataModel;
using GrpcService.Utils;

namespace GrpcService.Services
{
    public class RunIntoMeService : RunIntoMeGrpcService.RunIntoMeGrpcServiceBase
    {
        // Capacity of 5 is set here as a limit on how many unsent
        // notifications can the channel hold before it starts dropping
        // old ones. I don't think this ever happens but it's better
        // than having an unbounded channel.
        private const int notificationChannelCapacity = 5;


        private readonly NotificationProvider notificationProvider;
        private readonly PushNotificationProvider pushNotificationProvider;
        private readonly ILogger<RunIntoMeService> logger;
        private readonly BackendDataLayer.Access access;

        private const int MaximumDistanceForSendingNotificationsInMeters = 300;
        private readonly TimeSpan MaxTimeDifferenceToConsiderFolksStillNearby = TimeSpan.FromMinutes(20);

        public RunIntoMeService(NotificationProvider notificationProvider, PushNotificationProvider pushNotificationProvider, ILogger<RunIntoMeService> logger, IConfiguration configuration)
        {
            this.notificationProvider = notificationProvider;
            this.pushNotificationProvider = pushNotificationProvider;
            this.logger = logger;
            access = new BackendDataLayer.Access(configuration.GetConnectionString("MainDB"));
        }

        public override async Task<GenericOutputParam> GenericMethod(GenericInputParam request, ServerCallContext context)
        {
            var reqHash = request.GetHashCode();
            var reqId = $"REQUEST {reqHash}";
            var responseId = $"RESPONSE {reqHash}";
            Console.WriteLine(reqId);

            var (type, version) = Marshaller.ExtractMetadata(request.MsgIn);

            Console.WriteLine($"{reqId}  version: {version}  type: {type.FullName}");
            var deserializedRequest = Marshaller.DeserializeAbstract(request.MsgIn, type);

            if (deserializedRequest is RegisterNewAppInstallRequest _)
            {
                var newUserId = access.CreateNewUserId();
                Console.WriteLine($"{reqId}  RegisterNewAppInstallResponse: {newUserId}");
                try
                {
                    return new GenericOutputParam
                    {
                        MsgOut = newUserId.ToString()
                    };
                }
                finally
                {
                    Console.WriteLine(responseId);
                }
            }
            else if (deserializedRequest is UpdateGpsLocationRequest gpsLocationDetails)
            {
                Console.WriteLine($"{reqId}  UpdateGpsLocationReq: {gpsLocationDetails.UserId}, {gpsLocationDetails.Latitude} {gpsLocationDetails.Longitude}");
                access.UpdateGpsLocation(gpsLocationDetails);
                Console.WriteLine($"{reqId}  UpdateGpsLocationReq: {gpsLocationDetails.UserId} DB");

                var folks = access.GetFolksLocation(gpsLocationDetails.UserId);
                Console.WriteLine($"{reqId}  UpdateGpsLocationReq: {gpsLocationDetails.UserId} DB-SCAN");
                foreach (var (folkId, folkLatitude, folkLongitude, lastGpsUpdate) in folks)
                {
                    Console.WriteLine($"{reqId}  UpdateGpsLocationReq: {gpsLocationDetails.UserId} DB-SCAN-BEGIN {folkId}");
                    if (FSharpOption<DateTime>.get_IsNone(lastGpsUpdate))
                    {
                        Console.WriteLine($"{reqId}  UpdateGpsLocationReq: {gpsLocationDetails.UserId} DB-SCAN-CONTINUE {folkId}");
                        continue;
                    }

                    Console.WriteLine($"{reqId}  UpdateGpsLocationReq: {gpsLocationDetails.UserId} DB-SCAN-DISTANCE");
                    var distance = GpsUtil.GetDistanceInMeters(folkLongitude.Value, folkLatitude.Value, gpsLocationDetails.Longitude, gpsLocationDetails.Latitude);
                    Console.WriteLine($"{reqId}  UpdateGpsLocationReq: {gpsLocationDetails.UserId} DB-SCAN-DISTANCE {distance} meters to {folkId}");
                    if (distance < MaximumDistanceForSendingNotificationsInMeters)
                    {
                        Console.WriteLine($"{reqId}  UpdateGpsLocationReq: {gpsLocationDetails.UserId} DB-SCAN-DISTANCE-FIRE {folkId}");

                        var pushNotifText = Texts.FolkWasNearby;
                        if (DateTime.UtcNow - lastGpsUpdate.Value < MaxTimeDifferenceToConsiderFolksStillNearby)
                        {
                            pushNotifText = Texts.FolkIsNearby;
                        }
                        await pushNotificationProvider.SendTextPushNotification(folkId, Texts.Alert, pushNotifText);
                        await pushNotificationProvider.SendTextPushNotification(gpsLocationDetails.UserId, Texts.Alert, pushNotifText);
                    }
                    else
                    {
                        Console.WriteLine($"{reqId}  UpdateGpsLocationReq: {gpsLocationDetails.UserId} DB-SCAN-DISTANCE-DISCARD {folkId}");
                    }
                }

                try
                {
                    return new GenericOutputParam { MsgOut = String.Empty };
                }
                finally
                {
                    Console.WriteLine(responseId);
                }
            }
            else if (deserializedRequest is AddFolkRequest addFolkRequest)
            {
                Console.WriteLine($"{reqId}  AddFolkReq: {addFolkRequest.UserId}, {addFolkRequest.FolkId}-{addFolkRequest.Nickname}");

                var statusCode = access.CreateRelationship(addFolkRequest.UserId, addFolkRequest.FolkId);
                var addFolkNotification = new AddFolkSuccessNotification(addFolkRequest.UserId, addFolkRequest.Nickname);

                // Only notify the Folk if they were not our Folk before
                if (statusCode != AddFolkStatusCode.AlreadyDone)
                {
                    Console.WriteLine($"{reqId}  AddFolkReq: {addFolkRequest.UserId} DB-SCAN-RELSTATUS-DO {addFolkRequest.FolkId} ({statusCode})");
                    await notificationProvider.SendNotification(addFolkRequest.FolkId, Marshaller.Serialize(addFolkNotification));
                    Console.WriteLine($"{reqId}  AddFolkReq: {addFolkRequest.UserId} DB-SCAN-RELSTATUS-DONE {addFolkRequest.FolkId} ({statusCode})");
                }
                else
                {
                    Console.WriteLine($"{reqId}  AddFolkReq: {addFolkRequest.UserId} DB-SCAN-RELSTATUS-DISCARD {addFolkRequest.FolkId} ({statusCode})");
                }

                try
                {
                    return new GenericOutputParam
                    {
                        MsgOut = Marshaller.Serialize(new AddFolkResponse(statusCode))
                    };
                }
                finally
                {
                    Console.WriteLine(responseId);
                }
            }
            else if (deserializedRequest is GetFolksRequest getFolksRequest)
            {
                Console.WriteLine($"{reqId}  GetFolksRequest: {getFolksRequest.UserId}");
                GetFolksResponse response =
                    new(access.GetFolks(getFolksRequest.UserId));
                Console.WriteLine($"{reqId}  GetFolksResponse count: {response.Folks.Count}");
                try
                {
                    return new GenericOutputParam
                    {
                        MsgOut = Marshaller.Serialize(response)
                    };
                }
                finally
                {
                    Console.WriteLine(responseId);
                }
            }
            else if (deserializedRequest is UpdateClosenessRequest updateClosenessRequest)
            {
                Console.WriteLine($"{reqId}  UpdateClosenessReq: {updateClosenessRequest.UserId}, {updateClosenessRequest.NewCloseness}");
                access.UpdateCloseness(updateClosenessRequest);
                try
                {
                    return new GenericOutputParam { MsgOut = String.Empty };
                }
                finally
                {
                    Console.WriteLine(responseId);
                }
            }
            else
            {
                throw new InvalidOperationException("Unable to deserialize request: " + request.MsgIn);
            }

        }

        public override async Task GenericStreamOutputMethod(GenericInputParam request, IServerStreamWriter<GenericOutputParam> responseStream, ServerCallContext context)
        {
            var reqHash = request.GetHashCode();
            var reqId = $"STREAM-REQUEST {reqHash}";
            var responseId = $"STREAM-RESPONSE {reqHash}";
            Console.WriteLine(reqId);

            var (type, version) = Marshaller.ExtractMetadata(request.MsgIn);

            Console.WriteLine($"{reqId}  version: {version}  type: {type.FullName}");

            var deserializedRequest = Marshaller.DeserializeAbstract(request.MsgIn, type);

            if (deserializedRequest is GetNotificationRequest getNotificationRequest)
            {
                Console.WriteLine($"{reqId}  GetNotificationRequest: {getNotificationRequest.UserId}");

                var channel = Channel.CreateBounded<GenericOutputParam>(
                    new BoundedChannelOptions(capacity: notificationChannelCapacity)
                    {
                        FullMode = BoundedChannelFullMode.DropOldest
                    }
                );

                try
                {
                    notificationProvider.AddChannel(getNotificationRequest.UserId, channel.Writer);

                    await foreach (var message in channel.Reader.ReadAllAsync(context.CancellationToken))
                    {
                        await responseStream.WriteAsync(message);
                    }
                }
                finally
                {
                    notificationProvider.RemoveChannel(getNotificationRequest.UserId, channel.Writer);
                    Console.WriteLine(responseId);
                }
            }
        }
    }
}
