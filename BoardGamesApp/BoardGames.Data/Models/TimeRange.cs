using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
[Owned]
public class TimeRange
{
    public TimeRange() { }

    public TimeRange(DateTime startTime, DateTime endTime)
    {
        if (endTime.Date < startTime.Date) throw new ArgumentException("End date must be on or after the start date.");
        startTime = startTime.Date;
        endTime = endTime.Date;
        StartTime = startTime;
        EndTime = endTime;
    }

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("end_time")]
    public DateTime EndTime { get; set; }
}
