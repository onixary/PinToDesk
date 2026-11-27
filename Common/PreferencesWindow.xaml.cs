using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PinToDesk.Common
{
    /// <summary>
    /// PinToDesk 偏好设置中心窗口
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        private readonly SettingsManager _settingsManager;

        private bool _isInitializing = false;
        private bool _isSaved = false;

        // 请求重置待办窗口位置与大小的回调
        public event Action? ResetTodoBoundsRequested;

        public PreferencesWindow(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;

            LoadFromSettings();
        }

        // ══════════════════════════════════════════════
        // 从配置加载到 UI
        // ══════════════════════════════════════════════
        private void LoadFromSettings()
        {
            _isInitializing = true;
            try
            {
                var s = _settingsManager.Settings;

                // 1. TodoList
                ChkTodoExpandedDisplay.IsChecked = s.TodoExpandedDisplay;
                ChkTrayDoubleClick.IsChecked = s.TrayDoubleClickTodoEnabled;
                SelectComboByTag(CmbRetentionDays, s.RecycleBinRetentionDays, 1); // 默认 60 天

                // 2. General
                SelectComboByStringTag(CmbThemeMode, s.ThemeMode ?? "System", 0); // 默认跟随系统
                SelectComboByTag(CmbOpacity, s.WindowOpacity, 5); // 默认 100%
                ChkAutoStart.IsChecked = AutoStartHelper.IsAutoStartEnabled();
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private static void SelectComboByTag(System.Windows.Controls.ComboBox combo, int targetVal, int defaultIndex)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is ComboBoxItem item &&
                    int.TryParse(item.Tag?.ToString(), out int val) && val == targetVal)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            if (combo.Items.Count > defaultIndex) combo.SelectedIndex = defaultIndex;
        }

        private static void SelectComboByStringTag(System.Windows.Controls.ComboBox combo, string targetTag, int defaultIndex)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is ComboBoxItem item &&
                    string.Equals(item.Tag?.ToString(), targetTag, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            if (combo.Items.Count > defaultIndex) combo.SelectedIndex = defaultIndex;
        }

        // ══════════════════════════════════════════════
        // 主题模式与不透明度即时预览响应
        // ══════════════════════════════════════════════
        private void CmbThemeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            ApplyLivePreview();
        }

        private void CmbOpacity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            ApplyLivePreview();
        }

        private void ApplyLivePreview()
        {
            string mode = (CmbThemeMode?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "System";
            int opacity = 100;
            if (CmbOpacity?.SelectedItem is ComboBoxItem opItem && int.TryParse(opItem.Tag?.ToString(), out int val))
            {
                opacity = val;
            }
            ThemeManager.ApplyTheme(mode, opacity);
        }

        // ══════════════════════════════════════════════
        // 全局 Esc：关闭偏好设置窗口
        // ══════════════════════════════════════════════
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.Escape)
            {
                CancelAndClose();
                e.Handled = true;
            }
        }

        // ══════════════════════════════════════════════
        // 保存全部配置
        // ══════════════════════════════════════════════
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var settings = _settingsManager.Settings;

            settings.TodoExpandedDisplay        = ChkTodoExpandedDisplay.IsChecked ?? false;
            settings.TrayDoubleClickTodoEnabled = ChkTrayDoubleClick.IsChecked ?? true;

            if (CmbRetentionDays.SelectedItem is ComboBoxItem retItem &&
                int.TryParse(retItem.Tag?.ToString(), out int retDays))
            {
                settings.RecycleBinRetentionDays = retDays;
            }

            // 2. General
            if (CmbThemeMode.SelectedItem is ComboBoxItem themeItem)
            {
                settings.ThemeMode = themeItem.Tag?.ToString() ?? "System";
            }

            if (CmbOpacity.SelectedItem is ComboBoxItem opItem &&
                int.TryParse(opItem.Tag?.ToString(), out int opVal))
            {
                settings.WindowOpacity = opVal;
            }

            AutoStartHelper.SetAutoStart(ChkAutoStart.IsChecked ?? false);

            _settingsManager.SaveSettings(settings);

            _isSaved = true;
            Close();
        }

        // ══════════════════════════════════════════════
        // 恢复默认配置
        // ══════════════════════════════════════════════
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("确定恢复所有偏好设置为默认值吗？", "提示",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            var settings = _settingsManager.Settings;
            settings.TodoExpandedDisplay        = false;
            settings.TrayDoubleClickTodoEnabled = true;
            settings.RecycleBinRetentionDays    = 60;
            settings.ThemeMode                  = "System";
            settings.WindowOpacity              = 100;
            settings.TodoWindowLeft             = null;
            settings.TodoWindowTop              = null;
            settings.TodoWindowWidth            = null;
            settings.TodoWindowHeight           = null;

            _settingsManager.SaveSettings(settings);
            AutoStartHelper.SetAutoStart(false);

            LoadFromSettings();
            ResetTodoBoundsRequested?.Invoke();
        }

        private void BtnResetTodoBounds_Click(object sender, RoutedEventArgs e)
        {
            ResetTodoBoundsRequested?.Invoke();
            if (BtnResetTodoBounds != null)
            {
                var originalText = BtnResetTodoBounds.Content;
                BtnResetTodoBounds.Content = "已重置";
                BtnResetTodoBounds.IsEnabled = false;
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    BtnResetTodoBounds.Content = originalText;
                    BtnResetTodoBounds.IsEnabled = true;
                };
                timer.Start();
            }
        }

        private void CancelAndClose()
        {
            Close();
        }

        private void TitleBar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) => CloseBtn.Opacity = 1;
        private void TitleBar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => CloseBtn.Opacity = 0;

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) NativeMethods.DragMoveWithBounds(this);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => CancelAndClose();

        protected override void OnClosed(EventArgs e)
        {
            if (!_isSaved)
            {
                // 恢复已保存的配置主题（取消未保存的临时预览）
                ThemeManager.ApplyTheme();
            }
            base.OnClosed(e);
        }
    }
}
