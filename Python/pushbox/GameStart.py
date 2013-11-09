#coding=gbk
"""
Game:
	推箱子游戏的本体
	其实本来想做一个游戏地图编辑器的（暴雪的world editor是我的梦想），但是时间有限，所以就坑啦

Game的实现内容：
	地图载入
	执行游戏
		
Game的运行流程:
	1.	开始
	2.	载入地图数据
	3.	绘制地图
	4.	while 响应鼠标点击：
			鼠标分析:
				->人物移动:
					可行性分析
					if 可行 then 移动人物
				->箱子移动:
					可行性分析
					if 可行 then 移动箱子
				->触碰按钮
					按钮相应
			胜利条件分析:
				if 胜利:
					准备下一张地图
					跳转至2

Map的结构:
	map={"size":net, "state":a[i][j], "flag":[net,...]}
		net={"i":i , "j":j}
		a[i][j] = 0:blank
				= 1:man
				= 2:block
				= 3:box
""" 
from functools import*
from Assembly import*
from graphics import*

def tryload(func):
	"""try...except...函数模板"""
	@wraps(func)
	def _tryload(self,*args,**kwargs):
		try:
			func(self,*args,**kwargs)
		except:
			try:
				self.map="map/level_1.dat"
				func(self,*args,**kwargs)
			except:
				print "坑爹呢，一张地图都没有，玩毛啊！！！"
	return _tryload

"""定位器，实现网格点之间的转换"""		
class Locator(Assembly):
	def __init__(self,m=1,n=1,width=0,height=0,pos=Point(0,0)):
		Assembly.__init__(self,width,height,pos)
		self._m=m
		self._n=n
	
	def pointToNet(self,p):
		"""获得点p所在的网格坐标{i,j}"""
		netwidth=self._width*1.0/self._n
		j=int((p.getX()-self._pos.getX())/netwidth)
		netheight=self._height*1.0/self._m
		i=int((p.getY()-self._pos.getY())/netheight)
		return {"i":i,"j":j}
		
	def nearNetPoint(self,p):
		"""取得距离点p最近的格点坐标"""
		netwidth=self._width*1.0/self._n
		m=int((p.getX()-self._pos.getX())/netwidth+0.5)
		x=m*netwidth+self._pos.getX()
		
		netheight=self._height*1.0/self._m
		n=int((p.getY()-self._pos.getY())/netheight+0.5)
		y=n*netheight+self._pos.getY()
		return Point(x,y)
	
	def netToPoint(self,i,j):
		"""取得格点[i,j]的物理坐标"""
		netwidth=self._width*1.0/self._n
		x=self._pos.getX()+j*netwidth
		netheight=self._height*1.0/self._m
		y=self._pos.getY()+i*netheight
		return Point(x,y)

