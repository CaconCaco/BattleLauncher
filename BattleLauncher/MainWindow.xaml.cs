﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using BattleLauncher.Controls;
using BattleLauncher.Data.Model;
using BattleLauncher.Exceptions;
using BattleLauncher.Extensions;

using static BattleLauncher.Data.OverAll;

namespace BattleLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Mutex _dialogMutex = new Mutex();

        private async void ApplySetting(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = e.Parameter as ViewModels.ConfigsViewModel;
            var model = ConfigsViewModelExtension.ToModel(vm);

            Data.DDRAWUtils.CleanAll();//rm old ones.
            switch (vm.Renderer.Name)
            {
                case "NONE":
                    goto default;
                case "CNCDDRAW":
                    Data.DDRAWUtils.Apply(vm.Renderer.Directory);
                    await GameConfigExtensions.WriteCNCDDRAWConfig(model);
                    model.Borderless = model.IsWindowMode = false;
                    goto default;
                case "DDWRAPPER":
                    Data.DDRAWUtils.Apply(vm.Renderer.Directory);
                    goto default;
                case "DXWND":
                    Data.DDRAWUtils.Apply(vm.Renderer.Directory);
                    await GameConfigExtensions.WriteDxWndConfig(model);
                    model.Borderless = model.IsWindowMode = false;
                    goto default;
                case "TSDDRAW":
                    Data.DDRAWUtils.Apply(vm.Renderer.Directory);
                    goto default;
                case "IEDDRAW":
                    Data.DDRAWUtils.Apply(vm.Renderer.Directory);
                    goto default;
                case "COMPAT":
                    Data.DDRAWUtils.Apply(vm.Renderer.Directory);
                    goto default;
                default:
                    await GameConfigExtensions.WriteConfig(model);
                    break;
            }
            Commands.DialogRoutedCommands.CloseCommand.Execute(null, e.Source as IInputElement);
        }

        private void CanClearCommandLine(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = !string.IsNullOrEmpty((e.Source as TextBox)?.Text);

        private void ClearCommandLine(object sender, ExecutedRoutedEventArgs e) => (e.Source as TextBox).Text = string.Empty;

        private void CloseDialog(object sender, ExecutedRoutedEventArgs e) => (e.Source as GDialog).Hide();

        private async void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            switch (e.Exception)
            {
                case SpawnerInvalidException _:
                    await MessageBox.Show("txtSpawnerInvalidError".I18N(), "txtCaptain".I18N());
                    break;
                case AresNotFoundException _:
                    await MessageBox.Show("txtAresNotFoundError".I18N(), "txtCaptain".I18N());
                    break;
                case NoSaveLoadedException _:
                    await MessageBox.Show("txtNoSaveLoadedError".I18N(), "txtCaptain".I18N());
                    break;
                case DirectoryNotFoundException ex:
                    await MessageBox.Show(ex.Message, "txtError".I18N());
                    break;
                case IOException ex:
                    await MessageBox.Show(ex.Message, "txtError".I18N());
                    break;
                default:
                    e.Handled = false;
                    await MessageBox.Show(e.Exception.Message, "txtFatal".I18N());
                    File.WriteAllText($"LauncherExcept.{DateTime.Now.ToString("o").Replace('/', '-').Replace(':', '-')}.log", e.Exception.ToString());
                    break;
            }
        }

        private void Exit(object sender, ExecutedRoutedEventArgs e) => Close();

        private async void LoadGame(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is ViewModels.SavedGameViewModel vm))
                throw new NoSaveLoadedException();

            using (var fs = SpawnIni.Open(FileMode.Create))
                await vm.WriteSpawnAsync(new StreamWriter(fs));

            Commands.MainWindowRoutedCommands.RunGameCommand.Execute(null, this);
        }

        private async void OpenArchiveLoader(object sender, ExecutedRoutedEventArgs e) => await ShowDialog(new SaveLoaderDialog());

        private async void OpenGameSettings(object sender, ExecutedRoutedEventArgs e) => await ShowDialog(new UserInterface());

        private void RunGame(object sender, ExecutedRoutedEventArgs e)
        {
            new GameExecuteOptions
            {
                LogMode = cbDebug_Check.IsChecked ?? false,
                RunAs = cbAdmin_Check.IsChecked ?? false,
                Others = tbCommand.Text.Split(' ')
            }.RunGame();
            Close();
        }

        public MainWindow()
        {
            MessageBox.MainWindow = this;
            InitializeComponent();
            this.I18NInitialize();// I18N 初始化

#if DEBUG
            tbCommand.Text = "-speedcontrol";
#endif

            cbAdmin_Check.IsEnabled = !(bool)(cbAdmin_Check.IsChecked = IsAdministrator());

            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }
        private async Task<GDialogResult> ShowDialog(GDialog dialog)
        {
            var result = GDialogResult.FaildOpen;
            if (_dialogMutex.WaitOne(500))
            {
                try
                {
                    var pause = new ManualResetEvent(false);
                    dialogMask.Child = dialog;
                    dialogMask.Visibility = Visibility.Visible;
                    dialog.Show(pause);
                    await Task.Run(pause.WaitOne);
                    dialogMask.Visibility = Visibility.Collapsed;
                    dialogMask.Child = null;
                    result = dialog.Result;
                }
                finally
                {
                    _dialogMutex.ReleaseMutex();
                }
            }
            return result;
        }

        public async Task ShowMessageDialog(GDialog dialog)
        {
            var pause = new ManualResetEvent(false);
            msgDialogMask.Child = dialog;
            msgDialogMask.Visibility = Visibility.Visible;
            dialog.Show(pause);
            await Task.Run(pause.WaitOne);
            msgDialogMask.Visibility = Visibility.Collapsed;
            msgDialogMask.Child = null;
        }
    }
}
