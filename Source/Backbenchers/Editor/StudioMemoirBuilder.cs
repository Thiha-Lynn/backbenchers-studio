using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Backbenchers;
public static class StudioMemoirBuilder {
 const string Root="Assets/Backbenchers";
 static Material Mat(string name,Color color,Texture texture=null){string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.mainTexture=texture;m.SetFloat("_Glossiness",.12f);return m;}
 static void Part(Transform parent,string name,Vector3 p,Vector3 scale,Material mat){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(g.GetComponent<Collider>());}
 static void Book(Transform root,string id,string title,Vector3 p,Quaternion rotation,float height){var g=new GameObject("Memoir / "+title);g.transform.SetParent(root);g.transform.SetPositionAndRotation(p,rotation);float width=height*.72f;var cloth=Mat("Memoir cloth "+id,new Color(.23f,.34f,.32f));Part(g.transform,"Page block",Vector3.zero,new Vector3(width-.006f,height-.006f,.024f),Mat("Memoir paper",new Color(.91f,.88f,.79f)));Part(g.transform,"Spine",new Vector3(-width/2,0,0),new Vector3(.007f,height,.03f),cloth);Part(g.transform,"Back cover",new Vector3(0,0,.014f),new Vector3(width,height,.003f),cloth);var face=GameObject.CreatePrimitive(PrimitiveType.Quad);face.name="Publisher jacket";face.transform.SetParent(g.transform,false);face.transform.localPosition=new Vector3(0,0,-.014f);face.transform.localScale=new Vector3(width,height,1);face.GetComponent<Renderer>().sharedMaterial=Mat("Memoir jacket "+id,Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Books/"+id+".jpg"));face.GetComponent<Renderer>().sharedMaterial.shader=Shader.Find("Backbenchers/ArtPaper");Object.DestroyImmediate(face.GetComponent<Collider>());g.AddComponent<BoxCollider>().size=new Vector3(width+.045f,height+.045f,.045f);var item=g.AddComponent<StudioInspectable>();item.topic="book:"+id;item.displayName=title;}
 public static void Apply(){
 EditorSceneManager.OpenScene("Assets/Backbenchers/Scenes/BackbenchersStudio.unity");
 var old=GameObject.Find("Studio memoir desk");if(old)Object.DestroyImmediate(old);
 var root=new GameObject("Studio memoir desk").transform;
 Book(root,"memoir-june-11","June 11 / Thomas’s memoir",new Vector3(14.12f,.826f,7.00f),Quaternion.Euler(90,0,5),.30f);
 Book(root,"memoir-higher-education","အဆင့်မြင့်ပညာရေးဆီသို့ / Thomas’s memoir",new Vector3(14.46f,.826f,7.04f),Quaternion.Euler(90,0,-6),.30f);
 foreach(var item in Object.FindObjectsByType<StudioInspectable>(FindObjectsSortMode.None))if(item.topic=="book:odyssey"&&Vector3.Distance(item.transform.position,new Vector3(14.46f,.824f,7.04f))<.2f)item.transform.position=new Vector3(14.80f,.826f,7.04f);
 foreach(var id in new[]{"memoir-june-11","memoir-higher-education"}){var t=AssetImporter.GetAtPath(Root+"/Books/"+id+".jpg") as TextureImporter;if(t){t.maxTextureSize=1024;t.mipmapEnabled=true;t.isReadable=false;t.wrapMode=TextureWrapMode.Clamp;t.SaveAndReimport();}}
 EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
 }
 public static void Build(){Apply();var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Backbenchers/Scenes/BackbenchersStudio.unity"},locationPathName="/Users/thomas/Backbencher_Projects/Xue/studio-web/unity",target=BuildTarget.WebGL,options=BuildOptions.None});if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Web build failed");}

}
