using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HammerTime {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "HammerTime";
        public const string ModGuid = "com.maxsch.valheim.HammerTime";
        public const string ModVersion = "0.1.3";

        public static Plugin Instance { get; private set; }
        private Harmony harmony;

        private static Dictionary<string, PieceTable> pieceTables;
        public static Dictionary<int, string> categoryIdToName;
        private static Dictionary<string, List<PieceItem>> pieces;

        private void Awake() {
            Instance = this;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();
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
                    CustomPiece customPiece = PieceManager.Instance.GetPiece(pieceGameObject.name);

                    if (customPiece != null) {
                        string mod = customPiece.SourceMod.Name;
                        pieces[table.Key].Add(new PieceItem(pieceGameObject, customPiece.Piece, mod, mod));
                    } else {
                        Piece piece = pieceGameObject.GetComponent<Piece>();
                        pieces[table.Key].Add(new PieceItem(pieceGameObject, piece, "Unrecognized Mod", Helper.CleanTableName(table.Key)));
                    }
                }
            }

            foreach (string pieceTable in pieces.Keys) {
                UpdatePieceTable(pieceTable);
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
            bool disabled = HammerTime.Config.IsHammerDeactivated(pieceTable, pieceMap[0].modName, () => UpdatePieceTable(pieceTable));

            foreach (PieceItem pieceItem in pieceMap) {
                string category;
                string categoryUnCombined = Helper.GetCategory(pieceItem, false);
                string categoryCombined = Helper.GetCategory(pieceItem, true);

                if (disabled) {
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

            if (pieceTables[pieceTableFrom].m_pieces.Contains(gameObject)) {
                pieceTables[pieceTableFrom].m_pieces.Remove(gameObject);
            }
        }
    }
}
