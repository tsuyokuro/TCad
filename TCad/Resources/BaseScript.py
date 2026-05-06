# coding: cp932

# TCad script
# version 1.0

import time
import math
import sys

from System.Collections import *
from System.Collections.Generic import List
from System import UInt32 as uint

import clr
clr.AddReference('CadDataTypes')
clr.AddReference('OpenTK.Mathematics')


import CadDataTypes.CadVertex as Vertex
import CadDataTypes.VertexList as VertexList
import CadDataTypes.CadMesh as CadMesh
import CadDataTypes.CadFace as CadFace

import OpenTK.Mathematics.Vector3 as Vector3
import OpenTK.Mathematics.Vector3 as vector



###############################################################################
# Layer
#

#[AC] layer_list()
def layer_list():
    _se_.LayerList()

#[AC] add_layer(name)
def add_layer(name):
    _se_.AddLayer(name)


###############################################################################
# Last down point
#

#[AC] last_down()
def last_down():
    return _se_.GetLastDownPoint()

#[AC] get_last_down()
def get_last_down():
    pt = _se_.GetLastDownPoint()
    return pt

#[AC] move_last_down(x=10, y=0, z=0)
def move_last_down(x, y, z):
    _se_.MoveLastDownPoint(x, y, z)

#[AC] set_last_down(x=0, y=0, z=0)
def set_last_down(x, y, z):
    _se_.SetLastDownPoint(x, y, z)


###############################################################################
# Figure list
#

#[AC] get_tree_view_pos(id=current_fig_id())
def get_tree_view_pos(id):
    return _se_.GetTreeViewPos(id)

#[AC] set_tree_view_pos(idx=0)
def set_tree_view_pos(idx):
    return _se_.SetTreeViewPos(idx)

#[AC] sel_fig(id)
def sel_fig(id):
    _se_.SelectFigure(id)

#[AC] get_selected_fig_list()
def get_selected_fig_list():
	return _se_.GetSelectedFigList()

#[AC] to_fig_list(id_list=[1,2])
def to_fig_list(id_list):
	return _se_.ToFigList(id_list)

#[AC] to_fig_id_array(list)
def to_fig_id_array(list):
	ret = []
	for i in range(list.Count):
		f = list[i]
		ret = ret + [int(f.ID)]
	return ret

###############################################################################
# Group management
#

#[AC] group(list=get_selected_fig_list())
#[AC] group(list=[1,2])
def group(list):
    _se_.Group(list)

#[AC] ungroup(list=get_selected_fig_list())
#[AC] ungroup(list=[1,2])
#[AC] ungroup(1)
def ungroup(list):
    _se_.Ungroup(list)


###############################################################################
# Boolean operation
#

#[AC] sub(l_id=1, r_id=2)
def sub(l_id, r_id):
    _se_.AsubB(l_id, r_id)

#[AC] union(id1=1, id2=2)
def union(id1, id2):
    _se_.Union(id1, id2)

#[AC] intersection(id1=1, id2=2)
def intersection(id1, id2):
    _se_.Intersection(id1, id2)



###############################################################################
# Add figure
#

#[AC] add_rect(w=10, h=10)
def add_rect(w=10, h=10):
    return _se_.AddRect(w, h)

#[AC] add_rect_at(pv=last_down(), w=10, h=10)
def add_rect_at(pv, w=10, h=10):
    return _se_.AddRectAt(pv, w, h)


#[AC] add_rectc(w=10, h=10, c=1)
def add_rectc(w=10, h=10, c=1):
    return _se_.AddRectChamfer(w, h, c)

#[AC] add_rectc_at(pv=last_down(), w=10, h=10, c=1)
def add_rectc_at(pv, w=10, h=10, c=1):
    return _se_.AddRectChamferAt(pv, w, h, c)

#[AC] add_circle(r=10)
def add_circle(r=10):
    return _se_.AddCircle(r)

#[AC] add_circle_at(cv=last_down(), r=10)
def add_circle_at(cv, r=10):
    return _se_.AddCircleAt(cv, r)


#[AC] add_point(x=0, y=0, z=0)
def add_point(x, y, z):
    return _se_.AddPoint(x, y, z)

#[AC] add_point_v(last_down())
def add_point_v(p):
    return _se_.AddPoint(p)

#[AC] add_box(last_down(), size_x=40, size_y=40, size_z=20)
def add_box(pos, size_x, size_y, size_z):
    return _se_.AddBox(pos, size_x, size_y, size_z)

#[AC] add_1x4(last_down(), len=200)
def add_1x4(pos, len):
    return _se_.AddBox(pos, w_1x4, len, t_1x4)

#[AC] add_cylinder(pos=last_down(), circleDiv=16, slices=2, r=10, len=40)
def add_cylinder(pos, circleDiv, slices, r, len):
    return _se_.AddCylinder(pos, circleDiv, slices, r, len)

