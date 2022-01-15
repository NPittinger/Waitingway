﻿namespace Waitingway.Backend.Database.Models;

public class RecentlyActiveQueueSession
{
    /** QueueSession members */
    public int Id { get; set; }

    public Guid ClientId { get; set; }
    public string ClientSessionId { get; set; }
    public int DataCenter { get; set; }
    public QueueSession.Type SessionType { get; set; }
    public int? World { get; set; }
    public int? DutyContentId { get; set; }
    public string PluginVersion { get; set; }

    /** QueueSessionData members */
    public DateTime Time { get; set; }

    public QueueSessionData.DataType Type { get; set; }
    public uint? QueuePosition { get; set; }
    public TimeSpan? GameTimeEstimate { get; set; }
    public TimeSpan? OurTimeEstimate { get; set; }
}