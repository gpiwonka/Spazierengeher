using SQLite;


namespace Spazierengeher.Data;


public class DailyStepsDb
{
    private readonly SQLiteAsyncConnection _conn;


    public DailyStepsDb()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "spazierengeher.db3");
        _conn = new SQLiteAsyncConnection(path);
        _ = _conn.CreateTableAsync<DailySteps>();
        _ = _conn.CreateTableAsync<UserSettings>();
    }


    public static string TodayKey => DateTime.Today.ToString("yyyy-MM-dd");


    public async Task<int> GetStepsAsync(string dateKey)
    {
        var row = await _conn.FindAsync<DailySteps>(dateKey);
        return row?.Steps ?? 0;
    }


    public async Task UpsertStepsAsync(string dateKey, int steps)
    {
        var row = await _conn.FindAsync<DailySteps>(dateKey) ?? new DailySteps { DateKey = dateKey };
        row.Steps = steps;
        await _conn.InsertOrReplaceAsync(row);
    }


    public Task<List<DailySteps>> GetRecentAsync(int days = 14)
    {
        var cutoff = DateTime.Today.AddDays(-days);
        return _conn.Table<DailySteps>()
        .Where(x => string.Compare(x.DateKey, cutoff.ToString("yyyy-MM-dd")) >= 0)
        .OrderByDescending(x => x.DateKey)
        .ToListAsync();
    }


    public async Task<UserSettings> GetSettingsAsync()
    {
        var s = await _conn.FindAsync<UserSettings>(1);
        if (s == null)
        {
            s = new UserSettings();
            await _conn.InsertAsync(s);
        }
        return s;
    }


    public Task SaveSettingsAsync(UserSettings s) => _conn.InsertOrReplaceAsync(s);
}