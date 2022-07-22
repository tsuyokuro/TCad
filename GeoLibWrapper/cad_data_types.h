#pragma once
#ifdef WIN32
	#ifdef _DEBUG
		#using "..\\CadDataTypes\\bin\\x86\\Debug\\net6.0\\CadDataTypes.dll"
	#else
		#using "..\\CadDataTypes\\bin\\x86\\Release\\net6.0\\CadDataTypes.dll"
	#endif
#else
	#ifdef _DEBUG
		#using "..\\CadDataTypes\\bin\\x64\\Debug\\net6.0\\CadDataTypes.dll"
	#else
		#using "..\\CadDataTypes\\bin\\x64\\Release\\net6.0\\CadDataTypes.dll"
	#endif
#endif
