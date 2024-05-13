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

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ModTemplate
{
    [BepInPlugin(MOD_ID, "modname", "1.0.0")]
    public class ModTemplate : BaseUnityPlugin
    {
        public const string MOD_ID = "name.id";
        public static new ManualLogSource Logger { get; private set; }
        public static RemixTemplate remix;
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
            remix = new RemixTemplate();
            Logger = base.Logger;
        }

        private bool IsInit;

        private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsInit) return;

                MachineConnector.SetRegisteredOI(MOD_ID, remix);

                IsInit = true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Mod failed to load!");
                Logger.LogError(ex);
                throw;
            }
        }
    }
}