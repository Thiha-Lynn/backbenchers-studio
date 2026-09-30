import {createLofi} from './desktop/lofi.js?v=8';
import {createReader,studioJournal} from './desktop/reader.js?v=8';
import {createDesktop} from './desktop/desktop.js?v=8';
const $=id=>document.getElementById(id), canvas=$('unity-canvas');
let instance=null,loadingPromise=null,inStudio=false,cancelled=false,keys=new Set(),drag=null,currentPanel=null,lastFocus=null,renderer='';
const coarse=matchMedia('(pointer:coarse)').matches, reduced=matchMedia('(prefers-reduced-motion:reduce)').matches;
$('quality').value='auto';
const detailCap=()=>Math.min(devicePixelRatio||1,2,Math.sqrt((coarse?2400000:3800000)/(innerWidth*innerHeight)));
let renderScale=Math.min(detailCap(),coarse?1.35:1.65),lookX=0,lookY=0,lookFrame=0,seatPending=false,fastSamples=0;
const previews=new Map();
let lastViewport='',resizeTimer;
let prompt=null,focusedObject=null,pointerPoint='0.5,0.5',pointerAt=0,qualityCooldown=0,averageFps=60;
let motionReduced=reduced;
$('motion').value=reduced?'reduced':'smooth';
function queueLook(x,y){lookX+=x;lookY+=y;if(!lookFrame)lookFrame=requestAnimationFrame(()=>{lookFrame=0;const gain=Number($('sensitivity').value);send('Look',`${lookX*gain},${lookY*gain}`);lookX=lookY=0;});}
if(coarse)$('explore-hint').textContent='Left thumb to walk · Drag to look · Tap screens';
function send(method,value=''){if(instance)instance.SendMessage('StudioExperience',method,String(value));}
function stop(){keys.clear();lookX=lookY=0;cancelAnimationFrame(lookFrame);lookFrame=0;send('SetMove','0,0');drag=null;}
function setView(index){const names=['Make yourself at home.','Where ideas take shape.','The Backbenchers crew.','Curiosity takes flight.','Stories we keep close.'];$('view-number').textContent=`0${index+1} / ${['THE ROOM','THE WORKBENCH','BOUNTY GALLERY','DRONE LAB','READING ROOM'][index]}`;$('view-title').textContent=names[index];document.querySelectorAll('[data-view]').forEach(b=>b.setAttribute('aria-pressed',String(Number(b.dataset.view)===index)));}
function showStudio(){inStudio=true;document.body.classList.add('exploring');document.body.classList.remove('loading');$('welcome').hidden=true;$('corner-note').hidden=true;$('tour').hidden=false;$('loading').hidden=true;send('SetPaused','0');send('SetQuality',$('quality').value);send('SetMotion',motionReduced?'reduced':'smooth');sizeCanvas();canvas.focus();}
function welcome(){leaveInspection();hidePrompt();lofi.tour(false);inStudio=false;$('device-card').hidden=true;document.exitPointerLock?.();stop();send('SetPaused','1');document.body.classList.remove('exploring');$('welcome').hidden=false;$('corner-note').hidden=false;$('tour').hidden=true;$('enter').disabled=false;$('enter').querySelector('span').textContent=instance?'Return to the studio':'Step inside';$('enter').focus();}
function loadUnity(){if(loadingPromise)return loadingPromise;
loadingPromise=(async()=>{const response=await fetch('unity/build.json?v=8',{cache:'no-store'});if(!response.ok)throw Error('Studio build is not available.');const build=await response.json();await new Promise((resolve,reject)=>{const script=document.createElement('script');script.src='unity/'+build.loader;script.onload=resolve;script.onerror=()=>reject(Error('Could not load the studio engine.'));document.head.appendChild(script);});
const base='unity/';const config={dataUrl:base+build.data,frameworkUrl:base+build.framework,codeUrl:base+build.wasm,streamingAssetsUrl:base+'StreamingAssets',companyName:'Backbenchers Studio',productName:'Backbenchers Studio',productVersion:'1.6',matchWebGLToCanvasSize:true,devicePixelRatio:renderScale,showBanner:(message,type)=>{if(type==='error'){$('load-error').hidden=false;$('load-error').textContent='The 3D studio could not start on this device. Your portfolio links below remain available.';console.error(message);}},print:message=>console.log(message),printErr:message=>console.warn(message)};
const app=await window.createUnityInstance(canvas,config,progress=>{$('progress').style.width=Math.round(progress*100)+'%';$('loading-label').textContent=progress<.9?`Opening the doors… ${Math.round(progress*100)}%`:'Arranging the studio…';});instance=app;return app;})();return loadingPromise;}
$('enter').onclick=async()=>{lofi.tour(true);cancelled=false;$('load-error').hidden=true;if(instance){showStudio();return;}$('enter').disabled=true;$('loading').hidden=false;document.body.classList.add('loading');try{await loadUnity();if(!cancelled)showStudio();else{welcome();$('loading').hidden=true;}}catch(error){lofi.tour(false);console.error(error);loadingPromise=null;document.body.classList.remove('loading');$('loading').hidden=true;$('load-error').textContent='We couldn’t open the 3D studio. Please reload and try again, or explore the selected work and full portfolio.';$('load-error').hidden=false;$('enter').disabled=false;}};
$('cancel-load').onclick=()=>{lofi.tour(false);cancelled=true;$('loading-label').textContent='The studio is loading in the background. Browse the work while it finishes.';};
$('home').onclick=welcome;
function sizeCanvas(){qualityCooldown=performance.now()+4500;renderScale=Math.min(renderScale,detailCap());if(!instance)return;const viewport=innerWidth+'x'+innerHeight;if(viewport!==lastViewport){send('SetViewport',innerWidth/innerHeight<.8?'portrait':'landscape');lastViewport=viewport;}const dpr=$('quality').value==='auto'?renderScale:$('quality').value==='low'?.85:detailCap();if(instance.Module)instance.Module.devicePixelRatio=dpr;canvas.style.width='100%';canvas.style.height='100%';}
$('quality').onchange=()=>{fastSamples=0;send('SetQuality',$('quality').value);sizeCanvas();};
for(const button of document.querySelectorAll('[data-view]'))button.onclick=()=>{leaveInspection();stop();const i=Number(button.dataset.view);send('Visit',(motionReduced?'instant:':'')+i);setView(i);$('device-card').hidden=true;canvas.focus();};
function updateMove(){let x=0,y=0;if(keys.has('w')||keys.has('arrowup'))y++;if(keys.has('s')||keys.has('arrowdown'))y--;if(keys.has('a')||keys.has('arrowleft'))x--;if(keys.has('d')||keys.has('arrowright'))x++;send('SetMove',`${x},${y}`);}
addEventListener('keydown',e=>{if(!inStudio||currentPanel||seatPending||focusedObject||['INPUT','TEXTAREA','SELECT','BUTTON','A'].includes(document.activeElement.tagName))return;const k=e.key.toLowerCase();if(k==='e'){e.preventDefault();activatePrompt();return;}if(['w','a','s','d','arrowup','arrowdown','arrowleft','arrowright'].includes(k)){e.preventDefault();keys.add(k);updateMove();}});
addEventListener('keyup',e=>{keys.delete(e.key.toLowerCase());updateMove();});addEventListener('blur',stop);
canvas.onpointerdown=e=>{if(!inStudio||currentPanel||seatPending||e.button!==0)return;canvas.focus();canvas.setPointerCapture(e.pointerId);drag={id:e.pointerId,x:e.clientX,y:e.clientY,startX:e.clientX,startY:e.clientY,distance:0};};
canvas.onpointermove=e=>{if(!inStudio||currentPanel||focusedObject)return;pointerPoint=document.pointerLockElement===canvas?'0.5,0.5':`${e.clientX/innerWidth},${e.clientY/innerHeight}`;if(performance.now()-pointerAt>90){send('SetPointer',pointerPoint);pointerAt=performance.now();}if(document.pointerLockElement===canvas){queueLook(e.movementX,e.movementY);return;}if(!drag||drag.id!==e.pointerId)return;queueLook(e.clientX-drag.x,e.clientY-drag.y);drag.distance+=Math.hypot(e.clientX-drag.x,e.clientY-drag.y);drag.x=e.clientX;drag.y=e.clientY;};
canvas.onpointerleave=()=>{if(!drag&&document.pointerLockElement!==canvas){pointerPoint='0.5,0.5';send('SetPointer',pointerPoint);}};
canvas.onpointerup=e=>{if(drag&&drag.distance<7){const r=canvas.getBoundingClientRect();send('Interact',document.pointerLockElement===canvas?'0.5,0.5':`${(e.clientX-r.left)/r.width},${(e.clientY-r.top)/r.height}`);}drag=null;};canvas.onpointercancel=canvas.onlostpointercapture=()=>{drag=null;};
for(const b of document.querySelectorAll('[data-move]')){b.onpointerdown=e=>{e.preventDefault();b.setPointerCapture(e.pointerId);send('SetMove',b.dataset.move);};b.onpointerup=b.onpointercancel=b.onlostpointercapture=()=>send('SetMove','0,0');}
for(const b of document.querySelectorAll('[data-panel]'))b.onclick=()=>{leaveInspection();hidePrompt();stop();$('device-card').hidden=true;document.exitPointerLock?.();if(currentPanel)currentPanel.close();lastFocus=b;currentPanel=$(b.dataset.panel);send('SetPaused','1');currentPanel.showModal();};
for(const d of document.querySelectorAll('dialog')){d.querySelector('[data-close]').onclick=()=>d.close();d.addEventListener('close',()=>{if(currentPanel!==d)return;currentPanel=null;if(d===$('book-reader')){send('HoldBook','');document.body.classList.remove('reading-book');}if(d===$('desktop')){seatPending=false;desktop.close();document.body.classList.remove('using-computer','sitting-down');send('LeaveDevice');}qualityCooldown=performance.now()+4500;if(inStudio)send('SetPaused','0');lastFocus?.focus();});d.addEventListener('click',e=>{if(e.target===d){const r=d.getBoundingClientRect();if(e.clientX<r.left||e.clientX>r.right||e.clientY<r.top||e.clientY>r.bottom)d.close();}});}
document.addEventListener('visibilitychange',()=>{qualityCooldown=performance.now()+4500;stop();send('SetPaused',document.hidden||!inStudio||(currentPanel&&currentPanel!==$('desktop'))?'1':'0');});addEventListener('resize',()=>{clearTimeout(resizeTimer);resizeTimer=setTimeout(()=>{lastViewport='';sizeCanvas();},220);});
addEventListener('backbenchers-ready',e=>{renderer=e.detail.renderer;document.documentElement.dataset.renderer=renderer;});addEventListener('backbenchers-view',e=>setView(e.detail.index));

