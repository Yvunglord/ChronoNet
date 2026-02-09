using Microsoft.Win32;
using System.Windows;

namespace DynamicNetwork.Presentation.Services;

public class DialogService : IDialogService
{
    public bool? ShowDialog<TDialog>(Action<TDialog>? setup = null) where TDialog : Window, new()
    {
        var dialog = new TDialog();
        setup?.Invoke(dialog);
        return dialog.ShowDialog();
    }

    public string? ShowOpenFileDialog(string filter, string initialDirectory)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            InitialDirectory = initialDirectory
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public void ShowMessage(string message, string caption = "Сообщение")
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool ShowConfirmationDialog(string title, string message)
    {
        return MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        ) == MessageBoxResult.Yes;
    }

    public string? ShowSaveFileDialog(string defaultName, string defaultDir, string filter)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            InitialDirectory = defaultDir,
            FileName = defaultName
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public void ShowError(string error)
    {
        MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowInfo(string info)
    {
        MessageBox.Show(info, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowWarning(string warning)
    {
        MessageBox.Show(warning, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}

