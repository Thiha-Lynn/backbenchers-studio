export function createReader({paper,onImport}){
 const $=id=>document.getElementById(id),dialog=$('book-reader'),stage=$('reader-stage'),spread=$('reader-spread');
 let book=null,pages=[],page=0,turning=false,epoch=0,start=null,font=17,zoom=1,paintEpoch=0;
 let pdfModule;const pdfText=new Map(),renderTasks=new Set();
 const wide=matchMedia('(min-width:800px)'),reduced=matchMedia('(prefers-reduced-motion:reduce)');
 const read=(key,fallback)=>{try{return JSON.parse(localStorage.getItem(key))??fallback;}catch{return fallback;}};
 const save=(key,value)=>{try{localStorage.setItem(key,JSON.stringify(value));}catch{}};
 font=Math.max(14,Math.min(24,read('bb-reader-font',17)));dialog.style.setProperty('--reader-size',font+'px');
 const count=()=>wide.matches?2:1;
 const key=()=>`bb-reader:${book.id}`;
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
 function pageElement(index){
  const article=el('article');article.className='reader-page';article.setAttribute('aria-label',`Page ${index+1}`);const data=pages[index];if(!data)return article;
  if(book.pdfDoc){article.classList.add('pdf-page');const canvas=el('canvas');canvas.setAttribute('role','img');article.append(canvas);const loading=el('span','Opening page…');loading.className='pdf-loading';article.append(loading);requestAnimationFrame(()=>paintPdf(canvas,index));return article;}
  article.append(el('small',book.title),el('h3',data.title));
  for(const paragraph of (data.body||'').split('\n\n'))if(paragraph)article.append(el('p',paragraph));
  if(data.source){const a=el('a','Publisher / book details ↗');a.href=data.source;a.target='_blank';a.rel='noopener';article.append(a);}
  if(data.notes){const notes=el('textarea');notes.className='reader-notes';notes.placeholder='A sentence to remember. A thought to return to…';notes.setAttribute('aria-label','Your private reading notes');notes.value=read(key()+':notes','');notes.maxLength=12000;notes.oninput=()=>{save(key()+':notes',notes.value);$('reader-message').textContent='Notes saved in this browser';};article.append(notes);}
  const number=el('p',String(index+1));number.className='page-number';article.append(number);return article;
 }
 function render(){
  if(!book)return;paintEpoch++;for(const task of renderTasks)task.cancel();renderTasks.clear();page=Math.max(0,Math.min(page,pages.length));$('reader-cover').hidden=page!==0;spread.hidden=page===0;
  if(page===0&&book.pdfDoc)requestAnimationFrame(()=>paintPdf($('reader-pdf-cover'),0,true));
  spread.replaceChildren();if(page>0){spread.append(pageElement(page-1));if(wide.matches)spread.append(pageElement(page));}
  $('reader-status').textContent=page===0?'Cover':`${page}${count()===2&&page<pages.length?'–'+(page+1):''} / ${pages.length}`;
  $('reader-prev').disabled=page===0;$('reader-next').disabled=page>0&&page+count()>pages.length;$('reader-next').textContent=page===0?'Open book ↗':'Next →';
  $('reader-contents').value=String(page);$('reader-bookmark').setAttribute('aria-pressed',String(read(key()+':mark',-1)===page));save(key()+':page',page);
 }
 async function turn(direction){
  if(turning||!book)return;const next=direction>0?(page===0?1:page+count()):Math.max(0,page-count());if(next>pages.length||next===page)return;
  turning=true;const ticket=epoch;paper();
  if(page>0&&!reduced.matches){
   const original=spread.children[direction>0&&wide.matches?1:0];
   if(original){const leaf=original.cloneNode(true);const sourceCanvas=original.querySelector('canvas'),copyCanvas=leaf.querySelector('canvas');if(sourceCanvas&&copyCanvas)copyCanvas.getContext('2d').drawImage(sourceCanvas,0,0);leaf.classList.add('reader-leaf');leaf.setAttribute('aria-hidden','true');leaf.inert=true;leaf.style.left=wide.matches&&direction>0?'50%':'0';leaf.style.transformOrigin=direction>0?'left center':'right center';spread.append(leaf);
    try{await leaf.animate([{transform:'rotateY(0deg)',filter:'brightness(1)'},{transform:`rotateY(${direction>0?-165:165}deg)`,filter:'brightness(.75)'}],{duration:420,easing:'cubic-bezier(.3,.1,.2,1)'}).finished;}catch{}leaf.remove();}
  }
  if(ticket===epoch){if(dialog.open){page=next;render();}turning=false;}
 }
 function open(value){
  epoch++;turning=false;if(book?.pdfDoc&&book.pdfDoc!==value.pdfDoc)book.pdfDoc.loadingTask.destroy().catch(()=>{});book=value;pdfText.clear();zoom=1;stage.style.touchAction="pan-y";
  pages=value.pages||[
   {title:value.title,body:`${value.author}\n\n${value.category}\n\n${value.note}\n\nThis is a reading companion. The published book’s full text is not included.`,source:value.source},
   {title:'In the margins',body:'Keep a thought here while you browse. These notes stay in this browser.',notes:true}
  ];
  page=read(key()+':page',value.pdfDoc?1:0);$('reader-cover-image').hidden=!!value.pdfDoc;$('reader-pdf-cover').hidden=!value.pdfDoc;$('reader-cover').classList.toggle('pdf-cover',!!value.pdfDoc);$('reader-smaller').textContent=value.pdfDoc?'−':'A−';$('reader-larger').textContent=value.pdfDoc?'+':'A+';$('reader-smaller').setAttribute('aria-label',value.pdfDoc?'Zoom out':'Smaller text');$('reader-larger').setAttribute('aria-label',value.pdfDoc?'Zoom in':'Larger text');$('reader-title').textContent=value.title;$('reader-cover-image').src=value.cover.startsWith('assets/')?value.cover:'assets/books/'+value.cover;$('reader-cover-image').alt=value.title+' cover';
  const crop={ava2:[.83,.9,.07,.035],beyond:[.68,.97,.15,.01],odyssey:[.74,.79,.115,.02]}[value.id]||[1,1,0,0];const art=$('reader-cover-image');art.style.width=100/crop[0]+'%';art.style.height=100/crop[1]+'%';art.style.left=-crop[2]/crop[0]*100+'%';art.style.top=-(1-crop[1]-crop[3])/crop[1]*100+'%';
  $('reader-contents').replaceChildren();const coverOption=el('option','Cover');coverOption.value='0';$('reader-contents').append(coverOption);
  pages.forEach((p,i)=>{const o=el('option',`${i+1} · ${p.title}`);o.value=String(i+1);$('reader-contents').append(o);});$('reader-message').textContent='Swipe or use ← → to turn pages';$('reader-search-input').value='';render();
 }
 $('reader-next').onclick=()=>turn(1);$('reader-prev').onclick=()=>turn(-1);$('reader-cover').onclick=()=>turn(1);
 $('reader-contents').onchange=()=>{if(!turning){page=Number($('reader-contents').value);render();}};
 $('reader-bookmark').onclick=()=>{const marked=read(key()+':mark',-1)===page;save(key()+':mark',marked?-1:page);$('reader-message').textContent=marked?'Bookmark removed':'Page bookmarked';render();};
 $('reader-resume').onclick=()=>{const mark=read(key()+':mark',-1);if(mark>=0){page=mark;render();}else $('reader-message').textContent='Bookmark a page first';};
 function textSize(change){if(book?.pdfDoc){zoom=Math.max(.75,Math.min(3,zoom+change*.25));stage.style.touchAction=zoom>1?'pan-x pan-y':'pan-y';render();$('reader-message').textContent='Zoom '+Math.round(zoom*100)+'%';return;}font=Math.max(14,Math.min(24,font+change));dialog.style.setProperty('--reader-size',font+'px');save('bb-reader-font',font);}
 $('reader-smaller').onclick=()=>textSize(-1);$('reader-larger').onclick=()=>textSize(1);
 $('reader-fullscreen').onclick=async()=>{try{if(document.fullscreenElement)await document.exitFullscreen();else await dialog.requestFullscreen();}catch{$('reader-message').textContent='Fullscreen is unavailable in this browser';}};
 $('reader-search').onsubmit=async e=>{e.preventDefault();const q=$('reader-search-input').value.trim().toLocaleLowerCase();if(!q)return;const ticket=epoch;let found=-1;
  for(let offset=0;offset<pages.length;offset++){if(ticket!==epoch)return;const i=(page+offset)%pages.length;let body=pages[i].body||'';
   if(book.pdfDoc){$('reader-message').textContent=`Searching page ${i+1}…`;try{if(!pdfText.has(i)){const p=await book.pdfDoc.getPage(i+1);const t=await p.getTextContent();if(ticket!==epoch)return;pdfText.set(i,t.items.map(x=>x.str||'').join(' '));}body=pdfText.get(i);}catch{continue;}}
   if((pages[i].title+' '+body).toLocaleLowerCase().includes(q)){found=i;break;}}
  if(ticket!==epoch)return;if(found>=0){page=found+1;render();$('reader-message').textContent='Found on page '+page;}else $('reader-message').textContent=book.pdfDoc?'No matching text. Scanned pages may not contain searchable text.':'No matching page';};
 dialog.addEventListener('keydown',e=>{if(e.target.matches('input,textarea,select'))return;if(e.key==='ArrowRight'||e.key==='ArrowLeft'){e.preventDefault();turn(e.key==='ArrowRight'?1:-1);}if(e.key==='+'||e.key==='=')textSize(1);if(e.key==='-')textSize(-1);});
 stage.onpointerdown=e=>{if(book?.pdfDoc&&zoom>1)return;if(e.target.closest('input,textarea,select,a'))return;start={x:e.clientX,y:e.clientY,id:e.pointerId};};
 stage.onpointerup=e=>{if(!start||start.id!==e.pointerId)return;const dx=e.clientX-start.x,dy=e.clientY-start.y;start=null;if(Math.abs(dx)>60&&Math.abs(dy)<80){e.preventDefault();turn(dx<0?1:-1);}};stage.onpointercancel=()=>{start=null;};
 let resizeTimer;addEventListener('resize',()=>{clearTimeout(resizeTimer);resizeTimer=setTimeout(()=>{if(dialog.open)render();},150);});dialog.addEventListener('close',()=>{epoch++;paintEpoch++;for(const task of renderTasks)task.cancel();renderTasks.clear();turning=false;start=null;if(book?.pdfDoc){book.pdfDoc.loadingTask.destroy().catch(()=>{});book=null;pdfText.clear();}if(document.fullscreenElement===dialog)document.exitFullscreen().catch(()=>{});});
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
 return {open};
}

