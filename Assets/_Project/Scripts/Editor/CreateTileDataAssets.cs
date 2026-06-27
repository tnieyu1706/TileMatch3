using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using TileMatch3.Core.Tile;

namespace TileMatch3.Editor
{
    public static class CreateTileDataAssets
    {
        [MenuItem("Tools/TileMatch3/Create All TileData Assets")]
        public static void CreateAll()
        {
            string dataPath = "Assets/_Project/Data/Tiles";
            if (!AssetDatabase.IsValidFolder(dataPath))
            {
                string parent = "Assets/_Project/Data";
                if (!AssetDatabase.IsValidFolder(parent))
                    AssetDatabase.CreateFolder("Assets/_Project", "Data");
                AssetDatabase.CreateFolder(parent, "Tiles");
            }

            string sheetPath = "Assets/_Project/Art/Sprites/Game Sheet/fruits_sheet 1.png";
            var allSprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath);

            var fruitNames = new (string spriteName, string tileName)[]
            {
                ("fruits_sheet 1_0", "Apple"),
                ("fruits_sheet 1_1", "Lemon"),
                ("fruits_sheet 1_2", "Grape"),
                ("fruits_sheet 1_3", "Orange"),
                ("fruits_sheet 1_4", "Peach"),
                ("fruits_sheet 1_5", "Pear"),
                ("fruits_sheet 1_6", "Cherry"),
                ("fruits_sheet 1_7", "Banana"),
                ("fruits_sheet 1_8", "Lime"),
                ("fruits_sheet 1_9", "Kiwi"),
                ("fruits_sheet 1_10", "Pineapple"),
                ("fruits_sheet 1_11", "Strawberry"),
            };

            int created = 0;
            foreach (var (spriteName, tileName) in fruitNames)
            {
                Sprite sprite = Array.Find(allSprites, s => s.name == spriteName) as Sprite;
                if (sprite == null)
                {
                    Debug.LogWarning($"Sprite '{spriteName}' not found in {sheetPath}");
                    continue;
                }

                string assetPath = Path.Combine(dataPath, $"Tile_{tileName}.asset");
                if (File.Exists(assetPath))
                {
                    Debug.Log($"TileData for '{tileName}' already exists, skipping.");
                    continue;
                }

                var tileData = ScriptableObject.CreateInstance<TileData>();
                tileData.tileSprite = sprite;

                AssetDatabase.CreateAsset(tileData, assetPath);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Created {created} TileData assets in '{dataPath}'");
        }
    }
}
