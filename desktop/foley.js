// Original, quiet procedural room sounds. No recordings or network requests.
export function createFoley(button){
 let context,master,buffer,enabled=true,active=false,lastStep=0;
 try{enabled=localStorage.getItem('bb-foley')!=='off';}catch{}
 const label=()=>{button.textContent=enabled?'SFX On':'SFX Off';button.setAttribute('aria-pressed',String(enabled));};label();
 function unlock(){if(!active||!enabled||document.hidden)return;try{if(!context){context=new (window.AudioContext||window.webkitAudioContext)();master=context.createGain();master.gain.value=.27;master.connect(context.destination);buffer=context.createBuffer(1,context.sampleRate,context.sampleRate);const a=buffer.getChannelData(0);for(let i=0;i<a.length;i++)a[i]=Math.random()*2-1;}if(context.state!=='running')context.resume().catch(()=>{});}catch{}}
 function burst(duration,gain,frequency,delay=0){const t=context.currentTime+delay,s=context.createBufferSource(),filter=context.createBiquadFilter(),g=context.createGain();s.buffer=buffer;filter.type='lowpass';filter.frequency.value=frequency;g.gain.setValueAtTime(.0001,t);g.gain.linearRampToValueAtTime(gain,t+.014);g.gain.exponentialRampToValueAtTime(.0001,t+duration);s.connect(filter);filter.connect(g);g.connect(master);s.start(t,Math.random()*.4);s.stop(t+duration);s.onended=()=>{s.disconnect();filter.disconnect();g.disconnect();};}
 function tone(frequency,duration,gain,delay=0){const t=context.currentTime+delay,o=context.createOscillator(),g=context.createGain();o.frequency.setValueAtTime(frequency,t);o.frequency.exponentialRampToValueAtTime(frequency*.65,t+duration);g.gain.setValueAtTime(gain,t);g.gain.exponentialRampToValueAtTime(.0001,t+duration);o.connect(g);g.connect(master);o.start(t);o.stop(t+duration);o.onended=()=>{o.disconnect();g.disconnect();};}
 function play(kind){if(!enabled||!active||document.hidden)return;unlock();if(context?.state!=='running')return;
  if(kind==='step'){if(performance.now()-lastStep<270)return;lastStep=performance.now();burst(.15,.23,650);tone(94+Math.random()*15,.11,.09);}
  else if(kind==='paper'){burst(.34,.09,2400);burst(.22,.035,3800,.15);}
  else if(kind==='door'){tone(170,.48,.045);burst(.35,.07,750);burst(.09,.17,1000,.62);}
  else if(kind==='pickup'||kind==='return'){burst(.23,.1,1000);tone(150,.14,.04);}
  else if(kind==='switch'){burst(.045,.3,3200);tone(320,.035,.035);}
  else burst(.055,.065,1800);
 }
 button.onclick=()=>{enabled=!enabled;try{localStorage.setItem('bb-foley',enabled?'on':'off');}catch{}label();if(enabled){unlock();play('switch');}else context?.suspend();};
 document.addEventListener('visibilitychange',()=>{if(document.hidden)context?.suspend();else unlock();});
 return {play,tour(value){active=value;if(value)unlock();else context?.suspend();}};
}
