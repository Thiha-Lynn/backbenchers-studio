using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
namespace Backbenchers {
 public sealed partial class StudioExperience {
  float stepDistance;
#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")] static extern void BBStudioSound(string kind);
#endif
  internal static void Sound(string kind){
#if UNITY_WEBGL && !UNITY_EDITOR
   BBStudioSound(kind);
#endif
  }
  void StepSound(float distance){stepDistance+=distance;if(stepDistance>.68f){stepDistance=0;Sound("step");}}
  public void VisitEntrance(string unused){if(inspecting||seatedDevice)return;movement=velocity=Vector2.zero;targetFov=normalFov;destination=new Vector3(16.1f,1.5f,8.45f);destinationRotation=Quaternion.LookRotation(new Vector3(16.55f,1.14f,10.15f)-destination.Value);if(reducedMotion){body.enabled=false;viewCamera.transform.SetPositionAndRotation(destination.Value,destinationRotation);destination=null;body.enabled=true;yaw=smoothYaw=viewCamera.transform.eulerAngles.y;pitch=smoothPitch=viewCamera.transform.eulerAngles.x;}}
  public void RoomAction(string action){var room=GetComponent<StudioRoom>();if(room)room.Act(action);}
 }
 public sealed class StudioRoom:MonoBehaviour {
  public Transform door;
  public bool reduceMotion;
  public Renderer switchRocker;
  public Light[] roomLights;
  readonly Dictionary<Material,Color> colors=new Dictionary<Material,Color>();
  readonly Dictionary<Material,Color> emissions=new Dictionary<Material,Color>();
  float[] intensities;Quaternion closed;bool lightsOn=true,doorOpen;float illumination=1,doorAngle,ambient;
  void Start(){
   if(door)closed=door.localRotation;ambient=RenderSettings.ambientIntensity;
   var copies=new Dictionary<Material,Material>();
   foreach(var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None)){
    if(r.GetComponentInParent<StudioDevice>())continue;
    var mats=r.sharedMaterials;
    for(int i=0;i<mats.Length;i++){var original=mats[i];if(!original||!original.HasProperty("_Color"))continue;
     if(!copies.TryGetValue(original,out var m)){m=new Material(original);copies.Add(original,m);colors[m]=m.color;if(m.HasProperty("_EmissionColor"))emissions[m]=m.GetColor("_EmissionColor");}mats[i]=m;
    }r.sharedMaterials=mats;
   }
   intensities=new float[roomLights.Length];for(int i=0;i<roomLights.Length;i++)if(roomLights[i])intensities[i]=roomLights[i].intensity;
  }
  public void Act(string action){if(action=="lights"){lightsOn=!lightsOn;StudioExperience.Sound("switch");if(switchRocker)switchRocker.transform.localRotation=Quaternion.Euler(lightsOn?-12:12,0,0);}else if(action=="door"){doorOpen=!doorOpen;StudioExperience.Sound("door");}Report();}
  void Report(){
#if UNITY_WEBGL && !UNITY_EDITOR
   BBStudioRoom(lightsOn?1:0,doorOpen?1:0);
#endif
  }
#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")] static extern void BBStudioRoom(int lights,int door);
#endif
  void Update(){
   float target=lightsOn?1:.22f;
   if(Mathf.Abs(illumination-target)>.001f){illumination=reduceMotion?target:Mathf.MoveTowards(illumination,target,Time.unscaledDeltaTime*2.6f);foreach(var pair in colors)if(pair.Key)pair.Key.color=new Color(pair.Value.r*illumination,pair.Value.g*illumination,pair.Value.b*illumination,pair.Value.a);foreach(var pair in emissions)if(pair.Key)pair.Key.SetColor("_EmissionColor",pair.Value*(lightsOn?illumination:0));RenderSettings.ambientIntensity=ambient*illumination;for(int i=0;i<roomLights.Length;i++)if(roomLights[i])roomLights[i].intensity=intensities[i]*illumination;}
   if(door){doorAngle=reduceMotion?(doorOpen?-96:0):Mathf.MoveTowards(doorAngle,doorOpen?-96:0,Time.unscaledDeltaTime*90);door.localRotation=closed*Quaternion.Euler(0,doorAngle,0);}
  }
 }
}
