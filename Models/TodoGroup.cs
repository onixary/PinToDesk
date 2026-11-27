using System;
using System.ComponentModel;

namespace PinToDesk.Models;

public sealed class TodoGroup : INotifyPropertyChanged
{
    public Guid Id { get; init; }
    private string _name = "默认";
    public string Name
    {
        get => _name;
        set { _name = value; PropertyChanged?.Invoke(this, new(nameof(Name))); }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}
