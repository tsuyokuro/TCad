#pragma once
#ifdef WIN32
	#ifdef _DEBUG
		#using "..\\CadDataTypes\\bin\\x86\\Debug\\netcoreapp3.1\\CadDataTypes.dll"
	#else
		#using "..\\CadDataTypes\\bin\\x86\\Release\\netcoreapp3.1\\CadDataTypes.dll"
	#endif
#else
	#ifdef _DEBUG
		#using "..\\CadDataTypes\\bin\\x64\\Debug\\netcoreapp3.1\\CadDataTypes.dll"
	#else
		#using "..\\CadDataTypes\\bin\\x64\\Release\\netcoreapp3.1\\CadDataTypes.dll"
	#endif
#endif