export const studioJournal={id:'studio-journal',title:'A Little Room for Good Ideas',author:'Backbenchers Studio',cover:'assets/studio-journal-cover.svg',pages:[
 {title:'Make yourself at home.',body:'There is a certain kind of room that invites you to stay. A desk worn smooth at the edge. A book left open beside a keyboard. An idea that began as a scribble and is still finding its shape.\n\nThis is our small version of that room. Take your time. There is no correct route through it.'},
 {title:'A working shelf',body:'The files and boxes have work to do. They belong here as much as the books. A studio should leave room for the practical things: old folders, spare cables, a cup, the project that is not quite finished.\n\nThe reading shelf brings Myanmar fiction, remembered lives, poetry, and history into that everyday setting.'},
 {title:'Three ways of making',body:'Software begins with a question. A story begins with noticing. A flight experiment begins with looking up.\n\nThe three workspaces hold those different habits together. Open a screen, write a small program, or leave a note. The local workspace is yours to explore.'},
 {title:'Roots, rivers, flight',body:'Jasmine gives the gallery a living shape. The river suggests a longer view. A bird study speaks to the workbench’s curiosity about flight.\n\nWarm paper, oak, forest green, and small brass details connect the pieces without making them identical.'},
 {title:'The quiet between things',body:'A gentle rhythm can make a room feel occupied. The studio’s lo-fi sketch uses warm chords, a soft bass, and brushed percussion. You can mute it at any time or adjust its volume in the exploration controls.\n\nSilence belongs here too.'},
 {title:'Leave a line',body:'A thought, a question, a title to look for next time.\n\nThis short journal is original studio writing. The published books on the shelf have their own authors and stories; their companion pages link to the book sources.',notes:true}
]};
