import {PageFlip} from './pageflip/page-flip.module.js?v=11';
export function createReader({paper,onImport}){
 const $=id=>document.getElementById(id),dialog=$('book-reader'),stage=$('reader-stage'),spread=$('reader-spread');
 let book=null,pages=[],page=0,turning=false,epoch=0,start=null,font=17,zoom=1,paintEpoch=0;
 let transition=null,closing=false,returnPromise=null;
 let engine=null,pendingPage=null,chapters=[],leafWidth=320,leafHeight=460;
 let pdfModule;const pdfText=new Map(),renderTasks=new Set();
 const settle=animation=>Promise.race([animation.finished.catch(()=>{}),new Promise(resolve=>setTimeout(resolve,1200))]);
 const compact=matchMedia('(max-height:550px)');
 function setTools(open){$('reader-tools').hidden=!open;$('reader-tools-toggle').setAttribute('aria-expanded',String(open));}
 $('reader-tools-toggle').onclick=()=>{setTools($('reader-tools').hidden);reflow();};compact.addEventListener('change',()=>{setTools(!compact.matches);if(dialog.open)reflow();});
 const wide=matchMedia('(min-width:800px)'),reduced=matchMedia('(prefers-reduced-motion:reduce)');
 const read=(key,fallback)=>{try{return JSON.parse(localStorage.getItem(key))??fallback;}catch{return fallback;}};
 const save=(key,value)=>{try{localStorage.setItem(key,JSON.stringify(value));}catch{}};
 font=Math.max(14,Math.min(24,read('bb-reader-font',17)));dialog.style.setProperty('--reader-size',font+'px');
 const count=()=>wide.matches?2:1;
 function markedPage(){const m=read(key()+':mark',-1);return typeof m==='number'?m:findAnchor(m.anchor,m.page);}
 const key=()=>`bb-reader:${book.id}`;
 const chapterTokens=new WeakMap();
 function dimensions(){const css=getComputedStyle(stage);const h=Math.max(80,stage.clientHeight-parseFloat(css.paddingTop)-parseFloat(css.paddingBottom));const w=stage.clientWidth-parseFloat(css.paddingLeft)-parseFloat(css.paddingRight);leafHeight=Math.floor(Math.min(800,h*.90));leafWidth=Math.floor(Math.min(550,w*.98/count(),Math.max(240,leafHeight*.83)));dialog.style.setProperty('--leaf-width',leafWidth+'px');dialog.style.setProperty('--leaf-height',leafHeight+'px');}
 function disposeEngine(){if(engine){engine.destroy();engine=null;}pendingPage=null;}
 function paginate(){
  dimensions();if(book.pdfDoc){pages=chapters;return;}
  const measure=el('div');measure.className='reader-measure';dialog.append(measure);const result=[];
  for(let chapter=0;chapter<chapters.length;chapter++){const data=chapters[chapter];if(data.notes||!data.body){result.push({...data,anchor:[chapter,0]});continue;}
   let tokens=chapterTokens.get(data);if(!tokens){tokens=typeof Intl.Segmenter==='function'?Array.from(new Intl.Segmenter(undefined,{granularity:'word'}).segment(data.body),x=>x.segment):Array.from(data.body);chapterTokens.set(data,tokens);}
   let cursor=0;while(cursor<tokens.length){let lo=cursor+1,hi=tokens.length,best=lo;while(lo<=hi){const end=Math.floor((lo+hi)/2);const part={...data,title:cursor===0?data.title:'',body:tokens.slice(cursor,end).join(''),source:end===tokens.length?data.source:null};const probe=pageElement(result.length,part,true);measure.replaceChildren(probe);const content=probe.querySelector('.page-copy');if(content.scrollHeight<=content.clientHeight+1){best=end;lo=end+1;}else hi=end-1;}
    if(best<tokens.length&&tokens.length-best<35){const chunk=tokens.slice(cursor,best).join('');const breakAt=chunk.lastIndexOf('\n\n');if(breakAt>chunk.length*.35){let chars=0,end=cursor;while(end<best&&chars<breakAt){chars+=tokens[end].length;end++;}best=end;}}
    result.push({...data,title:cursor===0?data.title:'',body:tokens.slice(cursor,best).join('').trim(),source:best===tokens.length?data.source:null,anchor:[chapter,cursor]});cursor=best;}
  }
  measure.remove();pages=result;
 }
 function findAnchor(anchor,fallback){if(!anchor)return fallback;let found=0;for(let i=0;i<pages.length;i++){const a=pages[i].anchor;if(a&&a[0]===anchor[0]&&a[1]<=anchor[1])found=i+1;}return found||fallback;}
 function contents(){const cover=el('option','Cover');cover.value='0';$('reader-contents').replaceChildren(cover);pages.forEach((p,i)=>{const o=el('option',`${i+1} · ${p.title||'Continued'}`);o.value=String(i+1);$('reader-contents').append(o);});}
 function reflow(){if(closing)return;if(turning){scheduleFit();return;}const anchor=page>0?pages[page-1]?.anchor:null;paginate();page=page===0?0:findAnchor(anchor,page);contents();render();}
 const el=(tag,text)=>{const e=document.createElement(tag);if(text)e.textContent=text;return e;};
 async function paintPdf(canvas,index,cover=false){
  const ticket=paintEpoch,doc=book?.pdfDoc;if(!doc)return;
  try{const pdfPage=await doc.getPage(index+1);if(ticket!==paintEpoch||!canvas.isConnected)return;
   const host=canvas.parentElement,base=pdfPage.getViewport({scale:1});
   const fit=Math.min((host.clientWidth-32)/base.width,(host.clientHeight-32)/base.height)*(cover?1:zoom);
   const viewport=pdfPage.getViewport({scale:Math.max(.1,fit)}),output=Math.min(devicePixelRatio||1,2,Math.sqrt(2400000/(viewport.width*viewport.height)));
   canvas.width=Math.ceil(viewport.width*output);canvas.height=Math.ceil(viewport.height*output);canvas.style.width=viewport.width+'px';canvas.style.height=viewport.height+'px';
   const task=pdfPage.render({canvasContext:canvas.getContext('2d'),viewport,transform:[output,0,0,output,0,0]});renderTasks.add(task);
   try{await task.promise;}finally{renderTasks.delete(task);}if(ticket!==paintEpoch)return;
   canvas.parentElement.querySelector('.pdf-loading')?.remove();canvas.setAttribute('aria-label',`PDF page ${index+1}`);
   const content=await pdfPage.getTextContent();if(ticket!==paintEpoch)return;const text=content.items.map(x=>x.str||'').join(' ');pdfText.set(index,text);
   const transcript=el('div',text);transcript.className='sr-only';canvas.parentElement.append(transcript);
  }catch(error){if(ticket===paintEpoch&&error.name!=='RenderingCancelledException')$('reader-message').textContent='This PDF page could not render. Try another page.';}
 }
 function pageElement(index,data=pages[index],probe=false){
  const article=el('article');article.className='reader-page';article.setAttribute('aria-label',`Page ${index+1}`);if(!data)return article;
  if(book.pdfDoc){article.classList.add('pdf-page');const canvas=el('canvas');canvas.setAttribute('role','img');article.append(canvas);const loading=el('span','Opening page…');loading.className='pdf-loading';article.append(loading);if(!probe)requestAnimationFrame(()=>paintPdf(canvas,index));return article;}
  const running=el('small',book.title);running.className='page-running';const copy=el('div');copy.className='page-copy';article.append(running,copy);
  if(data.title)copy.append(el('h3',data.title));
  for(const paragraph of (data.body||'').split('\n\n'))if(paragraph)copy.append(el('p',paragraph));
  if(data.source){const a=el('a','Source / book details ↗');a.href=data.source;a.target='_blank';a.rel='noopener';copy.append(a);}
  if(data.notes){const notes=el('textarea');notes.className='reader-notes';notes.placeholder='A sentence to remember…';notes.setAttribute('aria-label','Your private reading notes');notes.value=read(key()+':notes','');notes.maxLength=12000;for(const type of ['pointerdown','mousedown','touchstart'])notes.addEventListener(type,e=>e.stopPropagation());notes.oninput=()=>{save(key()+':notes',notes.value);$('reader-message').textContent='Notes saved in this browser';};copy.append(notes);}
  const number=el('span',String(index+1));number.className='page-number';article.append(number);return article;
 }
 function mountBook(){
  const mount=el('div');mount.className='paper-binding';spread.append(mount);spread.style.width=leafWidth*count()+'px';spread.style.height=leafHeight+'px';
  const from=Math.max(0,page-1-count()),to=Math.min(pages.length,page-1+count()*2);const leaves=[];for(let i=from;i<to;i++)leaves.push(pageElement(i));if(wide.matches&&leaves.length%2)leaves.push(pageElement(pages.length));
  engine=new PageFlip(mount,{width:leafWidth,height:leafHeight,size:'fixed',autoSize:false,usePortrait:!wide.matches,showCover:false,startPage:page-1-from,flippingTime:920,maxShadowOpacity:.42,drawShadow:true,disableFlipByClick:true,showPageCorners:true,mobileScrollSupport:false,useMouseEvents:false});
  const ticket=epoch,generation=paintEpoch;let sounded=false;
  engine.on('flip',event=>{const next=from+event.data+1;if(next!==page&&next<=pages.length)pendingPage=next;});
  engine.on('changeState',event=>{turning=event.data!=='read';if(turning&&!sounded){paper();sounded=true;}if(event.data==='read')sounded=false;if(event.data==='read'&&pendingPage!==null){const target=pendingPage;pendingPage=null;setTimeout(()=>{if(ticket!==epoch||generation!==paintEpoch||!dialog.open)return;page=target;render();},0);}});
  engine.loadFromHTML(leaves);
  // Pointer capture starts a fold immediately and keeps it attached to the finger.
  const binding=engine;let finger=null;
  const position=e=>{const r=mount.querySelector('.stf__block').getBoundingClientRect();return {x:e.clientX-r.left,y:e.clientY-r.top};};
  mount.style.touchAction=book.pdfDoc&&zoom>1?'pan-x pan-y':'none';
  mount.onpointerdown=e=>{if(turning||e.button!==0||e.target.closest('textarea,input,a,button')||(book.pdfDoc&&zoom>1))return;finger={id:e.pointerId,x:e.clientX,y:e.clientY};mount.setPointerCapture(e.pointerId);binding.startUserTouch(position(e));e.preventDefault();};
  mount.onpointermove=e=>{if(!finger||finger.id!==e.pointerId||reduced.matches)return;const dx=e.clientX-finger.x;if(!finger.started){if(Math.abs(dx)<7)return;finger.direction=dx<0?-1:1;const bounds=binding.getBoundsRect();if(!binding.getFlipController().start({x:dx<0?bounds.left+2*bounds.pageWidth-2:2,y:position(e).y})){binding.userStop(position(e),true);finger=null;return;}finger.started=true;}binding.userMove(position(e),true);};
  mount.onpointerup=e=>{if(!finger||finger.id!==e.pointerId)return;const dx=e.clientX-finger.x,dy=e.clientY-finger.y,commit=Math.abs(dx)>leafWidth*.2&&Math.abs(dx)>Math.abs(dy),direction=finger.direction;finger=null;binding.userStop(position(e),true);if(reduced.matches){if(commit)turn(dx<0?1:-1);}else binding.getFlipController().stopMove(commit&&direction*dx>0);};
  mount.onpointercancel=e=>{if(finger){finger=null;binding.userStop(position(e),true);binding.getFlipController().stopMove(false);}};

 }
 function render(){
  if(!book)return;disposeEngine();turning=false;paintEpoch++;for(const task of renderTasks)task.cancel();renderTasks.clear();page=Math.max(0,Math.min(page,pages.length));if(page>0&&wide.matches)page=1+Math.floor((page-1)/2)*2;$('reader-cover').hidden=page!==0;spread.hidden=page===0;
  if(page===0&&book.pdfDoc)requestAnimationFrame(()=>paintPdf($('reader-pdf-cover'),0,true));
  spread.replaceChildren();if(page>0)mountBook();
  $('reader-status').textContent=page===0?'Cover':`${page}${count()===2&&page<pages.length?'–'+(page+1):''} / ${pages.length}`;
  $('reader-prev').disabled=page===0;$('reader-next').disabled=page>0&&page+count()>pages.length;$('reader-next').textContent=page===0?'Open book ↗':'Next →';
  $('reader-contents').value=String(page);$('reader-bookmark').setAttribute('aria-pressed',String(markedPage()===page));save(key()+':page',page);if(page>0&&pages[page-1]?.anchor)save(key()+':anchor',pages[page-1].anchor);
 }
 function turn(direction){
  if(turning||!book)return;const next=direction>0?(page===0?1:page+count()):Math.max(0,page-count());if(next>pages.length||next===page)return;
  if(page===0||next===0){hingeCover(next);return;}
  if(reduced.matches){paper();page=next;render();return;}
  if(engine){if(direction>0)engine.flipNext('bottom');else engine.flipPrev('bottom');}
 }
 async function hingeCover(next){
  const ticket=epoch,opening=page===0,cover=$('reader-cover');paper();
  if(reduced.matches){page=next;render();return;}
  const rect=cover.getBoundingClientRect(),bounds=stage.getBoundingClientRect();
  if(opening){
   const leaf=cover.cloneNode(true);leaf.removeAttribute('id');leaf.querySelectorAll('[id]').forEach(e=>e.removeAttribute('id'));leaf.querySelector('span')?.remove();const sourceCanvas=cover.querySelector('canvas'),copyCanvas=leaf.querySelector('canvas');if(sourceCanvas&&!sourceCanvas.hidden){copyCanvas.width=sourceCanvas.width;copyCanvas.height=sourceCanvas.height;copyCanvas.getContext('2d').drawImage(sourceCanvas,0,0);}
   leaf.classList.add('turning-cover');leaf.setAttribute('aria-hidden','true');leaf.disabled=true;
   Object.assign(leaf.style,{position:'absolute',left:(rect.left-bounds.left)+'px',top:(rect.top-bounds.top)+'px',width:rect.width+'px',height:rect.height+'px',maxWidth:'none',maxHeight:'none',margin:'0',transformOrigin:'left center',animation:'none'});
   page=next;render();turning=true;stage.append(leaf);
   transition=leaf.animate([{transform:'rotateY(-8deg) rotateZ(-1deg)',opacity:1},{transform:'rotateY(-110deg) rotateZ(0deg)',opacity:1,offset:.7},{transform:'rotateY(-172deg)',opacity:0}],{duration:820,easing:'cubic-bezier(.25,.65,.25,1)',fill:'forwards'});
   await settle(transition);leaf.remove();
  }else{
   turning=true;transition=spread.animate([{transform:'rotateX(0) scale(1)',opacity:1},{transform:'rotateX(12deg) scale(.94)',opacity:0}],{duration:260,easing:'ease-in',fill:'forwards'});await settle(transition);transition.cancel();if(ticket!==epoch)return;page=0;render();
  }
  if(ticket===epoch){turning=false;transition=null;}
 }
 function putBack(){returnPromise??=performReturn().finally(()=>returnPromise=null);return returnPromise;}
 async function performReturn(){
  if(closing)return;closing=true;epoch++;paper();
  const target=page===0?$('reader-cover'):spread;
  if(!reduced.matches){transition=target.animate([{transform:'translateY(0) rotateZ(0) scale(1)',opacity:1},{transform:'translateY(120px) rotateZ(6deg) scale(.7)',opacity:0}],{duration:360,easing:'ease-in',fill:'forwards'});await settle(transition);transition.cancel();transition=null;}
  closing=false;
 }
 function open(value){
  setTools(!compact.matches);
  epoch++;transition?.cancel();transition=null;closing=false;stage.querySelectorAll('.turning-cover').forEach(e=>e.remove());disposeEngine();paintEpoch++;for(const task of renderTasks)task.cancel();renderTasks.clear();turning=false;if(book?.pdfDoc&&book.pdfDoc!==value.pdfDoc)book.pdfDoc.loadingTask.destroy().catch(()=>{});book=value;pdfText.clear();zoom=1;stage.style.touchAction="pan-y";
  chapters=value.pages||(value.pdfUrl?[{title:value.title,body:'Opening the original novel from the studio archive…'}]:[
   {title:value.title,body:`${value.author}\n\n${value.category}\n\n${value.note}\n\nThis is a reading companion. The published book’s full text is not included.`,source:value.source},
   {title:'In the margins',body:'Keep a thought here while you browse. These notes stay in this browser.',notes:true}
  ]);
  paginate();page=value.pdfDoc?read(key()+':page',1):0;if(page>0&&!value.pdfDoc)page=findAnchor(read(key()+':anchor',null),page);$('reader-cover-image').hidden=!!value.pdfDoc;$('reader-pdf-cover').hidden=!value.pdfDoc;$('reader-cover').classList.toggle('pdf-cover',!!value.pdfDoc);$('reader-smaller').textContent=value.pdfDoc?'−':'A−';$('reader-larger').textContent=value.pdfDoc?'+':'A+';$('reader-smaller').setAttribute('aria-label',value.pdfDoc?'Zoom out':'Smaller text');$('reader-larger').setAttribute('aria-label',value.pdfDoc?'Zoom in':'Larger text');$('reader-title').textContent=value.title;$('reader-cover-image').src=value.cover.startsWith('assets/')?value.cover:'assets/books/'+value.cover;$('reader-cover-image').alt=value.title+' cover';
  const crop={ava2:[.83,.9,.07,.035],beyond:[.68,.97,.15,.01],odyssey:[.74,.79,.115,.02]}[value.id]||[1,1,0,0];const art=$('reader-cover-image');art.style.width=100/crop[0]+'%';art.style.height=100/crop[1]+'%';art.style.left=-crop[2]/crop[0]*100+'%';art.style.top=-(1-crop[1]-crop[3])/crop[1]*100+'%';
  contents();$('reader-message').textContent=value.pdfUrl?'Original 1904 novel · Archival scan · Drag a corner to turn':'Drag a paper corner · Swipe or use ← →';$('reader-search-input').value='';$('reader-search-input').disabled=!!value.pdfUrl;$('reader-search').querySelector('button').disabled=!!value.pdfUrl;$('reader-search-input').placeholder=value.pdfUrl?'Scan · use contents':'Find a passage';render();if(!reduced.matches){const target=page===0?$('reader-cover'):spread;target.animate([{transform:'translateY(100px) rotateX(18deg) rotateZ(-6deg) scale(.78)',opacity:0},{transform:'translateY(0) rotateX(0deg) rotateZ(0deg) scale(1)',opacity:1}],{duration:640,easing:'cubic-bezier(.18,.7,.2,1)'});}
  const ticket=epoch;document.fonts.load(`${font}px "Noto Serif Myanmar"`).then(()=>{if(ticket===epoch&&dialog.open&&!book.pdfDoc)reflow();});
 }
 $('reader-next').onclick=()=>turn(1);$('reader-prev').onclick=()=>turn(-1);$('reader-cover').onclick=()=>turn(1);
 $('reader-contents').onchange=()=>{if(!turning){page=Number($('reader-contents').value);render();}};
 $('reader-bookmark').onclick=()=>{const marked=markedPage()===page;save(key()+':mark',marked?-1:{page,anchor:pages[page-1]?.anchor});$('reader-message').textContent=marked?'Bookmark removed':'Page bookmarked';render();};
 $('reader-resume').onclick=()=>{const mark=markedPage();if(mark>=0){page=mark;render();}else $('reader-message').textContent='Bookmark a page first';};
 function textSize(change){if(book?.pdfDoc){zoom=Math.max(.75,Math.min(3,zoom+change*.25));stage.style.touchAction=zoom>1?'pan-x pan-y':'pan-y';render();$('reader-message').textContent='Zoom '+Math.round(zoom*100)+'%';return;}font=Math.max(14,Math.min(24,font+change));dialog.style.setProperty('--reader-size',font+'px');save('bb-reader-font',font);reflow();}
 $('reader-smaller').onclick=()=>textSize(-1);$('reader-larger').onclick=()=>textSize(1);
 $('reader-fullscreen').onclick=async()=>{dialog.classList.add('reader-focused');$('reader-unfocus').hidden=false;reflow();try{await dialog.requestFullscreen();}catch{}if(dialog.open)reflow();};
 $('reader-unfocus').onclick=async()=>{dialog.classList.remove('reader-focused');$('reader-unfocus').hidden=true;try{if(document.fullscreenElement===dialog)await document.exitFullscreen();}catch{}reflow();$('reader-fullscreen').focus();};
 $('reader-search').onsubmit=async e=>{e.preventDefault();const q=$('reader-search-input').value.trim().toLocaleLowerCase();if(!q)return;const ticket=epoch;let found=-1;
  for(let offset=0;offset<pages.length;offset++){if(ticket!==epoch)return;const i=(page+offset)%pages.length;let body=pages[i].body||'';
   if(book.pdfDoc){$('reader-message').textContent=`Searching page ${i+1}…`;try{if(!pdfText.has(i)){const p=await book.pdfDoc.getPage(i+1);const t=await p.getTextContent();if(ticket!==epoch)return;pdfText.set(i,t.items.map(x=>x.str||'').join(' '));}body=pdfText.get(i);}catch{continue;}}
   if((pages[i].title+' '+body).toLocaleLowerCase().includes(q)){found=i;break;}}
  if(ticket!==epoch)return;if(found>=0){page=found+1;render();$('reader-message').textContent='Found on page '+page;}else $('reader-message').textContent=book.pdfDoc?'No matching text. Scanned pages may not contain searchable text.':'No matching page';};
 dialog.addEventListener('keydown',e=>{if(e.target.matches('input,textarea,select'))return;if(e.key==='ArrowRight'||e.key==='ArrowLeft'){e.preventDefault();turn(e.key==='ArrowRight'?1:-1);}if(e.key==='+'||e.key==='=')textSize(1);if(e.key==='-')textSize(-1);});
 let resizeTimer;const scheduleFit=()=>{clearTimeout(resizeTimer);resizeTimer=setTimeout(()=>{if(dialog.open)reflow();},100);};addEventListener('resize',scheduleFit);window.visualViewport?.addEventListener('resize',scheduleFit);document.addEventListener('fullscreenchange',scheduleFit);new ResizeObserver(scheduleFit).observe(stage);dialog.addEventListener('close',()=>{transition?.cancel();transition=null;stage.querySelectorAll('.turning-cover').forEach(e=>e.remove());dialog.classList.remove('reader-focused');$('reader-unfocus').hidden=true;disposeEngine();epoch++;paintEpoch++;for(const task of renderTasks)task.cancel();renderTasks.clear();turning=false;start=null;if(book?.pdfDoc){book.pdfDoc.loadingTask.destroy().catch(()=>{});book=null;pdfText.clear();}if(document.fullscreenElement===dialog)document.exitFullscreen().catch(()=>{});});
 $('reader-import').onchange=async e=>{const file=e.target.files[0];if(!file)return;const importEpoch=epoch;try{
   if(/\.pdf$/i.test(file.name)||file.type==='application/pdf'){
    if(file.size>40*1024*1024)throw Error('Choose a PDF smaller than 40 MB.');$('reader-message').textContent='Opening your PDF locally…';
    pdfModule??=await import('./pdfjs/pdf.min.mjs');pdfModule.GlobalWorkerOptions.workerSrc=new URL('./pdfjs/pdf.worker.min.mjs',import.meta.url).href;
    const base=new URL('./pdfjs/',import.meta.url).href;const loading=pdfModule.getDocument({data:new Uint8Array(await file.arrayBuffer()),cMapUrl:base+'cmaps/',cMapPacked:true,standardFontDataUrl:base+'standard_fonts/',wasmUrl:base+'wasm/',isEvalSupported:false});
    let document;try{document=await loading.promise;}catch(error){await loading.destroy();throw Error(error.name==='PasswordException'?'This PDF is password-protected. Import an unlocked copy.':'This PDF could not open. Try another file.');}
    if(importEpoch!==epoch||!dialog.open){await loading.destroy();return;}if(document.numPages>2000){await loading.destroy();throw Error('Choose a PDF with fewer than 2,001 pages.');}
    onImport({id:'pdf-'+file.name+'-'+file.size+'-'+file.lastModified,title:file.name,cover:'assets/studio-journal-cover.svg',pdfDoc:document,pages:Array.from({length:document.numPages},(_,i)=>({title:'Page '+(i+1)}))});return;
   }
   if(file.size>2*1024*1024)throw Error('Choose a text file smaller than 2 MB.');const text=(await file.text()).replace(/\r\n?/g,'\n');if(!text.trim())throw Error('This file is empty.');
   const segments=typeof Intl.Segmenter==='function'?Array.from(new Intl.Segmenter(undefined,{granularity:'grapheme'}).segment(text),s=>s.segment):Array.from(text);const imported=[];
   for(let i=0;i<segments.length;i+=480)imported.push({title:i===0?file.name:`${file.name} · ${imported.length+1}`,body:segments.slice(i,i+480).join('')});
   if(importEpoch!==epoch||!dialog.open)return;onImport({id:'local-'+file.name+'-'+file.size,title:file.name,author:'Your local manuscript',cover:'assets/studio-journal-cover.svg',pages:imported});
  }catch(error){if(importEpoch===epoch&&dialog.open)$('reader-message').textContent=error.message;}finally{e.target.value='';}};
 async function openSource(value){
  const ticket=epoch;
  $('reader-message').textContent='Opening the archival novel…';
  pdfModule??=await import('./pdfjs/pdf.min.mjs');pdfModule.GlobalWorkerOptions.workerSrc=new URL('./pdfjs/pdf.worker.min.mjs',import.meta.url).href;
  const base=new URL('./pdfjs/',import.meta.url).href;const loading=pdfModule.getDocument({url:value.pdfUrl,cMapUrl:base+'cmaps/',cMapPacked:true,standardFontDataUrl:base+'standard_fonts/',wasmUrl:base+'wasm/',isEvalSupported:false,disableAutoFetch:true,disableStream:true});
  try{const doc=await loading.promise;if(!dialog.open||ticket!==epoch){await loading.destroy();return;}open({...value,pdfDoc:doc,pages:Array.from({length:doc.numPages},(_,i)=>({title:'စာမျက်နှာ '+(i+1)}))});}catch{await loading.destroy();if(ticket===epoch&&dialog.open)$('reader-message').textContent='The novel could not load. Please try again.';}
 }
 return {open,openSource,putBack};
}

