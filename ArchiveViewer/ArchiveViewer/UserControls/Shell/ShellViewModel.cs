using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using ArchiveViewer.UserControls.Home;
using ArchiveViewer.UserControls.Option;
using Caliburn.Micro;
using MessageBox = System.Windows.Forms.MessageBox;
using Screen = Caliburn.Micro.Screen;

namespace ArchiveViewer.UserControls.Shell
{
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly HomeViewModel _homeViewModel;
        private readonly OptionViewModel _optionViewModel;

        public ShellViewModel()
        {
            
        }

        public ShellViewModel(
            HomeViewModel homeViewModel,
            OptionViewModel optionViewModel)
        {
            _homeViewModel = homeViewModel;
            _optionViewModel = optionViewModel;
            ShowHome();
        }

        public void ShowHome()
        {
            ActivateItem(_homeViewModel);
        }

        public void ShowOption()
        {
            ActivateItem(_optionViewModel);
        }

        public void Closing(CancelEventArgs eventArgs)
        {
            if (_homeViewModel.HasUnsavedChanges)
            {
                var result = MessageBox.Show("There are unsaved changes. If you continue those changes are discarded.", "Error", MessageBoxButtons.OKCancel);
                if (result == DialogResult.Cancel)
                {
                    eventArgs.Cancel = true;
                }
            }
        }
    }
}