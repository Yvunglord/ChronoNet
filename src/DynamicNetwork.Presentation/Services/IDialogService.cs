using System.Windows;

namespace DynamicNetwork.Presentation.Services;

public interface IDialogService
{
    bool ShowConfirmationDialog(string title, string message);
    bool? ShowDialog<TDialog>(Action<TDialog>? setup = null) where TDialog : Window, new();
    string? ShowOpenFileDialog(string filter, string defaultDir);
    string? ShowSaveFileDialog(string defaultName, string defaultDir, string filter);
    void ShowMessage(string message, string caption = "Сообщение");
    void ShowError(string error);
    void ShowInfo(string info);
    void ShowWarning(string warning);
}
