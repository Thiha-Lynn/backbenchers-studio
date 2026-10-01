using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Backbenchers;
public static class StudioTechnologyBuilder {
 const string Root="Assets/Backbenchers";
 static Material Mat(string name,Color color,Texture texture=null){string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.mainTexture=texture;m.SetFloat("_Glossiness",.12f);return m;}
 static void Part(Transform parent,string name,Vector3 p,Vector3 scale,Material mat){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(g.GetComponent<Collider>());}
 static void Book(Transform root,string id,string title,Vector3 p,Quaternion rotation,float height){var g=new GameObject("Technology / "+title);g.transform.SetParent(root);g.transform.SetPositionAndRotation(p,rotation);float width=height*.72f;var cloth=Mat("Tech cloth "+id,new Color(.23f,.34f,.32f));Part(g.transform,"Page block",Vector3.zero,new Vector3(width-.006f,height-.006f,.024f),Mat("Tech paper",new Color(.91f,.88f,.79f)));Part(g.transform,"Spine",new Vector3(-width/2,0,0),new Vector3(.007f,height,.03f),cloth);Part(g.transform,"Back cover",new Vector3(0,0,.014f),new Vector3(width,height,.003f),cloth);var face=GameObject.CreatePrimitive(PrimitiveType.Quad);face.name="Publisher jacket";face.transform.SetParent(g.transform,false);face.transform.localPosition=new Vector3(0,0,-.014f);face.transform.localScale=new Vector3(width,height,1);face.GetComponent<Renderer>().sharedMaterial=Mat("Tech jacket "+id,Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Books/tech-"+id+".jpg"));face.GetComponent<Renderer>().sharedMaterial.shader=Shader.Find("Backbenchers/ArtPaper");Object.DestroyImmediate(face.GetComponent<Collider>());g.AddComponent<BoxCollider>().size=new Vector3(width,height,.035f);var item=g.AddComponent<StudioInspectable>();item.topic="book:tech-"+id;item.displayName=title;}
 public static void Apply(){var old=GameObject.Find("Studio technology shelf");if(old)Object.DestroyImmediate(old);var root=new GameObject("Studio technology shelf").transform;
 // Face-out on the existing shelf tops, separated from the studio logo and ornaments.
 Book(root,"agents-applications","AI Agents and Applications",new Vector3(13.99f,1.95f,5.53f),Quaternion.Euler(0,180,0),.28f);
 Book(root,"ai-engineering","AI Engineering",new Vector3(14.82f,1.95f,5.53f),Quaternion.Euler(0,180,0),.28f);
 Book(root,"agents-action","AI Agents in Action",new Vector3(11.62f,1.94f,6.64f),Quaternion.Euler(0,270,0),.26f);
 Book(root,"typescript","Learning TypeScript",new Vector3(11.62f,1.825f,8.13f),Quaternion.Euler(90,0,90),.25f);
 Book(root,"docker-lunches","Learn Docker in a Month of Lunches",new Vector3(11.62f,1.856f,8.13f),Quaternion.Euler(90,0,94),.25f);
 Book(root,"data-systems","Designing Data-Intensive Applications",new Vector3(11.62f,1.887f,8.13f),Quaternion.Euler(90,0,88),.28f);
 var phoneTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Brand/phone-home.png");foreach(var d in Object.FindObjectsByType<StudioDevice>(FindObjectsSortMode.None).Where(d=>d.deviceId=="phone"||d.deviceId=="benchphone")){d.displayName=d.deviceId=="phone"?"Studio phone":"Workbench phone";d.pages=new[]{phoneTexture};d.page=0;var materials=d.screenRenderer.sharedMaterials;var material=Mat("Studio phone home "+d.deviceId,Color.white,phoneTexture);material.EnableKeyword("_EMISSION");material.SetTexture("_EmissionMap",phoneTexture);material.SetColor("_EmissionColor",Color.white*.4f);materials[d.materialIndex]=material;d.screenRenderer.sharedMaterials=materials;EditorUtility.SetDirty(d);}
 PlayerSettings.bundleVersion="1.9";EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();}
}
