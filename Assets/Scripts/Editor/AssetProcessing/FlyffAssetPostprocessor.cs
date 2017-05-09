using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace uFlyff.Editor.AssetProcessing
{
    /// <summary>
    /// Internal class used to detect Flyff's proprietary file formats and interperate
    /// them into Unity's native structures. 
    /// The method of serialization is crucial to the project's usability. If something
    /// happens to be reimported, we need to minimize reference disconnections. 
    /// </summary>
    class FlyffAssetPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// Should the imported assets be relocated to their correct
        /// areas in the folder hierarchy?
        /// </summary>
        internal static bool relocateAssets = true;

        /// <summary>
        /// The file path to the prefab folder.
        /// </summary>
        internal static string prefabPath = "Assets/Prefabs/";


        /// <summary>
        /// The import path for .o3d files
        /// This is where .o3d files will be moved to if relocateAssets = true
        /// </summary>
        internal static string o3dImportPath = "Assets/Editor/Models";

        /// <summary>
        /// The output path for .o3d files
        /// When imported, .o3d files will become [Mesh] assets.
        /// </summary>
        internal static string o3dOutputPath = "Assets/Imported/Models/";


        /// <summary>
        /// The import path for .chr files
        /// This is where .chr files will be moved to if relocateAssets = true
        /// </summary>
        internal static string chrImportPath = "Assets/Editor/Avatars";
        /// <summary>
        /// The output path for .chr files
        /// When imported, .chr files will become [Avatar] assets.
        /// </summary>
        internal static string chrOutputPath = "Assets/Imported/Avatars/";


        /// <summary>
        /// The import path for .chr files
        /// This is where .ani files will be moved to if relocateAssets = true
        /// </summary>
        internal static string aniImportPath = "Assets/Editor/Animations";

        /// <summary>
        /// This is the output path for .ani files
        /// When imported, .ani files will become [AnimationClip] assets.
        /// </summary>
        internal static string aniOutputPath = "Assets/Imported/Animations/";

        /// <summary>
        /// Callback function for when an asset's status in the asset database changes.
        /// This could happen when dragging/dropping files in, clicking "Reimport", ect
        /// </summary>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            PostprocessImportedAssets(importedAssets);
            PostprocessDeletedAssets(deletedAssets);
            PostprocessMovedAssets(movedAssets, movedFromAssetPaths);
        }

        internal static void PostprocessImportedAssets(string[] importedAssets)
        {
            foreach(string asset in importedAssets)
            {
                // the file name of the asset
                // ex: mvr_female.o3d
                string fileName = Path.GetFileName(asset);
                try
                {
                    if (asset.EndsWith(".o3d"))
                    {
                        o3d o3dFile = new o3d(asset);
                        Mesh mesh = o3dFile.GetAssetObject();
                    }
                    else if (asset.EndsWith(".ani"))
                    {
                        ani aniFile = new ani(asset);
                        AnimationClip clip = aniFile.GetAssetObject();
                    }
                    else if (asset.EndsWith(".chr"))
                    {
                        chr chrFile = new chr(asset);
                        Avatar avatar = chrFile.GetAssetObject();
                    }
                }
                catch(Exception e)
                {
                    OnFailureToProcessAsset(asset, e);
                }
            }
        }

        internal static void PostprocessDeletedAssets(string[] deletedAssets)
        {

        }

        internal static void PostprocessMovedAssets(string[] movedAssets, string[] movedFromAssetPaths)
        {

        }

        /// <summary>
        /// Callback function for when an asset failed to import and threw an error
        /// during it's Load() function.
        /// The exception is passed in as an extra parameter to help with debugging
        /// </summary>
        internal static void OnFailureToProcessAsset(string asset, Exception exception)
        {
            Debug.LogError(string.Format("{0} failed to load while importing [{1}]", asset, exception.Message));
        }

        /// <summary>
        /// Moves an asset from one folder to the other.
        /// </summary>
        internal static void RelocateAsset(string path, string newPath)
        {
            // ValidateMoveAsset returns an empty string if it validates the move? wat
            string validation = AssetDatabase.ValidateMoveAsset(path, newPath);

            if (validation.Equals(string.Empty))           
                AssetDatabase.MoveAsset(path, newPath);
            else
                Debug.LogError(string.Format("Asset Relocation Error: {0}", validation));
        }
    }
}
