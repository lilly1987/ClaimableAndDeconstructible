using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lilly.ClaimableAndDeconstructible
{
    // 주의 - ModUI 보다 나중에 실행됨
    [StaticConstructorOnStartup]
    public class Startup
    {
        static Startup()
        {
            MyLog.Message($"ST");

            Patch.OnPatch();

            MyLog.Message($"ED");
        }
    }
}
