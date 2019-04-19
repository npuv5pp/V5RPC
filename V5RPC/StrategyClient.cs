using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using V5RPC.Proto;

namespace V5RPC
{
    public class StrategyClient : IStrategy, IDisposable
    {
        V5Client client;
        public int Timeout { get; set; } = 10000;
        public int RetryInterval { get; set; } = 50;

        public StrategyClient(IPEndPoint server, int port = 0)
        {
            client = new V5Client(port)
            {
                Server = server
            };
        }

        public void Dispose()
        {
            client.Dispose();
        }

        private byte[] CallRemote<MSG>(MSG message) where MSG : IMessage
        {
            var rpc = new RPCCall();
            switch (message)
            {
                case OnEventCall m:
                    rpc.OnEvent = m;
                    break;
                case GetTeamInfoCall m:
                    rpc.GetTeamInfo = m;
                    break;
                case GetInstructionCall m:
                    rpc.GetInstruction = m;
                    break;
                case GetPlacementCall m:
                    rpc.GetPlacement = m;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return client.Call(rpc.ToByteArray(), Timeout, RetryInterval);
        }

        public void OnEvent(EventType type, EventArguments arguments)
        {
            var call = new OnEventCall
            {
                Type = type,
                Arguments = arguments
            };
            CallRemote(call);
        }

        public TeamInfo GetTeamInfo()
        {
            var call = new GetTeamInfoCall();
            var ret = CallRemote(call);
            return GetTeamInfoResult.Parser.ParseFrom(ret).TeamInfo;
        }

        public Wheel[] GetInstruction(Field field)
        {
            var call = new GetInstructionCall
            {
                Field = field
            };
            var ret = CallRemote(call);
            return GetInstructionResult.Parser.ParseFrom(ret).Wheels.ToArray();
        }

        public Robot[] GetPlacement(Field field)
        {
            var call = new GetPlacementCall
            {
                Field = field
            };
            var ret = CallRemote(call);
            return GetPlacementResult.Parser.ParseFrom(ret).Robots.ToArray();
        }
    }
}
