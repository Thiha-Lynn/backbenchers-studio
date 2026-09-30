using UnityEngine;
namespace Backbenchers {
 public sealed class StudioDevice:MonoBehaviour {
  public string deviceId,displayName;
  public Renderer screenRenderer;
  public int materialIndex;
  public Texture2D[] pages;
  public int page;
  public bool powered=true;
  public Light glow;
  Material screen;
  void Awake(){screen=screenRenderer.materials[materialIndex];Apply();}
  public void Toggle(){powered=!powered;Apply();}
  public void Next(){page=(page+1)%pages.Length;Apply();}
  public void Apply(){
   if(screen==null)return;
   screen.mainTexture=powered?pages[page]:null;
   screen.color=powered?Color.white:new Color(.008f,.012f,.016f);
   screen.SetTexture("_EmissionMap",powered?pages[page]:null);
   screen.SetColor("_EmissionColor",powered?Color.white*.75f:Color.black);
   if(powered)screen.EnableKeyword("_EMISSION");else screen.DisableKeyword("_EMISSION");
   if(glow)glow.enabled=powered;
  }
  void OnDestroy(){if(screen)Destroy(screen);}
 }
}
