using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lilly
{
    public class HarmonyX : Harmony
    {

        public HarmonyX(string id) : base(id)
        {
        }

        public new void PatchAll()
        {
            MethodBase method = new StackTrace().GetFrame(1).GetMethod();
            Assembly assembly = method.ReflectedType.Assembly;
            AccessTools.GetTypesFromAssembly(assembly).Do(delegate (Type type)
            {
                PatchAll(type);
            });
        }

        public void PatchAll(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // HarmonyPatch가 메서드에 붙어 있는지 확인
                var patchAttr = method.GetCustomAttribute<HarmonyPatch>();
                if (patchAttr == null) continue;

                // HarmonyPatch에 지정된 대상 타입과 메서드 이름 추출
                var targetType = patchAttr.info?.declaringType;
                var targetMethodName = patchAttr.info?.methodName;

                if (targetType == null || string.IsNullOrEmpty(targetMethodName))
                {
                    MyLog.Error($"패치 정보 누락: <color=#FF8000FF>{method.Name}</color>");
                    continue;
                }

                var argumentTypes = patchAttr.info?.argumentTypes;
                //var argumentVariations = patchAttr.info?.argumentVariations;

                // 원본 메서드 찾기
                var original = AccessTools.Method(targetType, targetMethodName, argumentTypes);
                if (original == null)
                {
                    MyLog.Error($"원본 메서드 찾기 실패: <color=#00FF00FF>{targetType.Name}</color>.<color=#FF8000FF>{targetMethodName}</color>");
                    continue;
                }

                MyLog.Message($"패치 대상 : <color=#00FF00FF>{targetType.Name}</color>.<color=#FF8000FF>{targetMethodName}</color> <- <color=#00FF00FF>{method.DeclaringType.Name}</color>.<color=#FF8000FF>{method.Name}</color>");
                var to = new HarmonyMethod(method);
                base.Patch(original,
                    method.GetCustomAttribute<HarmonyPrefix>() == null ? null : to,
                    method.GetCustomAttribute<HarmonyPostfix>() == null ? null : to,
                    method.GetCustomAttribute<HarmonyTranspiler>() == null ? null : to,
                    method.GetCustomAttribute<HarmonyFinalizer>() == null ? null : to
                    );
                MyLog.Message($"패치 성공: <color=#00FF00FF>{targetType.Name}</color>.<color=#FF8000FF>{targetMethodName}</color> <- <color=#00FF00FF>{method.DeclaringType.Name}</color>.<color=#FF8000FF>{method.Name}</color>");
            }
        }
        public void UnPatchAll(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var patchAttr = method.GetCustomAttribute<HarmonyPatch>();
                if (patchAttr == null) continue;

                var targetType = patchAttr.info?.declaringType;
                var targetMethodName = patchAttr.info?.methodName;
                var argumentTypes = patchAttr.info?.argumentTypes;

                if (targetType == null || string.IsNullOrEmpty(targetMethodName))
                {
                    MyLog.Error($"패치 정보 누락: <color=#FF8000FF>{method.Name}</color>");
                    continue;
                }

                var original = AccessTools.Method(targetType, targetMethodName, argumentTypes);
                if (original == null)
                {
                    MyLog.Error($"원본 메서드 찾기 실패: <color=#00FF00FF>{targetType.Name}</color>.<color=#FF8000FF>{targetMethodName}</color>");
                    continue;
                }

                // 해당 메서드에 적용된 패치 중 내 Harmony ID로 된 것만 제거
                base.Unpatch(original, HarmonyPatchType.Prefix, Id);
                base.Unpatch(original, HarmonyPatchType.Postfix, Id);
                base.Unpatch(original, HarmonyPatchType.Transpiler, Id);
                base.Unpatch(original, HarmonyPatchType.Finalizer, Id);
                MyLog.Message($"패치 해제: <color=#00FF00FF>{targetType.Name}</color>.<color=#FF8000FF>{targetMethodName}</color> <- <color=#00FF00FF>{method.DeclaringType.Name}</color>.<color=#FF8000FF>{method.Name}</color>");
            }
        }

        public MethodInfo Patch(HarmonyPatchType harmonyPatchType, Type typePatch, string namePatch,Type typeOriginal, string nameOriginal, Type[] parameters = null, Type[] generics = null)
        {
            var original = AccessTools.Method(typeOriginal, nameOriginal, parameters, generics);
            var patch = AccessTools.Method(typePatch, namePatch);
            PatchProcessor patchProcessor = CreateProcessor(original);
            switch (harmonyPatchType)
            {
                case HarmonyPatchType.All:
                    patchProcessor.AddPrefix(patch);
                    patchProcessor.AddPostfix(patch);
                    patchProcessor.AddTranspiler(patch);
                    patchProcessor.AddFinalizer(patch);
                    break;
                case HarmonyPatchType.Prefix:
                    patchProcessor.AddPrefix(patch);
                    break;
                case HarmonyPatchType.Postfix:
                    patchProcessor.AddPostfix(patch);
                    break;
                case HarmonyPatchType.Transpiler:
                    patchProcessor.AddTranspiler(patch);
                    break;
                case HarmonyPatchType.Finalizer:
                    patchProcessor.AddFinalizer(patch);
                    break;
                case HarmonyPatchType.ReversePatch:
                    throw new Exception("구현 안됨");
                    //break;
                default:
                    throw new Exception("비정상 값");
                    //break;
            }
            return patchProcessor.Patch();
            //return Patch(original, prefix: new HarmonyMethod(patch));
        }

        public void UnpatchSelf()
        {
            var patches = GetPatchedMethods();
            foreach (var item in patches)
            {
                Unpatch(item, HarmonyPatchType.All, Id);
            }
        }
    }
}
