namespace Mimeo.DynamicUI.Data;

public class ImportEventArgs<T>
{
    public ImportEventArgs(IAsyncEnumerable<T> data)
    {
        Data = data;
    }

    public event Action<int>? OnImportedCountChanged;

    public IAsyncEnumerable<T> Data { get; }

    public int ImportedCount { get; private set; }

    public void SetImportedCount(int importedCount)
    {
        ImportedCount = importedCount;
        OnImportedCountChanged?.Invoke(importedCount);
    }
}