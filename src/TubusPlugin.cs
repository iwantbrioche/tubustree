global using System;
global using System.Linq;
global using System.Text.RegularExpressions;
global using System.Collections.Generic;
global using System.Reflection;
global using BepInEx;
global using MoreSlugcats;
global using Mono.Cecil.Cil;
global using MonoMod.Cil;
global using UnityEngine;
global using RWCustom;
global using Debug = UnityEngine.Debug;
global using Random = UnityEngine.Random;
global using Vector2 = UnityEngine.Vector2;
global using Color = UnityEngine.Color;
global using Custom = RWCustom.Custom;
using System.Security;
using System.Security.Permissions;
using BepInEx.Logging;
using System.Runtime.CompilerServices;
using Tubus.PomObjects;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace TubusTreeObject
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VER)]
    public class TubusPlugin : BaseUnityPlugin
    {
        public const string MOD_ID = "iwantbread.tubus";
        public const string MOD_NAME = "Tubus Tree";
        public const string MOD_VER = "1.0";
        public static new ManualLogSource Logger { get; private set; }
        //public static TemplateRemix remix;
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            //remix = new TemplateRemix();
            Logger = base.Logger;
        }

        private bool IsInit;
        private bool PostIsInit;

        public static FShader TubusTrunk;

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsInit) return;

                //MachineConnector.SetRegisteredOI(MOD_ID, remix);
                Hooks.Hooks.PatchAll();

                AssetBundle bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assets/tubus"));
                Shader trunkShader = bundle.LoadAsset<Shader>("Assets/tubustrunk.shader");
                TubusTrunk = FShader.CreateShader(trunkShader.name, trunkShader);

                if (TubusTrunk == null) Logger.LogError("TubusTrunk Shader is null!");
                else Logger.LogInfo("TubusTrunk Shader loaded!");

                IsInit = true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"{MOD_NAME} failed to load!");
                Logger.LogError(ex);
                throw;
            }
        }

        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (PostIsInit) return;

                RegisterObjects.RegisterPOMObjects();

                PostIsInit = true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"{MOD_NAME} PostModsInit failed to load!");
                Logger.LogError(ex);
                throw;
            }
        }

    }
}