using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MovieBase
{
    public class BaseWindow : Window
    {
        public BaseWindow()
        {
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (s, e) => SystemCommands.CloseWindow(this)));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (s, e) => SystemCommands.MinimizeWindow(this)));
            // Добавляем команду развертывания/восстановления
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (s, e) => DoMaximizeRestore()));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (s, e) => DoMaximizeRestore()));
        }

        private void DoMaximizeRestore()
        {
            if (this.WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(this);
            else
                SystemCommands.MaximizeWindow(this);
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
    }
}
