using SQLite;


namespace Spazierengeher.Data;


public class DailyStepsDb
{
    private readonly SQLiteAsyncConnection _conn;
    private readonly Task _initializationTask;


    public DailyStepsDb()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "spazierengeher.db3");
        _conn = new SQLiteAsyncConnection(path);
        _initializationTask = InitializeDatabaseAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        await _conn.CreateTableAsync<DailySteps>();
        await _conn.CreateTableAsync<UserSettings>();
    }


    public static DateTime TodayKey => DateTime.Today;


    public async Task<int> GetStepsAsync(DateTime dateKey)
    {
        await _initializationTask;
        var row = await _conn.FindAsync<DailySteps>(dateKey.Date);
        return row?.Steps ?? 0;
    }


    public async Task UpsertStepsAsync(DateTime dateKey, int steps)
    {
        await _initializationTask;
        var row = await _conn.FindAsync<DailySteps>(dateKey.Date) ?? new DailySteps { DateKey = dateKey.Date };
        row.Steps = steps;
        row.UpdatedAt = DateTime.Now;
        await _conn.InsertOrReplaceAsync(row);
    }


    public async Task<List<DailySteps>> GetRecentAsync(int days = 14)
    {
        await _initializationTask;
        var cutoff = DateTime.Today.AddDays(-days).Date;
        return await _conn.Table<DailySteps>()
        .Where(x => x.DateKey >= cutoff)
        .OrderByDescending(x => x.DateKey)
        .ToListAsync();
    }


    public async Task<UserSettings> GetSettingsAsync()
    {
        await _initializationTask;
        var s = await _conn.FindAsync<UserSettings>(1);
        if (s == null)
        {
            s = new UserSettings();
            await _conn.InsertAsync(s);
        }
        return s;
    }


    public async Task SaveSettingsAsync(UserSettings s)
    {
        await _initializationTask;
        await _conn.InsertOrReplaceAsync(s);
    }
}