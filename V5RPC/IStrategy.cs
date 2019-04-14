using System;
using System.Collections.Generic;
using System.Text;
using V5RPC.Proto;

namespace V5RPC
{
    public interface IStrategy
    {
        void OnEvent(EventType type, EventArguments arguments);//TODO
        TeamInfo GetTeamInfo();//TODO
        Wheel[] GetInstruction(Field field);
        Robot[] GetPlacement(Field field);
    }
}
