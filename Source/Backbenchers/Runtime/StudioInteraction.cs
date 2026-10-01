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
  float inspectDistance,inspectAngle,inspectElevation,inspectZoom=1,nextPrompt,inspectionInset=.3f;
  Vector2 inspectPan; bool inspectAuto;
  Vector2 pointer=new Vector2(.5f,.5f),lastPromptPosition;
  string lastPromptId="";
#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")] static extern void BBStudioPrompt(string json);
  [DllImport("__Internal")] static extern void BBStudioObject(string json);
#endif
  public void SetPointer(string value){var p=value.Split(',');if(p.Length==2&&float.TryParse(p[0],NumberStyles.Float,CultureInfo.InvariantCulture,out float x)&&float.TryParse(p[1],NumberStyles.Float,CultureInfo.InvariantCulture,out float y))pointer=new Vector2(x,y);}
  bool InteractionHit(out RaycastHit hit){return Physics.Raycast(viewCamera.ViewportPointToRay(new Vector3(pointer.x,1-pointer.y,0)),out hit,6f);}
  ObjectInfo Info(StudioInspectable item){return new ObjectInfo{id=item.GetEntityId().ToString(),topic=item.topic,label=string.IsNullOrEmpty(item.displayName)?item.name:item.displayName,kind=item.topic.StartsWith("book:")?"book":item.topic.StartsWith("room:")?"action":"object",flat=item.flat};}
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
    if(item.topic.StartsWith("art:")){ReportInspection(item);return;}
    ClearDisplay();
    if(!inspectionPosition.HasValue){inspectionPosition=viewCamera.transform.position;inspectionRotation=viewCamera.transform.rotation;}
    inspecting=item;movement=velocity=Vector2.zero;paused=false;body.enabled=false;
    var rs=item.GetComponentsInChildren<Renderer>();var bounds=new Bounds(item.transform.position,Vector3.zero);if(rs.Length>0){bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);}
    var initialDirection=(inspectionPosition.Value-bounds.center).normalized;
    inspectCenter=bounds.center;inspectAngle=0;inspectElevation=0;inspectPan=Vector2.zero;inspectAuto=false;inspectZoom=1;targetFov=48;
    // Inspect the authored object in place; only the camera moves.
    inspectDirection=item.flat?item.outward.normalized:initialDirection;
    if(!item.flat){inspectDirection.y=Mathf.Max(.4f,inspectDirection.y);inspectDirection.Normalize();}
    float width=item.flat?Mathf.Max(bounds.size.x,bounds.size.z):Mathf.Max(bounds.size.x,bounds.size.z)*1.15f;
    inspectDistance=Mathf.Max(.32f,Mathf.Max(bounds.size.y,width/Mathf.Max(.5f,viewCamera.aspect))/(2*Mathf.Tan(24*Mathf.Deg2Rad))*(item.flat?1.9f:portraitViewport?1.85f:1.95f));
    PositionInspection();ReportInspection(item);return;
   }
  }
  public void HoldArtwork(string id){foreach(var item in UnityEngine.Object.FindObjectsByType<StudioInspectable>(FindObjectsSortMode.None))if(item.topic.StartsWith("art:"))foreach(var r in item.GetComponentsInChildren<Renderer>())r.enabled=item.topic!="art:"+id;}
  void ClearDisplay(){inspectAuto=false;}
  void PositionInspection(){
   var horizontal=Quaternion.AngleAxis(inspectAngle,Vector3.up)*inspectDirection;
   var direction=Quaternion.AngleAxis(inspectElevation,Vector3.Cross(Vector3.up,horizontal).normalized)*horizontal;
   if(!inspecting.flat){direction.y=Mathf.Max(.3f,direction.y);direction.Normalize();}
   var right=Vector3.Cross(Vector3.up,direction).normalized;
   var center=inspectCenter+right*inspectPan.x+Vector3.up*inspectPan.y;
   var p=center+direction*inspectDistance*inspectZoom;
   p.x=Mathf.Clamp(p.x,11.55f,17.58f);p.z=Mathf.Clamp(p.z,5.45f,9.96f);p.y=Mathf.Clamp(p.y,.18f,3.1f);
   destination=p;destinationRotation=Quaternion.LookRotation(center-Vector3.up*(inspectDistance*inspectZoom*Mathf.Tan(24*Mathf.Deg2Rad)*inspectionInset)-p,Vector3.up);
   if(reducedMotion){viewCamera.transform.SetPositionAndRotation(p,destinationRotation);viewCamera.fieldOfView=targetFov;destination=null;}
  }
  void AnimateInspection(float dt){if(inspecting&&inspectAuto&&!reducedMotion){inspectAngle=Mathf.Repeat(inspectAngle+dt*14,360);PositionInspection();}}
  public void SetInspectionInset(string value){if(float.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var inset)){inspectionInset=Mathf.Clamp(inset,.12f,.6f);if(inspecting)PositionInspection();}}
  public void OrbitObject(string value){if(!inspecting)return;var parts=value.Split(',');if(float.TryParse(parts[0],NumberStyles.Float,CultureInfo.InvariantCulture,out var angle)){inspectAuto=false;inspectAngle+=angle;inspectAngle=inspecting.flat?Mathf.Clamp(inspectAngle,-35,35):Mathf.Repeat(inspectAngle+180,360)-180;if(parts.Length>1&&float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out var elevation))inspectElevation=Mathf.Clamp(inspectElevation+elevation,inspecting.flat?-25:-55,inspecting.flat?25:60);PositionInspection();}}
  public void PanObject(string value){if(!inspecting)return;var p=value.Split(',');if(p.Length==2&&float.TryParse(p[0],NumberStyles.Float,CultureInfo.InvariantCulture,out var x)&&float.TryParse(p[1],NumberStyles.Float,CultureInfo.InvariantCulture,out var y)){inspectPan=Vector2.ClampMagnitude(inspectPan+new Vector2(x,y),inspectDistance*.35f);PositionInspection();}}
  public void ResetObject(string unused){if(!inspecting)return;inspectAngle=inspectElevation=0;inspectPan=Vector2.zero;inspectZoom=1;inspectAuto=false;PositionInspection();}
  public void AutoObject(string value){inspectAuto=inspecting&&!inspecting.flat&&!reducedMotion&&value=="1";}
  public void ZoomObject(string value){if(!inspecting)return;if(float.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var zoom)){inspectZoom=Mathf.Clamp(inspectZoom+zoom,.5f,1.8f);PositionInspection();}}
  public void LeaveObject(string unused){if(!inspectionPosition.HasValue)return;ClearDisplay();inspecting=null;targetFov=normalFov;movement=velocity=Vector2.zero;destination=inspectionPosition.Value;destinationRotation=inspectionRotation;inspectionPosition=null;paused=false;if(reducedMotion){viewCamera.transform.SetPositionAndRotation(destination.Value,destinationRotation);destination=null;body.enabled=true;yaw=smoothYaw=viewCamera.transform.eulerAngles.y;pitch=viewCamera.transform.eulerAngles.x;if(pitch>180)pitch-=360;smoothPitch=pitch;}}
 }
}