let selectedDevice='thomas',deviceState={id:'thomas',power:true,page:0};
const desktop=createDesktop({power:id=>send('DeviceAction',id+':power'),preview:(id,image)=>{previews.set(id,image);send('ScreenPreview',id+'|'+image);}});
$('device-use').onclick=()=>{leaveInspection();hidePrompt();stop();document.exitPointerLock?.();$('device-card').hidden=true;lastFocus=canvas;seatPending=true;desktop.open(deviceState);send('SetPaused','0');document.body.classList.add('using-computer','sitting-down');$('seat-status').hidden=false;send('FocusDevice',(motionReduced?'instant:':'')+deviceState.id);};
addEventListener('backbenchers-seat',e=>{
 if(!seatPending&&currentPanel!==$('desktop'))return;
 const d=e.detail,screen=$('desktop');
 const aligned=innerWidth>=700&&innerWidth/innerHeight>=1.05&&d.x>=0&&d.x+d.width<=1.01&&d.width*innerWidth>=580&&d.height*innerHeight>=290;
 screen.classList.toggle('screen-aligned',aligned);
 if(aligned){screen.style.setProperty('--screen-left',`${d.x*100}vw`);screen.style.setProperty('--screen-top',`${d.y*100}dvh`);screen.style.setProperty('--screen-width',`${d.width*100}vw`);screen.style.setProperty('--screen-height',`${d.height*100}dvh`);}
 if(seatPending){seatPending=false;currentPanel=screen;document.body.classList.remove('sitting-down');$('seat-status').hidden=true;screen.showModal();}
});
addEventListener('keydown',e=>{if(e.key==='Escape'&&seatPending){seatPending=false;desktop.close();document.body.classList.remove('using-computer','sitting-down');$('seat-status').hidden=true;send('LeaveDevice');canvas.focus();}});
const devicePages=['code','content','drone'];
$('devices-button').onclick=()=>{leaveInspection();hidePrompt();stop();document.exitPointerLock?.();send('SelectDevice',selectedDevice);};
$('device-select').onchange=()=>{selectedDevice=$('device-select').value;send('SelectDevice',selectedDevice);};
$('device-close').onclick=()=>{$('device-card').hidden=true;canvas.focus();};
$('device-power').onclick=()=>send('DeviceAction',selectedDevice+':power');
$('device-next').onclick=()=>{previews.delete(selectedDevice);send('DeviceAction',selectedDevice+':page');};
addEventListener('backbenchers-device',e=>{hidePrompt();const d=e.detail;deviceState=d;desktop.update(d);selectedDevice=d.id;stop();document.exitPointerLock?.();$('device-card').hidden=currentPanel===$('desktop');$('device-use').disabled=!d.power;$('device-select').value=d.id;$('device-preview').src=previews.has(d.id)?'data:image/jpeg;base64,'+previews.get(d.id):(d.id==='benchphone'&&d.page===0?'assets/egunion-phone-screen.png':'assets/studio-screen-'+devicePages[d.page]+'.jpg');$('device-preview').classList.toggle('off',!d.power);$('device-status').textContent=d.power?'Screen on · '+(d.id==='benchphone'&&d.page===0?'EGUnion mobile lab':['Software workspace','Creative workspace','Drone research'][d.page]):'Screen off · nearby glow off';$('device-power').textContent=d.power?'Turn off':'Turn on';$('device-power').setAttribute('aria-pressed',String(d.power));$('device-next').disabled=!d.power;});
const objectStories={
 'portrait-art':['A quiet portrait.','A faceless graphite interpretation of Aung San Suu Kyi. Warm oak, a fine brass edge, and cotton paper give this original AI-generated studio artwork a quiet place on the wall.'],
 'nature-art':['Roots, rivers, flight.','Jasmine, the Irrawaddy, and a bird in flight. These original AI-generated studies connect our Myanmar roots with the natural world.'],
 'fpv-art':['FPVStrike / airframe study','A static EGUnion game-art model. Look closely at its silhouette, camera, propellers, and surface details.'],
 'drone:phantom':['Phantom / flight study','A white airframe from EGUnion’s model collection. Explore the camera, landing gear, and propeller geometry in this static workbench study.'],
 'drone:racer':['Racer / workbench study','An EGUnion racing-drone model with an exposed frame, electronics, and vivid propellers. A small tribute to Merlin’s curiosity about flight.'],
 'game-art':['A game-art study.','A static EGUnion reference prop. Its silhouette and materials belong to the game-development side of our shared workbench.'],
 'identity':['Our pirate spirit.','Programming. Play. Flight. The original Backbenchers mark brings code, games, and quadcopter rotors together. The Jolly Roger is its pirate-crew companion.'],
 'hardware:mini':['Merlin’s Mac mini','A compact aluminium model with rounded edges, an Apple inlay, front USB-C ports, a headphone jack, and a recessed ventilation base. Its own clear space beside the drone.'],
 'hardware:pc':['A little computing power.','The studio PC sits below the workbench, leaving room for sketches and hands-on work. Choose a monitor to sit down and explore the workspace.'],
 nature:['Room to grow.','A little greenery among the work in progress. The plants keep the original office’s lived-in rhythm.'],
 archive:['The working archive.','Project folders, material samples, and keepsakes. These original office details stay in place alongside the selected Myanmar reading shelf.'],
 photography:['A different way to notice.','The camera and tripod are part of our everyday tools for finding a frame, studying light, and collecting references.'],
 tools:['Tools within reach.','The keyboard and mouse stay close to the screen. Tap a monitor when you’re ready to use the studio workspace.'],
 print:['From screen to paper.','Printed drafts, colour studies, and experiments. The printer and plotter belong to the room’s creative workbench.'],
 lighting:['A warmer kind of focus.','Red pendants and task lights keep the original studio character. Their soft lighting is baked into the room for a lighter rendering workload.'],
 coffee:['Stay a little longer.','A cup beside the work in progress. Take a break, browse a book, or let the lo-fi sketch play.'],
 electronics:['Small experiments.','Circuit boards and battery models form the research bench’s collection of static electronics studies.'],
 making:['Ideas start small.','Paper, pens, colour swatches, and half-finished sketches. There is room here for the first rough version.'],
 objects:['The things around the work.','Small, familiar objects give this studio its original character. A collected room, with space between things.'],
 furniture:['A place at the studio.','Natural wood and simple furniture bring the three workspaces together. Choose a screen to pull up a chair.'],
 shelves:['A working shelf.','Files and boxes keep their practical places. Selected Myanmar and English covers sit among them; pick up a book or open Books to browse every title.']
};
const crewPosters={thomas:['Thomas D. Lynn','Software engineer, creative builder, and founder of Backbenchers Studio. A fictional bounty portrait from our pirate crew.','https://thomasdlynn.dev/'],hlaing:['Hlaing Gyi','The Black-Leg Love-Cook. Content, design, and a place in the Backbenchers crew. A fictional bounty portrait.','https://mfu-hlaing.github.io/'],merlin:['Trafalgar D. Merlin','Drone and IoT engineer. Curiosity about flight, connected devices, and the next experiment. A fictional bounty portrait.','https://afk-merlin.github.io/']};
function hidePrompt(){$('object-prompt').hidden=true;$('object-marker').hidden=true;}
function activatePrompt(){if(currentPanel||focusedObject||seatPending)return;if(prompt?.kind==='device')send('SelectDevice',prompt.id);else if(prompt?.kind==='book')openBook(prompt.topic.slice(5));else if(prompt)send('FocusObject',prompt.id);else send('Interact',pointerPoint);}
$('object-prompt').onclick=activatePrompt;
addEventListener('backbenchers-prompt',e=>{prompt=e.detail.id?e.detail:null;if(!prompt||!inStudio||currentPanel||focusedObject||seatPending||!$('device-card').hidden){hidePrompt();return;}$('prompt-name').textContent=prompt.label;$('prompt-action').textContent=prompt.kind==='book'?'PICK UP & READ':prompt.kind==='device'?'USE SCREEN':'LOOK CLOSER';$('object-prompt').setAttribute('aria-label',$('prompt-action').textContent+' '+prompt.label);$('object-prompt').hidden=false;$('object-marker').hidden=false;$('object-marker').style.left=`${Math.max(2,Math.min(98,prompt.x*100))}%`;$('object-marker').style.top=`${Math.max(3,Math.min(94,prompt.y*100))}%`;});
function leaveInspection(){if(!focusedObject)return;focusedObject=null;$('object-inspector').hidden=true;document.body.classList.remove('inspecting-object');send('LeaveObject');hidePrompt();qualityCooldown=performance.now()+4500;canvas.focus();}
$('inspect-close').onclick=leaveInspection;
$('inspect-left').onclick=()=>send('OrbitObject','-20');$('inspect-right').onclick=()=>send('OrbitObject','20');$('inspect-near').onclick=()=>send('ZoomObject','-.12');$('inspect-far').onclick=()=>send('ZoomObject','.12');
function nextPoster(step){const ids=Object.keys(crewPosters),i=ids.indexOf(focusedObject?.topic.split(':')[1]);send('FocusObject','bounty:'+ids[(i+step+ids.length)%ids.length]);}
$('inspect-previous').onclick=()=>nextPoster(-1);$('inspect-next').onclick=()=>nextPoster(1);
addEventListener('keydown',e=>{if(e.key==='Escape'&&focusedObject){e.preventDefault();leaveInspection();}});
addEventListener('backbenchers-inspect',e=>{const item=e.detail,t=item.topic;if(t.startsWith('book:')){openBook(t.slice(5));return;}if(t==='library'){openLibrary();return;}
 const isNew=focusedObject?.id!==item.id;focusedObject=item;stop();hidePrompt();document.exitPointerLock?.();$('device-card').hidden=true;
 const crew=t.startsWith('bounty:')?crewPosters[t.slice(7)]:null,story=crew||objectStories[t]||objectStories.objects;
 $('inspect-title').textContent=crew?story[0]:(item.label||story[0]);$('inspect-copy').textContent=story[1];$('inspect-category').textContent=crew?'BACKBENCHERS / BOUNTY GALLERY':'BACKBENCHERS / '+(item.flat?'WALL GALLERY':'OBJECT STUDY');
 $('inspect-left').hidden=$('inspect-right').hidden=!!item.flat;$('inspect-previous').hidden=$('inspect-next').hidden=!crew;$('inspect-link').hidden=!crew;if(crew)$('inspect-link').href=crew[2];
 $('object-inspector').hidden=false;document.body.classList.add('inspecting-object');if(isNew){send('FocusObject',item.id);$('inspect-title').focus({preventScroll:true});}
});
$('walk-button').onclick=async()=>{stop();$('device-card').hidden=true;canvas.focus();try{await canvas.requestPointerLock();}catch{$('explore-hint').textContent='Drag to look · WASD to walk · Mouse capture unavailable here';}};
document.addEventListener('pointerlockchange',()=>{const locked=document.pointerLockElement===canvas;document.body.classList.toggle('pointer-locked',locked);$('explore-hint').textContent=locked?'WASD to walk · E to inspect · Esc to release mouse':coarse?'Left thumb to walk · Drag to look · Tap screens':'Drag to look · WASD to walk · Tap a screen to explore';if(!locked)stop();});

