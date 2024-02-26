(*
let firstRelRow = BackendDataLayer.Access.TestRelationships()

match firstRelRow with
| None -> printfn "No relationships in DB so far"
| Some(userId, assigneeId) ->
    printfn "First relationship is %i with %i!" userId assigneeId
*)
let grpcClientInstance = GrpcClient.Instance()

let replyFromServerJob =
    Async.AwaitTask <| grpcClientInstance.RegisterNewAppInstall()

let replyFromServer = Async.RunSynchronously replyFromServerJob
System.Console.WriteLine("Reply from server was: " + replyFromServer.ToString())
