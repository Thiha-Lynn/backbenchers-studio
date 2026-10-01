using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Backbenchers;
// Uses the regenerated paper scans, keeping the source photographs in the web archive, mounted on subtly curled cotton sheets.
public static class StudioArtBuilder {
 const string Root="Assets/Backbenchers";
 static Material Mat(string name,Color color,Texture texture=null){string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.shader=Shader.Find("Backbenchers/ArtPaper");m.color=color;m.mainTexture=texture;return m;}
 static void Sheet(Transform parent,string id,string title,Vector3 position,float yaw,float width,float height){
  var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/"+id+"-paper.png");var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));importer.maxTextureSize=1024;importer.mipmapEnabled=true;importer.anisoLevel=4;importer.SaveAndReimport();
  var root=new GameObject("Drawing paper / "+title);root.transform.SetParent(parent);root.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
  var item=root.AddComponent<StudioInspectable>();item.topic="art:"+id;item.displayName=title+" / pencil drawing";item.flat=true;item.outward=Vector3.up;
  var collider=root.AddComponent<BoxCollider>();collider.size=new Vector3(width+.02f,.015f,height+.02f);
  var backing=GameObject.CreatePrimitive(PrimitiveType.Cube);backing.name="Cotton paper edge";backing.transform.SetParent(root.transform,false);backing.transform.localScale=new Vector3(width+.016f,.0015f,height+.016f);backing.GetComponent<Renderer>().sharedMaterial=Mat("Drawing cotton",new Color(.99f,.985f,.965f));Object.DestroyImmediate(backing.GetComponent<Collider>());
  const int n=12;var vertices=new Vector3[(n+1)*(n+1)];var uv=new Vector2[vertices.Length];var triangles=new int[n*n*6];int k=0;
  for(int row=0;row<=n;row++)for(int col=0;col<=n;col++){float u=(float)col/n,v=(float)row/n;int i=row*(n+1)+col;vertices[i]=new Vector3((u-.5f)*width,.001f+.0035f*Mathf.Pow(u,6)*Mathf.Pow(v,3),(v-.5f)*height);uv[i]=new Vector2(u,v);if(row<n&&col<n){triangles[k++]=i;triangles[k++]=i+n+1;triangles[k++]=i+1;triangles[k++]=i+1;triangles[k++]=i+n+1;triangles[k++]=i+n+2;}}
  string path=Root+"/DisplayMeshes/Drawing "+id+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}mesh.Clear();mesh.name="Curled drawing "+id;mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
  var face=new GameObject("Regenerated pencil study");face.transform.SetParent(root.transform,false);face.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=face.AddComponent<MeshRenderer>();renderer.sharedMaterial=Mat("Drawing "+id,Color.white,tex);renderer.receiveShadows=true;
 }
 static void TablePart(Transform parent,string name,Vector3 position,Vector3 size,Material mat){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.position=position;g.transform.localScale=size;var renderer=g.GetComponent<MeshRenderer>();renderer.sharedMaterial=mat;renderer.receiveGI=ReceiveGI.Lightmaps;GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.ContributeGI);}
 static void DraftStack(Transform parent,Vector3 position,Vector2 size,float yaw){for(int i=0;i<4;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Loose drafting paper";g.transform.SetParent(parent);g.transform.SetPositionAndRotation(position+new Vector3(i*.002f,i*.0015f,0),Quaternion.Euler(0,yaw+i*1.5f,0));g.transform.localScale=new Vector3(size.x,.0008f,size.y);g.GetComponent<Renderer>().sharedMaterial=Mat("Drawing cotton",new Color(.99f,.985f,.965f));Object.DestroyImmediate(g.GetComponent<Collider>());}}
 static void Supply(string original,Transform parent,string name,Vector3 center,float yaw){var source=GameObject.Find(original);if(!source)return;var g=Object.Instantiate(source,parent);g.name=name;g.transform.Rotate(Vector3.up,yaw,Space.World);var renderers=g.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);g.transform.position+=center-bounds.center;foreach(var r in renderers){r.lightmapIndex=-1;GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);}}
 [MenuItem("Backbenchers/Place drawing collection")]
 public static void Apply(){
  var old=GameObject.Find("Studio drawing collections");if(old)Object.DestroyImmediate(old);
  var root=new GameObject("Studio drawing collections").transform;
  foreach(var name in new[]{"Paper 1","Paper 2"}){var t=Resources.FindObjectsOfTypeAll<Transform>().FirstOrDefault(t=>t.gameObject.scene.IsValid()&&t.name==name);if(t)t.gameObject.SetActive(true);}
  var oldTable=GameObject.Find("Studio drawing table");if(oldTable)Object.DestroyImmediate(oldTable);
  var table=new GameObject("Studio drawing table").transform;
  var model=GameObject.Find("Table").GetComponent<Renderer>();var wood=model.sharedMaterials.First(m=>m.name.Contains("wood"));var metal=model.sharedMaterials.First(m=>m.name.Contains("metal"));
  TablePart(table,"Oak drawing surface",new Vector3(14.53f,.7425f,9.86f),new Vector3(1.48f,.035f,.40f),wood);
  foreach(float x in new[]{13.88f,15.18f}){foreach(float z in new[]{9.71f,10.01f})TablePart(table,"Ivory table leg",new Vector3(x,.36f,z),new Vector3(.03f,.72f,.03f),metal);TablePart(table,"Ivory foot rail",new Vector3(x,.035f,9.86f),new Vector3(.032f,.025f,.34f),metal);}
  TablePart(table,"Rear oak apron",new Vector3(14.53f,.69f,10.02f),new Vector3(1.32f,.07f,.025f),wood);
  // Preserve the main desks. Use measured gaps beside existing props.
  Sheet(root,"still-life","Still life",new Vector3(13.30f,.801f,7.52f),-3,.18f,.24f);
  Sheet(root,"tiger","Tiger study",new Vector3(13.645f,.801f,8.064f),174,.14f,.18f);
  Sheet(root,"itachi","Itachi study",new Vector3(13.689f,.807f,8.166f),184,.148f,.152f);
  // Two full drawings and quiet drafting stacks, rather than a row of prints.
  Sheet(root,"david","David study",new Vector3(14.46f,.762f,9.85f),-4,.22f,.293f);
  DraftStack(table,new Vector3(14.91f,.762f,9.85f),new Vector2(.23f,.32f),5);
  Sheet(root,"portrait","Portrait study",new Vector3(14.91f,.770f,9.85f),4,.21f,.315f);
  DraftStack(table,new Vector3(14.06f,.762f,9.85f),new Vector2(.18f,.255f),-7);
  Supply("Pencil",table,"Sketch pencil",new Vector3(14.03f,.776f,9.86f),-8);
  Supply("Pen",table,"Drawing pen",new Vector3(14.12f,.777f,9.82f),14);
  Supply("Eraser",table,"Drawing eraser",new Vector3(14.11f,.776f,9.96f),8);
  Supply("Pencil",root,"Creative desk pencil",new Vector3(13.775f,.814f,8.065f),0);
  // A flight note on the lab's existing storage box matches its working theme.
  var note=GameObject.CreatePrimitive(PrimitiveType.Quad);note.name="Lab paper / flight notes";note.transform.SetParent(root);note.transform.SetPositionAndRotation(new Vector3(17.535f,1.006f,6.82f),Quaternion.Euler(90,90,0));note.transform.localScale=new Vector3(.165f,.22f,1);note.GetComponent<Renderer>().sharedMaterial=Mat("Lab flight note",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/flight-wing.png"));Object.DestroyImmediate(note.GetComponent<Collider>());
  var experience=Object.FindFirstObjectByType<StudioExperience>();experience.viewpoints=experience.viewpoints.Take(5).Concat(new[]{new Vector3(14.90f,1.50f,8.70f)}).ToArray();experience.viewAngles=experience.viewAngles.Take(5).Concat(new[]{new Vector3(20,-18,0)}).ToArray();EditorUtility.SetDirty(experience);
  PlayerSettings.bundleVersion="1.8.1";EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
 }
}
