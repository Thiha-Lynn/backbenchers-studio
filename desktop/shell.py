"""Browser-local command interpreter backed by the real Pyodide filesystem."""
import os, sys, json, shlex, pathlib, shutil, io, contextlib, traceback, datetime, re
HOME='/home/studio'
os.makedirs(HOME, exist_ok=True)
os.chdir(HOME)
NS={'__name__':'__main__'}
HELP='''Studio shell · Python / WebAssembly
Files: pwd, ls [-la] [path], cd, cat, touch, mkdir [-p], cp, mv, rm [-r], head, tail
Text: echo, grep, wc, sort, date, whoami, uname, clear
Code: python file.py  |  python -c 'print(2 + 2)'
Use | for text pipelines, > to write and >> to append. Quotes are supported.
Files are saved in this browser. This is a browser-local shell, not an Ubuntu VM.
No apt, native binaries, host shell, or server access. Use Stop for long-running code.'''
def path(p):
    p=os.path.expanduser(p.replace('~',HOME,1) if p.startswith('~') else p)
    return os.path.abspath(p)
def read(p): return pathlib.Path(path(p)).read_text()
def command(argv, stdin=''):
    if not argv: return ''
    cmd,*args=argv
    positional=[a for a in args if not a.startswith('-')]
    if cmd=='help': return HELP+'\n'
    if cmd=='pwd': return os.getcwd()+'\n'
    if cmd=='whoami': return 'studio\n'
    if cmd=='uname': return 'Backbenchers browser workspace · Python '+sys.version.split()[0]+' / WebAssembly\n'
    if cmd=='date': return datetime.datetime.now().isoformat(sep=' ',timespec='seconds')+'\n'
    if cmd=='echo': return ' '.join(args)+'\n'
    if cmd=='cd': os.chdir(path(args[0] if args else HOME));return ''
    if cmd=='ls':
        base=path(positional[0] if positional else '.');p=pathlib.Path(base)
        if p.is_file(): return p.name+'\n'
        entries=sorted(p.iterdir(),key=lambda x:(not x.is_dir(),x.name.lower()))
        return '\n'.join(x.name+('/' if x.is_dir() else '') for x in entries if not x.name.startswith('.') or any('a' in a for a in args if a.startswith('-'))) +'\n'
    if cmd=='cat': return ''.join(read(p) for p in args) if args else stdin
    if cmd=='touch':
        for a in args:pathlib.Path(path(a)).touch()
        return ''
    if cmd=='mkdir':
        for a in positional:pathlib.Path(path(a)).mkdir(parents='-p' in args,exist_ok='-p' in args)
        return ''
    if cmd in ('cp','mv'):
        if len(args)!=2:raise ValueError(cmd+' requires source and destination')
        (shutil.copy2 if cmd=='cp' else shutil.move)(path(args[0]),path(args[1]));return ''
    if cmd=='rm':
        for a in positional:
            p=path(a)
            if not p.startswith(HOME+'/'):raise ValueError('Only workspace files can be removed')
            if os.path.isdir(p):
                if not any('r' in a for a in args if a.startswith('-')):raise ValueError('Use rm -r for a directory')
                shutil.rmtree(p)
            else:os.remove(p)
        return ''
    if cmd in ('head','tail'):
        n=10
        if '-n' in args:i=args.index('-n');n=int(args[i+1]);args=args[:i]+args[i+2:]
        text=read(args[0]) if args else stdin;lines=text.splitlines();return '\n'.join(lines[:n] if cmd=='head' else lines[-n:])+'\n'
    if cmd=='grep':
        if not args:raise ValueError('grep requires a pattern')
        text=''.join(read(p) for p in args[1:]) if len(args)>1 else stdin
        return '\n'.join(l for l in text.splitlines() if re.search(args[0],l))+'\n'
    if cmd=='sort':return '\n'.join(sorted((read(args[0]) if args else stdin).splitlines()))+'\n'
    if cmd=='wc':
        text=''.join(read(p) for p in positional) if positional else stdin
        return str(len(text.splitlines()))+'\n' if '-l' in args else f'{len(text.splitlines())} {len(text.split())} {len(text.encode())}\n'
    if cmd in ('python','python3'):
        if not args:return 'Use python filename.py or python -c "code"\n'
        code=args[1] if args[0]=='-c' else read(args[0]);sys.argv=args
        out=io.StringIO()
        with contextlib.redirect_stdout(out),contextlib.redirect_stderr(out):
            try:exec(compile(code,args[0],'exec'),NS)
            except BaseException:traceback.print_exc()
        return out.getvalue()
    raise ValueError(cmd+': command not available. Type help for supported commands.')
def shell(line):
    lexer=shlex.shlex(line,posix=True,punctuation_chars='|>');lexer.whitespace_split=True;lexer.commenters='';tokens=list(lexer)
    chunks=[[]]
    for token in tokens:
        if token=='|':chunks.append([])
        else:chunks[-1].append(token)
    data=''
    for tokens in chunks:
        dest=None;mode='w'
        for op in ('>>','>'):
            if op in tokens:
                i=tokens.index(op)
                if i+2!=len(tokens):raise ValueError('Use one output filename after '+op)
                dest=path(tokens[i+1]);mode='a' if op=='>>' else 'w';tokens=tokens[:i];break
        data=command(tokens,data)
        if dest:
            pathlib.Path(dest).open(mode).write(data);data=''
    return data

def handle(payload):
    a=payload['action']
    if a=='shell':result={'output':shell(payload['line'])}
    elif a=='list':
        p=path(payload.get('path') or os.getcwd());result={'path':p,'entries':[{'name':x.name,'path':str(x),'directory':x.is_dir()} for x in sorted(pathlib.Path(p).iterdir(),key=lambda x:(not x.is_dir(),x.name.lower()))]}
    elif a=='read':result={'path':path(payload['path']),'text':read(payload['path'])}
    elif a=='write':
        p=path(payload['path'])
        if not p.startswith(HOME+'/'):raise ValueError('Save inside /home/studio')
        pathlib.Path(p).parent.mkdir(parents=True,exist_ok=True);pathlib.Path(p).write_text(payload['text']);result={'path':p}
    elif a=='run':result={'output':command(['python','-c',payload['code']])}
    else:raise ValueError('Unknown action')
    result['cwd']=os.getcwd();return json.dumps(result)
