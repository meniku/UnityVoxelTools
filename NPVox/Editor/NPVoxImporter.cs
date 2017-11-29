/*
 * NPVox Importer 2016 By nilspferd.net
 */
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class NPVoxImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool importedSomething = false;
        // import models
        foreach (var asset in importedAssets)
        {
            if (!File.Exists(asset))
            {
                continue;
            }
            // string uuid = AssetDatabase.AssetPathToGUID(asset);

            string extension = Path.GetExtension(asset);
            if (extension == ".vox")
            {
                 if( ImportVoxModel(asset) ) 
                 {
                     importedSomething = true;
                 }
            }
        }

        if( importedSomething )
        {
            // AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private static bool ImportVoxModel(string asset)
    {
        string filename = Path.GetFileNameWithoutExtension(asset);
        string basename = Path.GetDirectoryName(asset);
        EnsureDirectoriesExist(basename);
        string pipelinePath = Path.Combine(Path.Combine(basename, "Pipeline/"), filename + ".asset");
        NPipeContainer pipeContainer = NPipelineUtils.GetContainerForVoxPath(asset);

        if (!pipeContainer)
        {
            if (File.Exists(pipelinePath))
            {
                // We don't need this anymore, as assets are reimported by the NPipeImporter anyway
                // AssetDatabase.ImportAsset(asset, ImportAssetOptions.Default);
//                Debug.Log("Did not create Pipeline for asset '" + asset + "' due to pipeline not yet ready (no problem as will get imported by the NPipeContainer anyway)");
                return false;
            }

            Debug.Log("Creating Pipeline for Voxmodel: " + asset);
            
            NPipeContainer template;
            bool unavailable;
            NPVoxUtils.LoadTemplateMetadata(out template, out unavailable);
            if (template == null)
            {
                if (!unavailable)
                {
                    // We don't need this anymore, as assets are reimported by the NPipeImporter anyway
                    // AssetDatabase.ImportAsset(asset, ImportAssetOptions.Default);
                    // Debug.Log("Delay import of '" + asset + "' due to template not yet ready");
                    Debug.Log("did not import '" + asset + "' due to template not yet ready");
                }
                return false;
            }
            
            pipeContainer = NPipelineUtils.ClonePipeContainer(template, pipelinePath);
        }

        NPipeIImportable[] importables = NPipelineUtils.GetImportables(pipeContainer);
        // NPipeIImportable[] outputPipes = NPipelineUtils.FindOutputPipes(importables);

        foreach (NPipeIImportable importable in importables)
        {
            if(importable is NPVoxMagickaSource)
            {
                ((NPVoxMagickaSource)importable).VoxModelUUID = AssetDatabase.AssetPathToGUID(asset);
            }
            
            importable.Invalidate();
            EditorUtility.SetDirty(importable as UnityEngine.Object );
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(pipelinePath, ImportAssetOptions.ForceSynchronousImport); // try ForceSynchronousImport
        return true;
    }

    private static void EnsureDirectoriesExist(string basename)
    {
        // NPVoxMetadata template = GetTemplateMetadata();
        // if (!template)
        // {
        //     return;
        // }
        // if (!File.Exists(Path.Combine(basename, template.Template_GeneratedSubfolder)))
        // {
        //     Directory.CreateDirectory(Path.Combine(basename, template.Template_GeneratedSubfolder));
        // }

        if (!File.Exists(Path.Combine(basename, "Pipeline/")))
        {
            Directory.CreateDirectory(Path.Combine(basename, "Pipeline/"));
        }
    }

}
