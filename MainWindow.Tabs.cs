using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PinToDesk.Models;
using TextBox = System.Windows.Controls.TextBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace PinToDesk;

public partial class MainWindow
{
    private void GroupTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GroupTabs.SelectedItem is not TodoGroup group || group == _currentGroup) return;
        List<TodoItem> items;
        try { items = _groups.Read(group); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            GroupTabs.SelectedItem = _currentGroup;
            MessageBox.Show(this, ex.Message, "无法切换标签", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        _currentGroup = group;
        _items.Clear();
        foreach (var item in items) _items.Add(item);
        GetTodoScrollViewer()?.ScrollToTop();
        _settingsManager.Settings.SelectedGroupId = group.Id;
        _settingsManager.SaveSettings(_settingsManager.Settings, notify: false);
        GroupTabs.ScrollIntoView(group);
    }

    private void NewGroup_Click(object sender, RoutedEventArgs e)
    {
        RunGroupChange(() =>
        {
            var group = _groups.Add();
            GroupTabs.SelectedItem = group;
            GroupTabs.UpdateLayout();
            if (GroupTabs.ItemContainerGenerator.ContainerFromItem(group) is ListBoxItem tab &&
                FindVisualChild<TextBox>(tab) is TextBox box) BeginRename(box);
        });
    }

    private void Tab_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2 || !_groups.CanEdit || sender is not Grid grid) return;
        if (FindVisualChild<TextBox>(grid) is not TextBox box || box.IsVisible) return;
        GroupTabs.SelectedItem = grid.DataContext;
        BeginRename(box);
        e.Handled = true;
    }

    private TextBox? _renamingBox;

    private void BeginRename(TextBox box)
    {
        _renamingBox = box;
        box.Text = ((TodoGroup)box.DataContext).Name;
        box.Visibility = Visibility.Visible;
        box.Focus();
        box.SelectAll();
        box.BringIntoView();
    }

    private void TabName_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Escape)) return;
        e.Handled = true;
        FinishRename((TextBox)sender, e.Key == Key.Enter);
    }

    private void TabName_LostFocus(object sender, KeyboardFocusChangedEventArgs e) =>
        FinishRename((TextBox)sender, true);

    private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_renamingBox is { } box && !(e.OriginalSource is System.Windows.Media.Visual source &&
            (source == box || box.IsAncestorOf(source))))
            FinishRename(box, true);
    }

    private void Window_Deactivated(object? sender, EventArgs e)
    {
        if (_renamingBox is { } box) FinishRename(box, true);
    }

    private void FinishRename(TextBox box, bool save)
    {
        if (box.Visibility != Visibility.Visible) return;
        if (_renamingBox == box) _renamingBox = null;
        box.Visibility = Visibility.Collapsed;
        if (save && box.DataContext is TodoGroup group)
            RunGroupChange(() => _groups.Rename(group, box.Text));
    }

    private void TabBar_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (FindVisualChild<ScrollViewer>(GroupTabs) is not { ScrollableWidth: > 0 } scroll) return;
        scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset - e.Delta * 0.5);
        e.Handled = true;
    }

    private void DeleteGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: TodoGroup group }) return;
        RunGroupChange(() =>
        {
            var todos = _groups.Read(group);
            if (_groups.Store(group).HasReadFault) return;
            var confirm = new DeleteGroupDialog(group.Name, todos.Count) { Owner = this };
            if (confirm.ShowDialog() != true) return;
            int index = _groups.Tabs.IndexOf(group);
            bool selected = _currentGroup == group;
            try { _groups.Delete(group, todos, _recycleBinService); }
            finally
            {
                if (selected && !_groups.Tabs.Contains(group))
                    GroupTabs.SelectedItem = _groups.Tabs[Math.Min(index, _groups.Tabs.Count - 1)];
            }
        });
    }

    private void RunGroupChange(Action action)
    {
        if (!_groups.CanEdit) return;
        try { action(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, ex.Message, "标签操作未完成", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
