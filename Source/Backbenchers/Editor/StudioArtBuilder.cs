using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Backbenchers;
// Uses the regenerated paper scans, keeping the source photographs in the web archive, mounted on subtly curled cotton sheets.
public static class StudioArtBuilder {
 const string Root="Assets/Backbenchers";
 static Material Mat(string name,Color color,Texture texture=null){string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.mainTexture=texture;m.SetFloat("_Glossiness",.04f);return m;}
 static void Sheet(Transform parent,string id,string title,Vector3 position,float yaw,float width,float height){
  var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/"+id+"-paper.png");var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));importer.maxTextureSize=1024;importer.mipmapEnabled=true;importer.anisoLevel=4;importer.SaveAndReimport();
  var root=new GameObject("Drawing paper / "+title);root.transform.SetParent(parent);root.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
  var item=root.AddComponent<StudioInspectable>();item.topic="art:"+id;item.displayName=title+" / pencil drawing";item.flat=true;item.outward=Vector3.up;
  var collider=root.AddComponent<BoxCollider>();collider.size=new Vector3(width+.02f,.015f,height+.02f);
  var backing=GameObject.CreatePrimitive(PrimitiveType.Cube);backing.name="Cotton paper edge";backing.transform.SetParent(root.transform,false);backing.transform.localScale=new Vector3(width+.016f,.0015f,height+.016f);backing.GetComponent<Renderer>().sharedMaterial=Mat("Drawing cotton",new Color(.94f,.91f,.83f));Object.DestroyImmediate(backing.GetComponent<Collider>());
  const int n=12;var vertices=new Vector3[(n+1)*(n+1)];var uv=new Vector2[vertices.Length];var triangles=new int[n*n*6];int k=0;
  for(int row=0;row<=n;row++)for(int col=0;col<=n;col++){float u=(float)col/n,v=(float)row/n;int i=row*(n+1)+col;vertices[i]=new Vector3((u-.5f)*width,.001f+.0035f*Mathf.Pow(u,6)*Mathf.Pow(v,3),(.5f-v)*height);uv[i]=new Vector2(u,v);if(row<n&&col<n){triangles[k++]=i;triangles[k++]=i+1;triangles[k++]=i+n+1;triangles[k++]=i+1;triangles[k++]=i+n+2;triangles[k++]=i+n+1;}}
  string path=Root+"/DisplayMeshes/Drawing "+id+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}mesh.Clear();mesh.name="Curled drawing "+id;mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
  var face=new GameObject("Regenerated pencil study");face.transform.SetParent(root.transform,false);face.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=face.AddComponent<MeshRenderer>();renderer.sharedMaterial=Mat("Drawing "+id,Color.white,tex);renderer.receiveShadows=true;
 }
 [MenuItem("Backbenchers/Place drawing collection")]
 public static void Apply(){
  var old=GameObject.Find("Studio drawing collections");if(old)Object.DestroyImmediate(old);
  var root=new GameObject("Studio drawing collections").transform;
  foreach(var name in new[]{"Paper 1","Paper 2"}){var t=Resources.FindObjectsOfTypeAll<Transform>().FirstOrDefault(t=>t.gameObject.scene.IsValid()&&t.name==name);if(t)t.gameObject.SetActive(false);}
  Sheet(root,"itachi","Itachi study",new Vector3(14.36f,.803f,8.14f),-8,.235f,.235f);
  Sheet(root,"tiger","Tiger study",new Vector3(14.52f,.806f,7.99f),7,.18f,.232f);
  Sheet(root,"still-life","Still life",new Vector3(14.51f,.81f,8.22f),-4,.20f,.267f);
  Sheet(root,"david","David study",new Vector3(14.48f,.803f,7.30f),174,.20f,.267f);
  Sheet(root,"portrait","Portrait study",new Vector3(14.48f,.807f,7.08f),186,.19f,.285f);
  PlayerSettings.bundleVersion="1.8.0";EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
 }
}
