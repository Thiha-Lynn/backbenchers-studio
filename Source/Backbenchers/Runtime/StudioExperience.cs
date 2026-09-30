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
        private Vector3? destination;
        private Quaternion destinationRotation;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void BBStudioReady(string renderer);
        [DllImport("__Internal")] private static extern void BBStudioView(int index);
#endif
        void Start()
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            body = viewCamera.GetComponent<CharacterController>();
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
                p.y = 1.6f;
                viewCamera.transform.position = p;
            }
        }
        public void SetMove(string value)
        {
            var parts=value.Split(',');
            if(parts.Length==2 && float.TryParse(parts[0],NumberStyles.Float,CultureInfo.InvariantCulture,out var x) && float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out var y))
                movement=Vector2.ClampMagnitude(new Vector2(x,y),1);
        }
        public void Look(string value)
        {
            if(paused || destination.HasValue)return;
            var parts=value.Split(',');
            if(parts.Length!=2 || !float.TryParse(parts[0],NumberStyles.Float,CultureInfo.InvariantCulture,out var x) || !float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out var y))return;
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
        public void SetViewport(string orientation){if(viewCamera)viewCamera.fieldOfView=orientation=="portrait"?90:65;}
        public void SetPaused(string value){paused=value=="1";movement=Vector2.zero;OnDemandRendering.renderFrameInterval=paused?12:(Application.isMobilePlatform?2:1);}
        public void SetQuality(string value)
        {
            bool low=value=="low";
            QualitySettings.shadows=low?ShadowQuality.Disable:ShadowQuality.HardOnly;
            QualitySettings.shadowDistance=low?0:18;
            QualitySettings.pixelLightCount=low?2:3;
            QualitySettings.antiAliasing=low?0:2;
            OnDemandRendering.renderFrameInterval=Application.isMobilePlatform?2:1;
        }
        void OnApplicationFocus(bool focus){if(!focus)movement=Vector2.zero;}
    }
}
