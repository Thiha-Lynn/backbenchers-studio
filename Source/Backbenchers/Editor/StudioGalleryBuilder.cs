using System.Linq;
using UnityEditor;
using UnityEngine;
using Backbenchers;

// Original office storage with a shared oak, brass and cotton-paper gallery.
public static class StudioGalleryBuilder {
 const string Root="Assets/Backbenchers";
 static Transform root;
 static Material Mat(string name,string hex,Texture texture=null){
  var path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
  ColorUtility.TryParseHtmlString(hex,out var color);m.color=color;m.mainTexture=texture;m.SetFloat("_Glossiness",.18f);m.enableInstancing=true;return m;
 }
 static GameObject Box(string name,Vector3 p,Vector3 size,Material material,Transform parent){
  var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;
 }
 static GameObject Print(string name,Vector3 p,Vector2 size,Quaternion rotation,Material material,Transform parent){
  var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localRotation=rotation;g.transform.localScale=new Vector3(size.x,size.y,1);g.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;
 }
 static void Frame(string name,Vector3 position,float yaw,Vector2 size,Material art){
  var group=new GameObject(name);group.transform.SetParent(root,false);group.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
  var oak=Mat("Gallery warm oak","#907352");var brass=Mat("Gallery aged brass","#b39760");var paper=Mat("Gallery cotton mat","#e9dfc9");
  Box("Oak shadow frame",Vector3.zero,new Vector3(size.x+.10f,size.y+.10f,.025f),oak,group.transform);
  Box("Fine brass inset",new Vector3(0,0,-.014f),new Vector3(size.x+.072f,size.y+.072f,.002f),brass,group.transform);
  Box("Cotton paper mat",new Vector3(0,0,-.016f),new Vector3(size.x+.067f,size.y+.067f,.002f),paper,group.transform);
  Print(name+" artwork",new Vector3(0,0,-.018f),size,Quaternion.identity,art,group.transform);
  group.AddComponent<BoxCollider>().size=new Vector3(size.x+.10f,size.y+.10f,.045f);group.AddComponent<StudioInspectable>().topic=name.StartsWith("Wall portrait")?"portrait-art":"nature-art";
 }
 static void Move(string name,Vector3 center){
  var t=Resources.FindObjectsOfTypeAll<Transform>().FirstOrDefault(x=>x.gameObject.scene.IsValid()&&x.name==name);if(!t)return;
  t.gameObject.SetActive(true);var rs=t.GetComponentsInChildren<Renderer>();if(rs.Length==0)return;var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);t.position+=center-b.center;
 }
 public static void Apply(string[] titles){
  var previous=GameObject.Find("Backbenchers gallery");if(previous)Object.DestroyImmediate(previous);root=new GameObject("Backbenchers gallery").transform;
  // Preserve the original office storage and ornaments; only the books are restaged.
  foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
   var p=r.bounds.center;bool rear=p.x>13.35f&&p.x<15.3f&&p.z<5.85f;bool west=p.x<11.9f&&p.z>6.45f&&p.z<7.43f;
   bool ornament=r.name.StartsWith("File ")||r.name.StartsWith("Box ")||r.name.StartsWith("Plant ")||r.name=="Letter A"||r.name=="Geosphere"||r.name=="Vase 1";
   if((rear||west)&&ornament)r.gameObject.SetActive(true);
  }
  Move("Plant 2 (1)",new Vector3(13.78f,1.18f,5.53f));Move("Plant 3",new Vector3(14.48f,1.49f,5.63f));
  var oak=Mat("Gallery warm oak","#907352");
  foreach(var name in new[]{"Picture 1","Picture 2"}){var old=GameObject.Find(name);if(old)old.SetActive(false);}
  var portrait=Mat("Faceless pencil portrait","#ffffff",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/aung-san-suu-kyi-pencil.png"));
  Frame("Wall portrait / Aung San Suu Kyi",new Vector3(17.63f,1.91f,8.35f),90,new Vector2(.39f,.585f),portrait);
  var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/studio-nature-triptych.png");
  for(int i=0;i<3;i++){
   var m=Mat("Nature study "+i,"#ffffff",atlas);m.mainTextureScale=new Vector2(.325f,.96f);m.mainTextureOffset=new Vector2(i/3f+.004f,.02f);
   if(i==1)Frame("Gallery / Irrawaddy",new Vector3(12.48f,1.87f,5.34f),180,new Vector2(.40f,.80f),m);
   else Frame(i==0?"Gallery / Jasmine":"Gallery / Flight",new Vector3(17.63f,i==0?2.18f:1.57f,8.96f),90,new Vector2(.22f,.44f),m);
  }
  foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Oak frame -")||r.name=="Jolly Roger oak frame"))r.sharedMaterial=oak;
  var hanger=GameObject.Find("Hanger");if(hanger)hanger.GetComponent<Renderer>().sharedMaterial=Mat("Studio forest enamel","#45624f");
  var mini=GameObject.Find("Merlin Mac mini study");var logo=Mat("Mini Apple inlay","#32383a",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/apple-logo.png"));
  logo.SetFloat("_Mode",1);logo.SetFloat("_Cutoff",.45f);logo.EnableKeyword("_ALPHATEST_ON");logo.renderQueue=2450;logo.SetFloat("_Metallic",.55f);
  Print("Apple logo",new Vector3(0,.059f,0),new Vector2(.033f,.041f),Quaternion.Euler(90,0,0),logo,mini.transform);
  var experience=Object.FindFirstObjectByType<StudioExperience>();
  experience.viewpoints=experience.viewpoints.Take(4).Concat(new[]{new Vector3(14.33f,1.32f,7.30f)}).ToArray();
  experience.viewAngles=experience.viewAngles.Take(4).Concat(new[]{new Vector3(0,180,0)}).ToArray();EditorUtility.SetDirty(experience);
  foreach(var name in new[]{"studio-nature-triptych.png","aung-san-suu-kyi-pencil.png","apple-logo.png"}){
   var ti=AssetImporter.GetAtPath(Root+"/Brand/"+name) as TextureImporter;if(!ti)continue;
   ti.maxTextureSize=name=="studio-nature-triptych.png"?2048:1024;ti.mipmapEnabled=true;ti.anisoLevel=8;ti.wrapMode=TextureWrapMode.Clamp;ti.textureCompression=TextureImporterCompression.CompressedHQ;ti.alphaIsTransparency=name=="apple-logo.png";ti.SaveAndReimport();
  }
 }
}
