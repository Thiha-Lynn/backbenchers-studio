import {loadPyodide} from 'https://cdn.jsdelivr.net/pyodide/v314.0.7/full/pyodide.mjs';
let py,chain=Promise.resolve();
async function start(snapshot){
 py=await loadPyodide({indexURL:'https://cdn.jsdelivr.net/pyodide/v314.0.7/full/'});
 py.FS.mkdirTree('/home/studio');
 for(const [path,value] of Object.entries(snapshot||{})){
  if(!path.startsWith('/home/studio/'))continue;
  if(value===null)py.FS.mkdirTree(path);else{py.FS.mkdirTree(path.slice(0,path.lastIndexOf('/')));py.FS.writeFile(path,value);}
 }
 if(!py.FS.analyzePath('/home/studio/welcome.txt').exists){
  py.FS.writeFile('/home/studio/welcome.txt','Welcome to Backbenchers Studio.\nCode. Play. Flight.\n\nYour files stay in this browser. Try: python hello.py\n');
  py.FS.writeFile('/home/studio/hello.py','crew = ["Thomas", "Hlaing", "Merlin"]\nfor name in crew:\n    print(f"Welcome aboard, {name}!")\nprint("Make something good.")\n');
  py.FS.mkdirTree('/home/studio/projects');
 }
 const response=await fetch(new URL('./shell.py',import.meta.url));if(!response.ok)throw Error('Could not load Studio shell');
 await py.runPythonAsync(await response.text());
}
function snapshot(){
 let files={},bytes=0;
 function walk(dir,depth=0){if(depth>15)return;for(const n of py.FS.readdir(dir)){if(n==='.'||n==='..')continue;const p=dir+'/'+n;const stat=py.FS.lstat(p);if(py.FS.isLink(stat.mode))continue;if(py.FS.isDir(stat.mode)){files[p]=null;walk(p,depth+1);}else if(stat.size<500000&&bytes+stat.size<2000000){try{files[p]=py.FS.readFile(p,{encoding:'utf8'});bytes+=stat.size;}catch{}}}}
 walk('/home/studio');return files;
}
self.onmessage=e=>{chain=chain.then(async()=>{const {id,action}=e.data;try{if(action==='init'){await start(e.data.snapshot);postMessage({id,ready:true,snapshot:snapshot()});return;}py.globals.set('_request_json',JSON.stringify(e.data));const result=JSON.parse(await py.runPythonAsync('handle(json.loads(_request_json))'));postMessage({id,...result,snapshot:snapshot()});}catch(error){postMessage({id,error:String(error)});}});};
