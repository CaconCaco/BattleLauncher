﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using BDLauncherCSharp.Data;
using BDLauncherCSharp.Extensions;

namespace BDLauncherCSharp.ViewModels
{
    public class ConfigsViewModel : INotifyPropertyChanged
    {
        // Lazy Load
        private static string[] _lazy_renderers = null;

        private static HashSet<string> _lazy_screenSize = null;
        private string _renderer; // = UserCustoms.DDrawApply;


        private static HashSet<string> InitScreeSizeSource()
        {
            var list = new HashSet<string>();
            for (var i = 0; Data.User32.EnumDisplaySettings(null, i, out var vDevMode); i++)
                list.Add(vDevMode.dmPelsWidth + "*" + vDevMode.dmPelsHeight);
            return list;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
                    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public byte Difficult
        {
            get => (byte)Data.GameConfigManager.Configs["[Options]"]["Difficulty"].Value; set
            {
                // 数据验证
                if (value > 2) return;
                Data.GameConfigManager.Configs["[Options]"]["Difficulty"].Value = value;
                OnPropertyChanged();
            }
        }

        public bool NoBorder
        {
            get => (bool)Data.GameConfigManager.Configs["Video"]["NoWindowFrame"].Value; set
            {
                Data.GameConfigManager.Configs["Video"]["NoWindowFrame"].Value = value;
                OnPropertyChanged();
            }
        }

        public string Renderer
        {
            get => _renderer ?? UserCustoms.CurRenderer; set
            {
                _renderer = value;
                OnPropertyChanged();
            }
        }

        public string[] Renderers_Source => _lazy_renderers ?? (_lazy_renderers = new[] { I18NExtension.I18N("cbRenderer.None"), I18NExtension.I18N("cbRenderer.CNCDDraw") });

        public string ScreenSize
        {
            get => string.Join("*",
                Data.GameConfigManager.Configs["Video"]["ScreenWidth"]?.Value ?? (Data.GameConfigManager.Configs["Video"]["ScreenWidth"].Value = Data.User32.GetSystemMetrics(0)), 
                Data.GameConfigManager.Configs["Video"]["ScreenHeight"]?.Value ?? (Data.GameConfigManager.Configs["Video"]["ScreenHeight"].Value = Data.User32.GetSystemMetrics(1)));
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                if (!value.Contains("*"))
                    return;
                else
                {
                    string[] size = value.Split('*');
                    Data.GameConfigManager.Configs["Video"]["ScreenWidth"].Value = uint.TryParse(size[0], out var width) ? width : throw new FormatException("宽度设置不正确");
                    Data.GameConfigManager.Configs["Video"]["ScreenHeight"].Value = uint.TryParse(size[1], out var height) ? height : throw new FormatException("高度设置不正确");
                    OnPropertyChanged();
                }
            }
        }
        public HashSet<string> ScreeSize_Source => _lazy_screenSize ?? (_lazy_screenSize = InitScreeSizeSource());

        public bool UseBuffer
        {
            get => (bool)Data.GameConfigManager.Configs["Video"]["VideoBackBuffer"].Value; set
            {
                Data.GameConfigManager.Configs["Video"]["VideoBackBuffer"].Value = value;
                OnPropertyChanged();
            }
        }
        public bool Windowed
        {
            get => (bool)Data.GameConfigManager.Configs["Video"]["Video.Windowed"].Value; set
            {
                if (!(bool)(Data.GameConfigManager.Configs["Video"]["Video.Windowed"].Value = value))
                    NoBorder = false;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
