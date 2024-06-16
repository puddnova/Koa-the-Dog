using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.GameData.Pets;
using xTile;

namespace Koa
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        //private ModConfig Config = null!; // set in Entry
        private Dictionary<string, List<PetBreed>> ModBreedsDict = null!; // set in Entry
        private Dictionary<string, string> ModTextureMapping = null!; // set in Entry
        private Dictionary<string, Texture2D> ModTextures = new();

        /*********
        ** Accessors
        *********/
        private IModHelper APIHelper = null!; // set in Entry
        private IMonitor APIMonitor = null!; // set in Entry

        private Texture2D Doggy = null!;
        private Texture2D DoggyIcon = null!;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        /// 
        public override void Entry(IModHelper helper)
        {
            // Registering SMAPI Accessors
            APIHelper = helper;
            APIMonitor = this.Monitor;

            ModBreedsDict = helper.ModContent.Load<Dictionary<string, List<PetBreed>>> ("assets/json/breedsDict.json");
            ModTextureMapping = helper.ModContent.Load<Dictionary<string, string>>("assets/json/textureMapping.json");

            //APIMonitor.Log($"Breeds in {ModBreedsDict.Keys.Count} PetTypes to be added.", LogLevel.Info);
            foreach (var pair in ModBreedsDict)
            {
                APIMonitor.Log($"Adding {pair.Value.Count} {pair.Key} Breed(s): {String.Join(", ", pair.Value.Select(b => b.Id))}", LogLevel.Info);
            }

            // Config
            //Config = this.Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Content.AssetReady += this.OnAssetReady;
        }


        /*********
        ** Private methods
        *********/

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Pets"))
            {
                e.Edit(asset =>
                    {
                        var data = asset.AsDictionary<string, PetData>().Data;
                        foreach (var item in this.ModBreedsDict)
                        { 
                            if (data.TryGetValue(item.Key, out var petData))
                            {
                                petData.Breeds.AddRange(item.Value);
                            }
                        }
                    } 
                );
            }

            if (e.NameWithoutLocale.StartsWith("Mods/Koa/Sprites") 
                || e.NameWithoutLocale.StartsWith("Mods/Koa/Icons"))
            {
                e.LoadFromModFile<Texture2D>(ModTextureMapping[e.NameWithoutLocale.BaseName], AssetLoadPriority.Medium);
            }
        }

        private void OnAssetReady(object? sender, AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.StartsWith("Mods/Koa/Sprites")
                || e.NameWithoutLocale.StartsWith("Mods/Koa/Icons"))
            {
                this.ModTextures[e.NameWithoutLocale.BaseName] = Game1.content.Load<Texture2D>(e.NameWithoutLocale.BaseName);
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            foreach (var pair in this.ModTextureMapping)
            {
                ModTextures[pair.Key] = Game1.content.Load<Texture2D>(pair.Key);
            }
        }
    }
}