using UnityEditor;
using UnityEngine;

public class WraithMaterialPostprocessor : AssetPostprocessor
{
    private const string WraithModelFileName = "/Wraith.fbx";
    private const string WraithMaterialPath = "Assets/Materials/Wraith.mat";

    private void OnPreprocessModel()
    {
        if (!assetPath.Replace('\\', '/').EndsWith(WraithModelFileName)) return;

        Material material = AssetDatabase.LoadAssetAtPath<Material>(WraithMaterialPath);
        if (material == null) return;

        ModelImporter importer = (ModelImporter)assetImporter;
        importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "MainMaterial"), material);
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "viking_uv"), material);
    }

    private void OnPostprocessModel(GameObject model)
    {
        if (!assetPath.Replace('\\', '/').EndsWith(WraithModelFileName)) return;

        Material material = AssetDatabase.LoadAssetAtPath<Material>(WraithMaterialPath);
        if (material == null) return;

        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }
            renderer.sharedMaterials = materials;
        }
    }
}
