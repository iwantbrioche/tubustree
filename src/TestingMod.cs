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

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace TestingMod
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VER)]
    public class TestingMod : BaseUnityPlugin
    {
        public const string MOD_ID = "iwantbread.testmod";
        public const string MOD_NAME = "Testin'";
        public const string MOD_VER = "1.0";
        public static new ManualLogSource Logger { get; private set; }
        public static RemixTesting remix;
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            remix = new RemixTesting();
            Logger = base.Logger;
        }

        private bool IsInit;
        private bool PostIsInit;

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsInit) return;

                MachineConnector.SetRegisteredOI(MOD_ID, remix);
                Hooks.PatchHooks();

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