#[AC] add_sphere(pos=last_down(), slices=16, r=20)
def add_sphere(pos, slices, r):
    return _se_.AddSphere(pos, slices, r)

#[AC] add_line(vector(0, 0, 0), vector(10, 20, 0))
def add_line(v0, v1):
    return _se_.AddLine(v0, v1)

#[AC] add_tetra(last_down(), size_x=20, size_y=20, size_z=20)
def add_tetra(pos, size_x, size_y, size_z):
    return _se_.AddTetrahedron(pos, size_x, size_y, size_z)

#[AC] add_octa(last_down(), size_x=20, size_y=20, size_z=20)
def add_octa(pos, size_x, size_y, size_z):
    return _se_.AddOctahedron(pos, size_x, size_y, size_z)

#[AC] add_picture(last_down(), r"H:\work4\test.png")
def add_picture(pos, fname):
    return _se_.AddPicture(pos, fname)


###############################################################################
# Edit

#[AC] extrude(id=current_fig_id(), dir=unit_vz, d=20, div=0)
def extrude(id, dir, d, div):
    _se_.Extrude(id, dir, d, div)

#[AC] move(id=current_fig_id(), x=0, y=0, z=0)
def move(id, x=0, y=0, z=0):
    _se_.Move(id, x, y, z)

#[AC] move_selected_point(x=0, y=0, z=0)
def move_selected_point(x=0, y=0, z=0):
    _se_.MoveSelectedPoint(x, y, z)

#[AC] triangulate(id=current_fig_id(), area=10000, deg=20)
def triangulate(id, area, deg):
    _se_.Triangulate(id, area, deg)

#[AC] triangulate_opt(id=current_fig_id(), option="a10000q")
def triangulate_opt(id, option):
    _se_.Triangulate(id, option)

#[AC] to_mesh(current_fig_id())
def to_mesh(id):
    _se_.ToMesh(id)

#[AC] to_poly(current_fig_id())
def to_poly(id):
    _se_.ToPolyLine(id)

#[AC] invert_dir()
def invert_dir():
    _se_.InvertDir()

#[AC] scale(id=current_fig_id(), org=last_down(), ratio=1.5)
def scale(id, org, ratio):
    _se_.Scale(id, org, ratio)

#[AC] rotate(id=current_fig_id(), p0=input_point(), v=view_dir(), t=45)
def rotate(id, p0, v, t):
    if is_invalid_vector(p0):
        return

    _se_.Rotate(id, p0, v, t)

#[AC] make_rotating_body_itr(current_fig_id(), top_cap=True, btm_cap=True)
def make_rotating_body_itr(id, top_cap, btm_cap):
    print(esc_bg_b_green + esc_black + " <<<< Input Axis >>>> " + esc_reset)

    (p1, p2) = input_line();

    if is_invalid_vector(p1):
        return

    org = p1;
    axis = (p2 - p1).Normalized()
    _se_.MakeRotatingBody(id, org, axis, top_cap, btm_cap);

#[AC] ins_point()
def ins_point():
    _se_.InsPoint()

#[AC] get_str(msg="Input", defStr="")
def get_str(msg, defStr):
    return _se_.GetString(msg, defStr)

#[AC] set_seg_len(len)
def set_seg_len(len):
    _se_.SetSelectedSegLen(len)

#[AC] set_fig_name(id=current_fig_id(), name="name")
def set_fig_name(id, name):
    _se_.SetFigName(id, name)

#[AC] get_point(figID=current_fig_id(), index=0)
def get_point(figID, index):
	return _se_.GetPoint(figID, index);

#[AC] set_point(figID=current_fig_id(), index=0, dv=vector(0,0,0))
def set_point(figID, index, dv):
	return _se_.SetPoint(figID, index, dv);

###############################################################################


#[AC] current_fig_id()
def current_fig_id():
    return _se_.GetCurrentFigureID()

#[AC] currentFig()
def currentFig():
    return _se_.GetCurrentFigure()


###############################################################################

#[AC] rotatev(v=unit_vx, axis=unit_vz, deg=45.0)
def rotatev(v, axis, deg):
    return _se_.RotateVector(v, axis, deg)

#[AC] is_valid_vector(v=unit_vx)
def is_valid_vector(v):
    return _se_.IsValidVector(v)

#[AC] is_invalid_vector(v=unit_vx)
def is_invalid_vector(v):
    return _se_.IsInvalidVector(v)

###############################################################################
# Print information
#

#[AC] puts(s)
def puts(s):
    _se_.PutMsg(s)

#[AC] print_vector(v)
def print_vector(v):
    _se_.PrintVector(v)

#[AC] dumpv(v=unit_vx)
def dumpv(v):
    return _se_.DumpVector(v)

#[AC] dump_mesh(id=current_fig_id())
def dump_mesh(id):
    _se_.DumpMesh(id)


###############################################################################
# Interraction
#

