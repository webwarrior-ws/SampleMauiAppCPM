namespace DataModel

// FIXME: maybe this should be extracted to a different lib, more related to Grpc, e.g. GrpcModel (or rename DataModel to Shared)
module NotificationIdentifiers =
    [<Literal>]
    let AddFolkSuccessNotificationId = "FolkAddedNotification"
