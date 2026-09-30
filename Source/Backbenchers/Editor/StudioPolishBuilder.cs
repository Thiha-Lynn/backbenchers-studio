using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Backbenchers;

// Additive, repeatable upgrade: retain the authored room, furniture and lighting bake.
public static class StudioPolishBuilder {
 const string Root="Assets/Backbenchers";
 static readonly string[] Books={"rangoon.jpg", "mandalay.jpg", "ava1.jpg", "ava2.jpg", "beyond.jpg", "hantharwaddy.jpg", "real-life.png", "odyssey.jpg", "prisoner.png", "new-shoots.png", "democracy.jpg", "spring.png", "jolly.png", "gender.jpg", "dark.jpg", "freedom.jpg", "letters.jpg", "hope.jpg", "hidden.jpg", "china-india.jpg", "river.jpg", "modern.jpg", "peacemaker.jpg", "finding.jpg"};
 static readonly string[] Titles={"The Guys of Rangoon 1930", "The Guys from Mandalay 1950s", "AVA 1740s · Season 1", "AVA 1740s · Season 2", "Beyond AVA 1750s", "The Fall of Hantharwaddy 1757", "REAL LIFE / REAL STORIES", "The Odyssey", "Prisoner of Conscience", "Picking Off New Shoots Will Not Stop the Spring", "The People Demand Democracy", "Myanmar’s Spring Revolution", "Jolly Roger’s Cookbook", "Gender Line 2081", "PRINCES IN THE DARKNESS / 1970s", "Freedom from Fear", "Letters from Burma", "The Voice of Hope", "The Hidden History of Burma", "Where China Meets India", "The River of Lost Footsteps", "The Making of Modern Burma", "Peacemaker", "Finding George Orwell in Burma"};
 static readonly string[] Colors={"#344842", "#b69056", "#ae6345", "#587f81", "#778491", "#146573", "#224b4a", "#d6cdbb", "#78443c", "#a33731", "#cb922f", "#283a4a", "#242b28", "#c8c5b9", "#bf623a", "#9b403c", "#c4ab86", "#576e7d", "#9a7137", "#654e3b", "#434f65", "#9a986d", "#746b59", "#48554b"};
 static Transform root;
 static Material Mat(string name,string hex,Texture tex=null){var path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}ColorUtility.TryParseHtmlString(hex,out var color);m.color=color;m.mainTexture=tex;m.SetFloat("_Glossiness",.18f);m.enableInstancing=true;return m;}
 static GameObject Box(string name,Vector3 p,Vector3 size,Material m,Transform parent){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;}
 static Material Cover(int i){var m=Mat("Reading cover "+i,"#ffffff",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Books/"+Books[i]));
  // UV windows isolate the photographed jackets without altering the reference images.
  var scale=Vector2.one;var offset=Vector2.zero;
  if(i==3){scale=new Vector2(.83f,.9f);offset=new Vector2(.07f,.035f);}
  if(i==4){scale=new Vector2(.68f,.97f);offset=new Vector2(.15f,.01f);}
  if(i==7){scale=new Vector2(.74f,.79f);offset=new Vector2(.115f,.02f);}
  m.mainTextureScale=scale;m.mainTextureOffset=offset;return m;
 }
 static void Book(int index,Vector3 p,Quaternion rotation,float height=.24f){int i=index;float w=height*.68f,thickness=.023f;
  var g=new GameObject("Reading / "+Titles[i]);g.transform.SetParent(root,false);g.transform.SetPositionAndRotation(p,rotation);g.isStatic=true;
  var paper=Mat("Reading paper","#ddd3b8");var cloth=Mat("Reading cloth "+i,Colors[i]);
  Box("Paper edges",Vector3.zero,new Vector3(w-.008f,height-.009f,thickness),paper,g.transform);
  Box("Cloth binding",new Vector3(-w/2+.003f,0,0),new Vector3(.009f,height,thickness+.005f),cloth,g.transform);
  var band=Mat("Reading binding foil","#b59a63");
  for(int k=0;k<2;k++)Box("Spine foil",new Vector3(-w/2-.002f,(k==0?-.34f:.34f)*height,0),new Vector3(.001f,.002f,thickness+.006f),band,g.transform);
  Box("Back jacket",new Vector3(0,0,.014f),new Vector3(w,height,.003f),cloth,g.transform);
  var face=GameObject.CreatePrimitive(PrimitiveType.Quad);face.name="Book jacket";face.transform.SetParent(g.transform,false);face.transform.localPosition=new Vector3(0,0,-.014f);face.transform.localScale=new Vector3(w,height,1);face.GetComponent<Renderer>().sharedMaterial=Cover(i);UnityEngine.Object.DestroyImmediate(face.GetComponent<Collider>());face.isStatic=true;
  var spine=GameObject.CreatePrimitive(PrimitiveType.Quad);spine.name="Printed spine";spine.transform.SetParent(g.transform,false);spine.transform.localPosition=new Vector3(-w/2-.003f,0,0);spine.transform.localRotation=Quaternion.Euler(0,90,0);spine.transform.localScale=new Vector3(thickness+.004f,height,1);spine.GetComponent<Renderer>().sharedMaterial=Mat("Reading spine "+i,"#ffffff",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Books/"+System.IO.Path.GetFileNameWithoutExtension(Books[i])+"-spine.png"));UnityEngine.Object.DestroyImmediate(spine.GetComponent<Collider>());spine.isStatic=true;
  var c=g.AddComponent<BoxCollider>();c.size=new Vector3(w,height,.034f);g.AddComponent<StudioInspectable>().topic="book:"+System.IO.Path.GetFileNameWithoutExtension(Books[i]);
 }
 static void Move(string name,Vector3 center){var g=GameObject.Find(name);if(!g)return;var rs=g.GetComponentsInChildren<Renderer>();if(rs.Length==0)return;var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);g.transform.position+=center-b.center;}
 static GameObject Rounded(string name,Vector3 p,Vector3 size,float radius,Material mat,Transform parent){
  // Four bevel rings around a rounded rectangle; 130 vertices, shared static mesh.
  string path=Root+"/DisplayMeshes/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(!mesh){
   var vs=new System.Collections.Generic.List<Vector3>();var ts=new System.Collections.Generic.List<int>();
   const int N=32;float bevel=Mathf.Min(.002f,size.y*.18f);
   float[] ys={-size.y/2,-size.y/2+bevel,size.y/2-bevel,size.y/2};
   for(int ring=0;ring<4;ring++)for(int i=0;i<N;i++){
    int quadrant=i/8;float a=(quadrant*90+(i%8)*90f/7)*Mathf.Deg2Rad;
    float inset=(ring==0||ring==3)?bevel:0;float r=radius-inset;
    float cx=(quadrant==0||quadrant==3?1:-1)*(size.x/2-radius);
    float cz=(quadrant<2?1:-1)*(size.z/2-radius);
    vs.Add(new Vector3(cx+Mathf.Cos(a)*r,ys[ring],cz+Mathf.Sin(a)*r));
   }
   for(int ring=0;ring<3;ring++)for(int i=0;i<N;i++){int a=ring*N+i,b=ring*N+(i+1)%N,c=a+N,d=b+N;ts.AddRange(new[]{a,c,b,b,c,d});}
   vs.Add(new Vector3(0,-size.y/2,0));vs.Add(new Vector3(0,size.y/2,0));
   for(int i=0;i<N;i++){int j=(i+1)%N;ts.AddRange(new[]{128,i,j,129,96+j,96+i});}
   mesh=new Mesh{name=name};mesh.SetVertices(vs);mesh.SetTriangles(ts,0);mesh.RecalculateNormals();mesh.RecalculateBounds();Unwrapping.GenerateSecondaryUVSet(mesh);AssetDatabase.CreateAsset(mesh,path);
  }
  var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=p;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;g.isStatic=true;return g;
 }
 static void Mini(){
  var silver=Mat("Mini anodized aluminium","#bec3c4");silver.SetFloat("_Metallic",.62f);silver.SetFloat("_Glossiness",.42f);
  var dark=Mat("Mini graphite","#171d1e");
  var mini=new GameObject("Merlin Mac mini study");mini.transform.SetParent(root);mini.transform.position=new Vector3(17.23f,.803f,7.57f);mini.transform.rotation=Quaternion.Euler(0,90,0);
  Rounded("Mini rounded aluminium shell",new Vector3(0,.031f,0),new Vector3(.16f,.055f,.16f),.022f,silver,mini.transform);
  Rounded("Mini recessed ventilation base",new Vector3(0,.003f,0),new Vector3(.132f,.006f,.132f),.024f,dark,mini.transform);
  for(int i=0;i<2;i++){var port=Rounded("Mini USB-C "+i,new Vector3(-.037f+i*.036f,.027f,-.0802f),new Vector3(.015f,.002f,.006f),.003f,dark,mini.transform);port.transform.localRotation=Quaternion.Euler(90,0,0);}
  var jack=GameObject.CreatePrimitive(PrimitiveType.Cylinder);jack.name="Mini headphone jack";jack.transform.SetParent(mini.transform,false);jack.transform.localPosition=new Vector3(.04f,.027f,-.0802f);jack.transform.localRotation=Quaternion.Euler(90,0,0);jack.transform.localScale=new Vector3(.005f,.001f,.005f);jack.GetComponent<Renderer>().sharedMaterial=dark;UnityEngine.Object.DestroyImmediate(jack.GetComponent<Collider>());jack.isStatic=true;
  var led=Mat("Mini power LED","#d8f4e6");led.EnableKeyword("_EMISSION");led.SetColor("_EmissionColor",Color.white*.35f);
  Box("Mini power indicator",new Vector3(.059f,.027f,-.0805f),new Vector3(.002f,.002f,.001f),led,mini.transform);
  for(int i=0;i<3;i++)Box("Mini rear connection",new Vector3(-.04f+i*.04f,.026f,.0802f),new Vector3(.017f,.007f,.001f),dark,mini.transform);
  for(int i=0;i<8;i++)Box("Mini base vent",new Vector3(-.052f+i*.015f,.009f,.069f),new Vector3(.007f,.004f,.001f),dark,mini.transform);
  Box("Mini display cable",new Vector3(17.38f,.803f,7.57f),new Vector3(.15f,.004f,.004f),dark,root);
  Box("Mini cable return",new Vector3(17.454f,.803f,7.60f),new Vector3(.004f,.004f,.06f),dark,root);
 }
 static void PortraitAndPhone(){
  var frame=GameObject.Find("Picture 1");
  if(frame){
   var existing=frame.transform.Find("Aung San Suu Kyi pencil print");if(existing)UnityEngine.Object.DestroyImmediate(existing.gameObject);
   var print=GameObject.CreatePrimitive(PrimitiveType.Quad);print.name="Aung San Suu Kyi pencil print";print.transform.SetParent(frame.transform,false);print.transform.localPosition=new Vector3(0,0,.013f);print.transform.localRotation=Quaternion.Euler(0,180,0);print.transform.localScale=new Vector3(.288f,.434f,1);print.GetComponent<Renderer>().sharedMaterial=Mat("Faceless pencil portrait","#ffffff",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/aung-san-suu-kyi-pencil.png"));UnityEngine.Object.DestroyImmediate(print.GetComponent<Collider>());print.isStatic=true;
   // Move the framed print into the clear rear corner beside the drone.
   Move("Picture 1",new Vector3(17.61f,1.045f,8.16f));
   Move("Picture 2",new Vector3(17.61f,.947f,7.91f));
  }
  var g=new GameObject("EGUnion mobile development phone");g.transform.SetParent(root);g.transform.position=new Vector3(16.99f,.808f,7.79f);g.transform.rotation=Quaternion.Euler(0,-8,0);
  var metal=Mat("Phone graphite aluminium","#3b4646");metal.SetFloat("_Metallic",.7f);
  var black=Mat("Phone glass bezel","#080e0f");black.SetFloat("_Glossiness",.65f);
  Rounded("Phone rounded body",Vector3.zero,new Vector3(.082f,.009f,.16f),.01f,metal,g.transform);
  Rounded("Phone black glass",new Vector3(0,.005f,0),new Vector3(.079f,.002f,.157f),.009f,black,g.transform);
  var face=GameObject.CreatePrimitive(PrimitiveType.Quad);face.name="EGUnion phone display";face.transform.SetParent(g.transform,false);face.transform.localPosition=new Vector3(0,.0065f,0);face.transform.localRotation=Quaternion.Euler(90,0,180);face.transform.localScale=new Vector3(.070f,.146f,1);
  var screen=Mat("EGUnion mobile screen","#ffffff",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/egunion-phone-screen.png"));screen.EnableKeyword("_EMISSION");screen.SetColor("_EmissionColor",Color.white*.4f);screen.SetTexture("_EmissionMap",screen.mainTexture);face.GetComponent<Renderer>().sharedMaterial=screen;UnityEngine.Object.DestroyImmediate(face.GetComponent<Collider>());face.isStatic=true;
  Box("Phone earpiece",new Vector3(0,.0075f,-.068f),new Vector3(.025f,.001f,.003f),black,g.transform);
  Box("Phone volume button",new Vector3(-.0415f,0,-.026f),new Vector3(.002f,.004f,.022f),metal,g.transform);
  Box("Phone power button",new Vector3(.0415f,0,-.021f),new Vector3(.002f,.004f,.014f),metal,g.transform);
  var c=g.AddComponent<BoxCollider>();c.size=new Vector3(.085f,.018f,.164f);
  var d=g.AddComponent<StudioDevice>();d.deviceId="benchphone";d.displayName="EGUnion / mobile development";d.screenRenderer=face.GetComponent<Renderer>();d.materialIndex=0;d.pages=new[]{(Texture2D)screen.mainTexture,AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/studio-screen-content.jpg"),AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/studio-screen-drone.jpg")};d.page=0;
 }
 [MenuItem("Backbenchers/Upgrade reading room and workstations")]
 public static void Upgrade(){
  var previous=GameObject.Find("Backbenchers reading room");if(previous)UnityEngine.Object.DestroyImmediate(previous);
  root=new GameObject("Backbenchers reading room").transform;
  var original=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>(r.name.StartsWith("Books ")||r.name=="Book 1"||r.name=="Magazine"||r.name=="Magazines")&&!r.transform.IsChildOf(root)).OrderBy(r=>r.name).ToArray();
  var remaining=new System.Collections.Generic.Queue<int>(Enumerable.Range(0,Books.Length).Where(i=>i!=0&&i!=5&&i!=7));
  int group=0;foreach(var r in original){var b=r.bounds;var pos=b.center;r.gameObject.SetActive(false);
   int available=Mathf.Max(1,remaining.Count-(original.Length-++group));
   if(b.size.y<.1f){
    // Keep the Demo's horizontal piles, with a readable jacket on the top volume.
    int layers=Mathf.Min(available,r.name=="Book 1"?1:3);
    for(int k=0;k<layers;k++)Book(remaining.Dequeue(),new Vector3(pos.x,b.min.y+.016f+k*.032f,pos.z),Quaternion.Euler(90,0,k*3-3),Mathf.Min(.28f,b.size.z*.83f));
    continue;
   }
   bool west=pos.x<12;float width=west?b.size.z:b.size.x;
   int count=Mathf.Min(available,Mathf.Clamp(Mathf.FloorToInt(width/.045f),1,3));
   // A low stack and one outward-facing volume retain the Demo's clutter rhythm.
   for(int j=0;j<count;j++){
    bool featured=j==count-1;float h=Mathf.Min(.225f,b.size.y);
    var p=pos;p.y=b.min.y+(featured?(count-1)*.031f+h/2:.016f+j*.031f);
    Book(remaining.Dequeue(),p,featured?Quaternion.Euler(-4,west?270:180,0):Quaternion.Euler(90,0,west?90+j*3:j*3),h);
   }
  }
  if(remaining.Count>0)throw new InvalidOperationException("Not all distinct books received a placement.");
  // Two exhibition jackets, with breathing room; other books retain the Demo shelf rhythm.
  Book(0,new Vector3(13.73f,1.94f,5.53f),Quaternion.Euler(0,180,0),.27f);
  Book(5,new Vector3(14.53f,1.94f,5.53f),Quaternion.Euler(0,180,0),.25f);
  Book(7,new Vector3(14.46f,.824f,7.04f),Quaternion.Euler(90,0,-6),.24f);
  var notebook=GameObject.Find("Thomas notebook");if(notebook)notebook.SetActive(false);
  // Dedicated clear zones on the worktops; remove the redundant under-book tablet-shaped prop.
  Move("Camera model",new Vector3(14.56f,.85f,7.30f));
  var hardDisk=GameObject.Find("Hard disk");if(hardDisk)hardDisk.SetActive(false);
  Move("Merlin compact keyboard",new Vector3(16.99f,.811f,7.57f));
  Move("File 5 (2)",new Vector3(17.53f,.83f,7.13f));
  var phantom=GameObject.Find("EGUnion Phantom research model");
  if(phantom){var rs=phantom.GetComponentsInChildren<Renderer>();var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);phantom.transform.localScale*=.40f/bounds.size.z;Move(phantom.name,new Vector3(17.08f,.872f,8.13f));}
  Mini();
  PortraitAndPhone();
  // Taller, wider curved panels, while the authored stand stays planted on the worktop.
  foreach(var d in UnityEngine.Object.FindObjectsByType<StudioDevice>(FindObjectsSortMode.None)){
   if(d.deviceId!="thomas"&&d.deviceId!="hlaing"&&d.deviceId!="merlin")continue;
   var mf=d.screenRenderer.GetComponent<MeshFilter>();var old=mf.sharedMesh;
   string path=Root+"/DisplayMeshes/Immersive monitor.asset";
   var improved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
   if(!improved){
    improved=UnityEngine.Object.Instantiate(old);improved.name="Immersive curved monitor";
    var v=improved.vertices;
    for(int i=0;i<v.Length;i++)if(v[i].y>-.14f){v[i].x*=1.10f;v[i].y=-.14f+(v[i].y+.14f)*1.22f;v[i].z+=.035f*Mathf.Pow(v[i].x/.40f,2);}
    improved.vertices=v;improved.RecalculateNormals();improved.RecalculateBounds();AssetDatabase.CreateAsset(improved,path);
   }
   mf.sharedMesh=improved;
   if(d.deviceId=="merlin"){var p=d.transform.position;p.y=1f;d.transform.position=p;}
   // Mesh UVs remain intact; the collider must follow the visible panel.
   var mc=d.screenRenderer.GetComponent<MeshCollider>();if(mc)mc.sharedMesh=improved;
  }
  // Cache the exact submesh corners at authoring time; no readable meshes or per-frame allocations needed.
  foreach(var d in UnityEngine.Object.FindObjectsByType<StudioDevice>(FindObjectsSortMode.None)){
   var mesh=d.screenRenderer.GetComponent<MeshFilter>().sharedMesh;var ix=mesh.GetTriangles(d.materialIndex);var vertices=mesh.vertices;var b=new Bounds(vertices[ix[0]],Vector3.zero);foreach(var j in ix)b.Encapsulate(vertices[j]);
   d.screenCenter=b.center;d.screenNormal=(d.deviceId=="thomas"||d.deviceId=="hlaing"||d.deviceId=="merlin")?Vector3.forward:Vector3.back;
   d.screenCorners=new[]{new Vector3(b.min.x,b.min.y,b.center.z),new Vector3(b.max.x,b.min.y,b.center.z),new Vector3(b.max.x,b.max.y,b.center.z),new Vector3(b.min.x,b.max.y,b.center.z)};
   EditorUtility.SetDirty(d);
  }
  foreach(var path in Books.Select(x=>Root+"/Books/"+x)){var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;importer.maxTextureSize=1024;importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Clamp;importer.anisoLevel=4;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();}
  foreach(var filter in root.GetComponentsInChildren<MeshFilter>()){
   var mesh=filter.sharedMesh;if(mesh && mesh.uv2.Length==0 && AssetDatabase.GetAssetPath(mesh).StartsWith(Root+"/DisplayMeshes/")){Unwrapping.GenerateSecondaryUVSet(mesh);EditorUtility.SetDirty(mesh);}
  }
  StudioGalleryBuilder.Apply(Titles);
  StudioInteractionBuilder.Apply();
  PlayerSettings.bundleVersion="1.6.0";
  EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("Reading room upgraded: 24 distinct catalog entries, physical books, Merlin compact PC, calibrated screen corners.");
 }
}
