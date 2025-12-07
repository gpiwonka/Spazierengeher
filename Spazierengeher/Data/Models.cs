using SQLite;
using System.Data.SqlTypes;

namespace Spazierengeher.Data;


[Table("daily_steps")]
public class DailySteps
{
    [PrimaryKey]
    public DateTime DateKey { get; set; }

    public int Steps { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}


[Table("user_settings")]
public class UserSettings
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } = 1;

    /// <summary>
    /// Tagesziel für Schritte
    /// </summary>
    public int DailyGoal { get; set; } = 10000;

    /// <summary>
    /// Geschätztes Gewicht des Benutzers in kg (für Kalorienberechnung)
    /// </summary>
    public double WeightKg { get; set; } = 70.0;

    /// <summary>
    /// Geschätzte Schrittlänge in Metern
    /// </summary>
    public double StepLengthMeters { get; set; } = 0.8;

    /// <summary>
    /// Benachrichtigungen aktiviert
    /// </summary>
    public bool NotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Tracking automatisch beim App-Start starten
    /// </summary>
    public bool AutoStartTracking { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}