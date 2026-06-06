#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Model.AutoSpriteAtlas
{
    /// <summary>
    /// 将 Sprites 下的一级子目录自动打包为 Atlas 目录中的 SpriteAtlas。
    /// 例：Sprites/main/*.png -> Atlas/main.spriteatlas
    /// </summary>
    internal static class AutoSpriteAtlas
    {
        // 图片资源目录
        internal const string SpritesRoot = "Assets/Model/AutoSpriteAtlas/Sprites";
        
        // 图集资源目录
        private const string AtlasRoot = "Assets/Model/AutoSpriteAtlas/Atlas";

        // 图片扩展名

        private static readonly string[] ImageExtensions =
        {
            ".png", ".jpg", ".jpeg", ".tga", ".psd", ".gif", ".bmp", ".tif", ".tiff", ".webp"
        };

        private static bool s_IsProcessing;
        private static readonly HashSet<string> s_PendingCategories = new HashSet<string>();

        [MenuItem("Tools/Auto Sprite Atlas/Refresh All")]
        private static void RefreshAll()
        {
            ProcessCategories(GetAllCategories());
            CleanupOrphanAtlases(GetAllCategories());
            AssetDatabase.SaveAssets();
        }

        internal static void QueueCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return;

            s_PendingCategories.Add(category);
            if (s_IsProcessing)
                return;

            s_IsProcessing = true;
            EditorApplication.delayCall += ProcessPendingCategories;
        }

        private static void ProcessPendingCategories()
        {
            EditorApplication.delayCall -= ProcessPendingCategories;

            var categories = new HashSet<string>(s_PendingCategories);
            s_PendingCategories.Clear();

            ProcessCategories(categories);
            CleanupOrphanAtlases(GetAllCategories());

            s_IsProcessing = false;

            if (s_PendingCategories.Count > 0)
            {
                s_IsProcessing = true;
                EditorApplication.delayCall += ProcessPendingCategories;
            }
        }

        private static void ProcessCategories(IEnumerable<string> categories)
        {
            foreach (var category in categories)
                UpdateAtlasForCategory(category);

            AssetDatabase.SaveAssets();
        }

        private static IEnumerable<string> GetAllCategories()
        {
            var spritesRoot = ToAbsolutePath(SpritesRoot);
            if (!Directory.Exists(spritesRoot))
                yield break;

            foreach (var directory in Directory.GetDirectories(spritesRoot))
            {
                var category = Path.GetFileName(directory);
                if (!string.IsNullOrEmpty(category))
                    yield return category;
            }
        }

        private static void UpdateAtlasForCategory(string category)
        {
            var folderPath = $"{SpritesRoot}/{category}";
            var atlasPath = $"{AtlasRoot}/{category}.spriteatlas";

            if (!Directory.Exists(ToAbsolutePath(folderPath)) || !HasImageFiles(folderPath))
            {
                if (File.Exists(ToAbsolutePath(atlasPath)))
                    AssetDatabase.DeleteAsset(atlasPath);
                return;
            }

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                ApplyDefaultSettings(atlas);
                AssetDatabase.CreateAsset(atlas, atlasPath);
            }

            var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
            if (folder == null)
                return;

            var packables = atlas.GetPackables();
            if (packables.Length == 1 && packables[0] == folder)
                return;

            foreach (var packable in packables)
                atlas.Remove(new[] { packable });

            atlas.Add(new Object[] { folder });
            EditorUtility.SetDirty(atlas);
        }

        private static void ApplyDefaultSettings(SpriteAtlas atlas)
        {
            atlas.SetPackingSettings(new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 4
            });

            atlas.SetTextureSettings(new SpriteAtlasTextureSettings
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear
            });

            atlas.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                name = "DefaultTexturePlatform",
                maxTextureSize = 2048,
                format = TextureImporterFormat.Automatic,
                textureCompression = TextureImporterCompression.Compressed
            });
        }

        private static void CleanupOrphanAtlases(IEnumerable<string> validCategories)
        {
            var validSet = new HashSet<string>(validCategories);
            var atlasRoot = ToAbsolutePath(AtlasRoot);
            if (!Directory.Exists(atlasRoot))
                return;

            foreach (var file in Directory.GetFiles(atlasRoot, "*.spriteatlas"))
            {
                var category = Path.GetFileNameWithoutExtension(file);
                if (!validSet.Contains(category) || !HasImageFiles($"{SpritesRoot}/{category}"))
                    AssetDatabase.DeleteAsset($"{AtlasRoot}/{category}.spriteatlas");
            }
        }

        private static bool HasImageFiles(string folderPath)
        {
            var absolutePath = ToAbsolutePath(folderPath);
            if (!Directory.Exists(absolutePath))
                return false;

            return Directory
                .EnumerateFiles(absolutePath, "*.*", SearchOption.AllDirectories)
                .Any(IsImageFile);
        }

        internal static bool TryGetCategory(string assetPath, out string category)
        {
            category = null;
            if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith(SpritesRoot + "/"))
                return false;

            var relative = assetPath.Substring(SpritesRoot.Length + 1);
            var slashIndex = relative.IndexOf('/');
            if (slashIndex <= 0)
                return false;

            category = relative.Substring(0, slashIndex);
            return !string.IsNullOrEmpty(category);
        }

        private static bool IsImageFile(string path)
        {
            var extension = Path.GetExtension(path);
            return !string.IsNullOrEmpty(extension) &&
                   ImageExtensions.Contains(extension.ToLowerInvariant());
        }

        private static string ToAbsolutePath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }
    }

    internal class AutoSpriteAtlasPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(AutoSpriteAtlas.SpritesRoot))
                return;

            var importer = (TextureImporter)assetImporter;
            if (importer.textureType == TextureImporterType.Sprite)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            QueueAffectedCategories(importedAssets);
            QueueAffectedCategories(deletedAssets);
            QueueAffectedCategories(movedAssets);
            QueueAffectedCategories(movedFromAssetPaths);
        }

        private static void QueueAffectedCategories(IEnumerable<string> assetPaths)
        {
            foreach (var assetPath in assetPaths)
            {
                if (AutoSpriteAtlas.TryGetCategory(assetPath, out var category))
                    AutoSpriteAtlas.QueueCategory(category);
            }
        }
    }
}
#endif