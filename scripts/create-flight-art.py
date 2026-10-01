from PIL import Image,ImageDraw,ImageFont
from pathlib import Path
out=Path(__file__).resolve().parents[1]/'assets'
font='/System/Library/Fonts/Supplemental/Courier New.ttf'
for kind in ['wing','quad','survey']:
 im=Image.new('RGB',(900,1200),'#e9e0c9');d=ImageDraw.Draw(im);ink='#28473e';accent='#b0754d'
 def line(points,w=3,fill=ink):d.line(points,fill=fill,width=w)
 def label(x,y,t,size=20,fill=ink):d.text((x,y),t,fill=fill,font=ImageFont.truetype(font,size))
 for x in range(50,860,40):line([(x,180),(x,1040)],1,'#d7cfb7')
 for y in range(180,1050,40):line([(50,y),(850,y)],1,'#d7cfb7')
 label(55,45,'BACKBENCHERS  /  FLIGHT LAB',22);line([(55,100),(845,100)],3)
 label(55,123,{'wing':'01   FIXED-WING STUDY','quad':'02   MULTIROTOR STUDY','survey':'03   AERIAL SURVEY'}[kind],32)
 if kind=='wing':
  # Original RC sailplane plan, front elevation, and section.
  pts=[(450,235),(472,280),(470,415),(788,525),(790,563),(475,516),(473,748),(560,793),(560,815),(455,800),(340,815),(340,793),(427,748),(425,516),(110,563),(112,525),(430,415),(428,280),(450,235)]
  line(pts,5);line([(450,235),(450,830)],2,accent)
  for x in range(145,790,35):
   if x<420:line([(x,555-(x-110)*.15),(x,525-(x-110)*.346)],2)
   if x>480:line([(x,518+(x-480)*.145),(x,418+(x-480)*.346)],2)
  d.ellipse((433,298,466,368),outline=ink,width=3);line([(120,891),(390,908),(450,887),(510,908),(780,891)],5);d.ellipse((425,881,475,934),outline=ink,width=3)
  line([(110,206),(790,206)],2,accent);line([(110,194),(110,222)],2,accent);line([(790,194),(790,222)],2,accent);label(340,177,'WINGSPAN / PLAN',18,accent)
  label(75,848,'FRONT ELEVATION',18);label(560,739,'TAILPLANE',18,accent)
 elif kind=='quad':
  for x,y in [(235,360),(665,360),(235,790),(665,790)]:
   line([(450,575),(x,y)],24);line([(450,575),(x,y)],12,'#e9e0c9');d.ellipse((x-143,y-143,x+143,y+143),outline=accent,width=2);d.ellipse((x-29,y-29,x+29,y+29),outline=ink,width=5)
   line([(x-120,y-50),(x+120,y+50)],15);d.ellipse((x-8,y-8,x+8,y+8),fill=accent)
  d.rounded_rectangle((377,468,523,681),30,fill='#e9e0c9',outline=ink,width=5);d.rectangle((405,505,495,625),outline=ink,width=3);d.ellipse((429,478,471,515),outline=ink,width=3)
  line([(450,680),(450,965)],2,accent);label(290,977,'X-FRAME / TOP VIEW',20);label(370,542,'FC',30);label(570,593,'AVIONICS',18,accent)
 else:
  for i in range(7):
   y=290+i*91;pts=[(90,y),(250,y-30),(400,y+25),(530,y-35),(720,y+10),(810,y-10)];line(pts,3)
  for i in range(5):
   y=340+i*120;line([(145,y),(745,y),(745,y+60),(145,y+60)],4,accent)
  for x,y in [(145,340),(745,880)]:d.ellipse((x-12,y-12,x+12,y+12),fill=ink)
  label(110,970,'OVERLAP  /  WAYPOINTS  /  RETURN',20);label(165,300,'TAKEOFF',18,accent)
 line([(55,1060),(845,1060)],3);label(55,1083,'FIELD NOTES  /  CODE . PLAY . FLIGHT',23);label(55,1130,'Original illustrative study — not a construction plan',17)
 im.save(out/f'flight-{kind}.png')
