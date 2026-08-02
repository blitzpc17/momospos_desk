using System.Windows.Forms;

namespace momospos.Views.Dialogs
{
    public static class CustomDialog
    {
        public static DialogResult Show(string message, string title = "Aviso", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            using (var dialog = new CustomMessageBoxForm(message, title, buttons, icon))
            {
                return dialog.ShowDialog();
            }
        }

        public static void ShowMessage(string message, string title = "Aviso")
        {
            Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowWarning(string message, string title = "Atención")
        {
            Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowError(string message, string title = "Error")
        {
            Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool ShowConfirm(string message, string title = "Confirmar")
        {
            return Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public static string ShowInput(string prompt, string title = "Entrada", string defaultValue = "")
        {
            using (var dialog = new CustomInputBoxForm(prompt, title, defaultValue))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.InputValue;
                }
                return string.Empty; // Equivalente a cancelar
            }
        }
    }
}
