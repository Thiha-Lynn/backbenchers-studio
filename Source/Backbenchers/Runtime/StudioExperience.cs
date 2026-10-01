using System;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backbenchers
{
    public sealed partial class StudioExperience : MonoBehaviour
    {
        public Camera viewCamera;
        public Vector3[] viewpoints;
        public Vector3[] viewAngles;
        public Vector2 xLimits;
        public Vector2 zLimits;
        private CharacterController body;
        private Vector2 movement, velocity;
        private float smoothYaw, smoothPitch, normalFov=62, targetFov=62, frameTime, frameWindow;
        private int frameSamples;
        private bool reducedMotion, portraitViewport;
        private StudioDevice seatedDevice;
        private bool seatReported;
        private float seatAspect;
        private float yaw, pitch;
        private bool paused;
        private float lastInteraction;
        private Vector3? destination;
        private Quaternion destinationRotation;
        private StudioDevice[] devices;
        private Vector3? explorePosition;
        private Quaternion exploreRotation;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void BBStudioPerformance(float fps);
        [DllImport("__Internal")] private static extern void BBStudioSeat(float x,float y,float width,float height);
        [DllImport("__Internal")] private static extern void BBStudioReady(string renderer);
        [DllImport("__Internal")] private static extern void BBStudioView(int index);
        [DllImport("__Internal")] private static extern void BBStudioDevice(string id,int power,int page);
        [DllImport("__Internal")] private static extern void BBStudioInspect(string topic);
#endif
        void Start()
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            body = viewCamera.GetComponent<CharacterController>();
            devices=UnityEngine.Object.FindObjectsByType<StudioDevice>(FindObjectsSortMode.None);
            yaw = viewCamera.transform.eulerAngles.y;
            pitch = viewCamera.transform.eulerAngles.x;
            if(pitch > 180) pitch -= 360;
            smoothYaw=yaw;smoothPitch=pitch;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetQuality(Application.isMobilePlatform ? "low" : "high");
            if(viewpoints!=null && viewpoints.Length>0)Visit("instant:0");
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;
            BBStudioReady(SystemInfo.graphicsDeviceType.ToString());
#endif
        }
        void Update()
        {
            if(paused || viewCamera == null) return;
            UpdatePrompt();
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);
            AnimateInspection(dt);
            // Keep active exploration at display cadence; only hidden/modal views throttle.
            OnDemandRendering.renderFrameInterval=1;
            frameTime+=Time.unscaledDeltaTime;frameSamples++;frameWindow+=Time.unscaledDeltaTime;
            if(frameWindow>=2f){
#if UNITY_WEBGL && !UNITY_EDITOR
                BBStudioPerformance(frameSamples/Mathf.Max(.001f,frameTime));
#endif
                frameSamples=0;frameTime=0;frameWindow=0;
            }
            if(seatedDevice && Mathf.Abs(viewCamera.aspect-seatAspect)>.001f){seatAspect=viewCamera.aspect;targetFov=SeatedFov(seatedDevice);seatReported=false;}
            viewCamera.fieldOfView=Mathf.Lerp(viewCamera.fieldOfView,targetFov,reducedMotion?1:1-Mathf.Exp(-9*dt));
            if(destination.HasValue)
            {
                body.enabled = false;
                viewCamera.transform.position = Vector3.Lerp(viewCamera.transform.position, destination.Value, 1f - Mathf.Exp(-8f * dt));
                viewCamera.transform.rotation = Quaternion.Slerp(viewCamera.transform.rotation, destinationRotation, 1f - Mathf.Exp(-8f * dt));
                if(Vector3.Distance(viewCamera.transform.position, destination.Value) < .012f && Quaternion.Angle(viewCamera.transform.rotation,destinationRotation)<.2f)
                {
                    viewCamera.transform.position = destination.Value;
                    viewCamera.transform.rotation = destinationRotation;
                    destination = null;
                    yaw = viewCamera.transform.eulerAngles.y;
                    pitch = viewCamera.transform.eulerAngles.x;
                    if(pitch>180) pitch-=360;
                    body.enabled = !seatedDevice && !inspecting;
                    smoothYaw=yaw;smoothPitch=pitch;
                }
                return;
            }
            if(inspecting)return;
            if(seatedDevice){if(!seatReported || Mathf.Abs(viewCamera.fieldOfView-targetFov)>.01f)ReportSeat();return;}
            smoothYaw=Mathf.LerpAngle(smoothYaw,yaw,reducedMotion?1:1-Mathf.Exp(-24*dt));
            smoothPitch=Mathf.Lerp(smoothPitch,pitch,reducedMotion?1:1-Mathf.Exp(-24*dt));
            viewCamera.transform.rotation=Quaternion.Euler(smoothPitch,smoothYaw,0);
            velocity=Vector2.Lerp(velocity,movement,1-Mathf.Exp(-(movement.sqrMagnitude>.001f?12f:20f)*dt));
            if(velocity.sqrMagnitude>.00001f){
                Vector3 forward=Quaternion.Euler(0,smoothYaw,0)*Vector3.forward;
                Vector3 right=Quaternion.Euler(0,smoothYaw,0)*Vector3.right;
                var before=viewCamera.transform.position;
                body.Move((forward*velocity.y+right*velocity.x)*1.65f*dt);
                StepSound(Vector3.Distance(before,viewCamera.transform.position));
                Vector3 p=viewCamera.transform.position;
                p.x=Mathf.Clamp(p.x,xLimits.x,xLimits.y);p.z=Mathf.Clamp(p.z,zLimits.x,zLimits.y);
                p.y=Mathf.MoveTowards(p.y,1.55f,dt*1.2f);viewCamera.transform.position=p;
            }
        }
        void ReportSeat(){
            if(!seatedDevice)return;
            var t=seatedDevice.screenRenderer.transform;
            var min=new Vector2(1,1);var max=Vector2.zero;
            foreach(var corner in seatedDevice.screenCorners){var v=viewCamera.WorldToViewportPoint(t.TransformPoint(corner));min=Vector2.Min(min,v);max=Vector2.Max(max,v);}
#if UNITY_WEBGL && !UNITY_EDITOR
            BBStudioSeat(min.x,1-max.y,max.x-min.x,max.y-min.y);
#endif
            seatReported=true;
        }
        public void SetMotion(string value){reducedMotion=value=="reduced";var room=GetComponent<StudioRoom>();if(room)room.reduceMotion=reducedMotion;}
        public void SetMove(string value)
        {
            if(paused||seatedDevice||inspecting)return;
            var parts=value.Split(',');
            if(parts.Length==2 && float.TryParse(parts[0],NumberStyles.Float,CultureInfo.InvariantCulture,out var x) && float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out var y))
                {movement=Vector2.ClampMagnitude(new Vector2(x,y),1);if(movement.sqrMagnitude>0&&destination.HasValue){destination=null;body.enabled=true;yaw=viewCamera.transform.eulerAngles.y;pitch=viewCamera.transform.eulerAngles.x;if(pitch>180)pitch-=360;smoothYaw=yaw;smoothPitch=pitch;}}
        }
        public void Look(string value)
        {
            if(paused||seatedDevice||inspecting)return;
            if(destination.HasValue){destination=null;body.enabled=true;yaw=viewCamera.transform.eulerAngles.y;pitch=viewCamera.transform.eulerAngles.x;if(pitch>180)pitch-=360;smoothYaw=yaw;smoothPitch=pitch;}
            var parts=value.Split(',');
            if(parts.Length!=2 || !float.TryParse(parts[0],NumberStyles.Float,CultureInfo.InvariantCulture,out var x) || !float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out var y))return;
            lastInteraction=Time.unscaledTime;
            yaw+=x*.13f;pitch=Mathf.Clamp(pitch+y*.13f,-65,65);

        }
        public void Visit(string index)
        {
            bool instant=index.StartsWith("instant:");
            if(instant)index=index.Substring(8);
            if(!int.TryParse(index,out int i)||i<0||i>=viewpoints.Length)return;
            ClearDisplay();inspecting=null;inspectionPosition=null;seatedDevice=null;targetFov=normalFov;movement=velocity=Vector2.zero;destination=viewpoints[i];destinationRotation=Quaternion.Euler(viewAngles[i]);
            if(i==4&&portraitViewport){destination=viewpoints[i]+new Vector3(0,.2f,.55f);destinationRotation=Quaternion.Euler(8,180,0);}
            if(instant){body.enabled=false;viewCamera.transform.SetPositionAndRotation(destination.Value,destinationRotation);body.enabled=true;destination=null;yaw=smoothYaw=destinationRotation.eulerAngles.y;pitch=smoothPitch=destinationRotation.eulerAngles.x;}
#if UNITY_WEBGL && !UNITY_EDITOR
            BBStudioView(i);
#endif
        }
        public void SetViewport(string orientation){portraitViewport=orientation=="portrait";normalFov=portraitViewport?82:62;targetFov=inspecting?48:seatedDevice?SeatedFov(seatedDevice):normalFov;if(inspecting)FocusObject(inspecting.GetEntityId().ToString());seatReported=false;lastInteraction=Time.unscaledTime;}
        public void SetPaused(string value){paused=value=="1";lastPromptId="reset";movement=velocity=Vector2.zero;OnDemandRendering.renderFrameInterval=paused?12:1;}
        public void SetQuality(string value)
        {
            bool low=value=="low";
            QualitySettings.shadows=ShadowQuality.Disable;
            QualitySettings.shadowDistance=low?0:18;
            QualitySettings.pixelLightCount=low?1:2;
            QualitySettings.antiAliasing=low?0:2;
            OnDemandRendering.renderFrameInterval=1;
        }
        public void Interact(string coordinates)
        {
            if(paused||inspecting)return;
            SetPointer(coordinates);
            if(!InteractionHit(out var hit))return;
            var device=hit.collider.GetComponentInParent<StudioDevice>();
            if(device){SelectDevice(device.deviceId);return;}
            var item=hit.collider.GetComponentInParent<StudioInspectable>();
            if(item)ReportInspection(item);
        }

        public void SelectDevice(string id)
        {
            if(devices==null)return;
            foreach(var d in devices)if(d.deviceId==id){
#if UNITY_WEBGL && !UNITY_EDITOR
                BBStudioDevice(d.deviceId,d.powered?1:0,d.page);
#endif
                return;
            }
        }
        float SeatedFov(StudioDevice d){
            if(d.screenCorners==null||d.screenCorners.Length<4)return 50;
            float width=Vector3.Distance(d.screenRenderer.transform.TransformPoint(d.screenCorners[0]),d.screenRenderer.transform.TransformPoint(d.screenCorners[1]));
            bool monitor=d.deviceId=="thomas"||d.deviceId=="hlaing"||d.deviceId=="merlin";
            return Mathf.Clamp(2*Mathf.Atan(width/(2*(monitor?.64f:.38f)*.88f*viewCamera.aspect))*Mathf.Rad2Deg,46,100);
        }
        public void FocusDevice(string value)
        {
            bool instant=value.StartsWith("instant:");var id=instant?value.Substring(8):value;
            if(!explorePosition.HasValue){explorePosition=viewCamera.transform.position;exploreRotation=viewCamera.transform.rotation;}
            movement=velocity=Vector2.zero;paused=false;
            foreach(var d in devices)if(d.deviceId==id){
                if(seatedDevice)seatedDevice.SetFocused(false);seatedDevice=d;d.SetFocused(true);seatReported=false;targetFov=SeatedFov(d);
                var target=d.screenRenderer.transform.TransformPoint(d.screenCenter);
                Vector3 normal=d.screenRenderer.transform.TransformDirection(d.screenNormal).normalized;
                var p=target+normal*(d.screenCorners.Length==4 && (id=="thomas"||id=="hlaing"||id=="merlin")?.64f:.38f);
                destination=p;destinationRotation=Quaternion.LookRotation(target-p,d.screenRenderer.transform.up);
                // Handhelds keep world-up and an overhead reading angle.
                if(id!="thomas"&&id!="hlaing"&&id!="merlin")destinationRotation=Quaternion.LookRotation(target-p,Vector3.forward);
                if(instant){body.enabled=false;viewCamera.transform.SetPositionAndRotation(p,destinationRotation);viewCamera.fieldOfView=targetFov;destination=null;ReportSeat();}
                lastInteraction=Time.unscaledTime;return;
            }
        }
        public void LeaveDevice(string unused)
        {
            if(!explorePosition.HasValue)return;
            if(seatedDevice)seatedDevice.SetFocused(false);seatedDevice=null;seatReported=false;targetFov=normalFov;movement=velocity=Vector2.zero;destination=explorePosition.Value;destinationRotation=exploreRotation;explorePosition=null;paused=false;lastInteraction=Time.unscaledTime;
        }
        public void HoldBook(string id)
        {
            foreach(var item in UnityEngine.Object.FindObjectsByType<StudioInspectable>(FindObjectsSortMode.None))
                if(item.topic.StartsWith("book:"))foreach(var renderer in item.GetComponentsInChildren<Renderer>())renderer.enabled=item.topic!="book:"+id;
        }
        public void ScreenPreview(string value)
        {
            var split=value.IndexOf('|');if(split<1||value.Length>250000)return;
            var id=value.Substring(0,split);foreach(var d in devices)if(d.deviceId==id){d.SetDesktop(value.Substring(split+1));return;}
        }
        public void DeviceAction(string value)
        {
            var parts=value.Split(':');if(parts.Length!=2||devices==null)return;
            foreach(var d in devices)if(d.deviceId==parts[0]){if(parts[1]=="power")d.Toggle();else if(parts[1]=="page")d.Next();SelectDevice(d.deviceId);lastInteraction=Time.unscaledTime;return;}
        }
        void OnApplicationFocus(bool focus){if(!focus)movement=velocity=Vector2.zero;}
    }
}
