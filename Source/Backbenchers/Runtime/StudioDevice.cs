using UnityEngine;
namespace Backbenchers {
 public sealed class StudioDevice:MonoBehaviour {
  public string deviceId,displayName;
  public Renderer screenRenderer;
  public int materialIndex;
  public Vector3 screenCenter,screenNormal=Vector3.forward;
  public Vector3[] screenCorners=new Vector3[0];
  public Texture2D[] pages;
  public int page;
  public bool powered=true;
  public Light glow;
  Material screen;
  bool focused;
  public void SetFocused(bool value){focused=value;Apply();}
  Texture2D desktopTexture;
  void Awake(){screen=screenRenderer.materials[materialIndex];Apply();}
  public void Toggle(){powered=!powered;Apply();}
  public void Next(){if(desktopTexture){Destroy(desktopTexture);desktopTexture=null;}page=(page+1)%pages.Length;Apply();}
  public void SetDesktop(string encoded){
   try{if(!desktopTexture)desktopTexture=new Texture2D(2,2,TextureFormat.RGB24,false);desktopTexture.LoadImage(System.Convert.FromBase64String(encoded));Apply();}catch(System.Exception){Debug.LogWarning("Could not refresh desktop preview");}
  }
  public void Apply(){
   if(screen==null)return;
   screen.mainTexture=powered&&!focused?(desktopTexture?desktopTexture:pages[page]):null;
   screen.color=powered&&!focused?Color.white:new Color(.008f,.012f,.016f);
   screen.SetTexture("_EmissionMap",powered&&!focused?(desktopTexture?desktopTexture:pages[page]):null);
   screen.SetColor("_EmissionColor",powered&&!focused?Color.white*.75f:Color.black);
   if(powered)screen.EnableKeyword("_EMISSION");else screen.DisableKeyword("_EMISSION");
   if(glow)glow.enabled=powered;
  }
  void OnDestroy(){if(screen)Destroy(screen);if(desktopTexture)Destroy(desktopTexture);}
 }
}
