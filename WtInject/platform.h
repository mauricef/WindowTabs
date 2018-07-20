#include "stdafx.h"

extern int nWinVersion;

#define WIN_VERSION_UNSUPPORTED    (-1)
#define WIN_VERSION_7              0
#define WIN_VERSION_8              1
#define WIN_VERSION_81             2

#define DO7X(d7, dx)               ((nWinVersion == WIN_VERSION_7) ? (d7) : (dx))
#define DOX81(dx, d81)             ((nWinVersion != WIN_VERSION_81) ? (dx) : (d81))
#define DO7_8_81(d7, d8, d81)      (DO7X(d7, DOX81(d8, d81)))

#ifdef _WIN64
#define DEF3264(d32, d64)          (d64)
#else
#define DEF3264(d32, d64)          (d32)
#endif

#define DO7X_3264(d7_32, d7_64, dx_32, dx_64) \
	DO7X(DEF3264(d7_32, d7_64), DEF3264(dx_32, dx_64))
#define DOX81_3264(dx_32, dx_64, d81_32, d81_64) \
	DOX81(DEF3264(dx_32, dx_64), DEF3264(d81_32, d81_64))
#define DO7_8_81_3264(d7_32, d7_64, d8_32, d8_64, d81_32, d81_64) \
	(DO7X(DEF3264(d7_32, d7_64), DOX81(DEF3264(d8_32, d8_64), DEF3264(d81_32, d81_64))))