#[AC] input_point()
def input_point():
    return _se_.InputPoint()

#[AC] input_unit_v()
def input_unit_v():
    return _se_.InputUnitVector()

#[AC] input_line()
def input_line():
    return _se_.InputLine()

###############################################################################
# User interface
#

#[AC] update_tree()
def update_tree():
    _se_.UpdateTV()

#[AC] view_dir()
def view_dir():
	return _se_.ViewDir()

#[AC] proj_dir()
def proj_dir():
    return _se_.GetProjectionDir()

#[AC] cut_mesh(id=current_fig_id())
def cut_mesh(id):
    _se_.CutMesh(id)

###############################################################################
# Mesure
#

#[AC] area_of_selected()
def area_of_selected():
    return _se_.AreaOfSelected()

#[AC] centroid_of_selected()
def centroid_of_selected():
	return _se_.CentroidOfSelected()

###############################################################################

#[AC] to_bmp(64, 64, 0xffffffff, 1, r"")
#[AC] to_bmp(128, 128, 0xffffffff, 1, r"")
def to_bmp(bw, bh, argb=0xffffffff, linew=1, fname=r""):
    _se_.CreateBitmap(bw, bh, argb, linew, fname)

#[AC] dev_p_to_world_p(p)
def dev_p_to_world_p(p):
    return _se_.DevPToWorldP(p)

#[AC] world_p_to_dev_p(p)
def world_p_to_dev_p(p):
    return _se_.WorldPToDevP(p)

#[AC] rad2deg(rad)
def rad2deg(rad):
	return 180.0 * rad / math.pi

#[AC] deg2rad(deg)
def deg2rad(deg):
	return math.pi * deg / 180.0

#[AC] get_fig(id=1)
def get_fig(id):
	return _se_.GetFigure(id)

#[AC] get_vertex(fig, index)
def get_vertex(fig, index):
	return _se_.FigVertexAt(fig, index)

###############################################################################
#[AC] set_color(id=current_fig_id(), r=1.0, g=1.0, b=1.0)
def set_color(id, r, g, b):
    _se_.SetColor(id, r, g, b)

#[AC] set_fill_color(id=current_fig_id(), r=1.0, g=1.0, b=1.0)
def set_fill_color(id, r, g, b):
    _se_.SetFillColor(id, r, g, b)

###############################################################################

#[AC] test()
def test():
	_se_.Test()

###############################################################################

class MyConsoleOut:
	def write(self, s):
		_se_.Print(s)

cout = MyConsoleOut()

sys.stdout = cout

#[AC] point0
point0 = Vector3(0,0,0)

#[AC] unit_vx
#[AC] unit_vy
#[AC] unit_vz
unit_vx = Vector3(1,0,0)
unit_vy = Vector3(0,1,0)
unit_vz = Vector3(0,0,1)

#[AC] w_1x4
#[AC] t_1x4
w_1x4 = 89
t_1x4 = 19

#[AC] esc_reset
esc_reset = "\x1b[0m"

#通常前景色
#[AC] esc_
esc_black = "\x1b[30m"
esc_red = "\x1b[31m"
esc_green = "\x1b[32m"
esc_yellow = "\x1b[33m"
esc_blue = "\x1b[34m"
esc_magenta = "\x1b[35m"
esc_cyan = "\x1b[36m"
esc_white = "\x1b[37m"

#明るい前景色
#[AC] esc_b_
esc_b_balck = "\x1b[90m"
esc_b_red = "\x1b[91m"
esc_b_green = "\x1b[92m"
esc_b_yellow = "\x1b[93m"
esc_b_blue = "\x1b[94m"
esc_b_magenta = "\x1b[95m"
esc_b_cyan = "\x1b[96m"
esc_b_white = "\x1b[97m"

#通常背景色
#[AC] esc_bg_
esc_bg_black = "\x1b[40m";
esc_bg_red = "\x1b[41m";
esc_bg_green = "\x1b[42m";
esc_bg_yellow = "\x1b[43m";
esc_bg_blue = "\x1b[44m";
esc_bg_magenta = "\x1b[45m";
esc_bg_cyan = "\x1b[46m";
esc_bg_white = "\x1b[47m";

#明るい背景色
#[AC] esc_bg_b_
esc_bg_b_black = "\x1b[100m";
esc_bg_b_red = "\x1b[101m";
esc_bg_b_green = "\x1b[102m";
esc_bg_b_yellow = "\x1b[103m";
esc_bg_b_blue = "\x1b[104m";
esc_bg_b_magenta = "\x1b[105m";
esc_bg_b_cyan = "\x1b[106m";
esc_bg_b_white = "\x1b[107m";

global _cancel_
_cancel_ = False

def raise_cancel():
    global _cancel_
    _cancel_ = True

def reset_cancel():
    global _cancel_
    _cancel_ = False

def check_cancel():
    if (_cancel_):
        sys.exit()
