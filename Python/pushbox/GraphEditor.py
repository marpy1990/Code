#coding=gbk
"""
GraphEditor:
	一个组图的编辑器，编辑图并以写入文件

GraphEditor的实现内容：
		组图载入
		组图编辑
		组图保存
""" 
from Assembly import*
from graphics import*
from cPickle import*

"""网格类，实现网格点之间的转换"""		
class NetAss(Assembly):
	def __init__(self,m=1,n=1,width=0,height=0,pos=Point(0,0)):
		Assembly.__init__(self,width,height,pos)
		self._m=m
		self._n=n
	
	def getM():
		return self._m
	
	def getN():
		return self._n
		
	def pointToNet(self,p):
		"""获得点p所在的网格坐标{m,n}"""
		if self._m==0:
			m=p.getX()-self._pos.getX()
		else:
			netwidth=self._width*1.0/self._m
			m=int((p.getX()-self._pos.getX())/netwidth)
		
		if self._n==0:
			n=p.getY()-self._pos.getY()
		else:
			netheight=self._height*1.0/self._n
			n=int((p.getY()-self._pos.getY())/netheight)
		return {"i":m,"j":n}
		
	def nearNetPoint(self,p):
		"""取得距离点p最近的格点坐标"""
		if self._m==0:
			x=p.getX()
		else:
			netwidth=self._width*1.0/self._m
			m=int((p.getX()-self._pos.getX())/netwidth+0.5)
			x=m*netwidth+self._pos.getX()
		
		if self._n==0:
			y=p.getY()
		else:
			netheight=self._height*1.0/self._n
			n=int((p.getY()-self._pos.getY())/netheight+0.5)
			y=n*netheight+self._pos.getY()
		return Point(x,y)
	
	def netToPoint(self,m,n):
		"""取得格点[m,n]的物理坐标"""
		if self._m==0:
			x=self._pos.getX()+m
			print x
		else:
			netwidth=self._width*1.0/self._m
			x=self._pos.getX()+m*netwidth
			
		if self._n==0:
			y=self._pos.getY()+n
		else:
			netheight=self._height*1.0/self._n
			y=self._pos.getY()+n*netheight
		return Point(x,y)
		
