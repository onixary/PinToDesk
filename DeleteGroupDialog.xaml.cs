using System.Windows;
using System.Windows.Input;
using PinToDesk.Common;

namespace PinToDesk;

public partial class DeleteGroupDialog : Window
{
    public DeleteGroupDialog(string name, int count)
    {
        InitializeComponent();
        Heading.Text = $"删除「{name}」标签？";
        Description.Text = count == 0
            ? "这个标签没有待办。"
            : $"{count} 条待办将移入待办历史。";
    }

    private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1) NativeMethods.DragMoveWithBounds(this);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    private void Delete_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
