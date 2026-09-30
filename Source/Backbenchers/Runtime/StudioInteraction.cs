using System;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;
namespace Backbenchers {
 public sealed partial class StudioExperience {
  [Serializable] class ObjectInfo { public string id,topic,label,kind; public float x,y; public bool flat; }
  StudioInspectable inspecting;
  Vector3? inspectionPosition;
  Quaternion inspectionRotation;
  Vector3 inspectCenter,inspectDirection;
  float inspectDistance,inspectAngle,inspectZoom=1,nextPrompt;
  Vector2 pointer=new Vector2(.5f,.5f),lastPromptPosition;
  string lastPromptId="";
#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")] static extern void BBStudioPrompt(string json);
  [DllImport("__Internal")] static extern void BBStudioObject(string json);
#endif
  public void SetPointer(string value){var p=value.Split(',');if(p.Length==2&&float.TryParse(p[0],NumberStyles.Float,CultureInfo.InvariantCulture,out float x)&&float.TryParse(p[1],NumberStyles.Float,CultureInfo.InvariantCulture,out float y))pointer=new Vector2(x,y);}
  bool InteractionHit(out RaycastHit hit){return Physics.Raycast(viewCamera.ViewportPointToRay(new Vector3(pointer.x,1-pointer.y,0)),out hit,6f);}
  ObjectInfo Info(StudioInspectable item){return new ObjectInfo{id=item.GetEntityId().ToString(),topic=item.topic,label=string.IsNullOrEmpty(item.displayName)?item.name:item.displayName,kind=item.topic.StartsWith("book:")?"book":"object",flat=item.flat};}
  void ReportInspection(StudioInspectable item){
#if UNITY_WEBGL && !UNITY_EDITOR
   BBStudioObject(JsonUtility.ToJson(Info(item)));
#endif
  }
  void UpdatePrompt(){
   if(Time.unscaledTime<nextPrompt)return;nextPrompt=Time.unscaledTime+.12f;
   var info=new ObjectInfo{id="",label="",topic="",kind=""};
   if(!seatedDevice&&!inspecting&&!destination.HasValue&&pointer.x>=0&&InteractionHit(out var hit)){
    var d=hit.collider.GetComponentInParent<StudioDevice>();var item=hit.collider.GetComponentInParent<StudioInspectable>();
    if(d)info=new ObjectInfo{id=d.deviceId,topic=d.deviceId,label=d.displayName,kind="device"};else if(item)info=Info(item);
    var v=viewCamera.WorldToViewportPoint(hit.point);info.x=v.x;info.y=1-v.y;
   }
   if(info.id==lastPromptId&&Vector2.Distance(lastPromptPosition,new Vector2(info.x,info.y))<.012f)return;
   lastPromptId=info.id;lastPromptPosition=new Vector2(info.x,info.y);
#if UNITY_WEBGL && !UNITY_EDITOR
   BBStudioPrompt(JsonUtility.ToJson(info));
#endif
  }
  public void FocusObject(string id){
   foreach(var item in UnityEngine.Object.FindObjectsByType<StudioInspectable>(FindObjectsSortMode.None))if(item.GetEntityId().ToString()==id||item.topic==id){
    if(!inspectionPosition.HasValue){inspectionPosition=viewCamera.transform.position;inspectionRotation=viewCamera.transform.rotation;}
    inspecting=item;movement=velocity=Vector2.zero;paused=false;body.enabled=false;
    var rs=item.GetComponentsInChildren<Renderer>();var bounds=new Bounds(item.transform.position,Vector3.zero);if(rs.Length>0){bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);}
    inspectCenter=bounds.center;inspectAngle=0;inspectZoom=1;targetFov=48;
    inspectDirection=item.flat?item.outward.normalized:(inspectionPosition.Value-inspectCenter).normalized;
    if(!item.flat){inspectDirection.y=Mathf.Max(.4f,inspectDirection.y);inspectDirection.Normalize();}
    float width=item.flat?Mathf.Max(bounds.size.x,bounds.size.z):Mathf.Max(bounds.size.x,bounds.size.z)*1.15f;
    inspectDistance=Mathf.Max(.32f,Mathf.Max(bounds.size.y,width/Mathf.Max(.5f,viewCamera.aspect))/(2*Mathf.Tan(24*Mathf.Deg2Rad))*(item.flat?1.9f:portraitViewport?1.85f:1.65f));
    PositionInspection();ReportInspection(item);return;
   }
  }
  void PositionInspection(){
   var p=inspectCenter+Quaternion.AngleAxis(inspectAngle,Vector3.up)*inspectDirection*inspectDistance*inspectZoom;
   p.x=Mathf.Clamp(p.x,11.55f,17.58f);p.z=Mathf.Clamp(p.z,5.45f,9.96f);p.y=Mathf.Clamp(p.y,.18f,3.1f);
   destination=p;destinationRotation=Quaternion.LookRotation(inspectCenter-Vector3.up*(inspectDistance*inspectZoom*Mathf.Tan(24*Mathf.Deg2Rad)*.34f)-p,Vector3.up);
   if(reducedMotion){viewCamera.transform.SetPositionAndRotation(p,destinationRotation);viewCamera.fieldOfView=targetFov;destination=null;}
  }
  public void OrbitObject(string value){if(!inspecting||inspecting.flat)return;if(float.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var angle)){inspectAngle=Mathf.Clamp(inspectAngle+angle,-55,55);PositionInspection();}}
  public void ZoomObject(string value){if(!inspecting)return;if(float.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var zoom)){inspectZoom=Mathf.Clamp(inspectZoom+zoom,.8f,1.5f);PositionInspection();}}
  public void LeaveObject(string unused){if(!inspectionPosition.HasValue)return;inspecting=null;targetFov=normalFov;movement=velocity=Vector2.zero;destination=inspectionPosition.Value;destinationRotation=inspectionRotation;inspectionPosition=null;paused=false;if(reducedMotion){viewCamera.transform.SetPositionAndRotation(destination.Value,destinationRotation);destination=null;body.enabled=true;yaw=smoothYaw=viewCamera.transform.eulerAngles.y;pitch=viewCamera.transform.eulerAngles.x;if(pitch>180)pitch-=360;smoothPitch=pitch;}}
 }
}
