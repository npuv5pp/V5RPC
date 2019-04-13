using System;
using System.Collections.Generic;
using System.Text;
using V5RPC.Proto;

namespace V5RPC
{
    public interface IStrategy
    {
        void OnEvent();//TODO
        TeamInfo GetTeamInfo();//TODO
        Wheel[] GetInstruction(Field field);
        Robot[] GetPlacement(Field field);
    }
}
