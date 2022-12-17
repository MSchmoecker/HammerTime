using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HammerTime.Compatibility;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HammerTime {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "HammerTime";
        public const string ModGuid = "com.maxsch.valheim.HammerTime";
        public const string ModVersion = "0.3.1";

        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }
        private Harmony harmony;

        private static Dictionary<string, PieceTable> pieceTables;
        public static Dictionary<int, string> categoryIdToName;
        private static Dictionary<string, List<PieceItem>> pieces;
        private static readonly List<ItemDrop> ToolItems = new List<ItemDrop>();

        private void Awake() {
            Instance = this;
            Log = Logger;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            ModQuery.Enable();
            HammerTime.Config.InitBaseConfig();
        }

        private void Start() {
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.auga")) {
                harmony.PatchAll(typeof(AugaCompat));
            }

            if (Chainloader.PluginInfos.ContainsKey("WackyMole.WackysDatabase")) {
                harmony.PatchAll(typeof(Patches.WackyDBPatch));
            }

            if (Chainloader.PluginInfos.ContainsKey("marcopogo.PlanBuild")) {
                harmony.PatchAll(typeof(Patches.PlanBuildPatch));
            } else {
                harmony.PatchAll(typeof(Patches.ObjectDBPatch));
            }
        }

        public static void IndexPrefabs() {
            if (SceneManager.GetActiveScene().name != "main") {
                return;
            }

            pieceTables = Helper.GetPieceTables();
            categoryIdToName = Helper.GetCategories();
            pieces = new Dictionary<string, List<PieceItem>>();

            foreach (KeyValuePair<string, PieceTable> table in pieceTables) {
                if (Helper.IsVanillaPieceTable(table.Key) && table.Key != "_HammerPieceTable") {
                    continue;
                }

                pieces.Add(table.Key, new List<PieceItem>());

                foreach (GameObject pieceGameObject in table.Value.m_pieces) {
                    IModPrefab modPrefab = ModQuery.GetPrefab(pieceGameObject.name);

                    if (modPrefab == null || !modPrefab.Prefab) {
                        Piece piece = pieceGameObject.GetComponent<Piece>();
                        pieces[table.Key].Add(new PieceItem(pieceGameObject, piece, "Vanilla", string.Empty));
                    } else {
                        string mod = modPrefab.SourceMod.Name;
                        Piece piece = modPrefab.Prefab.GetComponent<Piece>();
                        pieces[table.Key].Add(new PieceItem(pieceGameObject, piece, mod, string.Empty));
                    }
                }
            }

            Cookie.IndexPrefabs(pieces);

            UpdateAllPieceTables();
            IndexToolItems();
            UpdateDisabledRecipes();
        }

        public static void UpdateAllPieceTables() {
            foreach (string pieceTable in pieces.Keys) {
                UpdatePieceTable(pieceTable);
            }
        }

        private static void IndexToolItems() {
            foreach (GameObject item in ObjectDB.instance.m_items) {
                if (!item || !item.TryGetComponent(out ItemDrop itemDrop) || itemDrop.m_itemData?.m_shared == null) {
                    continue;
                }

                PieceTable pieceTable = itemDrop.m_itemData.m_shared.m_buildPieces;

                if (!pieceTable) {
                    continue;
                }

                Recipe recipe = ObjectDB.instance.GetRecipe(itemDrop.m_itemData);

                if (recipe && recipe.m_enabled) {
                    ToolItems.Add(itemDrop);
                }
            }
        }

        public static void UpdateDisabledRecipes() {
            foreach (ItemDrop item in ToolItems) {
                PieceTable pieceTable = item.m_itemData.m_shared.m_buildPieces;
                string pieceTableName = pieceTables.FirstOrDefault(x => x.Value == pieceTable).Key;

                if (Helper.IsVanillaPieceTable(pieceTableName)) {
                    continue;
                }

                if (string.IsNullOrEmpty(pieceTableName)) {
                    continue;
                }

                bool isHammerEnabled = HammerTime.Config.IsHammerEnabled(pieceTableName);
                bool disableRecipe = HammerTime.Config.disableRecipes.Value && isHammerEnabled;

                Recipe recipe = ObjectDB.instance.GetRecipe(item.m_itemData);
                recipe.m_enabled = !disableRecipe;
            }

            if (Player.m_localPlayer && InventoryGui.instance) {
                Player.m_localPlayer.UpdateKnownRecipesList();
                InventoryGui.instance.UpdateCraftingPanel();
            }
        }

        public static void Undo() {
            foreach (KeyValuePair<string, List<PieceItem>> pieces in pieces) {
                foreach (PieceItem pieceItem in pieces.Value) {
                    string category;

                    if (categoryIdToName.ContainsKey((int)pieceItem.originalCategory)) {
                        category = categoryIdToName[(int)pieceItem.originalCategory];
                    } else {
                        category = pieceItem.originalCategory.ToString();
                    }

                    MovePieceItemToTable(pieceItem, "_HammerPieceTable", pieces.Key, category);
                }
            }
        }

        private static void UpdatePieceTable(string pieceTable) {
            HashSet<string> potentialCategories = new HashSet<string>();
            HashSet<string> usedCategories = new HashSet<string>();

            potentialCategories.Add(" ");

            for (int i = 0; i < (int)Piece.PieceCategory.Max; i++) {
                potentialCategories.Add(Enum.GetName(typeof(Piece.PieceCategory), i));
            }

            Dictionary<string, int> pieceCountByMod = new Dictionary<string, int>();
            foreach (PieceItem pieceItem in pieces[pieceTable]) {
                if (!pieceCountByMod.ContainsKey(pieceItem.modName)) {
                    pieceCountByMod.Add(pieceItem.modName, 0);
                }

                pieceCountByMod[pieceItem.modName]++;
            }

            foreach (PieceItem pieceItem in pieces[pieceTable]) {
                bool enabled = HammerTime.Config.IsHammerEnabled(pieceTable, pieceItem.modName, () => {
                    UpdatePieceTable(pieceTable);
                    UpdateDisabledRecipes();
                });

                bool combineByDefault = pieceCountByMod[pieceItem.modName] <= 60;
                bool combine = HammerTime.Config.CombineModCategories(pieceTable, pieceItem.modName, combineByDefault, () => UpdatePieceTable(pieceTable));

                string originalCategory;

                if (string.IsNullOrEmpty(pieceItem.overrideCategory)) {
                    originalCategory = categoryIdToName[(int)pieceItem.originalCategory];
                } else {
                    originalCategory = pieceItem.overrideCategory;
                }

                string category;
                string categoryCombined = HammerTime.Config.GetCombinedCategoryName(pieceTable, pieceItem.modName, () => UpdatePieceTable(pieceTable));
                string categoryUnCombined = HammerTime.Config.GetCategoryName(pieceTable, pieceItem.modName, originalCategory, () => UpdatePieceTable(pieceTable));

                potentialCategories.Add(categoryIdToName[(int)pieceItem.originalCategory]);
                potentialCategories.Add(categoryCombined);
                potentialCategories.Add(categoryUnCombined);

                if (!enabled) {
                    category = categoryIdToName[(int)pieceItem.originalCategory];
                    usedCategories.Add(category);
                    MovePieceItemToTable(pieceItem, "_HammerPieceTable", pieceTable, category);
                    continue;
                }

                category = combine ? categoryCombined : categoryUnCombined;

                if (string.IsNullOrEmpty(category)) {
                    category = " ";
                }

                usedCategories.Add(category);
                MovePieceItemToTable(pieceItem, pieceTable, "_HammerPieceTable", category);
            }

            foreach (string category in potentialCategories) {
                if (!usedCategories.Contains(category)) {
                    PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", category);
                }
            }
        }

        private static void MovePieceItemToTable(PieceItem pieceItem, string pieceTableFrom, string pieceTableTo, string category) {
            GameObject gameObject = pieceItem.gameObject;

            if (pieceTableFrom != pieceTableTo && pieceTables.TryGetValue(pieceTableFrom, out PieceTable tableFrom)) {
                tableFrom.m_pieces.Remove(gameObject);
            }

            if (pieceTables.TryGetValue(pieceTableTo, out PieceTable table)) {
                if (pieceItem.originalCategory != Piece.PieceCategory.All) {
                    pieceItem.piece.m_category = PieceManager.Instance.AddPieceCategory(pieceTableTo, category);
                }

                int nameHash = pieceItem.nameHash;
                bool hasPiece = table.m_pieces.Contains(gameObject) ||
                                pieceItem.originalCategory == Piece.PieceCategory.All && table.m_pieces.Any(i => i.name.GetStableHashCode() == nameHash);

                if (!hasPiece) {
                    table.m_pieces.Add(gameObject);
                }
            }
        }
    }
}
