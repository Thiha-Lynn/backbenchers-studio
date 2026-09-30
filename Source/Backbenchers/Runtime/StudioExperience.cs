using System;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backbenchers
{
    public sealed class StudioExperience : MonoBehaviour
    {
        public Camera viewCamera;
        public Vector3[] viewpoints;
        public Vector3[] viewAngles;
        public Vector2 xLimits;
        public Vector2 zLimits;
        private CharacterController body;
        private Vector2 movement;
        private float yaw, pitch;
        private bool paused;
        private float lastInteraction;
        private Vector3? destination;
        private Quaternion destinationRotation;
        private StudioDevice[] devices;
        private Vector3? explorePosition;
        private Quaternion exploreRotation;

#if UNITY_WEBGL && !UNITY_EDITOR
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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetQuality(Application.isMobilePlatform ? "low" : "high");
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;
            BBStudioReady(SystemInfo.graphicsDeviceType.ToString());
#endif
        }
        void Update()
        {
            if(paused || viewCamera == null) return;
            OnDemandRendering.renderFrameInterval=destination.HasValue||movement.sqrMagnitude>.001f||Time.unscaledTime-lastInteraction<.5f?1:3;
            if(destination.HasValue)
            {
                body.enabled = false;
                viewCamera.transform.position = Vector3.Lerp(viewCamera.transform.position, destination.Value, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));
                viewCamera.transform.rotation = Quaternion.Slerp(viewCamera.transform.rotation, destinationRotation, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));
                if(Vector3.Distance(viewCamera.transform.position, destination.Value) < .012f && Quaternion.Angle(viewCamera.transform.rotation,destinationRotation)<.2f)
                {
                    viewCamera.transform.position = destination.Value;
                    viewCamera.transform.rotation = destinationRotation;
                    destination = null;
                    yaw = viewCamera.transform.eulerAngles.y;
                    pitch = viewCamera.transform.eulerAngles.x;
                    if(pitch>180) pitch-=360;
                    body.enabled = true;
                }
                return;
            }
            if(movement.sqrMagnitude > .001f)
            {
                Vector3 forward = Quaternion.Euler(0,yaw,0)*Vector3.forward;
                Vector3 right = Quaternion.Euler(0,yaw,0)*Vector3.right;
                body.Move((forward*movement.y+right*movement.x)*2.1f*Time.unscaledDeltaTime);
                Vector3 p = viewCamera.transform.position;
                p.x = Mathf.Clamp(p.x,xLimits.x,xLimits.y);
                p.z = Mathf.Clamp(p.z,zLimits.x,zLimits.y);
                p.y = 1.55f;
                viewCamera.transform.position = p;
            }
        }
        public void SetMove(string value)
        {
            var parts=value.Split(',');
            if(parts.Length==2 && float.TryParse(parts[0],NumberStyles.Float,CultureInfo.InvariantCulture,out var x) && float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out var y))
                {movement=Vector2.ClampMagnitude(new Vector2(x,y),1);if(movement.sqrMagnitude>0&&destination.HasValue){destination=null;body.enabled=true;yaw=viewCamera.transform.eulerAngles.y;pitch=viewCamera.transform.eulerAngles.x;}}
        }
        public void Look(string value)
        {
            if(paused)return;
            if(destination.HasValue){destination=null;body.enabled=true;yaw=viewCamera.transform.eulerAngles.y;pitch=viewCamera.transform.eulerAngles.x;if(pitch>180)pitch-=360;}
            var parts=value.Split(',');
            if(parts.Length!=2 || !float.TryParse(parts[0],NumberStyles.Float,CultureInfo.InvariantCulture,out var x) || !float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out var y))return;
            lastInteraction=Time.unscaledTime;
            yaw+=x*.13f;pitch=Mathf.Clamp(pitch+y*.13f,-65,65);
            viewCamera.transform.rotation=Quaternion.Euler(pitch,yaw,0);
        }
        public void Visit(string index)
        {
            bool instant=index.StartsWith("instant:");
            if(instant)index=index.Substring(8);
            if(!int.TryParse(index,out int i)||i<0||i>=viewpoints.Length)return;
            movement=Vector2.zero;destination=viewpoints[i];destinationRotation=Quaternion.Euler(viewAngles[i]);
            if(instant){body.enabled=false;viewCamera.transform.SetPositionAndRotation(viewpoints[i],destinationRotation);body.enabled=true;destination=null;yaw=viewAngles[i].y;pitch=viewAngles[i].x;}
#if UNITY_WEBGL && !UNITY_EDITOR
            BBStudioView(i);
#endif
        }
        public void SetViewport(string orientation){if(viewCamera)viewCamera.fieldOfView=orientation=="portrait"?82:62;lastInteraction=Time.unscaledTime;}
        public void SetPaused(string value){paused=value=="1";movement=Vector2.zero;OnDemandRendering.renderFrameInterval=paused?12:1;}
        public void SetQuality(string value)
        {
            bool low=value=="low";
            QualitySettings.shadows=ShadowQuality.Disable;
            QualitySettings.shadowDistance=low?0:18;
            QualitySettings.pixelLightCount=low?1:2;
            QualitySettings.antiAliasing=low?2:4;
            OnDemandRendering.renderFrameInterval=1;
        }
        public void Interact(string coordinates)
        {
            if(paused)return;
            var p=coordinates.Split(',');
            if(p.Length!=2||!float.TryParse(p[0],NumberStyles.Float,CultureInfo.InvariantCulture,out float x)||!float.TryParse(p[1],NumberStyles.Float,CultureInfo.InvariantCulture,out float y))return;
            var ray=viewCamera.ViewportPointToRay(new Vector3(x,1-y,0));
            if(!Physics.Raycast(ray,out var hit,6f))return;
            var device=hit.collider.GetComponentInParent<StudioDevice>();
            if(device){SelectDevice(device.deviceId);return;}
            var item=hit.collider.GetComponentInParent<StudioInspectable>();
#if UNITY_WEBGL && !UNITY_EDITOR
            if(item)BBStudioInspect(item.topic);
#endif
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
        public void FocusDevice(string value)
        {
            bool instant=value.StartsWith("instant:");var id=instant?value.Substring(8):value;
            if(!explorePosition.HasValue){explorePosition=viewCamera.transform.position;exploreRotation=viewCamera.transform.rotation;}
            movement=Vector2.zero;paused=false;
            foreach(var d in devices)if(d.deviceId==id){
                var target=d.screenRenderer.bounds.center;
                Vector3 p;
                if(id=="thomas"){p=new Vector3(13.82f,1.42f,6.2f);target=new Vector3(13.85f,1.05f,7.45f);}
                else if(id=="hlaing"){p=new Vector3(14.1f,1.42f,9.1f);target=new Vector3(14.03f,1.05f,7.8f);}
                else if(id=="merlin"){p=new Vector3(15.98f,1.42f,7.5f);target=new Vector3(17.45f,1.05f,7.5f);}
                else {var direction=viewCamera.transform.position-target;direction.y=0;p=target+direction.normalized*.9f+Vector3.up*.65f;}
                destination=p;destinationRotation=Quaternion.LookRotation(target-p);
                if(instant){body.enabled=false;viewCamera.transform.SetPositionAndRotation(p,destinationRotation);body.enabled=true;destination=null;}
                lastInteraction=Time.unscaledTime;return;
            }
        }
        public void LeaveDevice(string unused)
        {
            if(!explorePosition.HasValue)return;
            destination=explorePosition.Value;destinationRotation=exploreRotation;explorePosition=null;paused=false;lastInteraction=Time.unscaledTime;
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
        void OnApplicationFocus(bool focus){if(!focus)movement=Vector2.zero;}
    }
}
