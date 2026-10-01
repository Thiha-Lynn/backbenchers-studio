// Real Unity + two-finger browser touch input. Run with PLAYWRIGHT_MODULE and optional STUDIO_URL.
const assert=require('node:assert/strict');
const {chromium}=require(process.env.PLAYWRIGHT_MODULE||'playwright');
(async()=>{
 const browser=await chromium.launch({headless:false,executablePath:process.env.CHROMIUM_EXECUTABLE||'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'});
 try{
 const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:1,isMobile:true,hasTouch:true});
 const p=await context.newPage(),errors=[];p.on('pageerror',e=>errors.push(e.message));p.on('console',m=>{if(m.type()==='error')errors.push(m.text());});
 await p.addInitScript(()=>window.__sent=[]);
 await p.route('**/studio.js*',async route=>{const response=await route.fetch();const body=(await response.text()).replace("function send(method,value=''){","function send(method,value=''){window.__sent.push([method,value]);");await route.fulfill({response,body});});
 await p.goto(process.env.STUDIO_URL||'http://localhost:8765');await p.locator('#enter').click();await p.locator('#tour').waitFor({state:'visible',timeout:180000});await p.waitForTimeout(1800);
 const messages=()=>p.evaluate(()=>window.__sent);
 const cdp=await context.newCDPSession(p);const touch=(type,touchPoints)=>cdp.send('Input.dispatchTouchEvent',{type,touchPoints});
 const a={x:80,y:575,id:1},b={x:295,y:350,id:2};
 await touch('touchStart',[a]);await touch('touchMove',[{...a,y:525}]);await p.waitForTimeout(200);assert((await messages()).some(([m,v])=>m==='SetMove'&&Number(v.split(',')[1])>.6));
 await touch('touchStart',[{...a,y:525},b]);await touch('touchMove',[{...a,y:525},{...b,x:250,y:330}]);await p.waitForTimeout(160);assert((await messages()).some(([m])=>m==='Look'));await p.screenshot({path:'/tmp/tour-walking.png'});
 await touch('touchEnd',[{...b,x:250,y:330}]);assert.equal(await p.locator('body').evaluate(e=>e.classList.contains('walking-touch')),true,'lifting look finger must not cancel walking');
 await touch('touchMove',[{...a,y:500}]);await touch('touchEnd',[]);assert.equal((await messages()).filter(([m])=>m==='SetMove').at(-1)[1],'0,0');
 assert.equal(await p.locator('body').evaluate(e=>e.classList.contains('walking-touch')),false);
 await p.locator('#tour-menu').click();assert.equal(await p.locator('#tour-menu').getAttribute('aria-expanded'),'true');await p.locator('#room-lights').click();await p.waitForTimeout(700);assert.equal(await p.locator('#room-lights').getAttribute('aria-pressed'),'false');await p.locator('#room-lights').click();await p.locator('[data-panel="help"]').click();assert.equal(await p.locator('#room-menu').isVisible(),false);await p.locator('#walking-pace').selectOption('1');await p.locator('#help [data-close]').click();
 await touch('touchStart',[a]);await touch('touchMove',[{...a,y:520}]);await p.waitForTimeout(150);assert.equal((await messages()).filter(([m])=>m==='SetMove').at(-1)[1],'0.000,1.000');await touch('touchCancel',[]);assert.equal((await messages()).filter(([m])=>m==='SetMove').at(-1)[1],'0,0');
 // Opening any overlay during a gesture must release the thumbstick and clear movement.
 await touch('touchStart',[a]);await touch('touchMove',[{...a,y:520}]);await p.locator('#tour-menu').evaluate(e=>e.click());assert.equal((await messages()).filter(([m])=>m==='SetMove').at(-1)[1],'0,0');await touch('touchEnd',[]);await p.locator('#library-button').click();await p.locator('#library').waitFor({state:'visible'});await p.locator('#library [data-close]').click();
 for(const [width,height] of [[390,844],[320,568],[573,1133],[844,390],[1024,768],[1440,900]]){
  await p.setViewportSize({width,height});await p.waitForTimeout(350);await p.locator('[data-view="1"]').click();await p.waitForTimeout(800);
  const dock=await p.locator('.tour-bar').boundingBox();assert(dock.height<=68,`dock should be compact: ${width} ${dock.height}`);assert(dock.x>=0&&dock.x+dock.width<=width+1);assert(dock.y+dock.height<=height);
  assert.equal(await p.evaluate(()=>document.documentElement.scrollWidth),width);
  await p.locator('#tour-menu').click();const menu=await p.locator('#room-menu').boundingBox();assert(menu.y>=0&&menu.x>=0&&menu.x+menu.width<=width+1&&menu.y+menu.height<=height);await p.screenshot({path:`/tmp/tour-menu-${width}.png`});await p.locator('#menu-close').click();await p.screenshot({path:`/tmp/tour-${width}.png`});
 }
 await p.locator('#tour-menu').click();await p.locator('#quiet-view').click();assert.equal(await p.locator('header').isVisible(),false);await p.locator('#tour-menu').click();await p.locator('#quiet-view').click();assert.equal(await p.locator('header').isVisible(),true);
 await p.setViewportSize({width:390,height:844});await p.locator('#tour-menu').click();await p.locator('#devices-button').click();await p.locator('#device-select').selectOption('benchphone');await p.waitForTimeout(300);await p.locator('#device-use').click();await p.locator('#phone-unlock').click();await p.locator('#phone-viewer [data-close]').click();
 console.log('Mobile passed',await p.evaluate(()=>({renderer:document.documentElement.dataset.renderer,fps:document.getElementById('fps').textContent})));await context.close();const desktop=await browser.newPage({viewport:{width:1440,height:900}});desktop.on('pageerror',e=>errors.push(e.message));desktop.on('console',m=>{if(m.type()==='error')errors.push(m.text());});p.on('console',m=>{if(m.type()==='error')errors.push(m.text());});await desktop.goto(process.env.STUDIO_URL||'http://localhost:8765');await desktop.bringToFront();await desktop.locator('#enter').click();await desktop.locator('#tour').waitFor({state:'visible',timeout:180000});await desktop.waitForTimeout(1000);assert.equal(await desktop.locator('#move-stick').isVisible(),false);await desktop.locator('#unity-canvas').focus();await desktop.keyboard.down('w');await desktop.waitForTimeout(500);await desktop.keyboard.up('w');await desktop.locator('#tour-menu').click();await desktop.locator('[data-panel="research"]').click();await desktop.locator('[data-inspect="hardware:mini"]').click();await desktop.locator('#object-inspector').waitFor({state:'visible'});await desktop.locator('#inspect-close').click();await desktop.locator('[data-view="0"]').click();await desktop.waitForTimeout(1000);await desktop.screenshot({path:'/tmp/tour-desktop.png'});
 assert.deepEqual(errors,[]);console.log('PASS: simultaneous touch move/look, independent release, cancellation, modal stop, pace, lighting, quiet mode, phone, desktop inspection, 6 viewport fits; no runtime errors.');console.log(await desktop.evaluate(()=>({renderer:document.documentElement.dataset.renderer,fps:document.getElementById('fps').textContent})));
 }finally{await browser.close();}
})();