class Game:
	def __init__(self):
		w=50
		self.win=GraphWin('Game',14*w,12*w)
		self.win.setBackground('white')
		self.interval=w
		self.map="map/level_1.dat"
		self.locator=None		#网格定位器
		self.state=[]
		self.flag=[]
		self.actor=[]			#主管map上的对象
		#地图是m*n的
		self.m=0
		self.n=0
		
	def start(self):
		w=self.interval
		self.restart=Assembly(w,w,Point(13*w,11*w))
		self.restart.load("bin/restart.dat")
		self.restart.draw(self.win)
		while True:
			self._start()		#一个周期
			for list in self.actor:
				for g in list:
					g.undraw()
			for g in self.flaglist:
				g.undraw()
			self.frame.undraw()
	
	def _start(self):
		self.load()
		self.drawMap()
		loc=self.locator
		while True:
			p=self.win.getMouse()
			if loc.inside(p):		#鼠标分析
				self.dealMove(p)
			elif self.restart.inside(p):
				break
			
			if self.victory():		#胜利条件分析
				tail=self.map.split('_')[1]
				digit=int(tail.split('.')[0])+1
				self.map="map/level_"+str(digit)+".dat"
				break
	
	def victory(self):
		v=True
		for f in self.flag:
			if self.state[f["i"]][f["j"]]!=3:
				v=False
				break
		return v
		
	def _manPos(self):			#man的位置
		for i in range(0,self.m):
			for j in range(0,self.n):
				if self.state[i][j]==1:
					return {"i":i, "j":j}
		print "坑爹，有你这么制作地图的嘛"
		
	def dealMove(self,p):
		loc=self.locator
		state=self.state
		flag=self.flag
		actor=self.actor
		man=self._manPos()		#man的网格位置
		net=loc.pointToNet(p)	#p的网格位置
		if state[net["i"]][net["j"]]==0:	#移动man
			if self._canMoveMan(man,net):
				#移动man
				g=actor[man["i"]][man["j"]]
				actor[man["i"]][man["j"]]=actor[net["i"]][net["j"]]
				actor[net["i"]][net["j"]]=g
				g.moveTo(loc.netToPoint(net["i"],net["j"]))
				#修改状态
				state[man["i"]][man["j"]]=0
				state[net["i"]][net["j"]]=1
		elif state[net["i"]][net["j"]]==3:	#移动box
			if self._canMoveBox(man,net):
				dir=self._boxDir(man,net)
				#移动box
				g1=actor[net["i"]][net["j"]]
				actor[net["i"]][net["j"]]=actor[dir["i"]][dir["j"]]
				actor[dir["i"]][dir["j"]]=g1
				g1.moveTo(loc.netToPoint(dir["i"],dir["j"]))
				#移动man
				g2=actor[man["i"]][man["j"]]
				actor[man["i"]][man["j"]]=actor[net["i"]][net["j"]]
				actor[net["i"]][net["j"]]=g2
				g2.moveTo(loc.netToPoint(net["i"],net["j"]))
				#修改状态
				state[dir["i"]][dir["j"]]=3
				state[man["i"]][man["j"]]=0
				state[net["i"]][net["j"]]=1
		
	def _boxDir(self,man,net):
		di=net["i"]-man["i"]
		dj=net["j"]-man["j"]
		if di==0:
			if dj<0:
				return {"i":net["i"], "j":net["j"]-1}
			elif dj>0:
				return {"i":net["i"], "j":net["j"]+1}
		if dj==0:
			if di>0:
				return {"i":net["i"]+1, "j":net["j"]}
			elif di<0:
				return {"i":net["i"]-1, "j":net["j"]}
		return None
		
	def _canMoveBox(self,man,net):
		dir=self._boxDir(man,net)
		if not dir:
			return False
		if not dir["i"] in range(0,self.m):
			return False
		if not dir["j"] in range(0,self.n):
			return False
		if self.state[dir["i"]][dir["j"]]!=0:
			return False
		return True
	
	def matrix(self,m,n,val):	#生成m*n的二维数组
		a=[]
		for i in range(0,m):
			tmp=[]
			for j in range(0,n):
				tmp.append(val)
			a.append(tmp)
		return a
	
	def _flood(self,judge,i,j):
		judge[i][j]=True
		for di in [-1,1]:
			if i+di>=0 and i+di<self.m:
				if (not judge[i+di][j]) and self.state[i+di][j]==0:
					self._flood(judge,i+di,j)
		for dj in [-1,1]:
			if j+dj>=0 and j+dj<self.n:
				if (not judge[i][j+dj]) and self.state[i][j+dj]==0:
					self._flood(judge,i,j+dj)

				
	def _canMoveMan(self,man,net):
		judge=self.matrix(self.m,self.n,False)	#m*n的judge map
		self._flood(judge,man["i"],man["j"])
		return judge[net["i"]][net["j"]]
	
	
	@tryload					#若没有地图就用第一张地图
	def load(self):
		f=file(self.map)
		str_dct=f.read()
		dct=eval(str_dct,{"__builtins__":None},{})	#主要是不想做地图编辑器了，这里是便于手动写入地图
		f.close()

		self.m=dct["size"]["i"]
		self.n=dct["size"]["j"]
		self.state=dct["state"]
		self.flag=dct["flag"]
		#确定画布比例
		w=self.interval
		m,n=self.m,self.n
		if n>=m:
			width=10*w*1.0
			height=width*m/n
			pos=Point(w,w+(10*w-height)/2.0)
		else:
			height=10*w*1.0
			width=height*n/m
			pos=Point(w+(10*w-width)/2.0,w)
		self.locator=Locator(m,n,width,height,pos)	#装载定位器
		
	def drawMap(self):	
		loc=self.locator
		self.frame=Rectangle(loc.netToPoint(0,0),loc.netToPoint(loc._m,loc._n))
		self.frame.setOutline("black")
		#self.frame.setFill(color_rgb(0,255,255))
		self.frame.draw(self.win)
		w=self.locator._width*1.0/self.locator._n
		#画flag
		self.flaglist=[]
		for net in self.flag:
			pos=loc.netToPoint(net["i"],net["j"])
			g=Assembly(w,w,pos)
			g.load("bin/flag.dat")
			g.draw(self.win)
			self.flaglist.append(g)
		#画图形
		self.actor=self.matrix(self.m,self.n,None)
		for i in range(0,self.m):
			for j in range(0,self.n):
				pos=loc.netToPoint(i,j)
				g=Assembly(w,w,pos)
				if self.state[i][j]==1:				#载入man
					g.load("bin/man.dat")
				elif self.state[i][j]==2:			#载入block
					g.load("bin/block.dat")
				elif self.state[i][j]==3:			#载入box
					g.load("bin/box2.dat")
				g.draw(self.win)
				self.actor[i][j]=g					#装入控件

def game():
	exe=Game()
	exe.start()

if __name__ == "__main__":
    game()
