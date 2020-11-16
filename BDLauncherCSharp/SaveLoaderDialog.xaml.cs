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
            PrimaryButtonClick += GDialog_PrimaryButtonClick;
            this.I18NInitialize();
            this.SaveList.ItemsSource = new ObservableCollection<SavedGameInfo>(GetSavedGameInfoList(OverAll.SavedGameDirectory));
        }

        private static IEnumerable<Data.SavedGameInfo> GetSavedGameInfoList(DirectoryInfo dir)
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
            }
            else MessageBox.Show("无法载入，因为没有选中任何存档。", "「脑死」启动器: 运行时错误");
            base.PrimaryButton_Click(sender, e);
        }

        private void GDialog_PrimaryButtonClick(object sender, RoutedEventArgs e)
        {
            CriticalPEIdentify.SpawnerHash(OverAll.MainPath);
            if (!CriticalPEIdentify.IsBDFilelist) MessageBox.Show("无法加载「脑死」文件列表！", "「脑死」启动器");
            else if (!CriticalPEIdentify.IsThereAres)
                MessageBox.Show("此任务需要 Ares 扩展平台支持。\n\n请检查您的游戏文件是否含 Ares.dll 和 Syringe.exe。\n如找不到，建议重新下载安装。", "「脑死」启动器");
            else
            {
                var ita = new MainWindow();
                var option = new GameExecuteOptions
                {
                    LogMode = ita.Debug_Check.IsChecked ?? false,
                    RunAs = ita.Admin_Check.IsChecked ?? false,
                    Others = ita.TB_Command.Text.Split(' ')
                };
                GameExecute.RunGame(option);
                Environment.Exit(0);
            }
        }
    }
}