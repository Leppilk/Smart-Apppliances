using Kitchen;
using KitchenData;
using KitchenLib;
using KitchenLib.Customs;
using KitchenLib.Logging.Exceptions;
using KitchenMods;
using KitchenSmartAppliances2.Customs.Appliances;
using PreferenceSystem;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Entities;
using UnityEngine;
using static KitchenSmartAppliances2.ModMW;
using KitchenLogger = KitchenLib.Logging.KitchenLogger;




namespace KitchenSmartAppliances2
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]

    /*
     * This is the main class for your mod.
     *
     * This class should inherit from BaseMod and implement IModSystem.
     * BaseMod is a class from KitchenLib that contains all of the base functionality for your mod.
     * IModSystem is an interface from KitchenMods that ensures that your mod is also loaded as an ECS system.
     */
    public class ModSystem : GenericSystemBase, IModSystem
    {
        protected override void Initialise()
        {
            base.Initialise();
            RequireSingletonForUpdate<SModIsEnabled>();
        }

        protected override void OnUpdate()
        {
            if (Has<SModIsEnabled>()) { }

        }
    }

    public class ModMW : BaseMod, IModSystem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogInfo(string message)
        {
            Debug.Log("[Smart Appliances] " + message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogWarning(string message)
        {
            Debug.LogWarning("[Smart Appliances] " + message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError(string message)
        {
            Debug.LogError("[Smart Appliances] " + message);
        }

        /*
         * This is information related to your mod.
         *
         * GUID: A unique identifier for your mod. This should be unique to your mod. Once you set it, do not change it.
         * NAME: The name of your mod. This is what will be displayed in the mod manager.
         * VERSION: The version of your mod. This is what will be displayed in the mod manager.
         * AUTHOR: Your name.
         * GAMEVERSION: The version of the game that your mod is compatible with. This is uses Semantic Versioning which can be found here: https://semver.org/
         */
        public const string MOD_GUID = "com.pedrocosta.smartappliances2";
        public const string MOD_NAME = "SmartAppliances2";
        public const string MOD_VERSION = "0.0.1";
        public const string MOD_AUTHOR = "Pedro Costa";
        public const string MOD_GAMEVERSION = ">=1.1.9";

        /*
         * These are static variables that will be used throughout your mod.
         * Bundle: This is the asset bundle that contains all of your mod's assets.
         * Logger: This is the logger that you will use to log information to the console.
         */
        internal static AssetBundle Bundle;
        internal static KitchenLogger Logger;
        internal static PreferenceSystemManager PrefManager;

        /*
         * This is the constructor for your mod. This is where you will set the GUID, NAME, VERSION, AUTHOR, and GAMEVERSION for your mod.
         */
        public ModMW() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        /*
         * This is the OnInitialise method. This is called when the user loads into the lobby.
         */

        protected override void OnInitialise()
        {
    
        }

        /*
         * This is the OnUpdate method. This is called every frame after OnInitialise().
         */
        public struct SModIsEnabled : IComponentData, IModComponent { }
        protected override void OnUpdate()
        {


        }


        /*
         * This is the OnPostActivate method. This is called after the mod is activated.
         */

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            // This is where you will load your asset bundle and initialise your logger.
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).FirstOrDefault() ?? throw new MissingAssetBundleException(MOD_GUID);
            Logger = InitLogger();
            Logger.LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");

            
            /*
             * This is where you will add your custom GameDataObjects.
             *
             * You can also add IAutoRegsiterAll to your base class to automatically register all GameDataObjects in the assembly.
             */
            AddGameDataObject<SmartMicrowave>();
            PostActivate();
        }
        public void PostActivate()
        {

        }


    }
}