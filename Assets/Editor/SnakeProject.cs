using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using SnakeTrio;

public static class SnakeProject
{
    const string Root = "Assets/";
    static readonly string[] Scenes = { "Menu", "Juego", "Resultado" };
    static Sprite Sprite(string path)
    {
        string name = Path.GetFileName(path);
        int frame = name == "HeadBlink" || name == "FruitSpark" ? 1 : 0;
        if (name == "HeadBlink") name = "Head";
        if (name == "FruitSpark") name = "Fruit";
        string asset = Root + "Sprites/Aseprite/" + name + ".aseprite";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(asset).OfType<Sprite>().OrderBy(s => s.name, StringComparer.Ordinal).ToArray();
        if (sprites.Length <= frame) throw new BuildFailedException("Sprite Aseprite ausente: " + asset + " frame " + frame);
        return sprites[frame];
    }

    [MenuItem("Snake Trío/Preparar escenas")]
    public static void Prepare()
    {
        foreach (string dir in new[] { "Scenes", "Prefabs", "Animations", "Materials", "Tiles" }) Directory.CreateDirectory(Root + dir);
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Root + "Sprites/Aseprite", "*.aseprite"))
        {
            var importer = AssetImporter.GetAtPath(path.Replace('\\', '/')) as AsepriteImporter;
            if (importer == null) throw new BuildFailedException("Importador Aseprite ausente: " + path);
            importer.importMode = FileImportModes.AnimatedSprite;
            importer.layerImportMode = LayerImportModes.MergeFrame;
            importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point;
            importer.pivotSpace = PivotSpaces.Canvas; importer.pivotAlignment = SpriteAlignment.Center;
            importer.generateModelPrefab = false; importer.generateAnimationClips = true;
            importer.SaveAndReimport();
        }
        Debug.Log("ASEPRITE_OK: fuentes editables importadas y usadas por sprites, prefabs y animaciones.");
        foreach (string id in AssetDatabase.FindAssets("t:Texture2D", new[] { Root + "Sprites" }))
        {
            var importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(id)) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Root + "Materials/SinFriccion.physicsMaterial2D");
        if (material == null) { material = new PhysicsMaterial2D("Sin fricción"); AssetDatabase.CreateAsset(material, Root + "Materials/SinFriccion.physicsMaterial2D"); }
        material.friction = 0; material.bounciness = 0;
        var blink = Animation("Parpadeo", "Snake/Head");
        var sparkle = Animation("BrilloFruta", "Food/Fruit");
        var head = Prefab("Cabeza", Sprite("Snake/Head"), ContactKind.Body, material, blink, true);
        var body = Prefab("Segmento", Sprite("Snake/Body"), ContactKind.Body, material);
        var food = Prefab("Comida", Sprite("Food/Fruit"), ContactKind.Food, material, sparkle);
        var wall = Prefab("Pared", Sprite("Environment/Wall"), ContactKind.Wall, material);
        var tileA = Tile("SueloA", Sprite("Environment/TileA")); var tileB = Tile("SueloB", Sprite("Environment/TileB"));
        AssetDatabase.SaveAssets();
        for (int i = 0; i < Scenes.Length; i++)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 6.75f;
            camera.backgroundColor = SnakeScreen.Hex("102a2a"); camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.transform.position = new Vector3(0, 0, -10); cameraObject.AddComponent<AudioListener>();
            var screen = new GameObject("Interfaz " + Scenes[i]).AddComponent<SnakeScreen>();
            screen.headSprite = Sprite("Snake/Head"); screen.bodySprite = Sprite("Snake/Body");
            screen.fruitSprite = Sprite("Food/Fruit"); screen.tileSprite = Sprite("Environment/TileA");
            if (Scenes[i] == "Juego")
            {
                var controller = new GameObject("Controlador Snake").AddComponent<SnakeGame>();
                controller.headPrefab = head; controller.bodyPrefab = body; controller.foodPrefab = food;
                var gridObject = new GameObject("Grid del jardín", typeof(Grid));
                gridObject.GetComponent<Grid>().cellSize = Vector3.one * SnakeGame.Cell;
                gridObject.transform.position = new Vector3(-14 * SnakeGame.Cell, -.25f - 10 * SnakeGame.Cell, 0);
                var floor = new GameObject("Suelo Tilemap", typeof(Tilemap), typeof(TilemapRenderer)); floor.transform.SetParent(gridObject.transform, false);
                var map = floor.GetComponent<Tilemap>(); floor.GetComponent<TilemapRenderer>().sortingOrder = -10;
                var floorTiles = new TileBase[28 * 20];
                for (int y = 0; y < 20; y++) for (int x = 0; x < 28; x++) floorTiles[x + y * 28] = (x + y) % 2 == 0 ? tileA : tileB;
                map.SetTilesBlock(new BoundsInt(0, 0, 0, 28, 20, 1), floorTiles);
                map.RefreshAllTiles(); map.CompressBounds();
                if (!map.HasTile(Vector3Int.zero)) throw new BuildFailedException("Tilemap sin baldosas");
                Debug.Log("TILEMAP_OK: " + map.GetUsedTilesCount() + " baldosas distintas, 560 celdas.");
                Wall(wall, "Borde izquierdo", new Vector2(-14.5f * SnakeGame.Cell, -.25f), new Vector2(SnakeGame.Cell, 22 * SnakeGame.Cell));
                Wall(wall, "Borde derecho", new Vector2(14.5f * SnakeGame.Cell, -.25f), new Vector2(SnakeGame.Cell, 22 * SnakeGame.Cell));
                Wall(wall, "Borde superior", new Vector2(0, -.25f + 10.5f * SnakeGame.Cell), new Vector2(28 * SnakeGame.Cell, SnakeGame.Cell));
                Wall(wall, "Borde inferior", new Vector2(0, -.25f - 10.5f * SnakeGame.Cell), new Vector2(28 * SnakeGame.Cell, SnakeGame.Cell));
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, Root + "Scenes/" + Scenes[i] + ".unity")) throw new BuildFailedException("No se guardó " + Scenes[i]);
        }
        var settings = new EditorBuildSettingsScene[3];
        for (int i = 0; i < 3; i++) settings[i] = new EditorBuildSettingsScene(Root + "Scenes/" + Scenes[i] + ".unity", true);
        EditorBuildSettings.scenes = settings;
        PlayerSettings.productName = "Snake Trío · PGM-611"; PlayerSettings.companyName = "Equipo PGM-611";
        PlayerSettings.bundleVersion = "1.3.0";
        PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.pgm611.snaketrio");
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 });
        AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(Root + "Scenes/Menu.unity");
        Debug.Log("SCENES_OK: tres escenas, sprites, prefabs, animaciones, Tilemap y colliders.");
    }
    static Tile Tile(string name, Sprite sprite)
    {
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(Root + "Tiles/" + name + ".asset");
        if (tile == null) { tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile, Root + "Tiles/" + name + ".asset"); }
        tile.sprite = sprite; tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None;
        tile.transform = Matrix4x4.Scale(Vector3.one * SnakeGame.Cell); EditorUtility.SetDirty(tile); return tile;
    }
    static AnimatorController Animation(string name, string source)
    {
        string asset = AssetDatabase.GetAssetPath(Sprite(source));
        var imported = AssetDatabase.LoadAllAssetsAtPath(asset).OfType<AnimationClip>().FirstOrDefault();
        if (imported == null) throw new BuildFailedException("Animación Aseprite ausente: " + asset);
        var binding = AnimationUtility.GetObjectReferenceCurveBindings(imported)
            .FirstOrDefault(b => b.type == typeof(SpriteRenderer) && b.propertyName == "m_Sprite");
        var frames = AnimationUtility.GetObjectReferenceCurve(imported, binding);
        if (frames == null || frames.Length < 2) throw new BuildFailedException("Frames Aseprite ausentes: " + asset);
        var loop = frames.ToList();
        loop.Add(new ObjectReferenceKeyframe { time = imported.length + 1f / imported.frameRate, value = frames[0].value });
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Root + "Animations/" + name + ".anim");
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, Root + "Animations/" + name + ".anim"); }
        clip.frameRate = imported.frameRate;
        AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" },
            loop.ToArray());
        Debug.Log("ASEPRITE_ANIMATION_OK: " + name + ", " + clip.length + " segundos desde " + asset);
        var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true; AnimationUtility.SetAnimationClipSettings(clip, settings);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Root + "Animations/" + name + ".controller");
        if (controller == null) { controller = AnimatorController.CreateAnimatorControllerAtPath(Root + "Animations/" + name + ".controller"); controller.AddMotion(clip); }
        return controller;
    }
    static GameObject Prefab(string name, Sprite sprite, ContactKind kind, PhysicsMaterial2D material, AnimatorController animator = null, bool head = false)
    {
        var go = new GameObject(name); go.AddComponent<SpriteRenderer>().sprite = sprite;
        go.GetComponent<SpriteRenderer>().sortingOrder = 5;
        var box = go.AddComponent<BoxCollider2D>(); box.size = Vector2.one * .84f; box.sharedMaterial = material;
        box.isTrigger = kind == ContactKind.Food;
        var contact = go.AddComponent<SnakeContact2D>(); contact.kind = kind;
        if (kind != ContactKind.Wall) go.transform.localScale = Vector3.one * SnakeGame.Cell;
        if (head) { var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0; rb.constraints = RigidbodyConstraints2D.FreezeRotation; }
        if (animator != null) go.AddComponent<Animator>().runtimeAnimatorController = animator;
        var result = PrefabUtility.SaveAsPrefabAsset(go, Root + "Prefabs/" + name + ".prefab"); UnityEngine.Object.DestroyImmediate(go); return result;
    }
    static void Wall(GameObject prefab, string name, Vector2 position, Vector2 size)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab); go.name = name; go.transform.position = position;
        var render = go.GetComponent<SpriteRenderer>(); render.drawMode = SpriteDrawMode.Tiled; render.size = size;
        go.GetComponent<BoxCollider2D>().size = size;
    }
    [MenuItem("Snake Trío/Validar reglas")]
    public static void Validate()
    {
        Action<bool, string> require = (ok, message) => { if (!ok) throw new BuildFailedException(message); };
        foreach (string asset in Directory.GetFiles(Root + "Sprites/Aseprite", "*.aseprite"))
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(asset).OfType<Sprite>().ToArray();
            int expected = asset.EndsWith("Head.aseprite") || asset.EndsWith("Fruit.aseprite") ? 2 : 1;
            require(sprites.Length == expected, "Frames incorrectos: " + asset);
            foreach (var sprite in sprites) require(Mathf.Approximately(sprite.pixelsPerUnit, 32), "Escala Aseprite incorrecta");
        }
        require(AssetDatabase.GetAssetPath(Sprite("Snake/Head")).EndsWith(".aseprite"), "Cabeza debe usar Aseprite");
        var model = new SnakeModel(8, 6, 7);
        require(!model.QueueDirection(Vector2Int.left), "No permitir giro inverso");
        require(model.QueueDirection(Vector2Int.up) && !model.QueueDirection(Vector2Int.left), "Un giro por paso");
        model.Advance(); require(model.Direction == Vector2Int.up, "Aplicar giro");
        model = new SnakeModel(8, 6, 7); model.SetFoodForValidation(model.NextHead);
        require(model.Advance() == StepOutcome.Ate && model.Score == 10 && model.Body.Count == 4, "Comer debe sumar y crecer");
        require(!model.Body.Contains(model.Food), "No generar fruta dentro de la serpiente");
        model = new SnakeModel(8, 6, 7); model.SetFoodForValidation(Vector2Int.zero);
        for (int i = 0; i < 8 && !model.Finished; i++) model.Advance();
        require(model.Finished && !model.Victory && model.EndReason.Contains("borde"), "Colisión con borde");
        model = new SnakeModel(8, 6, 7); model.Body.Clear();
        model.Body.AddRange(new[] { new Vector2Int(4,4), new Vector2Int(4,3), new Vector2Int(5,3), new Vector2Int(5,4), new Vector2Int(6,4) });
        model.SetFoodForValidation(Vector2Int.zero); require(model.Advance() == StepOutcome.Lost, "Colisión con cuerpo");
        model = new SnakeModel(8, 6, 7); model.Body.Clear();
        model.Body.AddRange(new[] { new Vector2Int(4,4), new Vector2Int(4,3), new Vector2Int(5,3), new Vector2Int(5,4) });
        model.SetFoodForValidation(Vector2Int.zero); require(model.Advance() == StepOutcome.Moved, "Permitir entrar en cola liberada");
        model = new SnakeModel(6, 4, 7); model.Body.Clear(); model.Body.Add(new Vector2Int(4,2));
        for (int y = 0; y < 4; y++) for (int x = 0; x < 6; x++)
            if (new Vector2Int(x,y) != new Vector2Int(4,2) && new Vector2Int(x,y) != new Vector2Int(5,2)) model.Body.Add(new Vector2Int(x,y));
        model.SetFoodForValidation(new Vector2Int(5,2)); require(model.Advance() == StepOutcome.Won && model.Body.Count == 24, "Victoria al completar tablero");
        foreach (var scene in EditorBuildSettings.scenes) require(File.Exists(scene.path), "Escena ausente");
        require(SnakeSession.Clock(0) == "00:00" && SnakeSession.Clock(125.9f) == "02:05", "Cronómetro");
        model = new SnakeModel(8, 6, 7);
        require(model.Advance(true, "Choque físico") == StepOutcome.Lost && model.EndReason == "Choque físico", "Colisión Physics2D");
        int count = model.Body.Count; require(model.Advance() == StepOutcome.Lost && model.Body.Count == count, "Finalización estable");
        Debug.Log("RULES_OK: dirección, cola, cuerpo, borde, comida, puntaje y victoria.");
    }
    [MenuItem("Snake Trío/Compilar Windows")]
    public static void BuildWindows()
    {
        Prepare(); Validate();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = Array.ConvertAll(Scenes, s => Root + "Scenes/" + s + ".unity"),
            locationPathName = "Builds/Windows-v1.3/SnakeTrio.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
        if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("Compilación Windows: " + report.summary.result);
        Debug.Log("BUILD_OK: " + report.summary.totalSize + " bytes");
    }
}
