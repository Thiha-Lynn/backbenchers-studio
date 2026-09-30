using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Backbenchers;

public static class StudioBuilder
{
    const string Root="Assets/Backbenchers";
    public const string ScenePath=Root+"/Scenes/BackbenchersStudio.unity";
    static Color Hex(string h){ColorUtility.TryParseHtmlString(h,out var c);return c;}
    static Material Solid(string name,string color)
    {
        string path=Root+"/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
        m.shader=Shader.Find("Standard");m.color=Hex(color);m.SetFloat("_Glossiness",.22f);m.doubleSidedGI=true;return m;
    }
    static GameObject Quad(string name,Vector3 p,Vector2 size,Vector3 inward,Material material)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name=name;g.transform.position=p;g.transform.rotation=Quaternion.LookRotation(-inward,Vector3.up);g.transform.localScale=new Vector3(size.x,size.y,1);g.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;
    }
    static GameObject Box(string name,Vector3 p,Vector3 size,Material material)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;
    }
    static Material Image(string name,string texture,bool logo=false,bool screen=false)
    {
        string path=Root+"/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
        m.shader=Shader.Find("Standard");m.color=Color.white;m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/"+texture);
        m.mainTextureScale=logo?new Vector2(.82f,.23f):Vector2.one;m.mainTextureOffset=logo?new Vector2(.09f,.39f):Vector2.zero;
        m.SetFloat("_Glossiness",.05f);m.doubleSidedGI=true;
        if(logo){m.SetFloat("_Mode",1);m.SetInt("_SrcBlend",1);m.SetInt("_DstBlend",0);m.SetInt("_ZWrite",1);m.EnableKeyword("_ALPHATEST_ON");m.SetFloat("_Cutoff",.25f);m.renderQueue=2450;}
        if(screen){m.EnableKeyword("_EMISSION");m.SetTexture("_EmissionMap",m.mainTexture);m.SetColor("_EmissionColor",Color.white*.24f);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}
        return m;
    }
    static Light BakedLight(string name,LightType type,Vector3 position,Vector3 direction,float intensity,float kelvin,Vector2 area=default)
    {
        var g=new GameObject(name);g.transform.position=position;if(direction.sqrMagnitude>0)g.transform.rotation=Quaternion.LookRotation(direction);
        var l=g.AddComponent<Light>();l.type=type;l.intensity=intensity*.5f;l.color=Color.white;l.useColorTemperature=true;l.colorTemperature=kelvin;l.range=7;l.shadows=LightShadows.Soft;l.lightmapBakeType=LightmapBakeType.Baked;
        if(type==LightType.Rectangle)l.areaSize=area;
        return l;
    }
    [MenuItem("Backbenchers/Prepare models for soft baked lighting")]
    public static void PrepareModels()
    {
        foreach(var guid in AssetDatabase.FindAssets("t:Model",new[]{"Assets/Design Studio/Models"})){
            var i=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as ModelImporter;
            if(i&&!i.generateSecondaryUV){i.generateSecondaryUV=true;i.secondaryUVPackMargin=6;i.SaveAndReimport();}
        }
        Debug.Log("Studio lightmap UVs prepared.");
    }
    [MenuItem("Backbenchers/Rebuild branded studio")]
    public static void Rebuild()
    {
        if(!AssetDatabase.IsValidFolder(Root+"/Scenes"))AssetDatabase.CreateFolder(Root,"Scenes");
        if(!AssetDatabase.IsValidFolder(Root+"/Materials"))AssetDatabase.CreateFolder(Root,"Materials");
        var current=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(current.isDirty && current.path!=ScenePath)EditorSceneManager.SaveScene(current,Root+"/Scenes/BeforeWarmRedesign.unity",true);
        var scene=EditorSceneManager.OpenScene("Assets/Design Studio/Scene/Demo.unity",OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene,ScenePath);
        Lightmapping.lightingDataAsset=null;LightmapSettings.lightmaps=Array.Empty<LightmapData>();
        foreach(var t in UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(t.gameObject);
        foreach(var c in UnityEngine.Object.FindObjectsByType<RotateMoveCamera_InputSystem>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(c);
        var renderers=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        var copies=new System.Collections.Generic.Dictionary<Material,Material>();
        string[] remove={"wall text","pictures 3","picture"};
        foreach(var r in renderers)
        {
            string objectName=r.name.ToLowerInvariant();
            if(remove.Contains(objectName)){r.gameObject.SetActive(false);continue;}
            r.sharedMaterials=r.sharedMaterials.Select(m=>{
                if(!m)return m;if(copies.TryGetValue(m,out var copy))return copy;
                string path=Root+"/Materials/Warm-"+m.name+".mat";copy=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(!copy){copy=new Material(m);AssetDatabase.CreateAsset(copy,path);}else EditorUtility.CopySerialized(m,copy);
                copy.name="Warm-"+m.name;string n=m.name.ToLowerInvariant();
                if(n=="color wall material"){copy.color=Hex("#e5ded0");copy.mainTexture=null;}
                // Retain the Demo's original woods, upholstery, shelves, and red lamp accents.
                if(copy.HasProperty("_Glossiness"))copy.SetFloat("_Glossiness",.18f);
                if(copy.HasProperty("_Metallic"))copy.SetFloat("_Metallic",n.Contains("metal")?.25f:0);
                if(copy.HasProperty("_BumpScale"))copy.SetFloat("_BumpScale",.35f);
                copy.doubleSidedGI=true;
                if(n=="lamp light"||n=="light"||n=="light 2"){
                    copy.shader=Shader.Find("Standard");copy.color=Hex("#fff0d5");copy.EnableKeyword("_EMISSION");copy.SetColor("_EmissionColor",Hex("#ffe2af")*.6f);copy.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;
                }
                copies[m]=copy;return copy;
            }).ToArray();
            r.gameObject.isStatic=true;r.receiveGI=ReceiveGI.Lightmaps;r.scaleInLightmap=r.bounds.size.magnitude<.6f?.5f:1f;
            if((r.name.StartsWith("Wall")||r.name.StartsWith("Table")||r.name.StartsWith("Bookcase")||r.name.StartsWith("Chair")||r.name=="Floor"||r.name=="Door")&&!r.GetComponent<Collider>()){
                var mc=r.gameObject.AddComponent<MeshCollider>();mc.sharedMesh=r.GetComponent<MeshFilter>().sharedMesh;
            }
        }
        // Lamps retain the original Demo scene positions. Branding sits beneath their sightline.
        var oak=Solid("Natural oak frames","#9c7e5a");var paper=Solid("Warm paper mat","#e8dfca");
        Box("Small studio plaque",new Vector3(12.34f,1.48f,5.315f),new Vector3(1.12f,.41f,.025f),paper);
        Quad("Original studio identity",new Vector3(12.34f,1.48f,5.331f),new Vector2(.98f,.275f),Vector3.forward,Image("Small original identity","backbenchers-logo-color.png",true));
        string[] posters={"bounty-thomas.png","bounty-hlaing.png","bounty-merlin.png"};
        string[] people={"Thomas D. Lynn","Hlaing Gyi","Trafalgar D. Merlin"};
        for(int i=0;i<3;i++){
            float x=13.98f+i*.54f;
            Box("Oak frame - "+people[i],new Vector3(x,1.51f,10.074f),new Vector3(.43f,.64f,.025f),oak);
            Quad("Bounty poster - "+people[i],new Vector3(x,1.51f,10.058f),new Vector2(.398f,.603f),Vector3.back,Image("Bounty "+i,posters[i]));
        }
        foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Monitor"))){
            var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i].name.ToLower().Contains("screen"))mats[i]=Solid("Calm studio screen","#405557");r.sharedMaterials=mats;
        }
        // A small reading corner uses the source package's plants, books and side desk.
        foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(l.gameObject);
        BakedLight("Warm pendant above the desks",LightType.Point,new Vector3(12.93f,2.13f,7.59f),Vector3.zero,1.6f,3300);
        BakedLight("Warm pendant in the open space",LightType.Point,new Vector3(16.08f,2.13f,7.59f),Vector3.zero,1.5f,3300);
        BakedLight("Soft daylight through high windows",LightType.Rectangle,new Vector3(14.5f,2.9f,5.38f),new Vector3(0,-.25f,1),2.7f,6400,new Vector2(5,.8f));
        BakedLight("Gentle daylight from the west window",LightType.Rectangle,new Vector3(11.48f,3.04f,7.6f),new Vector3(1,-.2f,0),1.6f,6200,new Vector2(3.6f,.6f));
        BakedLight("Desk task light",LightType.Point,new Vector3(14.35f,1.17f,7.34f),Vector3.zero,.26f,3600).range=1.5f;
        var sun=BakedLight("Demo daylight direction",LightType.Directional,Vector3.zero,Vector3.zero,.7f,5800);sun.transform.eulerAngles=new Vector3(40.86f,332.5f,0);
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=Hex("#a8b8cd");RenderSettings.ambientEquatorColor=Hex("#a99c86");RenderSettings.ambientGroundColor=Hex("#6c665c");RenderSettings.ambientIntensity=.4f;
        RenderSettings.reflectionIntensity=.45f;
        foreach(var probe in UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)){probe.enabled=false;}
        var cam=UnityEngine.Object.FindFirstObjectByType<Camera>();cam.tag="MainCamera";cam.fieldOfView=62;cam.nearClipPlane=.07f;cam.farClipPlane=35;cam.allowHDR=false;cam.allowMSAA=true;
        cam.transform.position=new Vector3(16.45f,1.55f,6.65f);cam.transform.eulerAngles=new Vector3(2,300,0);
        var body=cam.GetComponent<CharacterController>();if(!body)body=cam.gameObject.AddComponent<CharacterController>();body.height=1.5f;body.radius=.19f;body.center=new Vector3(0,-.65f,0);body.stepOffset=.12f;
        var e=new GameObject("StudioExperience").AddComponent<StudioExperience>();e.viewCamera=cam;e.xLimits=new Vector2(11.72f,17.35f);e.zLimits=new Vector2(5.65f,9.7f);
        e.viewpoints=new[]{new Vector3(16.45f,1.55f,6.65f),new Vector3(15.2f,1.55f,6.35f),new Vector3(15.45f,1.55f,8.0f),new Vector3(16.25f,1.55f,8.8f)};
        e.viewAngles=new[]{new Vector3(2,300,0),new Vector3(9,314,0),new Vector3(1,336,0),new Vector3(4,229,0)};
        QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=4;QualitySettings.shadows=ShadowQuality.Disable;QualitySettings.pixelLightCount=0;
        var settings=AssetDatabase.LoadAssetAtPath<LightingSettings>(Root+"/WarmStudioLighting.lighting");
        if(!settings){settings=new LightingSettings();AssetDatabase.CreateAsset(settings,Root+"/WarmStudioLighting.lighting");}
        settings.name="Warm Studio Baked Lighting";settings.autoGenerate=false;settings.bakedGI=true;settings.realtimeGI=false;settings.lightmapper=LightingSettings.Lightmapper.ProgressiveGPU;
        settings.lightmapResolution=48;settings.lightmapMaxSize=2048;settings.lightmapPadding=4;settings.directionalityMode=LightmapsMode.NonDirectional;settings.compressLightmaps=true;
        settings.directSampleCount=64;settings.indirectSampleCount=512;settings.environmentSampleCount=128;settings.maxBounces=5;settings.ao=false;settings.filteringMode=LightingSettings.FilterMode.Auto;
        Lightmapping.lightingSettings=settings;EditorUtility.SetDirty(settings);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();
        Debug.Log("Warm studio rebuilt: no floating text, three bounty posters, original pendant positions, soft baked lighting setup.");
    }
    [MenuItem("Backbenchers/Bake warm studio lighting")]
    public static void BakeLighting(){if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=ScenePath)EditorSceneManager.OpenScene(ScenePath);Lightmapping.BakeAsync();}
    [MenuItem("Backbenchers/Apply Web Release Settings")]
    public static void OptimizeWeb()
    {
        var target=NamedBuildTarget.WebGL;
        PlayerSettings.companyName="Backbenchers Studio";PlayerSettings.productName="Backbenchers Studio";PlayerSettings.bundleVersion="1.1.0";
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{GraphicsDeviceType.WebGPU,GraphicsDeviceType.OpenGLES3});
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Brotli;
        // GitHub Pages cannot configure Unity's Content-Encoding headers.
        PlayerSettings.WebGL.decompressionFallback=true;
        PlayerSettings.WebGL.dataCaching=true;PlayerSettings.WebGL.initialMemorySize=128;PlayerSettings.WebGL.maximumMemorySize=1024;
        PlayerSettings.WebGL.memoryGrowthMode=WebGLMemoryGrowthMode.Geometric;
        PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;PlayerSettings.WebGL.debugSymbolMode=WebGLDebugSymbolMode.Off;
        PlayerSettings.WebGL.wasm2023=false;PlayerSettings.WebGL.threadsSupport=false;
        PlayerSettings.stripEngineCode=true;PlayerSettings.stripUnusedMeshComponents=true;
        PlayerSettings.SetManagedStrippingLevel(target,ManagedStrippingLevel.High);PlayerSettings.SetIl2CppCodeGeneration(target,Il2CppCodeGeneration.OptimizeSize);
        UnityEditor.WebGL.UserBuildSettings.codeOptimization=UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;
        PlayerSettings.runInBackground=false;PlayerSettings.defaultWebScreenWidth=1280;PlayerSettings.defaultWebScreenHeight=720;
        foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Design Studio",Root+"/Brand"})){
            string path=AssetDatabase.GUIDToAssetPath(guid);var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;
            importer.isReadable=false;var settings=importer.GetPlatformTextureSettings("WebGL");settings.overridden=true;settings.maxTextureSize=path.Contains("logo")||path.Contains("bounty")||path.Contains("studio-screen")?1024:512;settings.format=TextureImporterFormat.DXT5;settings.compressionQuality=60;importer.SetPlatformTextureSettings(settings);importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();Debug.Log("Web settings applied: WebGPU with WebGL2 fallback; Brotli with Pages decompression fallback; 128MB initial heap; textures capped for web.");
    }
    public static void ExportPoster()
    {
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=ScenePath)EditorSceneManager.OpenScene(ScenePath);
        var cam=Camera.main;var prior=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1600,1000,24);var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);
        try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(Path.GetFullPath("../studio-web/assets/studio-poster.jpg"),tex.EncodeToJPG(86));}
        finally{cam.targetTexture=prior;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
    }
}
