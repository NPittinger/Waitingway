﻿namespace XIVq.Common.Protocol.Serverbound;

public class QueueExit : IPacket
{
    public QueueExitReason Reason { get; init; }
    
    public enum QueueExitReason
    {
        Unknown = 0,
        Success = 1,
        Error = 2,
        UserCancellation = 3
    }
}
