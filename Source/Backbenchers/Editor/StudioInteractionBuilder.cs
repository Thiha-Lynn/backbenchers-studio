using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Backbenchers;
public static class StudioInteractionBuilder {
 static StudioInspectable Add(GameObject g,string topic,string label,bool flat=false,Vector3 outward=default){
  var item=g.GetComponent<StudioInspectable>();if(!item)item=g.AddComponent<StudioInspectable>();item.topic=topic;item.displayName=label;item.flat=flat;item.outward=outward;
  if(!g.GetComponent<Collider>()){
   var rs=g.GetComponentsInChildren<Renderer>();var b=new Bounds();bool first=true;
   foreach(var r in rs){var wb=r.bounds;for(int i=0;i<8;i++){var p=g.transform.InverseTransformPoint(wb.center+Vector3.Scale(wb.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));if(first){b=new Bounds(p,Vector3.zero);first=false;}else b.Encapsulate(p);}}
   var c=g.AddComponent<BoxCollider>();c.center=b.center;c.size=Vector3.Max(b.size,new Vector3(.012f,.012f,.012f));c.isTrigger=true;
  }
  EditorUtility.SetDirty(item);return item;
 }
 [MenuItem("Backbenchers/Apply object interactions")]
 public static void Apply(){
  foreach(var item in Object.FindObjectsByType<StudioInspectable>(FindObjectsSortMode.None)){
   item.displayName=item.name.Replace("Reading / ","").Replace("EGUnion ","").Replace(" research model","");
   if(item.topic=="portrait-art"||item.topic=="nature-art"||item.topic=="identity"){item.flat=true;item.outward=-item.transform.forward;}
   if(item.topic=="drones")item.topic=item.name.Contains("Phantom")?"drone:phantom":"drone:racer";
   EditorUtility.SetDirty(item);
  }
  string[] names={"Thomas D. Lynn","Hlaing Gyi","Trafalgar D. Merlin"};string[] ids={"thomas","hlaing","merlin"};
  for(int i=0;i<3;i++){var g=GameObject.Find("Bounty poster - "+names[i]);if(g)Add(g,"bounty:"+ids[i],names[i]+" · bounty poster",true,Vector3.back);}
  var mini=GameObject.Find("Merlin Mac mini study");if(mini)Add(mini,"hardware:mini","Merlin’s Mac mini");
  var logo=GameObject.Find("Backbenchers logo board");if(logo)Add(logo,"identity","Backbenchers Studio",true,Vector3.right);
  foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
   if(r.GetComponentInParent<StudioInspectable>()||r.GetComponentInParent<StudioDevice>())continue;
   string n=r.name.ToLowerInvariant(),topic=null,label=null;
   if(n.StartsWith("computer")){topic="hardware:pc";label="Studio PC tower";}
   else if(n.StartsWith("plant")){topic="nature";label="A little greenery";}
   else if(n.StartsWith("file ")){topic="archive";label="Studio project archive";}
   else if(n.StartsWith("box ")){topic="archive";label="Materials & keepsakes";}
   else if(n=="camera model"||n=="tripod"){topic="photography";label=n=="tripod"?"Studio tripod":"Studio camera";}
   else if(n.Contains("keyboard")||n.StartsWith("mouse")){topic="tools";label=n.Contains("keyboard")?"Workbench keyboard":"Studio mouse";}
   else if(n=="printer"||n=="plotter"){topic="print";label=n=="printer"?"Desk printer":"Large-format plotter";}
   else if(n=="lamp"||n.StartsWith("ceiling lamp")){topic="lighting";label="Warm studio light";}
   else if(n=="cup"){topic="coffee";label="Coffee break";}
   else if(n=="prototype circuit board"){topic="electronics";label="Prototype circuit board";}
   else if(n.StartsWith("bench battery")){topic="electronics";label="Bench battery study";}
   else if(n.StartsWith("canvas ")||n.StartsWith("paper")||n.StartsWith("pen")||n.StartsWith("marker")||n=="eraser"||n=="posit"){topic="making";label="Marks, sketches & ideas";}
   else if(n=="geosphere"||n=="letter a"||n.StartsWith("vase")){topic="objects";label="Objects we keep";}
   else if(n.StartsWith("chair")||n=="merlin research chair"||n=="stool"){topic="furniture";label="A place at the studio";}
   else if(n.StartsWith("table")){topic="furniture";label="Our shared workbench";}
   else if(n.StartsWith("bookcase")){topic="shelves";label="The working shelf";}
   else if(n.StartsWith("trashcan")||n=="hanger"){topic="objects";label="Everyday studio essentials";}
   if(topic!=null)Add(r.gameObject,topic,label);
  }
  PlayerSettings.bundleVersion="1.6.0";
  EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
  Debug.Log("Object prompts installed: "+Object.FindObjectsByType<StudioInspectable>(FindObjectsSortMode.None).Length+" objects and "+Object.FindObjectsByType<StudioDevice>(FindObjectsSortMode.None).Length+" devices.");
 }
}