export const studioJournal={id:'studio-journal',title:'A Little Room for Good Ideas',author:'Backbenchers Studio',cover:'assets/studio-journal-cover.svg',pages:[
 {title:'Make yourself at home.',body:'There is a certain kind of room that invites you to stay. A desk worn smooth at the edge. A book left open beside a keyboard. An idea that began as a scribble and is still finding its shape.\n\nThis is our small version of that room. Take your time. There is no correct route through it.'},
 {title:'A working shelf',body:'The files and boxes have work to do. They belong here as much as the books. A studio should leave room for the practical things: old folders, spare cables, a cup, the project that is not quite finished.\n\nThe reading shelf brings Myanmar fiction, remembered lives, poetry, and history into that everyday setting.'},
 {title:'Three ways of making',body:'Software begins with a question. A story begins with noticing. A flight experiment begins with looking up.\n\nThe three workspaces hold those different habits together. Open a screen, write a small program, or leave a note. The local workspace is yours to explore.'},
 {title:'Roots, rivers, flight',body:'The portrait, jasmine and bird studies share a quiet wall in warm oak frames. A river study sits beside the reading shelf. Pencil drawings rest on the desks, leaving room for the next idea.\n\nWarm paper, oak, forest green, and small brass details connect the pieces without making them identical.'},
 {title:'The quiet between things',body:'A gentle rhythm can make a room feel occupied. The studio’s lo-fi sketch uses warm chords, a soft bass, and brushed percussion. You can mute it at any time or adjust its volume in the exploration controls.\n\nSilence belongs here too.'},
 {title:'Leave a line',body:'A thought, a question, a title to look for next time.\n\nThis short journal is original studio writing. The published books on the shelf have their own authors and stories; their companion pages link to the book sources.',notes:true}
]};
