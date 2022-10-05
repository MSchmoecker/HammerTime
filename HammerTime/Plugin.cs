using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Jotunn.Entities;
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
        public const string ModVersion = "0.2.0";

        public static Plugin Instance { get; private set; }
        private Harmony harmony;

        private static Dictionary<string, PieceTable> pieceTables;
        public static Dictionary<int, string> categoryIdToName;
        private static Dictionary<string, List<PieceItem>> pieces;
        private static readonly List<ItemDrop> ToolItems = new List<ItemDrop>();

        private void Awake() {
            Instance = this;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            ModQuery.Enable();
            HammerTime.Config.InitBaseConfig();
        }

        private void Start() {
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.auga")) {
                harmony.PatchAll(typeof(AugaCompat));
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
                if (table.Key == "_HammerPieceTable" || table.Key == "_HoePieceTable" || table.Key == "_CultivatorPieceTable") {
                    continue;
                }

                pieces.Add(table.Key, new List<PieceItem>());

                foreach (GameObject pieceGameObject in table.Value.m_pieces) {
                    IModPrefab modPrefab = ModQuery.GetPrefab(pieceGameObject.name);

                    if (modPrefab == null || !modPrefab.Prefab) {
                        continue;
                    }

                    string mod = modPrefab.SourceMod.Name;
                    Piece piece = modPrefab.Prefab.GetComponent<Piece>();
                    pieces[table.Key].Add(new PieceItem(pieceGameObject, piece, mod, mod));
                }
            }

            foreach (string pieceTable in pieces.Keys) {
                UpdatePieceTable(pieceTable);
            }

            IndexToolItems();
            UpdateDisabledRecipes();
        }

        private static void IndexToolItems() {
            foreach (GameObject item in ObjectDB.instance.m_items) {
                if (!item || !item.TryGetComponent(out ItemDrop itemDrop)) {
                    continue;
                }

                PieceTable pieceTable = itemDrop.m_itemData.m_shared.m_buildPieces;

                if (!pieceTable) {
                    continue;
                }

                Recipe recipe = ObjectDB.instance.GetRecipe(itemDrop.m_itemData);

                if (recipe.m_enabled) {
                    ToolItems.Add(itemDrop);
                }
            }
        }

        public static void UpdateDisabledRecipes() {
            foreach (ItemDrop item in ToolItems) {
                PieceTable pieceTable = item.m_itemData.m_shared.m_buildPieces;
                string pieceTableName = pieceTables.FirstOrDefault(x => x.Value == pieceTable).Key;

                if (pieceTableName == "_HammerPieceTable" || pieceTableName == "_HoePieceTable" || pieceTableName == "_CultivatorPieceTable") {
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
            List<PieceItem> pieceMap = pieces[pieceTable];

            if (pieceMap.Count == 0) {
                return;
            }

            bool combine = HammerTime.Config.CombineModCategories(pieceTable, pieceMap[0].modName, () => UpdatePieceTable(pieceTable));
            bool enabled = HammerTime.Config.IsHammerEnabled(pieceTable, pieceMap[0].modName, () => {
                UpdatePieceTable(pieceTable);
                UpdateDisabledRecipes();
            });

            foreach (PieceItem pieceItem in pieceMap) {
                string category;
                string categoryUnCombined = Helper.GetCategory(pieceItem, false);
                string categoryCombined = Helper.GetCategory(pieceItem, true);

                if (!enabled) {
                    PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", categoryUnCombined);
                    PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", categoryCombined);

                    category = categoryIdToName[(int)pieceItem.originalCategory];
                    MovePieceItemToTable(pieceItem, "_HammerPieceTable", pieceTable, category);
                    continue;
                }

                if (combine) {
                    PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", categoryUnCombined);
                } else {
                    PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", categoryCombined);
                }

                category = Helper.GetCategory(pieceItem, combine);
                MovePieceItemToTable(pieceItem, pieceTable, "_HammerPieceTable", category);
            }
        }

        private static void MovePieceItemToTable(PieceItem pieceItem, string pieceTableFrom, string pieceTableTo, string category) {
            PieceTable table = pieceTables[pieceTableTo];
            GameObject gameObject = pieceItem.gameObject;
            int nameHash = pieceItem.nameHash;

            pieceItem.piece.m_category = PieceManager.Instance.AddPieceCategory(pieceTableTo, category);
            bool hasPiece = table.m_pieces.Contains(gameObject) ||
                            pieceItem.originalCategory == Piece.PieceCategory.All && table.m_pieces.Any(i => i.name.GetStableHashCode() == nameHash);

            if (!hasPiece) {
                table.m_pieces.Add(gameObject);
            }

            pieceTables[pieceTableFrom].m_pieces.Remove(gameObject);
        }
    }
}
