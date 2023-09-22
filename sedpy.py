import sys
import re

fname = sys.argv[1]

before_str = r'using vcompo_t = System.Single;.*using matrix4_t = OpenTK.Mathematics.Matrix4;'

after_str = r'''
#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif
'''

f = open(fname,'r', encoding="utf-8")
body = f.read()
f.close()

result = re.match(".*DEFAULT_DATA_TYPE_DOUBLE", body, flags=re.DOTALL)
if (result):
  print(fname + " <<<< Skip. Aleady Patched.")
  exit()

print(fname + " <<<< Patch")

s = "//#define DEFAULT_DATA_TYPE_DOUBLE\n"
s += re.sub(before_str, after_str, body, flags=re.DOTALL)

f = open(fname,'w', encoding="utf-8")
f.write(s)
f.close()

