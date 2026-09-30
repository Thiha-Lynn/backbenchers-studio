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
    static Material Solid(string name,string color,bool unlit=false)
    {
        string path=Root+"/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find(unlit?"Unlit/Color":"Standard"));AssetDatabase.CreateAsset(m,path);}
        m.color=Hex(color);if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",.18f);return m;
    }
    static GameObject Quad(string name,Vector3 p,Vector2 size,Vector3 inward,Material material)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name=name;g.transform.position=p;g.transform.rotation=Quaternion.LookRotation(-inward,Vector3.up);g.transform.localScale=new Vector3(size.x,size.y,1);g.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;
    }
    static void Text(string name,string text,Vector3 p,Vector3 inward,float height,Color color)
    {
        var g=new GameObject(name);g.transform.position=p;g.transform.rotation=Quaternion.LookRotation(-inward,Vector3.up);var tm=g.AddComponent<TextMesh>();tm.text=text;tm.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");tm.fontSize=96;tm.characterSize=height*(name.StartsWith("Founder")?.14f:.25f);tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=color;g.GetComponent<MeshRenderer>().sharedMaterial=tm.font.material;g.isStatic=true;
    }
    static Material Image(string name,string texture,bool crop=false,bool white=false)
    {
        string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find(crop?"Unlit/Transparent":"Unlit/Texture"));AssetDatabase.CreateAsset(m,path);}
        m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/"+texture);
        m.mainTextureScale=crop?new Vector2(.82f,.23f):Vector2.one;m.mainTextureOffset=crop?new Vector2(.09f,.39f):Vector2.zero;
        return m;
    }
    [MenuItem("Backbenchers/Rebuild branded studio")]
    public static void Rebuild()
    {
        if(!AssetDatabase.IsValidFolder(Root+"/Scenes"))AssetDatabase.CreateFolder(Root,"Scenes");if(!AssetDatabase.IsValidFolder(Root+"/Materials"))AssetDatabase.CreateFolder(Root,"Materials");
        var scene=EditorSceneManager.OpenScene("Assets/Design Studio/Scene/Demo.unity",OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene,ScenePath);
        foreach(var t in UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(t.gameObject);
        foreach(var c in UnityEngine.Object.FindObjectsByType<RotateMoveCamera_InputSystem>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(c);
        var renderers=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        var copies=new System.Collections.Generic.Dictionary<Material,Material>();
        foreach(var r in renderers)
        {
            if(r.name=="picture"||r.name=="Wall text"||r.name=="Pictures 3"||r.name=="Picture 1"||r.name=="Picture 2") {r.gameObject.SetActive(false);continue;}
            r.sharedMaterials=r.sharedMaterials.Select(m=>{
                if(!m)return m;if(copies.TryGetValue(m,out var copy))return copy;
                string path=Root+"/Materials/Adapted-"+m.name+".mat";copy=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(!copy){copy=new Material(m);AssetDatabase.CreateAsset(copy,path);}else EditorUtility.CopySerialized(m,copy);
                string n=m.name.ToLowerInvariant();
                if(n=="color wall material"){copy.color=Hex("#dad8d2");copy.mainTexture=null;}
                if(n=="white wood"||n.Contains("table metal")||n=="metal table 2"){copy.color=Hex("#333840");copy.mainTexture=null;}
                if(n=="wood 1"){copy.color=Hex("#3d4148");copy.mainTexture=null;}
                if(n=="wood 3")copy.color=Hex("#786455");
                if(n=="wood 2"||n=="wood 4")copy.color=Hex("#aa8772");
                if(n=="parquet")copy.color=Hex("#b6afa5");
                if(n=="metal lamp 1"||n=="lamp metal"||n=="cup"){copy.color=Hex("#fb343c");copy.mainTexture=null;}
                if(n=="fabric1")copy.color=Hex("#343941");
                if(copy.HasProperty("_Glossiness"))copy.SetFloat("_Glossiness",Mathf.Min(copy.GetFloat("_Glossiness"),.35f));
                copies[m]=copy;return copy;
            }).ToArray();
            r.gameObject.isStatic=true;
            if(r.name=="Wall 3"){var mats=r.sharedMaterials;mats[0]=Solid("FeatureWall","#262c35");r.sharedMaterials=mats;}
            if((r.name.StartsWith("Wall")||r.name.StartsWith("Table")||r.name.StartsWith("Bookcase")||r.name.StartsWith("Chair")||r.name=="Floor"||r.name=="Door")&&!r.GetComponent<Collider>()){
                var mc=r.gameObject.AddComponent<MeshCollider>();mc.sharedMesh=r.GetComponent<MeshFilter>().sharedMesh;
            }
        }
        foreach(var pendant in renderers.Where(r=>r.name.StartsWith("Ceiling lamp 2")))pendant.transform.position+=new Vector3(0,0,-1.05f);
        var red=Solid("BrandRed","#ff353f",true);var charcoal=Solid("BrandCharcoal","#191d25");var cream=Solid("BrandCream","#ebe7dd");
        Quad("Studio identity panel",new Vector3(11.438f,2.60f,7.65f),new Vector2(3.2f,1.12f),Vector3.right,charcoal);
        Quad("Original Backbenchers logo",new Vector3(11.45f,2.62f,7.65f),new Vector2(2.8f,.79f),Vector3.right,Image("Identity white","backbenchers-logo-white.png",true));
        Text("Studio subtitle","GAMES  /  PRODUCTS  /  PLAYABLE WORLDS",new Vector3(11.46f,2.12f,7.65f),Vector3.right,.032f,Hex("#ff666d"));
        Quad("Feature wall red line",new Vector3(11.44f,1.99f,7.65f),new Vector2(3.1f,.018f),Vector3.right,red);
        Quad("Studio manifesto panel",new Vector3(12.32f,1.54f,5.312f),new Vector2(1.38f,1.38f),Vector3.forward,charcoal);
        Text("Studio manifesto","USEFUL THINGS.\n\nUNEXPECTED\nWORLDS.",new Vector3(12.32f,1.65f,5.33f),Vector3.forward,.062f,Hex("#faf5ed"));
        Text("Manifesto signature","THOMAS D. LYNN",new Vector3(12.32f,.98f,5.335f),Vector3.forward,.032f,Hex("#ff666d"));
        string[] ids={"endgame","burmamart","glyph-studio"};string[] names={"END GAME UNION","BURMAMART","MYANMAR GLYPH STUDIO"};
        for(int i=0;i<3;i++){
            float x=12.5f+i*1.16f;Quad("Project frame "+i,new Vector3(x,1.7f,10.077f),new Vector2(1.02f,1.28f),Vector3.back,charcoal);
            Quad("Project image "+i,new Vector3(x,1.88f,10.064f),new Vector2(.94f,.60f),Vector3.back,Image("Project "+ids[i],ids[i]+".jpg"));
            Text("Project title "+i,names[i],new Vector3(x,1.38f,10.048f),Vector3.back,.027f,Color.white);
            Text("Project number "+i,"0"+(i+1)+"  /  SELECTED WORK",new Vector3(x,1.19f,10.044f),Vector3.back,.022f,Hex("#ff666d"));
        }
        Text("Door invitation","COME IN.\nMAKE SOMETHING.",new Vector3(16.55f,1.45f,10.02f),Vector3.back,.045f,Hex("#313640"));
        Text("Project wall heading","FROM THE WORKBENCH",new Vector3(13.66f,2.55f,10.05f),Vector3.back,.054f,Hex("#313640"));
        Quad("Founder panel",new Vector3(17.694f,1.95f,8.3f),new Vector2(2.18f,1.58f),Vector3.left,charcoal);
        Text("Founder heading","THOMAS D. LYNN",new Vector3(17.675f,2.38f,8.3f),Vector3.left,.064f,Color.white);
        Text("Founder bio","INDEPENDENT SOFTWARE ENGINEER\n& CREATIVE BUILDER\n\nMYANMAR ROOTS. INDEPENDENT SPIRIT.",new Vector3(17.67f,1.99f,8.3f),Vector3.left,.034f,Hex("#d4d1ca"));
        Text("Founder URL","thomasdlynn.dev",new Vector3(17.66f,1.51f,8.3f),Vector3.left,.048f,Hex("#ff666d"));
        // Replace the blank monitor screens with the studio's work.
        foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Monitor"))){
            var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i].name.Contains("screen"))mats[i]=Image("Workbench display","endgame.jpg");r.sharedMaterials=mats;
        }
        foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){
            if(light.type==LightType.Directional){light.intensity=.85f;light.color=Hex("#fff1dd");light.shadows=LightShadows.Hard;}
            else {light.intensity=.65f;light.range=8;light.shadows=LightShadows.None;light.color=Hex("#ffe5d1");}
        }
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=Hex("#8e9cac");RenderSettings.ambientEquatorColor=Hex("#8b827c");RenderSettings.ambientGroundColor=Hex("#555663");RenderSettings.ambientIntensity=1;
        var cam=UnityEngine.Object.FindFirstObjectByType<Camera>();cam.tag="MainCamera";cam.fieldOfView=65;cam.nearClipPlane=.06f;cam.farClipPlane=45;cam.allowHDR=false;cam.allowMSAA=true;
        cam.transform.position=new Vector3(16.3f,1.6f,7.10f);cam.transform.eulerAngles=new Vector3(0,278,0);
        var body=cam.GetComponent<CharacterController>();if(!body)body=cam.gameObject.AddComponent<CharacterController>();body.height=1.55f;body.radius=.19f;body.center=new Vector3(0,-.69f,0);body.stepOffset=.12f;
        var e=new GameObject("StudioExperience").AddComponent<StudioExperience>();e.viewCamera=cam;e.xLimits=new Vector2(11.72f,17.35f);e.zLimits=new Vector2(5.65f,9.7f);
        e.viewpoints=new[]{new Vector3(16.3f,1.6f,7.1f),new Vector3(15.15f,1.6f,6.25f),new Vector3(15.6f,1.6f,7.7f),new Vector3(15.6f,1.6f,8.35f)};
        e.viewAngles=new[]{new Vector3(0,278,0),new Vector3(12,314,0),new Vector3(-3,320,0),new Vector3(-8,90,0)};
        QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=2;QualitySettings.shadowDistance=18;QualitySettings.pixelLightCount=3;
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();
        Debug.Log("Backbenchers studio rebuilt with original branding, project gallery and four camera viewpoints.");
    }
    [MenuItem("Backbenchers/Apply Web Release Settings")]
    public static void OptimizeWeb()
    {
        var target=NamedBuildTarget.WebGL;
        PlayerSettings.companyName="Backbenchers Studio";PlayerSettings.productName="Backbenchers Studio";PlayerSettings.bundleVersion="1.0.0";
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
            importer.isReadable=false;var settings=importer.GetPlatformTextureSettings("WebGL");settings.overridden=true;settings.maxTextureSize=path.Contains("logo")?1024:512;settings.format=TextureImporterFormat.DXT5;settings.compressionQuality=60;importer.SetPlatformTextureSettings(settings);importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();Debug.Log("Web settings applied: WebGPU with WebGL2 fallback; Brotli with Pages decompression fallback; 128MB initial heap; textures capped for web.");
    }
    public static void ExportPoster()
    {
        var cam=Camera.main;var prior=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1600,1000,24);var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);
        try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(Path.GetFullPath("../studio-web/assets/studio-poster.jpg"),tex.EncodeToJPG(86));}
        finally{cam.targetTexture=prior;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
    }
}