class GraphEditor:
	def __init__(self):
		self._buff={"type":"line","fill":"","outline":"black"}
		self._w=40
		self._state="start"
		self._record=Assembly()
		self._filename=self.getFilename()
		self._exit=Assembly(1*self._w,1*self._w,Point(14*self._w,7*self._w))
		self._save=Assembly(1*self._w,1*self._w,Point(15*self._w,7*self._w))
		self._remove=Assembly(1*self._w,1*self._w,Point(13*self._w,7*self._w))
		self._circle=Assembly(1*self._w,1*self._w,Point(14*self._w,1*self._w))
		self._line=Assembly(1*self._w,1*self._w,Point(15*self._w,1*self._w))
		self._rect=Assembly(1*self._w,1*self._w,Point(16*self._w,1*self._w))
		self._fill=NetAss(8,1,8*self._w,1*self._w,Point(9*self._w,2*self._w))
		self._outline=NetAss(8,1,8*self._w,1*self._w,Point(9*self._w,3*self._w))
		
		#状态机
		self._stateGraph={	"start":{"boardclick":"ready","endclick":"end","default":"idle"},
							"idle":{"boardclick":"ready","endclick":"end","default":"idle"}	,
							"ready":{"boardclick":"idle","endclick":"end","default":"ready"},
							"end":{"default":"end"}}
		
		#确定画布比例
		m,n=input("m , n?")
		if m>=n:
			width=7*self._w*1.0
			if n==0:
				height=width
			else:
				height=width*n/m
			pos=Point(self._w,self._w+(7*self._w-height)/2.0)
		else:
			height=7*self._w*1.0
			if m==0:
				width=height
			else:
				width=height*m/n
			pos=Point(self._w+(7*self._w-width)/2.0,self._w)
		
		#画网格
		self._win=GraphWin(self._filename[4:].split(".")[0],17*self._w,9*self._w)
		self._win.setBackground('white')
		self._board=NetAss(m,n,width,height,pos)
		frame=Rectangle(pos,Point(pos.getX()+width,pos.getY()+height))
		frame.setOutline("grey")
		self._board.appendGraphs(frame)
		for i in range(1,m):
			for j in range(1,n):
				p=self._board.netToPoint(i,j)
				p.setOutline("grey")
				self._board.appendGraphs(p)
		self._board.draw(self._win)
		self._record.setState(self._board.pos(),self._board.width(),self._board.height())
		
		#置控件按钮
		self._remove.load("bin/remove.dat")
		self._save.load("bin/save.dat")
		self._exit.load("bin/exit.dat")
		self._rect.load("bin/rectangle.dat")
		self._circle.load("bin/circle.dat")
		self._line.load("bin/line.dat")
		self._fill.load("bin/fill.dat")
		self._outline.load("bin/outline.dat")
		self._remove.draw(self._win)
		self._save.draw(self._win)
		self._exit.draw(self._win)
		self._rect.draw(self._win)
		self._circle.draw(self._win)
		self._line.draw(self._win)
		self._fill.draw(self._win)
		self._outline.draw(self._win)

	def getFilename(self):
		name=raw_input("name?")
		name=name.split('.')[0]
		if name[0:4]=="bin/":
			self._filename=name+".dat"
		else:
			self._filename="bin/"+name+".dat"
		return self._filename

	def	load(self):
		if self._record.load(self._filename):
			self._record.draw(self._win)

	def changeState(self,event):
		if event in self._stateGraph[self._state]:
			self._state=self._stateGraph[self._state][event]
		else:
			self._state=self._stateGraph[self._state]["default"]

	def deal(self,mouse):
		"""根据状态机响应鼠标点击"""
		if self._board.inside(mouse):
			self.boarddeal(mouse)
			self.changeState("boardclick")
		elif self._exit.inside(mouse):
			self.exitdeal()
			self.changeState("endclick")
		elif self._save.inside(mouse):
			self.savedeal()
			self.changeState("endclick")
		elif self._remove.inside(mouse):
			self.removedeal()
			self.changeState("default")
		elif self._line.inside(mouse):
			self.linedeal()
			self.changeState("default")
		elif self._circle.inside(mouse):
			self.circledeal()
			self.changeState("default")
		elif self._rect.inside(mouse):
			self.rectdeal()
			self.changeState("default")
		elif self._fill.inside(mouse):
			self.filldeal(mouse)
			self.changeState("default")
		elif self._outline.inside(mouse):
			self.outlinedeal(mouse)
			self.changeState("default")
		else:
			self.blankdeal()
			self.changeState("blankclick")
	
	def blankdeal(self):
		pass

	def exitdeal(self):
		pass
	
	def outlinedeal(self,p):
		axis=self._fill.pointToNet(p)
		color=["red","blue","yellow","green","black","grey","white",""]
		self._buff["outline"]=color[axis["i"]]
		
	def filldeal(self,p):
		axis=self._fill.pointToNet(p)
		color=["red","blue","yellow","green","black","grey","white",""]
		self._buff["fill"]=color[axis["i"]]
	
	def linedeal(self):
		self._buff["type"]="line"
		
	def circledeal(self):
		self._buff["type"]="circle"
		
	def rectdeal(self):
		self._buff["type"]="rectangle"

	def removedeal(self):
		tail=len(self._record._graphList)-1
		if tail>=0:
			self._record._graphList[tail].undraw()
			del self._record._graphList[tail]
		
	def savedeal(self):
		self._record.save(self._filename)
		
	def boarddeal(self,p):
		if self._state in ["idle","start"]:
			self._buff["p1"]=self._board.nearNetPoint(p)
			self._buff["p1"].draw(self._win)
		elif self._state=="ready":
			self._buff["p1"].undraw()
			fill=self._buff["fill"]
			outline=self._buff["outline"]
			if self._buff["type"]=="line":
				line=Line(self._buff["p1"],self._board.nearNetPoint(p))
				line.setFill(fill)
				line.draw(self._win)
				self._record.appendGraphs(line)
			elif self._buff["type"]=="circle":
				oval=Oval(self._buff["p1"],self._board.nearNetPoint(p))
				oval.setFill(fill)
				oval.setOutline(outline)
				oval.draw(self._win)
				self._record.appendGraphs(oval)
			elif self._buff["type"]=="rectangle":
				rect=Rectangle(self._buff["p1"],self._board.nearNetPoint(p))
				rect.setFill(fill)
				rect.setOutline(outline)
				rect.draw(self._win)
				self._record.appendGraphs(rect)
				
	def editor(self):
		self.load()
		while self._state!="end":
			self.deal(self._win.getMouse())
			
exe=GraphEditor()
exe.editor()