// Frame pacing is measured inside Unity, so idle requestAnimationFrame callbacks do not inflate it.
addEventListener('backbenchers-performance',e=>{
 if(!inStudio||document.hidden||currentPanel||seatPending)return;
 const fps=e.detail.fps;$('fps').textContent=`${Math.round(fps)} fps`;
 if($('quality').value!=='auto'||performance.now()<qualityCooldown)return;
 averageFps=averageFps*.45+fps*.55;
 const old=renderScale;
 if(averageFps<51){renderScale=Math.max(.7,renderScale-(averageFps<38?.18:.10));fastSamples=0;}
 else if(averageFps>58){if(++fastSamples>=5){renderScale=Math.min(detailCap(),renderScale+.08);fastSamples=0;}}
 else fastSamples=0;
 if(old!==renderScale)sizeCanvas();
});
$('motion').onchange=()=>{motionReduced=$('motion').value==='reduced';send('SetMotion',motionReduced?'reduced':'smooth');};
const lofi=createLofi($('lofi-button'),$('lofi-volume'));
const reader=createReader({paper:()=>lofi.paper(),onImport:book=>openBook(book)});
let libraryLoaded=false,bookCatalog=null;
async function catalog(){if(bookCatalog)return bookCatalog;const response=await fetch('assets/books/catalog.json?v=8');if(!response.ok)throw Error('Could not load books');return bookCatalog=await response.json();}
async function openBook(id){
 try{const book=typeof id==='object'?id:(await catalog()).find(b=>b.id===id);if(!book)return;
 leaveInspection();hidePrompt();stop();document.exitPointerLock?.();$('device-card').hidden=true;const focus=currentPanel?canvas:document.activeElement;if(currentPanel&&currentPanel!==$('book-reader'))currentPanel.close();lastFocus=focus;currentPanel=$('book-reader');send('HoldBook',book.id);send('SetPaused','1');document.body.classList.add('reading-book');currentPanel.showModal();reader.open(book);if(book.pdfUrl)await reader.openSource(book);
 }catch(error){$('reader-message').textContent='The book could not open. Please close it and try again.';console.warn(error);}
}
async function openLibrary(){
 leaveInspection();hidePrompt();stop();document.exitPointerLock?.();lastFocus=document.activeElement;if(currentPanel)currentPanel.close();currentPanel=$('library');send('SetPaused','1');currentPanel.showModal();
 if(libraryLoaded)return;
 try{const books=await catalog();$('library-grid').replaceChildren();
 for(const book of books){const card=document.createElement('article');card.className='book-card';const img=document.createElement('img');img.src='assets/books/'+(book.preview||book.cover);img.alt=book.title+' book cover';img.loading='lazy';const coverLink=document.createElement('button');coverLink.className='book-cover-link';coverLink.setAttribute('aria-label','Pick up '+book.title);coverLink.onclick=()=>openBook(book);coverLink.append(img);const category=document.createElement('small');category.textContent=book.category;const title=document.createElement('h3');title.textContent=book.title;const author=document.createElement('p');author.textContent=book.author;const note=document.createElement('p');note.textContent=book.note;const link=document.createElement('a');link.href=book.source;link.target='_blank';link.rel='noopener';link.textContent='Book details ↗';card.append(coverLink,category,title,author,note,link);$('library-grid').append(card);}libraryLoaded=true;
 }catch{$('library-grid').textContent='The reading list could not load. Close and reopen to try again.';}
}
$('classic-button').onclick=()=>openBook('maung-yin-maung');
$('journal-button').onclick=()=>openBook(studioJournal);
$('open-pdf-button').onclick=()=>{openBook(studioJournal);$('reader-import').click();};
$('library-button').onclick=openLibrary;
for(const b of document.querySelectorAll('[data-library]'))b.onclick=openLibrary;
// A left-thumb stick leaves the right half of the scene available for simultaneous look input.
const stick=$('move-stick'),knob=$('stick-knob');let stickPointer=null;
function moveStick(e){const r=stick.getBoundingClientRect(),radius=r.width*.33;let x=(e.clientX-r.left-r.width/2)/radius,y=(e.clientY-r.top-r.height/2)/radius;const length=Math.hypot(x,y);if(length>1){x/=length;y/=length;}if(length<.13)x=y=0;knob.style.transform=`translate(${x*radius}px,${y*radius}px)`;send('SetMove',`${x.toFixed(3)},${(-y).toFixed(3)}`);}
stick.onpointerdown=e=>{if(!inStudio||currentPanel||seatPending)return;e.preventDefault();stickPointer=e.pointerId;stick.setPointerCapture(e.pointerId);moveStick(e);};
stick.onpointermove=e=>{if(e.pointerId===stickPointer)moveStick(e);};
stick.onpointerup=stick.onpointercancel=stick.onlostpointercapture=()=>{stickPointer=null;knob.style.transform='';send('SetMove','0,0');};

for(const button of document.querySelectorAll('[data-inspect]'))button.onclick=()=>{if(currentPanel)currentPanel.close();hidePrompt();stop();send('FocusObject',button.dataset.inspect);};
