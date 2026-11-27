using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using PinToDesk.Common;
using PinToDesk.Models;

namespace PinToDesk.Services;

internal sealed class TodoGroups : IDisposable
{
    private readonly JsonLinesStorage<TodoGroup> _catalog = new(Path.Combine(AppPaths.AppData, "tabs.jsonl"));
    private readonly Dictionary<Guid, TodoService> _stores = new();
    public ObservableCollection<TodoGroup> Tabs { get; }
    public bool CanEdit => !_catalog.HasReadFault;

    public TodoGroups()
    {
        Tabs = new(_catalog.Load());
        if (Tabs.Count == 0) Tabs.Add(new TodoGroup()); // Guid.Empty keeps the original todos.jsonl
    }

    public TodoService Store(TodoGroup group)
    {
        if (!_stores.TryGetValue(group.Id, out var store))
            _stores[group.Id] = store = new TodoService(group.Id);
        return store;
    }

    public TodoGroup Add()
    {
        var n = 1;
        while (Tabs.Any(g => g.Name == $"新标签 {n}")) n++;
        var group = new TodoGroup { Id = Guid.NewGuid(), Name = $"新标签 {n}" };
        _catalog.Save(Tabs.Append(group));
        Tabs.Add(group);
        return group;
    }

    public void Rename(TodoGroup group, string name)
    {
        name = name.Trim();
        if (name.Length == 0 || group.Name == name) return;
        _catalog.Save(Tabs.Select(g => g == group ? new TodoGroup { Id = g.Id, Name = name } : g));
        group.Name = name;
    }

    public List<TodoItem> Read(TodoGroup group)
    {
        var store = Store(group);
        store.Flush();
        return store.LoadTodos();
    }

    // Commit the history before removing the group from the index or touching its file.
    public void Delete(TodoGroup group, IReadOnlyList<TodoItem> items, RecycleBinService history)
    {
        if (!CanEdit || Store(group).HasReadFault) throw new IOException("标签数据处于读取保护状态。");
        history.AddRange(items, group.Id);
        var remaining = Tabs.Where(g => g != group).ToList();
        if (remaining.Count == 0) remaining.Add(new TodoGroup { Id = Guid.NewGuid() });
        _catalog.Save(remaining);
        if (Tabs.Count == 1) Tabs.Add(remaining[0]);
        Tabs.Remove(group);
        _stores[group.Id].Dispose();
        _stores.Remove(group.Id);
        File.Delete(TodoService.FileFor(group.Id));
    }

    public void Dispose()
    {
        foreach (var store in _stores.Values) store.Dispose();
        _catalog.Dispose();
    }
}
