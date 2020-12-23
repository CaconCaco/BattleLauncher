using System.Collections.Generic;
using System.Diagnostics;

using BattleLauncher.Data.Model;

using static BattleLauncher.Data.OverAll;

namespace BattleLauncher.Extensions
{
    /// <summary>
    /// ������Ϸ������ 
    /// </summary>
    public static class GameExecuteHelper
    {
        public static void RunGame(this GameExecuteOptions options)
        {
            var list = new List<string>
            {
                $"\"GAMEMD.EXE\"", //Syringe�Լ����ڹ���Ŀ¼�±���������ȫ·��
                "-SPAWN",
                "-CD"
            };

            var proc = new Process();
            proc.StartInfo.FileName = AresInjector.FullName;
            if (options.RunAs)
                proc.StartInfo.Verb = "runas";
            if (options.LogMode)
                list.Add("-LOG");
            list.AddRange(options.Others);
            proc.StartInfo.Arguments = string.Join(" ", list);

            proc.Start();
        }
    }
}