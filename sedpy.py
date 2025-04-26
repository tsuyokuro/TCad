import sys
import re

fname = sys.argv[1]

before_str1 = r'namespace Plotter.Controller;'
after_str1 = r'namespace TCad.Plotter.Controller;'

before_str2 = r'using Plotter.Controller;'
after_str2 = r'using TCad.Plotter.Controller;'


f = open(fname,'r', encoding="utf-8")
source = f.read()
f.close()

match1 = re.match(before_str1, source, flags=re.DOTALL)
match2 = re.match(before_str2, source, flags=re.DOTALL)

print(fname + " <<<< PATCH")

s1 = re.sub(before_str1, after_str1, source, flags=re.DOTALL)
s2 = re.sub(before_str2, after_str2, s1, flags=re.DOTALL)

f = open(fname,'w', encoding="utf-8")
f.write(s2)
f.close()

