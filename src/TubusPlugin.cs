global using System;
global using System.Linq;
global using System.Text.RegularExpressions;
global using System.Collections.Generic;
global using System.Reflection;
global using BepInEx;
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
using Tubus.Objects;
using System.IO;
using Tubus.Hooks;

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
        private const string ATLASES_DIR = "tubusAtlases";
        public static new ManualLogSource Logger { get; private set; }
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            Logger = base.Logger;
        }

        private bool IsInit;
        private bool PostIsInit;

        public static FShader TubusTrunk;
        public static FShader TubusFlower;

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsInit) return;

                Hooks.PatchAll();

                try
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assets/tubus"));
                    Shader trunkShader = bundle.LoadAsset<Shader>("Assets/TubusTrunk.shader");
                    TubusTrunk = FShader.CreateShader(trunkShader.name, trunkShader);

                    Shader flowerShader = bundle.LoadAsset<Shader>("Assets/TubusFlower.shader");
                    TubusFlower = FShader.CreateShader(flowerShader.name, flowerShader);
                }
                catch (NullReferenceException ex) 
                {
                    Logger.LogError($"{MOD_NAME} failed to load! A shader was null!");
                    Logger.LogError(ex);
                    throw;
                }

                if (TubusTrunk != null) Logger.LogInfo("TubusTrunk Shader loaded!");
                if (TubusFlower != null) Logger.LogInfo("TubusFlower Shader loaded!");

                LoadAtlases();

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

                ObjectTypes.RegisterPOMObjects();

                PostIsInit = true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"{MOD_NAME} PostModsInit failed to load!");
                Logger.LogError(ex);
                throw;
            }
        }

        private static void LoadAtlases()
        {
            string[] atlasPaths = AssetManager.ListDirectory(ATLASES_DIR);
            foreach(string filePath in atlasPaths)
            {
                if (Path.GetExtension(filePath) == ".txt")
                {
                    string atlasName = Path.GetFileNameWithoutExtension(filePath);
                    try
                    {
                        Logger.LogDebug($"loading {ATLASES_DIR + Path.AltDirectorySeparatorChar + atlasName} atlas!");

                        Futile.atlasManager.LoadAtlas(ATLASES_DIR + Path.AltDirectorySeparatorChar + atlasName);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error while loading {MOD_NAME} atlases!");
                        Logger.LogError(ex);
                        throw;
                    }
                }
            }

            Logger.LogInfo($"Loaded {MOD_NAME} atlases successfully!");
        }

    }
}