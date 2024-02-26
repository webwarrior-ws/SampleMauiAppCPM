namespace BackendDataLayer

open System
open System.Linq

open FSharp.Data.Sql

open DataModel

type MutableDictionary<'T, 'G> = System.Collections.Generic.Dictionary<'T, 'G>

type SQL =
    SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL, Constants.DevelopmentConnStr, Owner="public, admin, references", UseOptionTypes=true, ResolutionPath=Constants.ResolutionPath>

type Access(runtimeConnectionString: string) =

    let GetDataContext() =
        typeof<Npgsql.NpgsqlConnection>.Assembly |> ignore
        SQL.GetDataContext runtimeConnectionString

    member _.UpdateGpsLocation(gpsLocationDetails: UpdateGpsLocationRequest) =
        let ctx = GetDataContext()

        let maybeFoundUser =
            query {
                for user in ctx.Public.Users do
                    where(user.UserId = gpsLocationDetails.UserId)
                    select(Some user)
                    exactlyOneOrDefault
            }

        match maybeFoundUser with
        | Some user ->
            user.GpsLatitude <- Some gpsLocationDetails.Latitude
            user.GpsLongitude <- Some gpsLocationDetails.Longitude
            user.LastGpsUpdate <- Some DateTime.UtcNow
            ctx.SubmitUpdates()
        | None -> failwithf "User %i not found" gpsLocationDetails.UserId

    member _.GetFolksLocation(userId: int) =
        let ctx = GetDataContext()

        let folkIds =
            query {
                for relationship in ctx.Public.Relationships do
                    where(relationship.UserId = userId)
                    select(relationship.AssigneeId)
            }

        let users =
            query {
                for user in ctx.Public.Users do
                    where(folkIds.Contains(user.UserId))

                    select(
                        user.UserId,
                        user.GpsLatitude,
                        user.GpsLongitude,
                        user.LastGpsUpdate
                    )
            }

        users |> Seq.toArray

    member _.CreateNewUserId() =
        let ctx = GetDataContext()
        let newUser = ctx.Public.Users.Create()
        ctx.SubmitUpdates()
        newUser.UserId

    member _.CreateRelationship (userId: int) (folkId: int) =
        let ctx = GetDataContext()

        let createRelationshipObj(userId: int, folkId: int) =
            let relationshipObj = ctx.Public.Relationships.Create()
            relationshipObj.UserId <- userId
            relationshipObj.AssigneeId <- folkId
            relationshipObj.Closeness <- 0

        // Returns true if exception indicates uniqueness problem
        let isDuplicatePrimaryKeyError(ex: Npgsql.PostgresException) =
            let uniqueViolationErrorCode = "23505"

            ex.ConstraintName = "Relationships_pkey"
            && ex.SqlState = uniqueViolationErrorCode

        try
            createRelationshipObj(userId, folkId)
            createRelationshipObj(folkId, userId)

            ctx.SubmitUpdates()

            AddFolkStatusCode.ConnectedSuccess
        with
        | :? Npgsql.PostgresException as ex when isDuplicatePrimaryKeyError ex ->
            try
                ctx.ClearUpdates() |> ignore<List<Common.SqlEntity>>
                createRelationshipObj(userId, folkId)
                ctx.SubmitUpdates()
                AddFolkStatusCode.ConnectedCompleted
            with
            | :? Npgsql.PostgresException as ex when
                isDuplicatePrimaryKeyError ex
                ->
                AddFolkStatusCode.AlreadyDone


    member __.GetFolks(userId: int) =
        let ctx = GetDataContext()

        query {
            for relationship in ctx.Public.Relationships do
                where(relationship.UserId = userId)
                select(relationship.AssigneeId, relationship.Closeness)
        }
        |> dict
        |> MutableDictionary<int, int>

    member _.UpdateCloseness(updateClosenessDetails: UpdateClosenessRequest) =
        let ctx = GetDataContext()

        let maybeFoundRelationship =
            query {
                for relationship in ctx.Public.Relationships do
                    where(
                        relationship.UserId = updateClosenessDetails.UserId
                        && relationship.AssigneeId = updateClosenessDetails.FolkId
                    )

                    select(Some relationship)
                    exactlyOneOrDefault
            }

        match maybeFoundRelationship with
        | Some relationship ->
            relationship.Closeness <- updateClosenessDetails.NewCloseness
            ctx.SubmitUpdates()
        | None ->
            failwithf
                "Relationship %i, %i not found"
                updateClosenessDetails.UserId
                updateClosenessDetails.FolkId

    member _.TestRelationships() : option<_> =
        let ctx = GetDataContext()

        let firstRelRow =
            query {
                for e in ctx.Public.Relationships do
                    select(e.UserId, e.AssigneeId)
            }
            |> Seq.tryHead

        firstRelRow
