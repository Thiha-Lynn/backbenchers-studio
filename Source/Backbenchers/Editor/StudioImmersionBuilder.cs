using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Backbenchers;
public static class StudioImmersionBuilder {
 const string Root="Assets/Backbenchers";
 static Material Material(string name,Color color,Texture texture=null){var path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.mainTexture=texture;m.SetFloat("_Glossiness",.15f);return m;}
 static GameObject Box(string name,Transform parent,Vector3 position,Vector3 size,Material mat){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;return g;}
 static void ArtFrame(string name,Vector3 position,float yaw,Vector2 size,Material material,string topic){
  var root=GameObject.Find("Backbenchers gallery").transform;var previous=GameObject.Find(name);if(previous)UnityEngine.Object.DestroyImmediate(previous);
  var g=new GameObject(name);g.transform.SetParent(root);g.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
  var oak=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Gallery warm oak.mat");var paper=Material("Flight paper",new Color(.88f,.84f,.73f));
  Box("Oak shadow frame",g.transform,Vector3.zero,new Vector3(size.x+.075f,size.y+.075f,.024f),oak);
  Box("Cotton mat",g.transform,new Vector3(0,0,-.014f),new Vector3(size.x+.04f,size.y+.04f,.006f),paper);
  var art=GameObject.CreatePrimitive(PrimitiveType.Quad);art.name="Framed artwork";art.transform.SetParent(g.transform,false);art.transform.localPosition=new Vector3(0,0,-.019f);art.transform.localScale=new Vector3(size.x,size.y,1);art.GetComponent<Renderer>().sharedMaterial=material;
  var item=g.AddComponent<StudioInspectable>();item.topic=topic;item.displayName=name;item.flat=true;item.outward=-g.transform.forward;
 }
 [MenuItem("Backbenchers/Upgrade immersive room")]
 public static void Apply(){
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  if(!scene.path.EndsWith("BackbenchersStudio.unity"))scene=EditorSceneManager.OpenScene(Root+"/Scenes/BackbenchersStudio.unity");
  var e=UnityEngine.Object.FindFirstObjectByType<StudioExperience>();var room=e.GetComponent<StudioRoom>();if(!room)room=e.gameObject.AddComponent<StudioRoom>();
  var portrait=GameObject.Find("Wall portrait / Aung San Suu Kyi");if(portrait)portrait.transform.SetPositionAndRotation(new Vector3(17.63f,1.94f,8.37f),Quaternion.Euler(0,90,0));
  var river=GameObject.Find("Gallery / Irrawaddy");if(river)river.transform.position=new Vector3(12.40f,1.93f,5.34f);
  ArtFrame("Gallery / Jasmine",new Vector3(17.63f,2.20f,8.94f),90,new Vector2(.22f,.44f),AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Nature study 0.mat"),"nature-art");
  ArtFrame("Gallery / Flight",new Vector3(17.63f,1.60f,8.94f),90,new Vector2(.22f,.44f),AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Nature study 2.mat"),"nature-art");
  // Return the original small desk frames, keeping the portrait unique on the reading wall.
  foreach(var name in new[]{"Picture 1","Picture 2"}){var t=Resources.FindObjectsOfTypeAll<Transform>().FirstOrDefault(x=>x.gameObject.scene.IsValid()&&x.name==name);if(!t)continue;t.gameObject.SetActive(true);var overlay=t.Find("Aung San Suu Kyi pencil print");if(overlay)UnityEngine.Object.DestroyImmediate(overlay.gameObject);var item=t.GetComponent<StudioInspectable>()??t.gameObject.AddComponent<StudioInspectable>();item.topic="desk-art";item.displayName="Collected desk frame";if(!t.GetComponent<Collider>())t.gameObject.AddComponent<BoxCollider>();}

  foreach(var g in UnityEngine.Object.FindObjectsByType<StudioInspectable>(FindObjectsSortMode.None).Where(i=>i.topic=="flight-art").ToArray())UnityEngine.Object.DestroyImmediate(g.gameObject);
  // Preserve the plotter's curved paper mesh and give its print surface full-page UVs.
  var plotter=GameObject.Find("Plotter");var mf=plotter.GetComponent<MeshFilter>();var path=Root+"/DisplayMeshes/Flight plotter.asset";
  var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(!mesh){mesh=UnityEngine.Object.Instantiate(mf.sharedMesh);mesh.name="Flight plotter";var indices=mesh.GetTriangles(1).Distinct().ToArray();var uv=mesh.uv;var min=new Vector2(indices.Min(i=>uv[i].x),indices.Min(i=>uv[i].y));var max=new Vector2(indices.Max(i=>uv[i].x),indices.Max(i=>uv[i].y));foreach(int i in indices)uv[i]=new Vector2((uv[i].x-min.x)/(max.x-min.x),(uv[i].y-min.y)/(max.y-min.y));mesh.uv=uv;AssetDatabase.CreateAsset(mesh,path);}mf.sharedMesh=mesh;
  var materials=plotter.GetComponent<Renderer>().sharedMaterials;materials[1]=Material("Flight plotter print",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/flight-wing.png"));plotter.GetComponent<Renderer>().sharedMaterials=materials;
  var hinge=GameObject.Find("Studio door hinge");if(!hinge){var leaf=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).First(r=>r.name=="Door");hinge=new GameObject("Studio door hinge");hinge.transform.SetParent(leaf.transform.parent,true);hinge.transform.position=leaf.transform.position;leaf.transform.SetParent(hinge.transform,true);var handle=GameObject.Find("crank");if(handle)handle.transform.SetParent(hinge.transform,true);}
  foreach(var r in hinge.GetComponentsInChildren<Renderer>())if(!r.GetComponent<Collider>())r.gameObject.AddComponent<BoxCollider>();
  room.door=hinge.transform;var action=hinge.GetComponent<StudioInspectable>()??hinge.AddComponent<StudioInspectable>();action.topic="room:door";action.displayName="Open / close studio door";
  var sw=GameObject.Find("Switcher");var si=sw.GetComponent<StudioInspectable>()??sw.AddComponent<StudioInspectable>();si.topic="room:lights";si.displayName="Switch room lights on / off";
  if(!sw.GetComponent<Collider>())sw.AddComponent<BoxCollider>();
  var rocker=GameObject.Find("Working light rocker")??Box("Working light rocker",sw.transform,new Vector3(0,0,.014f),new Vector3(.029f,.065f,.014f),Material("Switch ivory",new Color(.84f,.80f,.68f)));room.switchRocker=rocker.GetComponent<Renderer>();
  room.roomLights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>!l.GetComponentInParent<StudioDevice>()).ToArray();
  // Door leaf and inspectable models must remain movable rather than static-batched.
  StudioInteractionBuilder.Apply();
  action.displayName="Studio door / open or close";si.displayName="Room lights / on or off";plotter.GetComponent<StudioInspectable>().topic="flight-print";plotter.GetComponent<StudioInspectable>().displayName="RC sailplane / plotter print";
  foreach(var item in UnityEngine.Object.FindObjectsByType<StudioInspectable>(FindObjectsSortMode.None))foreach(var r in item.GetComponentsInChildren<Renderer>()){
   GameObjectUtility.SetStaticEditorFlags(r.gameObject,GameObjectUtility.GetStaticEditorFlags(r.gameObject)&~StaticEditorFlags.BatchingStatic);
   if(item.topic=="portrait-art"||item.topic=="nature-art"||item.topic=="flight-art"||item.topic.StartsWith("room:")){r.lightmapIndex=-1;GameObjectUtility.SetStaticEditorFlags(r.gameObject,item.topic.StartsWith("room:")?0:StaticEditorFlags.ContributeGI);if(!item.topic.StartsWith("room:")&&r is MeshRenderer mr)mr.receiveGI=ReceiveGI.Lightmaps;}
  }
  foreach(var i in new[]{action,si})EditorUtility.SetDirty(i);
  if(portrait){var item=portrait.GetComponent<StudioInspectable>();item.flat=true;item.outward=Vector3.left;}
  // A softly lit vestibule gives the open door depth while keeping the tour inside the office.
  if(!GameObject.Find("Studio vestibule")){var g=new GameObject("Studio vestibule");var wall=Material("Vestibule plaster",new Color(.36f,.40f,.34f));Box("Hall beyond door",g.transform,new Vector3(16.56f,1.15f,10.7f),new Vector3(1.1f,2.3f,.06f),wall);Box("Hall floor",g.transform,new Vector3(16.56f,-.015f,10.4f),new Vector3(1.1f,.04f,.7f),wall);}
  StudioArtBuilder.Apply();StudioTechnologyBuilder.Apply();PlayerSettings.bundleVersion="1.9";EditorUtility.SetDirty(room);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("Immersive room 1.7 applied");
 }
 public static void Build(){var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"/Scenes/BackbenchersStudio.unity"},locationPathName=System.IO.Path.GetFullPath("../studio-web/unity"),target=BuildTarget.WebGL,options=BuildOptions.None});System.IO.File.WriteAllText("Inspection/immersion-build.json",JsonUtility.ToJson(new BuildResult{result=report.summary.result.ToString(),errors=report.summary.totalErrors,bytes=report.summary.totalSize},true));if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Web build failed");}
 [Serializable] class BuildResult{public string result;public int errors;public ulong bytes;}
}
