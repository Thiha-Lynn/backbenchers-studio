const drawings=[
 {id:'still-life',title:'Still life',alt:'Pencil still life with a glass kettle, fruit and a drinking glass'},
 {id:'tiger',title:'Tiger study',alt:'Detailed pencil study of a tiger with its mouth open'},
 {id:'itachi',title:'Itachi study',alt:'Pencil character portrait with red eyes'},
 {id:'david',title:'David study',alt:'Pencil study of the sculpted head and shoulders of David'},
 {id:'portrait',title:'Portrait study',alt:'Pencil portrait on a sketchbook beside drawing tools'}
];
export function createArtViewer({paper,onSelect}){
 const $=id=>document.getElementById(id),dialog=$('art-viewer'),stage=$('art-stage'),image=$('art-image');
 const reduced=matchMedia('(prefers-reduced-motion:reduce)');let index=0,zoom=1,rotation=0,x=0,y=0,ticket=0,animation=null,pointers=new Map(),gesture=null;
 function paint(){image.style.transform=`translate(-50%,-50%) translate(${x}px,${y}px) rotate(${rotation*90}deg) scale(${zoom})`;}
 function fit(){if(!image.naturalWidth)return;const margin=innerWidth<800?116:200;const w=Math.max(60,stage.clientWidth-margin),h=Math.max(60,stage.clientHeight-38),odd=Math.abs(rotation%2)===1;const scale=Math.min(w/(odd?image.naturalHeight:image.naturalWidth),h/(odd?image.naturalWidth:image.naturalHeight));image.style.width=image.naturalWidth*scale+'px';image.style.height=image.naturalHeight*scale+'px';paint();}
 function reset(){zoom=1;x=y=0;fit();}
 function magnify(delta){zoom=Math.max(1,Math.min(4,zoom+delta));if(zoom===1)x=y=0;paint();}
 async function show(next,direction=0){
  const entry=drawings[next];if(!entry)return;const request=++ticket;animation?.cancel();index=next;zoom=1;x=y=0;rotation=entry.rotation||0;pointers.clear();gesture=null;
  image.style.visibility='hidden';image.src=`assets/art/${entry.id}-paper.png`;image.alt=entry.alt;image.parentElement.setAttribute('aria-label',entry.alt);$('art-title').textContent=entry.title;$('art-status').textContent=`${next+1} / ${drawings.length} · Restored paper study`;$('art-original').href=`assets/art/${entry.id}.jpg`;$('art-download').href=image.src;$('art-download').download=`studio-${entry.id}.png`;$('art-prev').disabled=next===0;$('art-next').disabled=next===drawings.length-1;onSelect(entry.id);
  try{await image.decode();}catch{if(request===ticket)$('art-status').textContent='Could not load this drawing. Try the original link.';return;}
  if(request!==ticket||!dialog.open)return;fit();image.style.visibility='visible';paper();
  if(!reduced.matches)animation=image.animate([{opacity:0,transform:`translate(-50%,-50%) translate(${direction*70}px,35px) rotate(${rotation*90-direction*4}deg) scale(.94)`},{opacity:1,transform:image.style.transform}],{duration:420,easing:'cubic-bezier(.2,.7,.25,1)'});
  const adjacent=drawings[next+1];if(adjacent){const preload=new Image();preload.src=`assets/art/${adjacent.id}-paper.png`;}
 }
 $('art-prev').onclick=()=>show(index-1,-1);$('art-next').onclick=()=>show(index+1,1);$('art-in').onclick=()=>magnify(.35);$('art-out').onclick=()=>magnify(-.35);$('art-reset').onclick=reset;$('art-rotate').onclick=()=>{rotation=(rotation+1)%4;reset();};
 addEventListener('keydown',e=>{if(!dialog.open)return;if(e.altKey||e.metaKey||e.ctrlKey)return;if(e.key==='ArrowLeft'){e.preventDefault();show(index-1,-1);}if(e.key==='ArrowRight'){e.preventDefault();show(index+1,1);}if(e.key==='+'||e.key==='='){e.preventDefault();magnify(.35);}if(e.key==='-'){e.preventDefault();magnify(-.35);}if(e.key.toLowerCase()==='r'){e.preventDefault();reset();}});
 stage.addEventListener('wheel',e=>{e.preventDefault();animation?.cancel();magnify(e.deltaY<0?.12:-.12);},{passive:false});
 const distance=()=>{const [a,b]=[...pointers.values()];return a&&b?Math.hypot(a.x-b.x,a.y-b.y):0;};
 stage.onpointerdown=e=>{if(e.button!==0||e.target.closest('button'))return;animation?.cancel();stage.setPointerCapture(e.pointerId);pointers.set(e.pointerId,{x:e.clientX,y:e.clientY});gesture={x:e.clientX,y:e.clientY,lastX:e.clientX,lastY:e.clientY,distance:distance(),pinched:pointers.size>1};};
 stage.onpointermove=e=>{if(!pointers.has(e.pointerId)||!gesture)return;pointers.set(e.pointerId,{x:e.clientX,y:e.clientY});if(pointers.size>1){const d=distance();if(gesture.distance)magnify((d-gesture.distance)/180);gesture.distance=d;gesture.pinched=true;}else if(zoom>1){x=Math.max(-stage.clientWidth,Math.min(stage.clientWidth,x+e.clientX-gesture.lastX));y=Math.max(-stage.clientHeight,Math.min(stage.clientHeight,y+e.clientY-gesture.lastY));paint();}gesture.lastX=e.clientX;gesture.lastY=e.clientY;};
 stage.onpointerup=e=>{if(!pointers.has(e.pointerId))return;const g=gesture;pointers.delete(e.pointerId);if(g&&!g.pinched&&zoom===1&&Math.abs(e.clientX-g.x)>55&&Math.abs(e.clientX-g.x)>Math.abs(e.clientY-g.y))show(index+(e.clientX<g.x?1:-1),e.clientX<g.x?1:-1);if(!pointers.size)gesture=null;};stage.onpointercancel=()=>{pointers.clear();gesture=null;};
 new ResizeObserver(()=>{if(dialog.open){x=y=0;fit();}}).observe(stage);
 dialog.addEventListener('close',()=>{ticket++;animation?.cancel();pointers.clear();gesture=null;});
 return {open:id=>show(Math.max(0,drawings.findIndex(d=>d.id===id)))};
}
