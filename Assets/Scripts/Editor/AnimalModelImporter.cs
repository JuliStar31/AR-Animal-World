using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ARAnimalWorld.EditorTools
{
    // Menu: AR Animal World > Siapkan Model Hewan 3D
    // Looks through the 3D model files in Assets (for example an animal pack
    // copied into Assets/ThirdParty) for a lion, an elephant and a rabbit,
    // and saves each one as Assets/Resources/Models/{modelName}.prefab, which
    // AnimalImageTargetHandler loads by name. The animal is copied out of its
    // file into plain meshes with its own materials, turned so its head faces
    // -Z, sized 1 unit and set on y = 0, like the placeholder animals.
    // A photo of each animal is saved in ModelPreviews/ to check the result.
    public static class AnimalModelImporter
    {
        private const string OutputFolder = "Assets/Resources/Models";
        private const string PartsFolder = OutputFolder + "/Parts";
        private const string PreviewFolder = "ModelPreviews";

        // Searched for in model file names and in the objects inside them.
        // "lion" also finds a lioness.
        private static readonly Dictionary<AnimalId, string[]> Keywords = new Dictionary<AnimalId, string[]>
        {
            { AnimalId.Lion, new[] { "lion", "singa" } },
            { AnimalId.Elephant, new[] { "eleph", "gajah" } },
            { AnimalId.Rabbit, new[] { "rabbit", "bunny", "kelinci" } }
        };

        private class Part
        {
            public string name;
            public Mesh source;
            public Material[] materials;
            public Vector3[] positions;
            public Vector3[] normals;
            public bool mirrored;
        }

        [MenuItem("AR Animal World/Siapkan Model Hewan 3D", priority = 1)]
        public static void PrepareModels()
        {
            EnsureFolder(PartsFolder);
            var files = FindModelFiles();
            var lines = new List<string>();

            foreach (var animal in AnimalCatalog.Animals)
            {
                if (!TryFindAnimal(animal.id, files, out string file, out int nodeIndex, out string nodeName))
                {
                    lines.Add($"{animal.displayName}: model belum ada, tetap pakai model sederhana.");
                    continue;
                }

                if (BuildPrefab(animal, file, nodeIndex))
                {
                    lines.Add($"{animal.displayName}: dari {Path.GetFileName(file)} ({nodeName}).");
                }
                else
                {
                    lines.Add($"{animal.displayName}: {nodeName} di {Path.GetFileName(file)} tidak punya mesh yang bisa dipakai.");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            int photos = SavePreviews();

            string summary = string.Join("\n", lines);
            if (photos > 0)
            {
                summary += $"\n\nFoto cek ada di folder {PreviewFolder} (di samping folder Assets).";
            }
            Debug.Log("Model hewan 3D:\n" + summary);
            EditorUtility.DisplayDialog("Model hewan 3D", summary, "OK");
        }

        public static int CountReadyModels()
        {
            return AnimalCatalog.Animals.Count(a => AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(a)) != null);
        }

        private static string PrefabPath(AnimalData animal)
        {
            return $"{OutputFolder}/{animal.modelName}.prefab";
        }

        private static List<string> FindModelFiles()
        {
            return AssetDatabase.FindAssets("t:GameObject", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !path.StartsWith(OutputFolder + "/") && AssetImporter.GetAtPath(path) is ModelImporter)
                .Distinct()
                .OrderBy(path => path)
                .ToList();
        }

        private static bool Matches(AnimalId id, string name)
        {
            string lower = name.ToLowerInvariant();
            return Keywords[id].Any(lower.Contains);
        }

        // The animal is the first object, parents before children, whose
        // name has a keyword and that has something to draw. A model file
        // named after the animal (like ELEPHANT_M.3ds) matches at its root
        // and is preferred over a pack that has the animal somewhere inside.
        private static bool TryFindAnimal(AnimalId id, List<string> files, out string file, out int nodeIndex, out string nodeName)
        {
            foreach (var path in files.OrderBy(p => Matches(id, Path.GetFileNameWithoutExtension(p)) ? 0 : 1))
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root == null)
                {
                    continue;
                }

                var nodes = root.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (Matches(id, nodes[i].name) && nodes[i].GetComponentsInChildren<Renderer>(true).Length > 0)
                    {
                        file = path;
                        nodeIndex = i;
                        nodeName = nodes[i].name;
                        return true;
                    }
                }
            }

            file = null;
            nodeIndex = -1;
            nodeName = null;
            return false;
        }

        private static bool BuildPrefab(AnimalData animal, string file, int nodeIndex)
        {
            MakeReadable(file);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(file);

            // A scene copy gives every part its world transform. The file's
            // own root rotation and scale are kept: that is where Unity puts
            // the axis conversion for some formats.
            var instance = Object.Instantiate(asset);
            instance.transform.position = Vector3.zero;
            List<Part> parts;
            try
            {
                var node = instance.GetComponentsInChildren<Transform>(true)[nodeIndex];
                parts = Bake(node);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }

            if (parts.Count == 0)
            {
                return false;
            }

            float yaw = HeadingYaw(parts);
            var turn = Quaternion.Euler(0f, yaw, 0f);

            bool any = false;
            var bounds = new Bounds();
            foreach (var part in parts)
            {
                for (int i = 0; i < part.positions.Length; i++)
                {
                    part.positions[i] = turn * part.positions[i];
                    part.normals[i] = turn * part.normals[i];
                    if (!any)
                    {
                        bounds = new Bounds(part.positions[i], Vector3.zero);
                        any = true;
                    }
                    else
                    {
                        bounds.Encapsulate(part.positions[i]);
                    }
                }
            }

            float largest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            float scale = largest > 0f ? 1f / largest : 1f;
            var offset = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

            DeleteOldParts(animal.modelName);
            var template = DefaultMaterial();
            var converted = new Dictionary<Material, Material>();
            int materialCount = 0;
            var root = new GameObject(animal.modelName);
            try
            {
                for (int p = 0; p < parts.Count; p++)
                {
                    var part = parts[p];
                    var mesh = MakeMesh(part, offset, scale);
                    mesh.name = $"{animal.modelName}_{p}";
                    AssetDatabase.CreateAsset(mesh, $"{PartsFolder}/{mesh.name}.asset");

                    var materials = new Material[part.materials.Length];
                    for (int m = 0; m < materials.Length; m++)
                    {
                        var source = part.materials[m];
                        if (source == null || !converted.TryGetValue(source, out var material))
                        {
                            material = ConvertMaterial(source, file, template, $"{PartsFolder}/{animal.modelName}_{materialCount++}.mat");
                            if (source != null)
                            {
                                converted[source] = material;
                            }
                        }
                        materials[m] = material;
                    }

                    var child = new GameObject(part.name);
                    child.transform.SetParent(root.transform, false);
                    child.AddComponent<MeshFilter>().sharedMesh = mesh;
                    child.AddComponent<MeshRenderer>().sharedMaterials = materials;
                }

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath(animal));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            Debug.Log($"{animal.displayName}: {parts.Count} bagian dari {file}, diputar {yaw:0} derajat.");
            return true;
        }

        // Mesh data is copied on the CPU, which needs Read/Write on the model.
        private static void MakeReadable(string file)
        {
            if (AssetImporter.GetAtPath(file) is ModelImporter importer && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        // Every drawn part under the node, with its vertices in world space.
        // Skinned parts are posed as they stand in the file; only the most
        // detailed level of a LOD group is used.
        private static List<Part> Bake(Transform node)
        {
            var lowerLods = new HashSet<Renderer>();
            foreach (var group in node.GetComponentsInChildren<LODGroup>(true))
            {
                var lods = group.GetLODs();
                for (int i = 1; i < lods.Length; i++)
                {
                    foreach (var r in lods[i].renderers)
                    {
                        if (r != null)
                        {
                            lowerLods.Add(r);
                        }
                    }
                }
            }

            var parts = new List<Part>();
            foreach (var renderer in node.GetComponentsInChildren<Renderer>(true))
            {
                if (lowerLods.Contains(renderer))
                {
                    continue;
                }

                var skinned = renderer as SkinnedMeshRenderer;
                Mesh mesh = null;
                if (skinned != null)
                {
                    mesh = skinned.sharedMesh;
                }
                else if (renderer is MeshRenderer)
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    mesh = filter != null ? filter.sharedMesh : null;
                }
                if (mesh == null || mesh.vertexCount == 0)
                {
                    continue;
                }

                var vertices = mesh.vertices;
                var normals = mesh.normals;
                bool hasNormals = normals.Length == vertices.Length;
                var part = new Part
                {
                    name = renderer.name,
                    source = mesh,
                    materials = renderer.sharedMaterials,
                    positions = new Vector3[vertices.Length],
                    normals = new Vector3[vertices.Length]
                };

                var weights = skinned != null ? mesh.boneWeights : null;
                var bones = skinned != null ? skinned.bones : null;
                if (weights != null && weights.Length == vertices.Length && bones.Length > 0)
                {
                    var bindposes = mesh.bindposes;
                    var matrices = new Matrix4x4[bones.Length];
                    for (int b = 0; b < bones.Length; b++)
                    {
                        matrices[b] = bones[b] != null && b < bindposes.Length
                            ? bones[b].localToWorldMatrix * bindposes[b]
                            : renderer.transform.localToWorldMatrix;
                    }

                    for (int v = 0; v < vertices.Length; v++)
                    {
                        var w = weights[v];
                        var normal = hasNormals ? normals[v] : Vector3.up;
                        Vector3 p = Vector3.zero, n = Vector3.zero;
                        Accumulate(matrices, w.boneIndex0, w.weight0, vertices[v], normal, ref p, ref n);
                        Accumulate(matrices, w.boneIndex1, w.weight1, vertices[v], normal, ref p, ref n);
                        Accumulate(matrices, w.boneIndex2, w.weight2, vertices[v], normal, ref p, ref n);
                        Accumulate(matrices, w.boneIndex3, w.weight3, vertices[v], normal, ref p, ref n);
                        part.positions[v] = p;
                        part.normals[v] = n.normalized;
                    }
                    part.mirrored = matrices[0].determinant < 0f;
                }
                else
                {
                    var matrix = renderer.transform.localToWorldMatrix;
                    var normalMatrix = matrix.inverse.transpose;
                    for (int v = 0; v < vertices.Length; v++)
                    {
                        part.positions[v] = matrix.MultiplyPoint3x4(vertices[v]);
                        part.normals[v] = hasNormals ? normalMatrix.MultiplyVector(normals[v]).normalized : Vector3.up;
                    }
                    part.mirrored = matrix.determinant < 0f;
                }

                parts.Add(part);
            }
            return parts;
        }

        private static void Accumulate(Matrix4x4[] matrices, int bone, float weight, Vector3 vertex, Vector3 normal, ref Vector3 position, ref Vector3 normalSum)
        {
            if (weight <= 0f || bone < 0 || bone >= matrices.Length)
            {
                return;
            }
            position += matrices[bone].MultiplyPoint3x4(vertex) * weight;
            normalSum += matrices[bone].MultiplyVector(normal) * weight;
        }

        // Turn about the vertical axis that points the head to -Z. The long
        // axis of the body comes from how the vertices spread over the
        // ground; the head is the end with more vertices, since faces, ears
        // and trunks are modelled in more detail than rumps and tails. This
        // picked the right end for the pack's lioness and rabbit and for
        // ELEPHANT_M.3ds, where "the higher end" would have picked the
        // elephant's rump.
        private static float HeadingYaw(List<Part> parts)
        {
            double cx = 0, cz = 0;
            int count = 0;
            foreach (var part in parts)
            {
                foreach (var p in part.positions)
                {
                    cx += p.x;
                    cz += p.z;
                    count++;
                }
            }
            cx /= count;
            cz /= count;

            double sxx = 0, szz = 0, sxz = 0;
            foreach (var part in parts)
            {
                foreach (var p in part.positions)
                {
                    double dx = p.x - cx, dz = p.z - cz;
                    sxx += dx * dx;
                    szz += dz * dz;
                    sxz += dx * dz;
                }
            }

            float angle = 0.5f * Mathf.Atan2((float)(2 * sxz), (float)(sxx - szz));
            var axis = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            float lo = float.MaxValue, hi = float.MinValue;
            foreach (var part in parts)
            {
                foreach (var p in part.positions)
                {
                    float t = (float)((p.x - cx) * axis.x + (p.z - cz) * axis.y);
                    lo = Mathf.Min(lo, t);
                    hi = Mathf.Max(hi, t);
                }
            }

            float end = 0.3f * (hi - lo);
            int back = 0, front = 0;
            foreach (var part in parts)
            {
                foreach (var p in part.positions)
                {
                    float t = (float)((p.x - cx) * axis.x + (p.z - cz) * axis.y);
                    if (t <= lo + end)
                    {
                        back++;
                    }
                    else if (t >= hi - end)
                    {
                        front++;
                    }
                }
            }
            if (back > front)
            {
                axis = -axis;
            }

            // Yaw 0 looks along +Z; turning by (180 - heading) points the
            // head at -Z, towards the child holding the phone.
            float heading = Mathf.Atan2(axis.x, axis.y) * Mathf.Rad2Deg;
            return 180f - heading;
        }

        private static Mesh MakeMesh(Part part, Vector3 offset, float scale)
        {
            var source = part.source;
            var mesh = new Mesh();
            if (part.positions.Length > 65535)
            {
                mesh.indexFormat = IndexFormat.UInt32;
            }

            var positions = new Vector3[part.positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = (part.positions[i] - offset) * scale;
            }
            mesh.vertices = positions;
            mesh.normals = part.normals;

            var uv = new List<Vector2>();
            source.GetUVs(0, uv);
            if (uv.Count == positions.Length)
            {
                mesh.SetUVs(0, uv);
            }

            mesh.subMeshCount = source.subMeshCount;
            for (int s = 0; s < source.subMeshCount; s++)
            {
                var topology = source.GetTopology(s);
                var indices = source.GetIndices(s);
                if (part.mirrored && topology == MeshTopology.Triangles)
                {
                    for (int i = 0; i + 2 < indices.Length; i += 3)
                    {
                        (indices[i + 1], indices[i + 2]) = (indices[i + 2], indices[i + 1]);
                    }
                }
                mesh.SetIndices(indices, topology, s);
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        // The material Unity gives a new cube: Lit in URP, Standard in the
        // built-in pipeline.
        private static Material DefaultMaterial()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var material = cube.GetComponent<Renderer>().sharedMaterial;
            Object.DestroyImmediate(cube);
            return material;
        }

        // A fresh material for the active render pipeline with the source's
        // texture, so a file made for another renderer never shows up pink.
        // When there is a texture the colour is white: files often store a
        // colour that darkens it (ELEPHANT_M.3ds stores black).
        private static Material ConvertMaterial(Material source, string modelFile, Material template, string path)
        {
            var material = new Material(template);
            Texture texture = null;
            var color = Color.white;
            if (source != null)
            {
                material.name = source.name;
                texture = FirstTexture(source, "_BaseMap", "_MainTex", "_BaseColorMap");
                color = FirstColor(source, "_BaseColor", "_Color");
                if (texture == null)
                {
                    texture = FindTextureByName(source.name, modelFile);
                }
            }

            material.mainTexture = texture;
            material.color = texture != null ? Color.white : new Color(color.r, color.g, color.b, 1f);
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.15f);
            }
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.15f);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Texture FirstTexture(Material material, params string[] names)
        {
            foreach (var name in names)
            {
                if (material.HasProperty(name) && material.GetTexture(name) != null)
                {
                    return material.GetTexture(name);
                }
            }
            return null;
        }

        private static Color FirstColor(Material material, params string[] names)
        {
            foreach (var name in names)
            {
                if (material.HasProperty(name))
                {
                    return material.GetColor(name);
                }
            }
            return Color.white;
        }

        // Some files (like .3ds) store only a texture's file name, and Unity
        // does not find it when the textures sit in a sibling folder. Look
        // for a texture named like the material near the model file:
        // material "Ele_Tuskh" finds ELETUSK.jpg.
        private static Texture FindTextureByName(string materialName, string modelFile)
        {
            string wanted = Simplify(materialName);
            if (wanted.Length < 3)
            {
                return null;
            }

            foreach (var folder in NearbyFolders(modelFile))
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string name = Simplify(Path.GetFileNameWithoutExtension(path));
                    if (name.Length >= 3 && (wanted.Contains(name) || name.Contains(wanted)))
                    {
                        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    }
                }
            }
            return null;
        }

        private static string Simplify(string name)
        {
            return new string(name.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        }

        // The model's folder, then its parent (source/ELEPHANT_M.3ds next to
        // textures/), unless the parent is a shared folder of many packs.
        private static IEnumerable<string> NearbyFolders(string modelFile)
        {
            string folder = Path.GetDirectoryName(modelFile).Replace('\\', '/');
            yield return folder;

            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent) && parent != "Assets" && parent != "Assets/ThirdParty")
            {
                yield return parent;
            }
        }

        private static void DeleteOldParts(string modelName)
        {
            foreach (var guid in AssetDatabase.FindAssets(modelName + "_", new[] { PartsFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path).StartsWith(modelName + "_"))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        // Photos of each prefab as it will stand on the card, turned like
        // AnimalImageTargetHandler turns it, for checking without a phone.
        private static int SavePreviews()
        {
            int saved = 0;
            Directory.CreateDirectory(PreviewFolder);
            foreach (var animal in AnimalCatalog.Animals)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(animal));
                if (prefab == null)
                {
                    continue;
                }

                PreviewRenderUtility preview = null;
                try
                {
                    preview = new PreviewRenderUtility();
                    var camera = preview.camera;
                    camera.fieldOfView = 30f;
                    camera.nearClipPlane = 0.01f;
                    camera.farClipPlane = 20f;
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(0.75f, 0.9f, 0.98f);
                    camera.transform.position = new Vector3(0f, 1f, -2.6f);
                    camera.transform.LookAt(new Vector3(0f, 0.4f, 0f));
                    preview.lights[0].intensity = 1.2f;
                    preview.lights[0].transform.rotation = Quaternion.Euler(40f, -30f, 0f);
                    preview.lights[1].intensity = 0.5f;
                    preview.ambientColor = new Color(0.35f, 0.35f, 0.35f);

                    var model = Object.Instantiate(prefab);
                    model.transform.rotation = Quaternion.Euler(0f, 30f, 0f);
                    preview.AddSingleGO(model);

                    preview.BeginStaticPreview(new Rect(0, 0, 512, 512));
                    preview.Render(true);
                    var shot = preview.EndStaticPreview();

                    // EncodeToPNG needs an 8-bit texture.
                    var png = new Texture2D(shot.width, shot.height, TextureFormat.RGB24, false);
                    png.SetPixels(shot.GetPixels());
                    png.Apply();
                    File.WriteAllBytes(Path.Combine(PreviewFolder, animal.modelName + ".png"), png.EncodeToPNG());
                    Object.DestroyImmediate(png);
                    Object.DestroyImmediate(shot);
                    saved++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Foto cek {animal.modelName} gagal: {e.Message}");
                }
                finally
                {
                    preview?.Cleanup();
                }
            }
            return saved;
        }
    }
}
