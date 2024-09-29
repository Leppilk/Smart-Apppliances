using KitchenLib;
using KitchenLib.Logging;
using KitchenLib.Logging.Exceptions;
using KitchenMods;
using System.Linq;
using System.Reflection;
using SmartAppliances.Customs.Appliances;
using UnityEngine;
using KitchenLogger = KitchenLib.Logging.KitchenLogger;

namespace SmartAppliances
{
    public class Mod : BaseMod, IModSystem
    {
        public const string MOD_GUID = "com.pedrocosta.smartappliances";
        public const string MOD_NAME = "Smart Appliances";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Pedro Costa";
        public const string MOD_GAMEVERSION = ">=1.1.9";

        internal static AssetBundle Bundle;
        internal static KitchenLogger Logger;

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            Logger.LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).FirstOrDefault() ?? throw new MissingAssetBundleException(MOD_GUID);
            Logger = InitLogger();

            AddGameDataObject<SmartMicrowave>();
        }
    }
}