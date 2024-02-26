namespace DataModel

open System.Collections.Generic

type RegisterNewAppInstallRequest() =
    class
    end

type UpdateGpsLocationRequest =
    {
        UserId: int
        Latitude: double
        Longitude: double
    }

type AddFolkRequest =
    {
        UserId: int
        FolkId: int
        Nickname: string
    }

type AddFolkStatusCode =
    | ConnectedSuccess = 1
    | ConnectedCompleted = 2
    | AlreadyDone = 3

type AddFolkResponse =
    {
        StatusCode: AddFolkStatusCode
    }

type GetFolksRequest =
    {
        UserId: int
    }

type UpdateClosenessRequest =
    {
        UserId: int
        FolkId: int
        NewCloseness: int
    }

type GetFolksResponse =
    {
        Folks: Dictionary<int, int>
    }

type GetNotificationRequest =
    {
        UserId: int
    }

type AddFolkSuccessNotification =
    {
        UserId: int
        Nickname: string
    }
