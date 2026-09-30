using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Backbenchers;
public static class StudioLabBuilder {
 const string Root="Assets/Backbenchers";
 static Color C(string h){ColorUtility.TryParseHtmlString(h,out var c);return c;}
 static Material Mat(string name,string color,Texture texture=null,float emission=0){
  var path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
  m.shader=Shader.Find("Standard");m.color=C(color);m.mainTexture=texture;m.SetFloat("_Glossiness",.3f);m.SetFloat("_Metallic",0);m.doubleSidedGI=true;
  if(emission>0){m.EnableKeyword("_EMISSION");m.SetTexture("_EmissionMap",texture);m.SetColor("_EmissionColor",C(color)*emission);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;}return m;
 }
 static Texture2D Tex(string name)=>AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/"+name);
 static GameObject Cube(string name,Vector3 p,Vector3 size,Material m,bool collider=false){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;}
 static GameObject Face(string name,Vector3 p,Vector2 size,Vector3 inward,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name=name;g.transform.position=p;g.transform.rotation=Mathf.Abs(inward.y)>.9f?Quaternion.Euler(90,0,0):Quaternion.LookRotation(-inward,Vector3.up);g.transform.localScale=new Vector3(size.x,size.y,1);g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;}
 static void Label(string name,string text,Vector3 p,Quaternion rot,float size,Color color){var g=new GameObject(name);g.transform.SetPositionAndRotation(p,rot);var t=g.AddComponent<TextMesh>();t.text=text;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color;var path=Root+"/Materials/World label font.mat";var fm=AssetDatabase.LoadAssetAtPath<Material>(path);if(!fm){fm=new Material(Shader.Find("Backbenchers/WorldLabel"));AssetDatabase.CreateAsset(fm,path);}fm.mainTexture=t.font.material.mainTexture;g.GetComponent<MeshRenderer>().sharedMaterial=fm;}
 static Light Area(string name,Vector3 p,Vector3 direction,float strength,Vector2 size,float temperature){var g=new GameObject(name);g.transform.position=p;g.transform.rotation=Quaternion.LookRotation(direction);var l=g.AddComponent<Light>();l.type=LightType.Rectangle;l.areaSize=size;l.intensity=strength;l.useColorTemperature=true;l.colorTemperature=temperature;l.lightmapBakeType=LightmapBakeType.Baked;l.shadows=LightShadows.Soft;return l;}
 static Bounds Bounds(GameObject g){var rs=g.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
 static GameObject Model(string path,string name,Vector3 floor,float width,Vector3 rotation,string matPath,string topic){
  var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/ResearchModels/"+path);var g=UnityEngine.Object.Instantiate(source);g.name=name;g.transform.position=Vector3.zero;g.transform.rotation=Quaternion.Euler(rotation);
  var bounds=Bounds(g);g.transform.localScale*=width/Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);bounds=Bounds(g);g.transform.position+=floor-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
  var mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/ResearchModels/"+matPath);
  foreach(var r in g.GetComponentsInChildren<Renderer>()){r.sharedMaterials=Enumerable.Repeat(mat,r.sharedMaterials.Length).ToArray();r.gameObject.isStatic=true;if(name.Contains("Racer")&&(r.name=="lights"||r.name=="displaylight"))r.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"/ResearchModels/Racer/Materials/RacerLights.mat");if(name.Contains("Glock")&&r.name=="Triggers2")r.gameObject.SetActive(false);}
  if(topic!=null){var c=g.AddComponent<BoxCollider>();bounds=Bounds(g);c.center=g.transform.InverseTransformPoint(bounds.center);c.size=g.transform.InverseTransformVector(bounds.size);c.size=new Vector3(Mathf.Abs(c.size.x),Mathf.Abs(c.size.y),Mathf.Abs(c.size.z));g.AddComponent<StudioInspectable>().topic=topic;}
  return g;
 }
 static StudioDevice Device(GameObject root,Renderer screen,int slot,string id,string title,int page,Vector3 glowPosition){
  var d=root.AddComponent<StudioDevice>();d.deviceId=id;d.displayName=title;d.screenRenderer=screen;d.materialIndex=slot;d.pages=new[]{Tex("studio-screen-code.jpg"),Tex("studio-screen-content.jpg"),Tex("studio-screen-drone.jpg")};d.page=page;
  var mats=screen.sharedMaterials;mats[slot]=Mat("Interactive screen "+id,"#ffffff",d.pages[page],.75f);if(root.name.Contains("Monitor")||root.name.Contains("monitor")){mats[slot].mainTextureScale=new Vector2(-1,1);mats[slot].mainTextureOffset=new Vector2(1,0);mats[slot].SetTextureScale("_EmissionMap",new Vector2(-1,1));mats[slot].SetTextureOffset("_EmissionMap",new Vector2(1,0));}screen.sharedMaterials=mats;
  if(!root.GetComponent<Collider>()){var mesh=root.GetComponent<MeshFilter>();if(mesh)root.AddComponent<MeshCollider>().sharedMesh=mesh.sharedMesh;else root.AddComponent<BoxCollider>();}
  var lamp=new GameObject(title+" screen spill");lamp.transform.position=glowPosition;var l=lamp.AddComponent<Light>();l.type=LightType.Point;l.color=C("#a7d5ef");l.intensity=.7f;l.range=.85f;l.shadows=LightShadows.None;l.renderMode=LightRenderMode.Auto;l.lightmapBakeType=LightmapBakeType.Realtime;d.glow=l;return d;
 }
 [MenuItem("Backbenchers/Prepare lab models")]
 public static void Prepare(){
  foreach(var guid in AssetDatabase.FindAssets("t:Model",new[]{Root+"/ResearchModels"})){
   var i=(ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));i.generateSecondaryUV=true;i.secondaryUVPackMargin=6;i.importAnimation=false;i.isReadable=false;i.SaveAndReimport();
  }
  foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{Root+"/ResearchModels"})){
   var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));var tex=m.GetTexture("_MainTex");m.shader=Shader.Find("Standard");m.mainTexture=tex;m.SetFloat("_Glossiness",.32f);m.color=Color.white;EditorUtility.SetDirty(m);
  }
  var glock=AssetDatabase.LoadAssetAtPath<Material>(Root+"/ResearchModels/Glock/Model/Materials/Glock.mat");glock.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/ResearchModels/Glock/Textures/albedo.png");glock.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/ResearchModels/Glock/Textures/pistol_normal.bmp"));glock.EnableKeyword("_NORMALMAP");
  AssetDatabase.SaveAssets();
 }
 [MenuItem("Backbenchers/Rebuild research studio")]
 public static void Rebuild(){
  StudioBuilder.Rebuild();var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  foreach(var n in new[]{"Small studio plaque","Original studio identity"}){var g=GameObject.Find(n);if(g)UnityEngine.Object.DestroyImmediate(g);}
  var paper=Mat("Lab warm ivory","#f4eedf");var black=Mat("Lab charcoal","#252930");var red=Mat("Lab signal red","#ed3b39");var oak=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Natural oak frames.mat");
  // Clearly visible original identity on the wall above the rear shelving.
  Cube("Backbenchers oak sign surround",new Vector3(11.49f,2.07f,7.67f),new Vector3(.055f,.60f,1.95f),oak);
  Cube("Backbenchers ivory sign face",new Vector3(11.526f,2.07f,7.67f),new Vector3(.025f,.55f,1.90f),paper);
  var logo=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Small original identity.mat");Face("Backbenchers logo board",new Vector3(11.542f,2.07f,7.67f),new Vector2(1.67f,.47f),Vector3.right,logo);
  Cube("Jolly Roger oak frame",new Vector3(17.66f,1.97f,7.45f),new Vector3(.04f,.8f,.8f),oak);
  var jolly=Face("Framed Backbenchers Jolly Roger",new Vector3(17.635f,1.97f,7.45f),new Vector2(.74f,.74f),Vector3.left,Mat("Original Jolly Roger","#ffffff",Tex("backbenchers-jolly-roger.jpg")));
  jolly.AddComponent<BoxCollider>().size=new Vector3(1,1,.035f);jolly.AddComponent<StudioInspectable>().topic="identity";
  // Warm-white room fill supplements the original pendants and cool clerestory windows.
  foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){
   if(l.lightmapBakeType==LightmapBakeType.Baked){l.intensity*=1.65f;if(l.name.Contains("pendant"))l.colorTemperature=3700;}
  }
  Area("Warm white ceiling fill",new Vector3(14.45f,2.98f,7.65f),Vector3.down,.75f,new Vector2(4.4f,2.8f),4100);
  Area("Soft logo board wash",new Vector3(11.85f,2.6f,7.65f),new Vector3(-1,-.6f,0),.5f,new Vector2(1.6f,.3f),3900);
  Area("Research bench lamp fill",new Vector3(17.1f,2.45f,7.5f),Vector3.down,.45f,new Vector2(.5f,1.6f),3900);
  foreach(var m in AssetDatabase.FindAssets("t:Material",new[]{Root+"/Materials"}).Select(g=>AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))))if(m.name.StartsWith("Warm-")&&(m.name.ToLower().Contains("lamp light")||m.name=="Warm-light"||m.name=="Warm-light 2")){m.SetColor("_EmissionColor",new Color(1,.85f,.65f)*1.25f);EditorUtility.SetDirty(m);}
  // Two original desks plus the source side desk become the three crew workstations.
  Cube("Merlin research mat",new Vector3(17.19f,.796f,7.1f),new Vector3(.62f,.006f,.69f),Mat("Lab work mat","#385256"));
  Model("Racer/Meshes/Racer.FBX","EGUnion Racer research model",new Vector3(17.18f,.804f,7.06f),.53f,new Vector3(0,16,0),"Racer/Materials/Racer.mat","drones");
  Model("Phantom/Meshes/Phantom.FBX","EGUnion Phantom research model",new Vector3(17.18f,.80f,8.04f),.48f,new Vector3(0,-15,0),"Phantom/Materials/Phantom.mat","drones");
  Cube("Game art sample tray",new Vector3(13.3f,.80f,8.16f),new Vector3(.38f,.025f,.28f),black);
  Model("Glock/Model/Pistol.fbx","EGUnion Glock game-art model",new Vector3(13.3f,.82f,8.16f),.29f,new Vector3(90,0,20),"Glock/Model/Materials/Glock.mat","game-art");
  // A third seat and monitor make the research desk usable.
  var chair=UnityEngine.Object.Instantiate(GameObject.Find("Chair"));chair.name="Merlin research chair";chair.transform.position=new Vector3(16.5f,.47f,7.45f);chair.transform.eulerAngles=new Vector3(0,190,0);
  var monitor=UnityEngine.Object.Instantiate(GameObject.Find("Monitor (1)"));monitor.name="Merlin research monitor";monitor.transform.position=new Vector3(17.48f,1f,7.53f);monitor.transform.eulerAngles=new Vector3(0,270,0);
  Device(GameObject.Find("Monitor"),GameObject.Find("Monitor").GetComponent<Renderer>(),0,"thomas","Thomas / software & games",0,new Vector3(13.85f,1.02f,7.34f));
  Device(GameObject.Find("Monitor (1)"),GameObject.Find("Monitor (1)").GetComponent<Renderer>(),0,"hlaing","Hlaing / content & design",1,new Vector3(14.04f,1.02f,8.05f));
  var keyboardSource=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).FirstOrDefault(r=>r.name.ToLower().Contains("keyboard"));if(keyboardSource){var keyboard=UnityEngine.Object.Instantiate(keyboardSource.gameObject);keyboard.name="Merlin compact keyboard";keyboard.transform.position=new Vector3(17.06f,.805f,7.6f);keyboard.transform.eulerAngles=new Vector3(0,270,0);keyboard.transform.localScale*=.66f;}
  Device(monitor,monitor.GetComponent<Renderer>(),0,"merlin","Merlin / drone research",2,new Vector3(17.27f,1.02f,7.53f));
  var phone=GameObject.Find("Phone");var ps=Face("Interactive phone display",phone.transform.position+new Vector3(0,.008f,0),new Vector2(.072f,.112f),Vector3.up,black);ps.transform.Rotate(Vector3.forward,-phone.transform.eulerAngles.y,Space.Self);ps.transform.SetParent(phone.transform,true);Device(phone,ps.GetComponent<Renderer>(),0,"phone","Studio phone",0,phone.transform.position+Vector3.up*.055f).glow.intensity=.12f;
  int ti=0;foreach(var tablet in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Graphic tablet")).ToArray()){
   var surface=Face("Interactive tablet display "+ti,tablet.bounds.center+Vector3.up*.007f,new Vector2(.108f,.165f),Vector3.up,black);surface.transform.Rotate(Vector3.forward,-tablet.transform.eulerAngles.y,Space.Self);surface.transform.SetParent(tablet.transform,true);Device(tablet.gameObject,surface.GetComponent<Renderer>(),0,"tablet"+ti,"Creative tablet "+(ti+1),ti==0?1:2,tablet.bounds.center+Vector3.up*.055f).glow.intensity=.14f;ti++;
  }
  // Practical research clutter: batteries, notebooks, small boards and cables, kept on surfaces.
  for(int i=0;i<3;i++){Cube("Bench battery "+i,new Vector3(16.99f+i*.085f,.817f,6.72f),new Vector3(.055f,.035f,.1f),black);Cube("Battery red tag "+i,new Vector3(16.99f+i*.085f,.838f,6.72f),new Vector3(.03f,.003f,.055f),red);}
  Cube("Prototype circuit board",new Vector3(17.31f,.806f,7.34f),new Vector3(.14f,.012f,.10f),Mat("Circuit green","#296356"));
  for(int i=0;i<4;i++)Cube("Circuit component "+i,new Vector3(17.27f+(i%2)*.06f,.824f,7.31f+(i/2)*.04f),new Vector3(.035f,.02f,.018f),black);
  Cube("Thomas notebook",new Vector3(14.40f,.80f,7.1f),new Vector3(.16f,.018f,.22f),red);
  var cup=GameObject.Find("Cup");if(cup)cup.transform.position=new Vector3(13.60f,.87f,8.35f);
  var magazine=GameObject.Find("Magazine");if(magazine)magazine.transform.position=new Vector3(14.38f,.83f,8.20f);
  // Small physical desk nameplates, rather than room-sized text.
  string[] names={"THOMAS / BUILD","HLAING / CREATE","MERLIN / RESEARCH"};Vector3[] plates={new Vector3(13.33f,.86f,7.12f),new Vector3(14.52f,.86f,8.0f),new Vector3(17.15f,.86f,8.3f)};
  for(int i=0;i<3;i++){var rot=Quaternion.Euler(0,i==0?0:i==1?180:90,0);var plaque=Cube(names[i]+" plaque",plates[i],new Vector3(.30f,.075f,.035f),paper);plaque.transform.rotation=rot;Label(names[i],names[i],plates[i]+rot*Vector3.back*.02f,rot,.005f,C("#30343b"));}
  var e=UnityEngine.Object.FindFirstObjectByType<StudioExperience>();e.viewpoints=new[]{new Vector3(16.45f,1.55f,6.65f),new Vector3(15.2f,1.55f,6.35f),new Vector3(15.45f,1.55f,8f),new Vector3(15.8f,1.55f,6.75f)};e.viewAngles=new[]{new Vector3(2,300,0),new Vector3(9,314,0),new Vector3(1,336,0),new Vector3(8,64,0)};
  QualitySettings.pixelLightCount=2;
  foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(!r.GetComponent<TextMesh>()){r.gameObject.isStatic=true;r.receiveGI=ReceiveGI.Lightmaps;r.scaleInLightmap=r.bounds.size.magnitude<.6f?.4f:1;}
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Research studio ready: two EGUnion drones, Glock art study, six interactive devices, three crew workstations.");
 }
}
