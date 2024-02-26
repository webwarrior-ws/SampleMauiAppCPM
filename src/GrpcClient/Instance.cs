
using System;
using System.Threading.Tasks;

using Grpc.Net.Client;
using GrpcService;
using DataModel;

namespace GrpcClient
{
    public enum ServerEnvironment
    {
        Local,
        Unknown,
        Production,
    }

    public class Instance
    {
        private static string serverFqdn =
#if DEBUG
            "localhost"
#else
            "grpcserver.runinto.me"
#endif
            ;

        public RunIntoMeGrpcService.RunIntoMeGrpcServiceClient Connect()
        {
            var channel = GrpcChannel.ForAddress($"http://{serverFqdn}:8080");
            var client = new RunIntoMeGrpcService.RunIntoMeGrpcServiceClient(channel);
            return client;
        }

        public static ServerEnvironment ServerEnvironment {
            get {
                if (serverFqdn == "localhost")
                {
                    return ServerEnvironment.Local;
                }
                else if (serverFqdn.EndsWith(".runinto.me"))
                {
                    return ServerEnvironment.Production;
                }
                else
                {
                    return ServerEnvironment.Unknown;
                }
            }
        }

        public async Task<int> RegisterNewAppInstall()
        {
            var client = Connect();
            var reply = await client.GenericMethodAsync(
                new GenericInputParam { MsgIn = Marshaller.Serialize(new RegisterNewAppInstallRequest()) }
            );
            int newUserId;
            if (!int.TryParse(reply.MsgOut, out newUserId) || newUserId < 1)
            {
                throw new InvalidOperationException("Unexpected newUserId type or value: " + reply.MsgOut);
            }
            return newUserId;
        }

        public async Task UpdateGpsLocation(UpdateGpsLocationRequest gpsLocationUpdateDetails)
        {
            var client = Connect();
            await client.GenericMethodAsync(
                new GenericInputParam { MsgIn = Marshaller.Serialize(gpsLocationUpdateDetails) }
            );
        }

        public async Task<AddFolkResponse> AddFolk(AddFolkRequest addFolkRequest)
        {
            var client = Connect();
            var addFolkResponseMsg =
                await client.GenericMethodAsync(
                    new GenericInputParam { MsgIn = Marshaller.Serialize(addFolkRequest) }
                );

            var addFolkResponse =
                Marshaller.Deserialize<AddFolkResponse>(addFolkResponseMsg.MsgOut);

            return addFolkResponse;
        }

        public async Task<GetFolksResponse> GetFolks(GetFolksRequest getFolksRequest)
        {
            var client = Connect();
            var getFolksResponseMsg =
                await client.GenericMethodAsync(
                    new GenericInputParam { MsgIn = Marshaller.Serialize(getFolksRequest) }
                );

            var getFolksResponse = Marshaller.Deserialize<GetFolksResponse>(getFolksResponseMsg.MsgOut);
            return getFolksResponse;
        }
        public async Task UpdateCloseness(UpdateClosenessRequest updateClosenessRequest)
        {
            var client = Connect();
            await client.GenericMethodAsync(
                new GenericInputParam { MsgIn = Marshaller.Serialize(updateClosenessRequest) }
            );
        }
    }
}
