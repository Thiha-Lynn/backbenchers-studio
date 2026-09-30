// Original, browser-synthesized 72 BPM sketch. No recordings or external streams.
export function createLofi(button,volume){
 let context,master,noiseBuffer,timer,step=0,next=0,touring=false,preferred=true,level=.2;
 try{preferred=localStorage.getItem('bb-lofi')!=='off';level=Number(localStorage.getItem('bb-lofi-volume')||.2);}catch{}
 level=Number.isFinite(level)?Math.max(0,Math.min(.6,level)):.2;volume.value=String(level);
 const chords=[[50,57,60,64,69],[46,53,57,60,65],[41,53,57,60,64],[48,55,58,62,67]];
 const note=n=>440*Math.pow(2,(n-69)/12);
 function voice(n,t,duration,amount,type='sine'){
  const o=context.createOscillator(),g=context.createGain();o.type=type;o.frequency.value=note(n);g.gain.setValueAtTime(0,t);g.gain.linearRampToValueAtTime(amount,t+.025);g.gain.exponentialRampToValueAtTime(.0001,t+duration);o.connect(g);g.connect(master);o.start(t);o.stop(t+duration+.03);o.onended=()=>{o.disconnect();g.disconnect();};
 }
 function noise(t,duration,amount,frequency){
  const s=context.createBufferSource(),g=context.createGain(),f=context.createBiquadFilter();s.buffer=noiseBuffer;f.type='lowpass';f.frequency.value=frequency;g.gain.setValueAtTime(amount,t);g.gain.exponentialRampToValueAtTime(.0001,t+duration);s.connect(f);f.connect(g);g.connect(master);s.start(t);s.stop(t+duration);s.onended=()=>{s.disconnect();f.disconnect();g.disconnect();};
 }
 function tick(){
  while(next<context.currentTime+.3){
   const position=step%8,bar=Math.floor(step/8),chord=chords[bar%4],t=next+(position%2?.037:0);
   if(position===0){chord.slice(1).forEach((n,i)=>{voice(n,t+i*.015,2.7,.042);voice(n+12,t+i*.015,1.2,.006,'triangle');});voice(chord[0]-12,t,1.6,.13);}
   if(position===4)voice(chord[0]-12,t,1.15,.09);
   if(position===0||position===4){const o=context.createOscillator(),g=context.createGain();o.frequency.setValueAtTime(105,t);o.frequency.exponentialRampToValueAtTime(38,t+.16);g.gain.setValueAtTime(.2,t);g.gain.exponentialRampToValueAtTime(.0001,t+.23);o.connect(g);g.connect(master);o.start(t);o.stop(t+.25);o.onended=()=>{o.disconnect();g.disconnect();};}
   if(position===2||position===6)noise(t,.13,.035,1800);
   noise(t,.045,position%2?.012:.007,4200);
   if(position===3||position===7)voice(chord[(bar+position)%4+1]+12,t,.8,.017);
   step++;next+=60/72/2;
  }
 }
 async function sync(){
  const audible=preferred&&touring&&!document.hidden;
  button.setAttribute('aria-pressed',String(audible));button.textContent=audible?'♫ On':'♫ Lo-fi';button.setAttribute('aria-label',audible?'Mute lo-fi music':'Play lo-fi music');
  if(!audible){clearInterval(timer);timer=null;if(context){master.gain.setTargetAtTime(0,context.currentTime,.09);await context.suspend();}return;}
  try{
   if(!context){context=new (window.AudioContext||window.webkitAudioContext)();master=context.createGain();master.gain.value=level;const filter=context.createBiquadFilter();filter.type='lowpass';filter.frequency.value=4200;master.connect(filter);filter.connect(context.destination);noiseBuffer=context.createBuffer(1,context.sampleRate,context.sampleRate);const data=noiseBuffer.getChannelData(0);let seed=31415;for(let i=0;i<data.length;i++){seed=(seed*16807)%2147483647;data[i]=seed/1073741824-1;}}
   await context.resume();if(!preferred||!touring||document.hidden){await context.suspend();return;}master.gain.setTargetAtTime(level,context.currentTime,.2);
   if(!timer){next=context.currentTime+.06;tick();timer=setInterval(tick,120);}
  }catch{button.textContent='♫ Tap';button.setAttribute('aria-pressed','false');}
 }
 button.onclick=()=>{preferred=!preferred;try{localStorage.setItem('bb-lofi',preferred?'on':'off');}catch{}sync();};
 volume.oninput=()=>{level=Number(volume.value);if(master)master.gain.setTargetAtTime(level,context.currentTime,.08);try{localStorage.setItem('bb-lofi-volume',String(level));}catch{}};
 document.addEventListener('visibilitychange',sync);
 return {tour(value){touring=value;sync();},paper(){if(context?.state==='running'&&preferred)noise(context.currentTime,.14,.025,1200);}};
}
