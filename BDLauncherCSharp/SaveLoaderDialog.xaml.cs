﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

using BDLauncherCSharp.Controls;
using BDLauncherCSharp.Data;
using BDLauncherCSharp.Extensions;

namespace BDLauncherCSharp
{
    /// <summary>
    /// SaveLoaderDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SaveLoaderDialog : GDialog
    {
        public SaveLoaderDialog()
        {
            InitializeComponent();
            this.I18NInitialize();
            SaveList.ItemsSource = new ObservableCollection<SavedGameInfo>(GetSavedGameInfoList(OverAll.SavedGameDirectory));
        }

        private static IEnumerable<SavedGameInfo> GetSavedGameInfoList(DirectoryInfo dir)
            => dir.GetFiles("*.sav").Select(SavedGameExtension.GetSavedGameInfo);

        protected override void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            var phi = SaveList.SelectedItem;
            if (phi is SavedGameInfo ksi)
            {
                var k = ksi.RealFile.Name;
                StreamWriter sw = new StreamWriter(OverAll.SpawnIni, false, Encoding.Default);
                sw.WriteLine(";generated by Singleplayer Campaign Launcher");
                sw.WriteLine("[Settings]");
                sw.WriteLine("Scenario=spawnmap.ini");
                sw.WriteLine("SaveGameName=" + k);
                sw.WriteLine("LoadSaveGame=Yes");
                sw.WriteLine("SidebarHack=False");
                sw.WriteLine("Firestorm=No");
                sw.WriteLine("GameSpeed=2");
                sw.Flush();
                base.PrimaryButton_Click(sender, e);
            }
            else
            {
                MessageBox.Show(I18NExtension.I18N("msgNoSaveLoadedError"), I18NExtension.I18N("msgCaptain"));
            }
        }
    }
}