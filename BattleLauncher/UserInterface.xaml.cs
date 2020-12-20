﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using BattleLauncher.Controls;
using BattleLauncher.Extensions;

using static BattleLauncher.Data.OverAll;
//using BDLauncherCSharp.ViewModels;

namespace BattleLauncher
{
    /// <summary>
    /// UserInterface.xaml 的交互逻辑
    /// </summary>
    public partial class UserInterface : GDialog
    {
        public UserInterface()
        {
            InitializeComponent();
            this.I18NInitialize();
            _ = InitView();
        }

        private async Task InitView()
        {
            var vm = new ViewModels.ConfigsViewModel(await ConfigureIO.GetConfigure());
            DataContext = vm;
            cbRenderer.SelectedIndex = 0;
        }

        private void Difficulty_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => (sender as Slider).Value = Math.Ceiling((sender as Slider).Value);

        private void cbRenderer_Apply(object sender, RoutedEventArgs e)
        {
            DDRAWDLL.Refresh();
            var rmCD = new DirectoryInfo(WorkDir.FullName).GetFiles("ddraw.*");
            var rmRD = new DirectoryInfo(Path.Combine(WorkDir.FullName, "Renderer")).GetFiles("cnc-ddraw.*");
            if ((string)cbRenderer.SelectedItem == I18NExtension.I18N("cbRenderer.None") && IsCNCDDraw)
            {
                foreach (var file in rmCD) file.Delete();
                return;
            }
            if ((string)cbRenderer.SelectedItem == I18NExtension.I18N("cbRenderer.CNCDDraw") && IsCNCDDraw) return;
            if ((string)cbRenderer.SelectedItem == I18NExtension.I18N("cbRenderer.None") && !IsCNCDDraw) return;
            if ((string)cbRenderer.SelectedItem == I18NExtension.I18N("cbRenderer.CNCDDraw") && !IsCNCDDraw)
            {
                foreach (var source in rmRD)
                {
                    var filetype = Path.GetExtension(source.FullName);
                    source.CopyTo(Path.Combine(WorkDir.FullName, "ddraw" + filetype), true);
                }
                return;
            }
        }
    }
}